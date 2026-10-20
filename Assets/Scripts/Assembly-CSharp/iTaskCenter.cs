using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class iTaskCenter : iBaseCenter
{
	protected Dictionary<int, CTaskInfo> m_dictTaskInfo;
	protected Dictionary<int, Dictionary<int, CTaskInfo>> m_dictTaskOverride;

	public iTaskCenter()
	{
		m_dictTaskInfo = new Dictionary<int, CTaskInfo>();
		m_dictTaskOverride = new Dictionary<int, Dictionary<int, CTaskInfo>>();
	}

	public CTaskInfo Get(int nID)
	{
		int difficulty = 1;
		var dataCenter = iGameApp.GetInstance()?.m_GameData?.GetDataCenter();
		if (dataCenter != null)
			difficulty = dataCenter.Difficulty;
		if (m_dictTaskOverride.TryGetValue(nID, out var overrideDict)
		&& overrideDict.TryGetValue(difficulty, out var overrideTask))
			return overrideTask;
		if (m_dictTaskInfo.TryGetValue(nID, out var baseTask))
			return baseTask;
		return null;
	}

	protected override void LoadData(string content)
	{
		m_dictTaskInfo.Clear();
		m_dictTaskOverride.Clear();
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(content);
		string value = string.Empty;
		XmlNode documentElement = xmlDocument.DocumentElement;
		foreach (XmlNode childNode in documentElement.ChildNodes)
		{
			if (childNode.Name != "task" || !MyUtils.GetAttribute(childNode, "id", ref value))
				continue;
			int nID = int.Parse(value);
			if (!MyUtils.GetAttribute(childNode, "type", ref value))
				continue;
			int num = int.Parse(value);
			int[] overrideDifficulties = null;
			if (MyUtils.GetAttribute(childNode, "override", ref value))
				overrideDifficulties = ParseOverrideDifficulties(value, nID);
			CTaskInfo cTaskInfo = null;
			switch (num)
			{
				case 1: cTaskInfo = LoadAttributeCollect(nID, childNode); break;
				case 2: cTaskInfo = LoadAttributeHunter(nID, childNode); break;
				case 3: cTaskInfo = LoadAttributeDefence(nID, childNode); break;
				case 4: cTaskInfo = LoadAttributeSurvival(nID, childNode); break;
				case 5: cTaskInfo = LoadAttributeButcher(nID, childNode); break;
				case 6: cTaskInfo = LoadAttributeInfinite(nID, childNode); break;
				case 7: cTaskInfo = LoadAttributeTimed(nID, childNode); break;
			}
			if (cTaskInfo == null)
				continue;
			cTaskInfo.nOverride = overrideDifficulties != null ? overrideDifficulties[0] : -1;
			if (MyUtils.GetAttribute(childNode, "name", ref value))
				cTaskInfo.sName = value;
			else
				cTaskInfo.sName = "This is task " + cTaskInfo.nID;
			if (MyUtils.GetAttribute(childNode, "desc", ref value))
				cTaskInfo.sDesc = value;
			else
				cTaskInfo.sDesc = "This is desc of task " + cTaskInfo.nID;
			if (MyUtils.GetAttribute(childNode, "rewards", ref value))
				cTaskInfo.sRewards = value;
			else
				cTaskInfo.sDesc = "EXP: \nGOLD: " + cTaskInfo.nID;
			if (MyUtils.GetAttribute(childNode, "FinishList", ref value))
			{
				string[] array = value.Split(',');
				for (int i = 0; i < array.Length && i < cTaskInfo.arrFail.Length; i++)
					cTaskInfo.arrFail[i] = int.Parse(array[i]);
			}
			if (MyUtils.GetAttribute(childNode, "FinishValueList", ref value))
			{
				string[] array = value.Split(',');
				for (int j = 0; j < array.Length && j < cTaskInfo.arrFailValue.Length; j++)
					cTaskInfo.arrFailValue[j] = int.Parse(array[j]);
			}
			if (overrideDifficulties == null)
			{
				if (m_dictTaskInfo.ContainsKey(cTaskInfo.nID))
					Debug.LogWarning($"Duplicate base task ID {cTaskInfo.nID}. Overwriting.");
				m_dictTaskInfo[cTaskInfo.nID] = cTaskInfo;
			}
			else
			{
				if (!m_dictTaskOverride.ContainsKey(cTaskInfo.nID))
					m_dictTaskOverride[cTaskInfo.nID] = new Dictionary<int, CTaskInfo>();
				foreach (int ov in overrideDifficulties)
				{
					if (m_dictTaskOverride[cTaskInfo.nID].ContainsKey(ov))
						Debug.LogWarning($"Duplicate task ID {cTaskInfo.nID} for difficulty {ov}. Overwriting.");
					m_dictTaskOverride[cTaskInfo.nID][ov] = cTaskInfo;
				}
			}
		}
	}

	private static int[] ParseOverrideDifficulties(string raw, int taskId)
	{
		if (string.IsNullOrEmpty(raw))
			return null;
		string[] parts = raw.Split(',');
		List<int> result = new List<int>();
		foreach (string p in parts)
		{
			string t = p.Trim();
			if (int.TryParse(t, out int ov))
			{
				if (!result.Contains(ov))
					result.Add(ov);
			}
			else if (t.Length > 0)
			{
				Debug.LogWarning($"[iTaskCenter] Invalid override value '{t}' on task ID {taskId}, skipping.");
			}
		}
		return result.Count > 0 ? result.ToArray() : null;
	}

	protected CTaskInfoCollect LoadAttributeCollect(int nID, XmlNode node)
	{
		string value = string.Empty;
		CTaskInfoCollect cTaskInfoCollect = new CTaskInfoCollect();
		cTaskInfoCollect.nID = nID;
		cTaskInfoCollect.nType = 1;
		if (MyUtils.GetAttribute(node, "IDList", ref value))
		{
			string[] array = value.Split(',');
			for (int i = 0; i < array.Length; i++)
				cTaskInfoCollect.ltCollectInfo.Add(new CTaskInfoCollect.CCollectInfo(int.Parse(array[i]), 1));
		}
		if (MyUtils.GetAttribute(node, "NUMList", ref value))
		{
			string[] array = value.Split(',');
			for (int j = 0; j < array.Length && j < cTaskInfoCollect.ltCollectInfo.Count; j++)
				cTaskInfoCollect.ltCollectInfo[j].nMaxCount = int.Parse(array[j]);
		}
		if (MyUtils.GetAttribute(node, "EggLife", ref value))
			cTaskInfoCollect.fLife = MyUtils.ParseFloat(value);
		return cTaskInfoCollect;
	}

	protected CTaskInfoHunter LoadAttributeHunter(int nID, XmlNode node)
	{
		string value = string.Empty;
		CTaskInfoHunter cTaskInfoHunter = new CTaskInfoHunter();
		cTaskInfoHunter.nID = nID;
		cTaskInfoHunter.nType = 2;
		if (MyUtils.GetAttribute(node, "IDList", ref value))
		{
			string[] array = value.Split(',');
			for (int i = 0; i < array.Length; i++)
				cTaskInfoHunter.ltHunterInfo.Add(new CTaskInfoHunter.CHunterInfo(int.Parse(array[i]), 1));
		}
		if (MyUtils.GetAttribute(node, "NUMList", ref value))
		{
			string[] array = value.Split(',');
			for (int j = 0; j < array.Length && j < cTaskInfoHunter.ltHunterInfo.Count; j++)
				cTaskInfoHunter.ltHunterInfo[j].nMaxCount = int.Parse(array[j]);
		}
		return cTaskInfoHunter;
	}

	protected CTaskInfoDefence LoadAttributeDefence(int nID, XmlNode node)
	{
		string value = string.Empty;
		CTaskInfoDefence cTaskInfoDefence = new CTaskInfoDefence();
		cTaskInfoDefence.nID = nID;
		cTaskInfoDefence.nType = 3;
		if (MyUtils.GetAttribute(node, "WaveCount", ref value))
			cTaskInfoDefence.nWaveCount = int.Parse(value);
		if (MyUtils.GetAttribute(node, "ZhaLanLife", ref value))
			cTaskInfoDefence.fLife = MyUtils.ParseFloat(value);
		return cTaskInfoDefence;
	}

	protected CTaskInfoSurvival LoadAttributeSurvival(int nID, XmlNode node)
	{
		CTaskInfoSurvival cTaskInfoSurvival = new CTaskInfoSurvival();
		cTaskInfoSurvival.nID = nID;
		cTaskInfoSurvival.nType = 4;
		return cTaskInfoSurvival;
	}

	protected CTaskInfoButcher LoadAttributeButcher(int nID, XmlNode node)
	{
		CTaskInfoButcher cTaskInfoButcher = new CTaskInfoButcher();
		cTaskInfoButcher.nID = nID;
		cTaskInfoButcher.nType = 5;
		return cTaskInfoButcher;
	}

	protected CTaskInfoInfinite LoadAttributeInfinite(int nID, XmlNode node)
	{
		CTaskInfoInfinite cTaskInfoInfinite = new CTaskInfoInfinite();
		cTaskInfoInfinite.nID = nID;
		cTaskInfoInfinite.nType = 6;
		return cTaskInfoInfinite;
	}

	protected CTaskInfoTimed LoadAttributeTimed(int nID, XmlNode node)
	{
		CTaskInfoTimed cTaskInfoTimed = new CTaskInfoTimed();
		cTaskInfoTimed.nID = nID;
		cTaskInfoTimed.nType = 7;
		return cTaskInfoTimed;
	}
}