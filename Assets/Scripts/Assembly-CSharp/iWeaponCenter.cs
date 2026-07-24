using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class iWeaponCenter : iBaseCenter
{
	protected Dictionary<int, CWeaponInfo> m_dictWeaponInfo;

	public iWeaponCenter()
	{
		m_dictWeaponInfo = new Dictionary<int, CWeaponInfo>();
	}

	public Dictionary<int, CWeaponInfo> GetData()
	{
		return m_dictWeaponInfo;
	}

	public CWeaponInfo Get(int nID)
	{
		if (!m_dictWeaponInfo.ContainsKey(nID))
			return null;
		return m_dictWeaponInfo[nID];
	}

	public CWeaponInfoLevel Get(int nID, int nLevel)
	{
		CWeaponInfo cWeaponInfo = Get(nID);
		if (cWeaponInfo == null)
			return null;
		return cWeaponInfo.Get(nLevel);
	}

	public int GetLvlCount(int nID)
	{
		CWeaponInfo cWeaponInfo = Get(nID);
		if (cWeaponInfo == null)
			return 0;
		return cWeaponInfo.GetLvlCount();
	}

	public string GetLevelUpDesc(int weaponID, int level)
	{
		CWeaponInfo info = Get(weaponID);
		if (info == null) return string.Empty;
		CWeaponInfoLevel cur = info.Get(level);
		CWeaponInfoLevel prev = null;
		int bestPrev = int.MinValue;
		foreach (var kvp in info.m_dictWeaponLvlInfo)
		{
			if (kvp.Key < level && kvp.Key > bestPrev)
			{
				bestPrev = kvp.Key;
				prev = kvp.Value;
			}
		}
		return WeaponDescriptionBuilder.BuildLevelUpDesc(cur, prev);
	}

	public static class WeaponDescriptionBuilder
	{
		private static readonly string[] WeaponTypeDescriptions = new string[]
		{
			"{color:FF0000FF}Crossbows deal high dps to singular targets. An excellent boss killer.{color}",
			"{color:00ABFFFF}Melee weapons are incredible crowd control weapons and boss killers, but they're very heavy and hard to use.{color}",
			"{color:870096FF}Rifles are great because they increase your speed, although they lack damage and cannot crit.{color}",
			"{color:009600FF}Shotguns are overall good crowd control weapons, though they deal noticeably less DPS than weapons like Crossbows.{color}",
			"{color:000096FF}Flamethrowers deal crowd control damage and set enemies on fire, dealing extra damage over time. Good for weakening enemies.{color}",
			"{color:D1FF00FF}Cannons are amazing for crowd control, but their weight makes them hard to use and they cannot crit while lacking DPS.{color}"
		};

		private static float RoundDelta(float value) => Mathf.Round(value * 100f) / 100f;

		private static string GetDeltaColor(float delta, bool isFireRate = false)
		{
			if (delta == 0) return string.Empty;
			if (isFireRate)
				return delta < 0 ? "1EFF0000" : "FF0000FF";
			else
				return delta > 0 ? "1EFF0000" : "FF0000FF";
		}

		public static string BuildLevelUpDesc(CWeaponInfoLevel curLevel, CWeaponInfoLevel prevLevel)
		{
			if (curLevel == null) return string.Empty;

			bool isInitial = prevLevel == null;
			List<string> statStrings = new List<string>();
			if (curLevel.fDamage > 0)
			{
				float prevVal = prevLevel?.fDamage ?? 0;
				float delta = RoundDelta(curLevel.fDamage - prevVal);
				if (isInitial)
					statStrings.Add($"DMG: {{color:1EFF0000}}{curLevel.fDamage}{{color}}");
				else if (delta != 0)
				{
					string color = GetDeltaColor(delta);
					string sign = delta > 0 ? "+" : "";
					statStrings.Add($"DMG: {curLevel.fDamage} {{color:{color}}}({sign}{delta}){{color}}");
				}
			}
			if (curLevel.fCritical > 0)
			{
				float prevVal = prevLevel?.fCritical ?? 0;
				float delta = RoundDelta(curLevel.fCritical - prevVal);
				if (isInitial)
					statStrings.Add($"Crit Chance: {{color:1EFF0000}}{curLevel.fCritical}%{{color}}");
				else if (delta != 0)
				{
					string color = GetDeltaColor(delta);
					string sign = delta > 0 ? "+" : "";
					statStrings.Add($"Crit Chance: {curLevel.fCritical}% {{color:{color}}}({sign}{delta}%){{color}}");
				}
			}
			if (curLevel.fCriticalDmg > 0)
			{
				float prevVal = prevLevel?.fCriticalDmg ?? 0;
				float delta = RoundDelta(curLevel.fCriticalDmg - prevVal);
				if (isInitial)
					statStrings.Add($"Crit DMG: {{color:1EFF0000}}{curLevel.fCriticalDmg}%{{color}}");
				else if (delta != 0)
				{
					string color = GetDeltaColor(delta);
					string sign = delta > 0 ? "+" : "";
					statStrings.Add($"Crit DMG: {curLevel.fCriticalDmg}% {{color:{color}}}({sign}{delta}%){{color}}");
				}
			}
			if (curLevel.fShootSpeed > 0)
			{
				float prevVal = prevLevel?.fShootSpeed ?? 0;
				float delta = RoundDelta(curLevel.fShootSpeed - prevVal);
				if (isInitial)
					statStrings.Add($"Fire Rate: {{color:1EFF0000}}{curLevel.fShootSpeed}{{color}}");
				else if (delta != 0)
				{
					string color = GetDeltaColor(delta, true);
					string sign = delta > 0 ? "+" : "";
					statStrings.Add($"Fire Rate: {curLevel.fShootSpeed} {{color:{color}}}({sign}{delta}){{color}}");
				}
			}
			if (curLevel.nCapacity > 0)
			{
				int prevVal = prevLevel?.nCapacity ?? 0;
				int delta = curLevel.nCapacity - prevVal;
				if (isInitial)
					statStrings.Add($"Ammo: {{color:1EFF0000}}{curLevel.nCapacity}{{color}}");
				else if (delta != 0)
				{
					string color = GetDeltaColor(delta);
					string sign = delta > 0 ? "+" : "";
					statStrings.Add($"Ammo: {curLevel.nCapacity} {{color:{color}}}({sign}{delta}){{color}}");
				}
			}
			List<string> lines = new List<string>();
			string typeDesc = GetWeaponTypeDescription(curLevel.nType);
			if (!string.IsNullOrEmpty(typeDesc))
				lines.Add(typeDesc);
			lines.Add(isInitial ? "Initial Stats:" : "Next Upgrade:");
			if (statStrings.Count > 0)
				lines.Add(string.Join(", ", statStrings));
			else
				lines.Add("No stat changes at this level");
			return string.Join("\n", lines);
		}

		private static string GetWeaponTypeDescription(int type)
		{
			if (type >= 0 && type < WeaponTypeDescriptions.Length)
				return WeaponTypeDescriptions[type];
			return string.Empty;
		}
	}

	protected override void LoadData(string content)
	{
		m_dictWeaponInfo.Clear();
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(content);
		string value = string.Empty;
		XmlNode documentElement = xmlDocument.DocumentElement;
		foreach (XmlNode childNode in documentElement.ChildNodes)
		{
			if (childNode.Name != "weapon" || !MyUtils.GetAttribute(childNode, "id", ref value))
			{
				continue;
			}
			int num = int.Parse(value);
			if (!MyUtils.GetAttribute(childNode, "lvl", ref value))
			{
				continue;
			}
			int nLevel = int.Parse(value);
			CWeaponInfo cWeaponInfo = Get(num);
			if (cWeaponInfo == null)
			{
				cWeaponInfo = new CWeaponInfo();
				cWeaponInfo.nID = num;
				m_dictWeaponInfo.Add(num, cWeaponInfo);
			}
			CWeaponInfoLevel cWeaponInfoLevel = cWeaponInfo.Get(nLevel);
			if (cWeaponInfoLevel == null)
			{
				cWeaponInfoLevel = new CWeaponInfoLevel();
				cWeaponInfoLevel.nLevel = nLevel;
				cWeaponInfo.Add(nLevel, cWeaponInfoLevel);
			}
			if (MyUtils.GetAttribute(childNode, "unlockstage", ref value))
			{
				cWeaponInfo.m_nUnlockStageID = int.Parse(value);
			}
			if (MyUtils.GetAttribute(childNode, "unlockhunterlvl", ref value))
			{
				cWeaponInfo.m_nUnlockHunterLvl = int.Parse(value);
			}
			if (MyUtils.GetAttribute(childNode, "type", ref value))
			{
				cWeaponInfoLevel.nType = int.Parse(value);
			}
			if (MyUtils.GetAttribute(childNode, "elementtype", ref value))
			{
				cWeaponInfoLevel.nElementType = int.Parse(value);
			}
			if (MyUtils.GetAttribute(childNode, "attackmode", ref value))
			{
				cWeaponInfoLevel.nAttackMode = int.Parse(value);
			}
			if (MyUtils.GetAttribute(childNode, "attackmodevalue", ref value))
			{
				cWeaponInfoLevel.ltAttackModeValue.Clear();
				string[] array = value.Split(',');
				for (int i = 0; i < array.Length; i++)
				{
					cWeaponInfoLevel.ltAttackModeValue.Add(MyUtils.ParseFloat(array[i]));
				}
			}
			if (MyUtils.GetAttribute(childNode, "actiontype", ref value))
			{
				cWeaponInfoLevel.nActionType = int.Parse(value);
			}
			if (MyUtils.GetAttribute(childNode, "model", ref value))
			{
				cWeaponInfoLevel.nModel = int.Parse(value);
			}
			if (MyUtils.GetAttribute(childNode, "audiofire", ref value))
			{
				cWeaponInfoLevel.sAudioFire = value;
			}
			if (MyUtils.GetAttribute(childNode, "eff_bullet", ref value))
			{
				cWeaponInfoLevel.nBullet = int.Parse(value);
			}
			if (MyUtils.GetAttribute(childNode, "eff_fire", ref value))
			{
				cWeaponInfoLevel.nFire = int.Parse(value);
			}
			if (MyUtils.GetAttribute(childNode, "eff_hit", ref value))
			{
				cWeaponInfoLevel.nHit = int.Parse(value);
			}
			if (MyUtils.GetAttribute(childNode, "name", ref value))
			{
				cWeaponInfoLevel.sName = value;
			}
			else
			{
				cWeaponInfoLevel.sName = "WeaponID " + cWeaponInfo.nID;
			}
			if (MyUtils.GetAttribute(childNode, "desc", ref value))
			{
				cWeaponInfoLevel.sDesc = value;
			}
			else
			{
				cWeaponInfoLevel.sDesc = "Weapon Description";
			}
			if (MyUtils.GetAttribute(childNode, "icon", ref value))
			{
				cWeaponInfoLevel.sIcon = value;
				cWeaponInfoLevel.sAudioFire = value;
			}
			if (MyUtils.GetAttribute(childNode, "shootaudio", ref value))
			{
				cWeaponInfoLevel.sAudioFire = value;
			}
			if (MyUtils.GetAttribute(childNode, "damage", ref value))
			{
				cWeaponInfoLevel.fDamage = MyUtils.ParseFloat(value);
			}
			if (MyUtils.GetAttribute(childNode, "critical", ref value))
			{
				cWeaponInfoLevel.fCritical = MyUtils.ParseFloat(value);
			}
			if (MyUtils.GetAttribute(childNode, "criticaldmg", ref value))
			{
				cWeaponInfoLevel.fCriticalDmg = MyUtils.ParseFloat(value);
			}
			if (MyUtils.GetAttribute(childNode, "shootspeed", ref value))
			{
				cWeaponInfoLevel.fShootSpeed = MyUtils.ParseFloat(value);
			}
			if (MyUtils.GetAttribute(childNode, "msdownshoot", ref value))
			{
				cWeaponInfoLevel.fMSDownRateShoot = MyUtils.ParseFloat(value);
			}
			if (MyUtils.GetAttribute(childNode, "msdownequip", ref value))
			{
				cWeaponInfoLevel.fMSDownRateEquip = MyUtils.ParseFloat(value);
			}
			if (MyUtils.GetAttribute(childNode, "precise", ref value))
			{
				cWeaponInfoLevel.fPrecise = MyUtils.ParseFloat(value);
			}
			if (MyUtils.GetAttribute(childNode, "capacity", ref value))
			{
				cWeaponInfoLevel.nCapacity = int.Parse(value);
			}
			if (MyUtils.GetAttribute(childNode, "func", ref value))
			{
				string[] array = value.Split(',');
				for (int j = 0; j < array.Length; j++)
				{
					cWeaponInfoLevel.arrFunc[j] = int.Parse(array[j]);
				}
			}
			if (MyUtils.GetAttribute(childNode, "valuex", ref value))
			{
				string[] array = value.Split(',');
				for (int k = 0; k < array.Length; k++)
				{
					cWeaponInfoLevel.arrValueX[k] = int.Parse(array[k]);
				}
			}
			if (MyUtils.GetAttribute(childNode, "valuey", ref value))
			{
				string[] array = value.Split(',');
				for (int l = 0; l < array.Length; l++)
				{
					cWeaponInfoLevel.arrValueY[l] = int.Parse(array[l]);
				}
			}
			if (MyUtils.GetAttribute(childNode, "elementup", ref value))
			{
				cWeaponInfoLevel.fElementUp = MyUtils.ParseFloat(value);
			}
			if (MyUtils.GetAttribute(childNode, "elementupmonster", ref value))
			{
				cWeaponInfoLevel.ltElementUpMonster.Clear();
				string[] array = value.Split(',');
				for (int m = 0; m < array.Length; m++)
				{
					cWeaponInfoLevel.ltElementUpMonster.Add(int.Parse(array[m]));
				}
			}
			if (MyUtils.GetAttribute(childNode, "elementdown", ref value))
			{
				cWeaponInfoLevel.fElementDown = MyUtils.ParseFloat(value);
			}
			if (MyUtils.GetAttribute(childNode, "elementdownmonster", ref value))
			{
				cWeaponInfoLevel.ltElementDownMonster.Clear();
				string[] array = value.Split(',');
				for (int n = 0; n < array.Length; n++)
				{
					cWeaponInfoLevel.ltElementDownMonster.Add(int.Parse(array[n]));
				}
			}
			if (MyUtils.GetAttribute(childNode, "materials", ref value))
			{
				cWeaponInfoLevel.ltMaterials.Clear();
				string[] array = value.Split(',');
				for (int num2 = 0; num2 < array.Length; num2++)
				{
					cWeaponInfoLevel.ltMaterials.Add(int.Parse(array[num2]));
				}
			}
			if (MyUtils.GetAttribute(childNode, "materialscount", ref value))
			{
				cWeaponInfoLevel.ltMaterialsCount.Clear();
				string[] array = value.Split(',');
				for (int num3 = 0; num3 < array.Length; num3++)
				{
					cWeaponInfoLevel.ltMaterialsCount.Add(int.Parse(array[num3]));
				}
			}
			if (MyUtils.GetAttribute(childNode, "iscrystalpurchase", ref value))
			{
				cWeaponInfoLevel.isCrystalPurchase = bool.Parse(value);
			}
			if (MyUtils.GetAttribute(childNode, "purchaseprice", ref value))
			{
				cWeaponInfoLevel.nPurchasePrice = int.Parse(value);
			}
			CWeaponInfoLevel prevLevel = null;
			int bestPrev = int.MinValue;
			foreach (var kvp in cWeaponInfo.m_dictWeaponLvlInfo)
			{
				if (kvp.Key < nLevel && kvp.Key > bestPrev)
				{
					bestPrev = kvp.Key;
					prevLevel = kvp.Value;
				}
			}
			cWeaponInfoLevel.sLevelUpDesc = WeaponDescriptionBuilder.BuildLevelUpDesc(cWeaponInfoLevel, prevLevel);
			if (string.IsNullOrEmpty(cWeaponInfoLevel.sLevelUpDesc))
			{
				cWeaponInfoLevel.sLevelUpDesc = "DMG: " + cWeaponInfoLevel.fDamage;
			}
		}
	}
}