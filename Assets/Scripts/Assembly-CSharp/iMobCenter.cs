using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class iMobCenter : iBaseCenter
{
	protected Dictionary<int, CMobInfo> m_dictMobInfo;

	public iMobCenter()
	{
		m_dictMobInfo = new Dictionary<int, CMobInfo>();
	}

	public CMobInfo Get(int nID)
	{
		if (!m_dictMobInfo.ContainsKey(nID))
		{
			return null;
		}
		return m_dictMobInfo[nID];
	}

	public CMobInfoLevel Get(int nID, int nLevel)
	{
		CMobInfo cMobInfo = Get(nID);
		if (cMobInfo == null)
		{
			return null;
		}
		CMobInfoLevel levelInfo = cMobInfo.Get(nLevel);
		if (levelInfo == null && cMobInfo.HasBaseData())
		{
			levelInfo = GenerateLevelData(cMobInfo, nLevel);
			if (levelInfo != null)
			{
				cMobInfo.Add(nLevel, levelInfo);
			}
		}
		
		return levelInfo;
	}
	
	private float CalculateScaledValue(float startValue, float endValue, float t, string scaleType)
	{
		if (scaleType == "exponential")
		{
			if (startValue <= 0) return endValue * t;
			float ratio = endValue / startValue;
			return startValue * Mathf.Pow(ratio, t);
		}
		else
		{
			return Mathf.Lerp(startValue, endValue, t);
		}
	}
	
	private int CalculateScaledValueInt(int startValue, int endValue, float t, string scaleType)
	{
		return Mathf.RoundToInt(CalculateScaledValue((float)startValue, (float)endValue, t, scaleType));
	}
	
	private float CalculateBoostedScaledValue(float startValue, float endValue, float t, string scaleType, 
		int currentLevel, int minBoost, int maxBoost)
	{
		float baseValue = CalculateScaledValue(startValue, endValue, t, scaleType);
		if (minBoost <= 0 || maxBoost <= 0 || currentLevel > maxBoost)
		{
			return baseValue;
		}
		float boostAmount = 0.24f;
		float boostFactor = 1f;
		if (currentLevel <= minBoost)
		{
			boostFactor = 1f + boostAmount;
		}
		else if (currentLevel <= maxBoost)
		{
			float fadeT = (float)(currentLevel - minBoost) / (maxBoost - minBoost);
			fadeT = 1f - Mathf.Pow(1f - fadeT, 1.24f);
			boostFactor = 1f + boostAmount * (1f - fadeT);
		}
		else
		{
			boostFactor = 1f;
		}
		return baseValue * boostFactor;
	}

	private int CalculateBoostedScaledValueInt(int startValue, int endValue, float t, string scaleType,
		int currentLevel, int minBoost, int maxBoost)
	{
		return Mathf.RoundToInt(CalculateBoostedScaledValue((float)startValue, (float)endValue, t, scaleType,
			currentLevel, minBoost, maxBoost));
	}

	private CMobInfoLevel GenerateLevelData(CMobInfo mobInfo, int targetLevel)
	{
		CMobInfoLevel baseLevel = mobInfo.Get(1);
		if (baseLevel == null)
		{
			return null;
		}
		int maxLevel = mobInfo.GetMaxLevel();
		if (maxLevel <= 0 || targetLevel > maxLevel)
		{
			return null;
		}
		CMobInfoLevel newLevel = new CMobInfoLevel();
		newLevel.nLevel = targetLevel;
		newLevel.nRareType = baseLevel.nRareType;
		newLevel.nType = baseLevel.nType;
		newLevel.nModel = baseLevel.nModel;
		newLevel.sName = baseLevel.sName;
		newLevel.sDesc = baseLevel.sDesc;
		newLevel.sIcon = baseLevel.sIcon;
		newLevel.fMeleeRange = baseLevel.fMeleeRange;
		newLevel.ltSkill = baseLevel.ltSkill;
		newLevel.ltSkillPassive = baseLevel.ltSkillPassive;
		newLevel.nAIManagerID = baseLevel.nAIManagerID;
		newLevel.fMoveSpeedRate = baseLevel.fMoveSpeedRate;
		newLevel.fRushSpeedRate = baseLevel.fRushSpeedRate;
		newLevel.isWaitRot = baseLevel.isWaitRot;
		newLevel.nGoldRate = baseLevel.nGoldRate;
		newLevel.nDropGroup = baseLevel.nDropGroup;
		newLevel.arrDropCount = baseLevel.arrDropCount;
		newLevel.arrDropCountRate = baseLevel.arrDropCountRate;
		newLevel.ltHardinessInfo = baseLevel.ltHardinessInfo;
		newLevel.bIgnoreKnock = baseLevel.bIgnoreKnock;
		newLevel.nMinBoost = mobInfo.GetMinBoost();
		newLevel.nMaxBoost = mobInfo.GetMaxBoost();
		string scaleType = mobInfo.GetScaleType();
		float t = (float)(targetLevel - 1) / (maxLevel - 1);
		if (baseLevel.fLifeMax > 0)
		{
			newLevel.fLife = CalculateBoostedScaledValue(
				baseLevel.fLife,
				baseLevel.fLifeMax,
				t,
				scaleType,
				targetLevel,
				newLevel.nMinBoost,
				newLevel.nMaxBoost
			);
		}
		else
		{
			newLevel.fLife = baseLevel.fLife;
		}
		if (baseLevel.fMoveSpeedMax > 0)
		{
			newLevel.fMoveSpeed = CalculateScaledValue(baseLevel.fMoveSpeed, baseLevel.fMoveSpeedMax, t, scaleType);
		}
		else
		{
			newLevel.fMoveSpeed = baseLevel.fMoveSpeed;
		}
		if (baseLevel.fDamageMax > 0)
		{
			newLevel.fDamage = CalculateBoostedScaledValue(
				baseLevel.fDamage,
				baseLevel.fDamageMax,
				t,
				scaleType,
				targetLevel,
				newLevel.nMinBoost,
				newLevel.nMaxBoost
			);
		}
		else
		{
			newLevel.fDamage = baseLevel.fDamage;
		}
		if (baseLevel.fHardinessMax > 0)
		{
			newLevel.fHardiness = CalculateScaledValue(baseLevel.fHardiness, baseLevel.fHardinessMax, t, scaleType);
		}
		else
		{
			newLevel.fHardiness = baseLevel.fHardiness;
		}
		if (baseLevel.nGoldMax > 0)
		{
			newLevel.nGold = CalculateScaledValueInt(baseLevel.nGold, baseLevel.nGoldMax, t, scaleType);
		}
		else
		{
			newLevel.nGold = baseLevel.nGold;
		}
		if (baseLevel.nExpMax > 0)
		{
			newLevel.nExp = CalculateScaledValueInt(baseLevel.nExp, baseLevel.nExpMax, t, scaleType);
		}
		else
		{
			newLevel.nExp = baseLevel.nExp;
		}
		return newLevel;
	}

	protected override void LoadData(string content)
	{
		m_dictMobInfo.Clear();
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(content);
		string value = string.Empty;
		m_nReadIndex = 0;
		XmlNode documentElement = xmlDocument.DocumentElement;
		foreach (XmlNode childNode in documentElement.ChildNodes)
		{
			m_nReadIndex++;
			if (childNode.Name != "mob" || !MyUtils.GetAttribute(childNode, "id", ref value))
			{
				continue;
			}
			int num = int.Parse(value);
			string levelAttr = string.Empty;
			MyUtils.GetAttribute(childNode, "lvl", ref levelAttr);
			int nLevel = string.IsNullOrEmpty(levelAttr) ? 1 : int.Parse(levelAttr);
			CMobInfo mobInfo = Get(num);
			if (mobInfo == null)
			{
				mobInfo = new CMobInfo();
				mobInfo.nID = num;
				m_dictMobInfo.Add(num, mobInfo);
			}
			CMobInfoLevel mobInfoLevel = mobInfo.Get(nLevel);
			if (mobInfoLevel == null)
			{
				mobInfoLevel = new CMobInfoLevel();
				mobInfoLevel.nLevel = nLevel;
				mobInfo.Add(nLevel, mobInfoLevel);
			}
			if (nLevel == 1)
			{
				if (MyUtils.GetAttribute(childNode, "maxlvl", ref value))
				{
					mobInfo.SetMaxLevel(int.Parse(value));
				}
				if (MyUtils.GetAttribute(childNode, "scaletype", ref value))
				{
					mobInfo.SetScaleType(value);
				}
			}
			if (MyUtils.GetAttribute(childNode, "minboost", ref value))
			{
				mobInfo.SetMinBoost(int.Parse(value));
				mobInfoLevel.nMinBoost = int.Parse(value);
			}
			if (MyUtils.GetAttribute(childNode, "maxboost", ref value))
			{
				mobInfo.SetMaxBoost(int.Parse(value));
				mobInfoLevel.nMaxBoost = int.Parse(value);
			}
			if (MyUtils.GetAttribute(childNode, "rare", ref value))
				mobInfoLevel.nRareType = int.Parse(value);
			if (MyUtils.GetAttribute(childNode, "type", ref value))
				mobInfoLevel.nType = int.Parse(value);
			if (MyUtils.GetAttribute(childNode, "model", ref value))
				mobInfoLevel.nModel = int.Parse(value);
			if (MyUtils.GetAttribute(childNode, "name", ref value))
				mobInfoLevel.sName = value;
			if (MyUtils.GetAttribute(childNode, "desc", ref value))
				mobInfoLevel.sDesc = value;
			if (MyUtils.GetAttribute(childNode, "icon", ref value))
				mobInfoLevel.sIcon = value;
			if (MyUtils.GetAttribute(childNode, "life", ref value))
				mobInfoLevel.fLife = MyUtils.ParseFloat(value);
			if (MyUtils.GetAttribute(childNode, "lifemax", ref value))
				mobInfoLevel.fLifeMax = MyUtils.ParseFloat(value);
			if (MyUtils.GetAttribute(childNode, "movespeed", ref value))
				mobInfoLevel.fMoveSpeed = MyUtils.ParseFloat(value);
			if (MyUtils.GetAttribute(childNode, "movespeedmax", ref value))
				mobInfoLevel.fMoveSpeedMax = MyUtils.ParseFloat(value);
			if (MyUtils.GetAttribute(childNode, "meleerange", ref value))
				mobInfoLevel.fMeleeRange = MyUtils.ParseFloat(value);
			if (MyUtils.GetAttribute(childNode, "damage", ref value))
				mobInfoLevel.fDamage = MyUtils.ParseFloat(value);
			if (MyUtils.GetAttribute(childNode, "damagemax", ref value))
				mobInfoLevel.fDamageMax = MyUtils.ParseFloat(value);
			if (MyUtils.GetAttribute(childNode, "skill", ref value))
			{
				mobInfoLevel.ltSkill.Clear();
				string[] array = value.Split(',');
				for (int i = 0; i < array.Length; i++)
				{
					SkillComboRateInfo item = new SkillComboRateInfo(int.Parse(array[i]), 100f);
					mobInfoLevel.ltSkill.Add(item);
				}
			}
			if (mobInfoLevel.ltSkill != null && MyUtils.GetAttribute(childNode, "skillrate", ref value))
			{
				string[] array = value.Split(',');
				for (int j = 0; j < array.Length && j < mobInfoLevel.ltSkill.Count; j++)
				{
					mobInfoLevel.ltSkill[j].m_fRate = MyUtils.ParseFloat(array[j]);
				}
			}
			if (MyUtils.GetAttribute(childNode, "skillpassive", ref value))
			{
				mobInfoLevel.ltSkillPassive.Clear();
				string[] array = value.Split(',');
				for (int k = 0; k < array.Length; k++)
				{
					mobInfoLevel.ltSkillPassive.Add(int.Parse(array[k]));
				}
			}
			if (MyUtils.GetAttribute(childNode, "hardiness", ref value))
				mobInfoLevel.fHardiness = MyUtils.ParseFloat(value);
			if (MyUtils.GetAttribute(childNode, "hardinessmax", ref value))
				mobInfoLevel.fHardinessMax = MyUtils.ParseFloat(value);
			if (MyUtils.GetAttribute(childNode, "ignoreknock", ref value))
				mobInfoLevel.bIgnoreKnock = bool.Parse(value);
			if (MyUtils.GetAttribute(childNode, "aimanager", ref value))
				mobInfoLevel.nAIManagerID = int.Parse(value);
			if (MyUtils.GetAttribute(childNode, "movespeedrate", ref value))
				mobInfoLevel.fMoveSpeedRate = MyUtils.ParseFloat(value);
			if (MyUtils.GetAttribute(childNode, "rushspeedrate", ref value))
				mobInfoLevel.fRushSpeedRate = MyUtils.ParseFloat(value);
			if (MyUtils.GetAttribute(childNode, "iswaitrot", ref value))
				mobInfoLevel.isWaitRot = bool.Parse(value);
			if (MyUtils.GetAttribute(childNode, "goldrate", ref value))
				mobInfoLevel.nGoldRate = int.Parse(value);
			if (MyUtils.GetAttribute(childNode, "gold", ref value))
				mobInfoLevel.nGold = int.Parse(value);
			if (MyUtils.GetAttribute(childNode, "goldmax", ref value))
				mobInfoLevel.nGoldMax = int.Parse(value);
			if (MyUtils.GetAttribute(childNode, "exp", ref value))
				mobInfoLevel.nExp = int.Parse(value);
			if (MyUtils.GetAttribute(childNode, "expmax", ref value))
				mobInfoLevel.nExpMax = int.Parse(value);
			if (MyUtils.GetAttribute(childNode, "dropgroup", ref value))
				mobInfoLevel.nDropGroup = int.Parse(value);
			if (MyUtils.GetAttribute(childNode, "dropcount", ref value))
			{
				string[] array = value.Split(',');
				for (int l = 0; l < array.Length && l < mobInfoLevel.arrDropCount.Length; l++)
				{
					mobInfoLevel.arrDropCount[l] = int.Parse(array[l]);
				}
			}
			if (MyUtils.GetAttribute(childNode, "dropcountrate", ref value))
			{
				string[] array = value.Split(',');
				for (int m = 0; m < array.Length && m < mobInfoLevel.arrDropCountRate.Length; m++)
				{
					mobInfoLevel.arrDropCountRate[m] = int.Parse(array[m]);
				}
			}
			mobInfoLevel.ltHardinessInfo.Clear();
			for (int n = 1; n <= 5; n++)
			{
				if (MyUtils.GetAttribute(childNode, "bodypart" + n, ref value))
				{
					string[] array = value.Split(',');
					if (array.Length == 4)
					{
						CHardinessInfo cHardinessInfo = new CHardinessInfo();
						cHardinessInfo.nPartID = int.Parse(array[0]);
						cHardinessInfo.fHardiness = MyUtils.ParseFloat(array[1]);
						cHardinessInfo.nAnimEnum = int.Parse(array[2]);
						cHardinessInfo.fDmgRate = MyUtils.ParseFloat(array[3]);
						mobInfoLevel.ltHardinessInfo.Add(cHardinessInfo);
					}
				}
			}
		}
	}
}