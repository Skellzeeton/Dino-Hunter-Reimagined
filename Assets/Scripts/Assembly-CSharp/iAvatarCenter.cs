using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class iAvatarCenter : iBaseCenter
{
	protected Dictionary<int, CAvatarInfo> m_dictAvatar;

	public iAvatarCenter()
	{
		m_dictAvatar = new Dictionary<int, CAvatarInfo>();
	}

	public CAvatarInfo Get(int nID)
	{
		if (!m_dictAvatar.ContainsKey(nID))
		{
			return null;
		}
		return m_dictAvatar[nID];
	}

	public CAvatarInfoLevel Get(int nID, int nLevel)
	{
		CAvatarInfo cAvatarInfo = Get(nID);
		if (cAvatarInfo == null)
		{
			return null;
		}
		return cAvatarInfo.Get(nLevel);
	}

	public Dictionary<int, CAvatarInfo> GetData()
	{
		return m_dictAvatar;
	}

	public CAvatarInfo GetRandomByType(int nType)
	{
		List<CAvatarInfo> list = new List<CAvatarInfo>();
		foreach (CAvatarInfo value in m_dictAvatar.Values)
		{
			if (value.m_nType == nType)
			{
				list.Add(value);
			}
		}
		if (list.Count < 1)
		{
			return null;
		}
		return list[Random.Range(0, list.Count)];
	}

	public static class AvatarDescriptionBuilder
	{
		public static string BuildDesc(CAvatarInfoLevel curLevel)
		{
			if (curLevel == null) return string.Empty;

			List<string> descLines = new();
			List<string> hpParts = new();

			for (int j = 0; j < curLevel.arrValueX.Length; j++)
			{
				int statEnum = curLevel.arrValueX[j];
				float statValue = curLevel.arrValueY[j];
				if (statEnum == 0) continue;
				if ((kProEnum)statEnum == kProEnum.Protect) continue;
				bool isPercent = IsPercentStat((kProEnum)statEnum);
				string sign = statValue >= 0 ? "Increases By " : "Decreases By ";
				string colorCode = statValue >= 0 ? "1EFF0000" : "FF0000FF";
				string coloredValue = string.Format("{{color:{0}}}{1}{2}{{color}}", colorCode, statValue, isPercent ? "%" : "");
				if ((kProEnum)statEnum == kProEnum.Char_RecoverLife)
				{
					descLines.Add("Slowly regenerates " + coloredValue + " of your Max HP every 20 seconds.");
				}
				else if ((kProEnum)statEnum == kProEnum.Char_RecoverBullet)
				{
					descLines.Add("Slowly regenerates " + coloredValue + " of your Max Ammo every 20 seconds.");
				}
				else if ((kProEnum)statEnum == kProEnum.AntiStun)
				{
					descLines.Add("You have a " + coloredValue + " chance to avoid stuns.");
				}
				else if ((kProEnum)statEnum == kProEnum.HPMax)
				{
					hpParts.Add(coloredValue);
				}
				else if ((kProEnum)statEnum == kProEnum.HPMaxUp)
				{
					hpParts.Add(coloredValue);
				}
				else
				{
					descLines.Add(GetStatName((kProEnum)statEnum) + " " + sign + coloredValue);
				}
			}
			if (hpParts.Count > 0)
			{
				string hpLine = "HP " + (hpParts.Count == 2 ? "increases by " + hpParts[0] + " & " + hpParts[1] : "increases by " + hpParts[0]);
				descLines.Insert(0, hpLine);
			}

			return descLines.Count > 0 ? string.Join("\n", descLines) : "nothing...";
		}

		public static string BuildLevelUpDesc(CAvatarInfoLevel curLevel, CAvatarInfoLevel prevLevel)
		{
			if (curLevel == null) return string.Empty;
			List<string> upLines = new()
			{
				prevLevel != null ? "Next Upgrade:" : "Initial Stats:"
			};
			string hpStat = null;
			string hpUpStat = null;
			for (int j = 0; j < curLevel.arrValueX.Length; j++)
			{
				if (curLevel.arrValueX[j] == 0) continue;

				int statEnum = curLevel.arrValueX[j];
				if (statEnum == 0) continue;

				float prevVal = 0f;
				if (prevLevel != null)
				{
					for (int i = 0; i < prevLevel.arrValueX.Length; i++)
					{
						if (prevLevel.arrValueX[i] == statEnum)
						{
							prevVal = prevLevel.arrValueY[i];
							break;
						}
					}
				}
				float curVal = curLevel.arrValueY[j];
				if (curVal <= 0 && prevLevel != null) continue;
				float delta = curVal - prevVal;
				bool isPercent = IsPercentStat((kProEnum)statEnum);
				string percentStr = isPercent ? "%" : "";
				string colorCode = curVal >= 0 ? "1EFF0000" : "FF0000FF";
				string coloredCurVal = string.Format("{{color:{0}}}{1}{2}{{color}}", colorCode, curVal, percentStr);
				if ((kProEnum)statEnum == kProEnum.HPMax)
				{
					if (prevLevel != null && prevVal > 0 && delta != 0)
					{
						hpStat = coloredCurVal + string.Format(" {{color:1eff0000}}(+{0}){{color}}", delta);
					}
					else if (prevLevel != null && prevVal > 0 && delta == 0)
					{
						hpStat = null;
					}
					else
					{
						hpStat = coloredCurVal;
					}
				}
				else if ((kProEnum)statEnum == kProEnum.HPMaxUp)
				{
					if (prevLevel != null && prevVal > 0 && delta != 0)
					{
						hpUpStat = coloredCurVal + string.Format(" {{color:1eff0000}}(+{0}%){{color}}", delta);
					}
					else if (prevLevel != null && prevVal > 0 && delta == 0)
					{
						hpUpStat = null;
					}
					else
					{
						hpUpStat = coloredCurVal;
					}
				}
				else
				{
					string statName = GetStatName((kProEnum)statEnum);
					string line;
					if (prevLevel != null && prevVal > 0)
					{
						if (delta != 0)
						{
							line = statName + " Increases By " + prevVal + percentStr + string.Format(" {{color:1eff0000}}(+{0}{1}){{color}}", delta, percentStr);
						}
						else
						{
							continue;
						}
					}
					else
					{
						line = statName + " Increases By " + coloredCurVal;
					}
					upLines.Add(line);
				}
			}
			if (hpStat != null || hpUpStat != null)
			{
				string hpLine = "HP ";
				if (hpStat != null && hpUpStat != null)
				{
					hpLine += "increases by " + hpStat + " & " + hpUpStat;
				}
				else if (hpStat != null)
				{
					hpLine += "increases by " + hpStat;
				}
				else if (hpUpStat != null)
				{
					hpLine += "increases by " + hpUpStat;
				}
				if (upLines.Count > 1)
					upLines.Insert(1, hpLine);
				else
					upLines.Add(hpLine);
			}
			return upLines.Count > 1 ? string.Join("\n", upLines) : "";
		}

		private static bool IsPercentStat(kProEnum stat)
		{
			switch (stat)
			{
				case kProEnum.HPMaxUp:
				case kProEnum.All_Dmg:
				case kProEnum.All_Dmg_Rate:
				case kProEnum.MoveSpeed:
				case kProEnum.Critical:
				case kProEnum.CriticalDmg:
				case kProEnum.All_Critical:
				case kProEnum.All_CriticalDmg:
				case kProEnum.All_Speed:
				case kProEnum.Melee_Speed:
				case kProEnum.Melee_Critical:
				case kProEnum.Melee_CriticalDmg:
				case kProEnum.Range_Speed:
				case kProEnum.Range_Critical:
				case kProEnum.Range_CriticalDmg:
				case kProEnum.Crossbow_Speed:
				case kProEnum.Crossbow_Critical:
				case kProEnum.Crossbow_CriticalDmg:
				case kProEnum.AutoRifle_Speed:
				case kProEnum.AutoRifle_Critical:
				case kProEnum.AutoRifle_CriticalDmg:
				case kProEnum.ShotGun_Speed:
				case kProEnum.ShotGun_Critical:
				case kProEnum.ShotGun_CriticalDmg:
				case kProEnum.HoldGun_Speed:
				case kProEnum.HoldGun_Critical:
				case kProEnum.HoldGun_CriticalDmg:
				case kProEnum.Rocket_Speed:
				case kProEnum.Rocket_Critical:
				case kProEnum.Rocket_CriticalDmg:
				case kProEnum.AntiStun:
				case kProEnum.Char_IncreaseExp:
				case kProEnum.Char_IncreaseGold:
				case kProEnum.Char_RecoverLife:
				case kProEnum.Char_RecoverBullet:
				case kProEnum.Skill_CD_Faster_Rate:
					return true;
				default:
					return false;
			}
		}

		private static string GetStatName(kProEnum stat)
		{
			switch (stat)
			{
				case kProEnum.HPMax: return "HP";
				case kProEnum.HPMaxUp: return "Max HP";
				case kProEnum.MoveSpeed: return "Movement Speed";
				case kProEnum.Protect: return "Defense";
				case kProEnum.All_Dmg: return "DMG";
				case kProEnum.All_Dmg_Rate: return "DMG";
				case kProEnum.All_Speed: return "Fire Rate";
				case kProEnum.All_Critical: return "Crit Chance";
				case kProEnum.All_CriticalDmg: return "Crit DMG";
				case kProEnum.All_Protect: return "Defense";
				case kProEnum.All_Capacity: return "Ammo";
				case kProEnum.Char_MoveSpeedUp: return "Movement Speed";
				case kProEnum.Char_RecoverLife: return "HP Regeneration";
				case kProEnum.Char_RecoverBullet: return "Ammo Recovery";
				case kProEnum.Char_IncreaseGold: return "Gold";
				case kProEnum.Char_IncreaseExp: return "Experience";
				case kProEnum.Rocket_AOE_Range: return "Rocket AOE Range";
				case kProEnum.Melee_Dmg: return "Melee DMG";
				case kProEnum.Melee_Dmg_Rate: return "Melee DMG";
				case kProEnum.Melee_Speed: return "Melee Fire Rate";
				case kProEnum.Melee_Critical: return "Melee Crit Chance";
				case kProEnum.Melee_CriticalDmg: return "Melee Crit DMG";
				case kProEnum.Range_Dmg: return "Range DMG";
				case kProEnum.Range_Speed: return "Range Fire Rate";
				case kProEnum.Range_Critical: return "Range Crit Chance";
				case kProEnum.Range_CriticalDmg: return "Range Crit DMG";
				case kProEnum.Crossbow_Dmg: return "Crossbow DMG";
				case kProEnum.Crossbow_Dmg_Rate: return "Crossbow DMG";
				case kProEnum.Crossbow_Speed: return "Crossbow Fire Rate";
				case kProEnum.Crossbow_Critical: return "Crossbow Crit Chance";
				case kProEnum.Crossbow_CriticalDmg: return "Crossbow Crit DMG";
				case kProEnum.AutoRifle_Dmg: return "Rifle DMG";
				case kProEnum.AutoRifle_Dmg_Rate: return "Rifle DMG";
				case kProEnum.AutoRifle_Speed: return "Rifle Fire Rate";
				case kProEnum.AutoRifle_Critical: return "Rifle Crit Chance";
				case kProEnum.AutoRifle_CriticalDmg: return "Rifle Crit DMG";
				case kProEnum.ShotGun_Dmg: return "Shotgun DMG";
				case kProEnum.ShotGun_Dmg_Rate: return "Shotgun DMG";
				case kProEnum.ShotGun_Speed: return "Shotgun Fire Rate";
				case kProEnum.ShotGun_Critical: return "Shotgun Crit Chance";
				case kProEnum.ShotGun_CriticalDmg: return "Shotgun Crit DMG";
				case kProEnum.HoldGun_Dmg: return "Flamethrower DMG";
				case kProEnum.HoldGun_Dmg_Rate: return "Flamethrower DMG";
				case kProEnum.HoldGun_Speed: return "Flamethrower Fire Rate";
				case kProEnum.HoldGun_Critical: return "Flamethrower Crit Chance";
				case kProEnum.HoldGun_CriticalDmg: return "Flamethrower Crit DMG";
				case kProEnum.Rocket_Dmg: return "RPG DMG";
				case kProEnum.Rocket_Dmg_Rate: return "RPG DMG";
				case kProEnum.Rocket_Speed: return "RPG Fire Rate";
				case kProEnum.Rocket_Critical: return "RPG Crit Chance";
				case kProEnum.Rocket_CriticalDmg: return "RPG Crit DMG";
				case kProEnum.AntiStun: return "Stun Immunity";
				default:
					return stat.ToString().Replace('_', ' ');
			}
		}
	}

	public string GetDesc(int avatarID, int level)
	{
		CAvatarInfoLevel lvl = Get(avatarID, level);
		return AvatarDescriptionBuilder.BuildDesc(lvl);
	}

	public string GetLevelUpDesc(int avatarID, int level)
	{
		CAvatarInfo info = Get(avatarID);
		if (info == null) return string.Empty;
		CAvatarInfoLevel cur = info.Get(level);
		CAvatarInfoLevel prev = null;

		int bestPrev = int.MinValue;
		foreach (var kvp in info.m_dictAvatarInfoLevel)
		{
			if (kvp.Key < level && kvp.Key > bestPrev)
			{
				bestPrev = kvp.Key;
				prev = kvp.Value;
			}
		}

		return AvatarDescriptionBuilder.BuildLevelUpDesc(cur, prev);
	}

	protected override void LoadData(string content)
	{
		m_dictAvatar.Clear();
		XmlDocument xmlDocument = new XmlDocument();
		xmlDocument.LoadXml(content);
		string value = string.Empty;
		XmlNode documentElement = xmlDocument.DocumentElement;
		foreach (XmlNode childNode in documentElement.ChildNodes)
		{
			if (childNode.Name != "avatar" || !MyUtils.GetAttribute(childNode, "id", ref value))
			{
				continue;
			}
			int nID = int.Parse(value);
			int nLevel = 1;
			if (MyUtils.GetAttribute(childNode, "level", ref value))
			{
				nLevel = int.Parse(value);
			}
			CAvatarInfo cAvatarInfo = Get(nID);
			if (cAvatarInfo == null)
			{
				cAvatarInfo = new CAvatarInfo();
				cAvatarInfo.m_nID = nID;
				m_dictAvatar.Add(cAvatarInfo.m_nID, cAvatarInfo);
			}
			CAvatarInfoLevel cAvatarInfoLevel = cAvatarInfo.Get(nLevel);
			if (cAvatarInfoLevel == null)
			{
				cAvatarInfoLevel = new CAvatarInfoLevel();
				cAvatarInfoLevel.m_nLevel = nLevel;
				cAvatarInfo.m_dictAvatarInfoLevel.Add(cAvatarInfoLevel.m_nLevel, cAvatarInfoLevel);
			}
			if (MyUtils.GetAttribute(childNode, "type", ref value))
			{
				cAvatarInfo.m_nType = int.Parse(value);
			}
			if (MyUtils.GetAttribute(childNode, "icon", ref value))
			{
				cAvatarInfo.m_sIcon = value.Trim();
			}
			if (MyUtils.GetAttribute(childNode, "name", ref value))
			{
				cAvatarInfo.m_sName = value.Trim();
			}
			if (MyUtils.GetAttribute(childNode, "model", ref value))
			{
				cAvatarInfo.m_sModel = value;
				cAvatarInfo.m_sTexture = value;
			}
			if (MyUtils.GetAttribute(childNode, "texture", ref value))
			{
				cAvatarInfo.m_sTexture = value;
			}
			if (MyUtils.GetAttribute(childNode, "effect", ref value))
			{
				cAvatarInfo.m_sEffect = value;
			}
			if (MyUtils.GetAttribute(childNode, "islinkchar", ref value))
			{
				cAvatarInfo.m_bLinkChar = bool.Parse(value);
			}
			if (MyUtils.GetAttribute(childNode, "unlockstage", ref value))
			{
				cAvatarInfo.m_nUnlockStageID = int.Parse(value);
			}
			if (MyUtils.GetAttribute(childNode, "unlockhunterlvl", ref value))
			{
				cAvatarInfo.m_nUnlockHunterLvl = int.Parse(value);
			}
			if (MyUtils.GetAttribute(childNode, "func", ref value))
			{
				string[] array = value.Split(',');
				for (int i = 0; i < array.Length && i < cAvatarInfoLevel.arrFunc.Length; i++)
				{
					cAvatarInfoLevel.arrFunc[i] = int.Parse(array[i]);
				}
			}
			if (MyUtils.GetAttribute(childNode, "valuex", ref value))
			{
				string[] array = value.Split(',');
				for (int j = 0; j < array.Length && j < cAvatarInfoLevel.arrValueX.Length; j++)
				{
					cAvatarInfoLevel.arrValueX[j] = int.Parse(array[j]);
				}
			}
			if (MyUtils.GetAttribute(childNode, "valuey", ref value))
			{
				string[] array = value.Split(',');
				for (int k = 0; k < array.Length && k < cAvatarInfoLevel.arrValueY.Length; k++)
				{
					cAvatarInfoLevel.arrValueY[k] = int.Parse(array[k]);
				}
			}
			if (MyUtils.GetAttribute(childNode, "iscrystalpurchase", ref value))
			{
				cAvatarInfoLevel.isCrystalPurchase = bool.Parse(value);
			}
			if (MyUtils.GetAttribute(childNode, "purchaseprice", ref value))
			{
				cAvatarInfoLevel.nPurchasePrice = int.Parse(value);
			}
			if (MyUtils.GetAttribute(childNode, "desc", ref value))
			{
				cAvatarInfoLevel.sDesc = value;
			}
			if (MyUtils.GetAttribute(childNode, "levelupdesc", ref value))
			{
				cAvatarInfoLevel.sLevelUpDesc = value;
			}
			if (MyUtils.GetAttribute(childNode, "materials", ref value))
			{
				string[] array = value.Split(',');
				for (int l = 0; l < array.Length; l++)
				{
					cAvatarInfoLevel.ltMaterials.Add(int.Parse(array[l]));
				}
			}
			if (MyUtils.GetAttribute(childNode, "materialscount", ref value))
			{
				string[] array = value.Split(',');
				for (int m = 0; m < array.Length && m < cAvatarInfoLevel.ltMaterials.Count; m++)
				{
					cAvatarInfoLevel.ltMaterialsCount.Add(int.Parse(array[m]));
				}
			}
		}
	}
}