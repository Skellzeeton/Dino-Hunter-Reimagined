using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class iMGCenter : iBaseCenter
{
	protected Dictionary<int, WaveInfo> m_dictWaveInfo;
	protected Dictionary<int, Dictionary<int, WaveInfo>> m_dictWaveOverride;

	public iMGCenter()
	{
		m_dictWaveInfo = new Dictionary<int, WaveInfo>();
		m_dictWaveOverride = new Dictionary<int, Dictionary<int, WaveInfo>>();
	}

	protected override void LoadData(string content)
	{
		m_dictWaveInfo.Clear();
		m_dictWaveOverride.Clear();
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(content);
		string value = string.Empty;
		XmlNode documentElement = xmlDocument.DocumentElement;
		foreach (XmlNode childNode in documentElement.ChildNodes) {
			if (childNode.Name != "gamewave" || !MyUtils.GetAttribute(childNode, "id", ref value))
				continue;
			WaveInfo waveInfo = new WaveInfo();
			waveInfo.nID = int.Parse(value);
			int overrideDifficulty = -1;
			// NEW
			int[] overrideDifficulties = null;
			if (MyUtils.GetAttribute(childNode, "override", ref value))
				overrideDifficulties = ParseOverrideDifficulties(value, waveInfo.nID);
			waveInfo.nOverride = overrideDifficulties != null ? overrideDifficulties[0] : -1;
			if (MyUtils.GetAttribute(childNode, "trigger", ref value))
				waveInfo.nEventType = int.Parse(value);
			if (MyUtils.GetAttribute(childNode, "triggervalue", ref value))
			{
				waveInfo.ltEventParam = new List<int>();
				string[] array = value.Split(',');
				for (int i = 0; i < array.Length; i++)
					waveInfo.ltEventParam.Add(int.Parse(array[i]));
			}
			if (MyUtils.GetAttribute(childNode, "triggerloop", ref value))
				waveInfo.bEventLoop = bool.Parse(value);
			if (MyUtils.GetAttribute(childNode, "delay", ref value))
				waveInfo.m_fDelayTime = MyUtils.ParseFloat(value);
			if (MyUtils.GetAttribute(childNode, "interval", ref value))
				waveInfo.m_fInterval = MyUtils.ParseFloat(value);
			if (MyUtils.GetAttribute(childNode, "number", ref value))
				waveInfo.m_nNumAtOnce = int.Parse(value);
			if (MyUtils.GetAttribute(childNode, "loop", ref value))
				waveInfo.m_nLoop = int.Parse(value);
			if (MyUtils.GetAttribute(childNode, "cutscene", ref value))
				waveInfo.sCutScene = value;
			if (MyUtils.GetAttribute(childNode, "cutscenecontent", ref value))
				waveInfo.sCutSceneContent = value;
			if (MyUtils.GetAttribute(childNode, "cutscene_BGM", ref value))
				waveInfo.sCutSceneBGM = value;
			if (MyUtils.GetAttribute(childNode, "cutscene_ambience", ref value))
				waveInfo.sCutSceneAmbience = value;
			if (MyUtils.GetAttribute(childNode, "israndom", ref value))
				waveInfo.m_bRandom = bool.Parse(value);
			if (MyUtils.GetAttribute(childNode, "mobcount", ref value))
				waveInfo.m_nMobCount = int.Parse(value);
			if (MyUtils.GetAttribute(childNode, "moblevel_default", ref value))
				waveInfo.nDefaultMobLevel = int.Parse(value);
			foreach (XmlNode childNode2 in childNode.ChildNodes) {
				if (childNode2.Name != "mob")
					continue;
				waveInfo.m_ltWaveMobInfo.Clear();
				if (!MyUtils.GetAttribute(childNode2, "mob_id", ref value))
					continue;
				string[] array = value.Split(',');
				if (array != null && array.Length > 0)
				{
					for (int j = 0; j < array.Length; j++) {
						WaveMobInfo waveMobInfo = new WaveMobInfo();
						waveMobInfo.nID = int.Parse(array[j]);
						waveInfo.m_ltWaveMobInfo.Add(waveMobInfo);
					}
				}
				if (MyUtils.GetAttribute(childNode2, "mob_level", ref value))
				{
					array = value.Split(',');
					if (array != null && array.Length > 0)
					{
						for (int k = 0; k < array.Length && k < waveInfo.m_ltWaveMobInfo.Count; k++)
							waveInfo.m_ltWaveMobInfo[k].nLevel = int.Parse(array[k]);
					}
				}
				if (MyUtils.GetAttribute(childNode2, "spawnmode", ref value))
				{
					array = value.Split(',');
					if (array != null && array.Length > 0)
					{
						for (int l = 0; l < array.Length && l < waveInfo.m_ltWaveMobInfo.Count; l++)
							waveInfo.m_ltWaveMobInfo[l].SpawnMode = int.Parse(array[l]);
					}
				}
				if (MyUtils.GetAttribute(childNode2, "startpoint", ref value))
				{
					array = value.Split(',');
					if (array != null && array.Length > 0)
					{
						for (int m = 0; m < array.Length && m < waveInfo.m_ltWaveMobInfo.Count; m++)
							waveInfo.m_ltWaveMobInfo[m].nStartPoint = int.Parse(array[m]);
					}
				}
				if (!MyUtils.GetAttribute(childNode2, "random", ref value))
					continue;
				array = value.Split(',');
				if (array != null && array.Length > 0)
				{
					for (int n = 0; n < array.Length && n < waveInfo.m_ltWaveMobInfo.Count; n++)
						waveInfo.m_ltWaveMobInfo[n].nRandom = int.Parse(array[n]);
				}
			}
			if (overrideDifficulties == null)
			{
				if (m_dictWaveInfo.ContainsKey(waveInfo.nID))
					Debug.LogWarning($"Duplicate base wave ID {waveInfo.nID}. Overwriting.");
				m_dictWaveInfo[waveInfo.nID] = waveInfo;
			}
			else
			{
				if (!m_dictWaveOverride.ContainsKey(waveInfo.nID))
					m_dictWaveOverride[waveInfo.nID] = new Dictionary<int, WaveInfo>();
				foreach (int ov in overrideDifficulties) {
					if (m_dictWaveOverride[waveInfo.nID].ContainsKey(ov))
						Debug.LogWarning($"Duplicate override wave ID {waveInfo.nID} for difficulty {ov}. Overwriting.");
					m_dictWaveOverride[waveInfo.nID][ov] = waveInfo;
				}
			}
		}
	}

	private static int[] ParseOverrideDifficulties(string raw, int waveId)
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
				Debug.LogWarning($"[iMGCenter] Invalid override value '{t}' on wave ID {waveId}, skipping.");
			}
		}
		return result.Count > 0 ? result.ToArray() : null;
	}

	public Dictionary<int, WaveInfo> GetData()
	{
		return m_dictWaveInfo;
	}

	public WaveInfo Get(int nID)
	{
		int difficulty = 1;
		iDataCenter dataCenter = iGameApp.GetInstance().m_GameData.GetDataCenter();
		if (dataCenter != null)
			difficulty = dataCenter.Difficulty;
		if (m_dictWaveOverride.TryGetValue(nID, out var overrideDict))
		{
			if (overrideDict.TryGetValue(difficulty, out var overrideWave))
				return overrideWave;
		}
		if (m_dictWaveInfo.TryGetValue(nID, out var baseWave))
			return baseWave;
		return null;
	}
}