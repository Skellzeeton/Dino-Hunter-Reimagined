using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class iDataCenter
{
	public class CCrystalInBackground
	{
		public float m_fMoney;
		public string m_sCombineKey;
		public SafeInteger m_nCrystal;

		public CCrystalInBackground()
		{
			m_nCrystal = new SafeInteger();
			m_nCrystal.Set(0);
		}
	}

	public class CUnlockSign
	{
		public int m_nType;
		public int m_nID;

		public CUnlockSign() { }
		public CUnlockSign(int type, int id)
		{
			m_nType = type;
			m_nID = id;
		}
	}
	private int m_nCurrentSlot = 0;
	public bool m_bSlotLoaded = false;
	private const int MAX_SLOTS = 5;
	private const string SAVE_FILE_FORMAT = "gamedata_slot{0}.json";
	private const string BACKUP_FILE_FORMAT = "gamedata_slot{0}.json.bak";
	private const string TEMP_FILE_FORMAT = "gamedata_slot{0}.json.tmp";

	public bool isFirstTimePlay
	{
		get { return m_bFirstTimePlay; }
	}

	protected string m_sSaveVersion = "1.0.0";
	protected string m_sGameVersion = "3.1.7a";
	protected SafeInteger m_nGold;
	protected SafeInteger m_nCrystal;
	protected List<CCrystalInBackground> m_ltCrystalInBackground;
	protected SafeInteger m_nStashLevel;
	protected int m_nCrystalTotalGain;
	protected int m_nCrystalTotalConsume;
	protected Dictionary<int, int> m_dictMaterials;
	protected Dictionary<int, int> m_dictWeapon;
	protected Dictionary<int, int> m_dictEquipStone;
	protected Dictionary<int, int> m_dictSkill;
	protected Dictionary<int, int> m_dictPassiveSkill;
	protected Dictionary<int, int> m_dictAvatar;
	protected Dictionary<int, CCharSaveInfo> m_dictCharSaveInfo;
	protected List<CLevelSaveInfo> m_ltLevelSaveInfo;
	protected List<int> m_ltFreeWeapon;
	protected Dictionary<int, CAchievementData> m_dictAchievementData;
	protected DateTime m_lastLoginTime;
	protected int m_nDailyRewardCount;
	protected int m_nDailyRewardHasGot;
	protected List<int> m_ltDailyTask;
	protected bool m_bMusic;
	protected bool m_bSound;
	protected bool m_bAutoAim;
	protected List<CUnlockSign> m_ltUnlockSign;
	protected float m_fSceneProccess;
	protected bool m_bTutorial;
	protected int m_nTutorialVillageState;
	protected bool m_bEvaluate;
	protected int m_nEnterAppCount;
	protected Dictionary<int, int> m_dictWeaponSign;
	protected Dictionary<int, int> m_dictEquipStoneSign;
	protected Dictionary<int, int> m_dictSkillSign;
	protected Dictionary<int, int> m_dictCharacterSign;
	protected Dictionary<int, int> m_dictAvatarSign;
	protected int m_nCurCharID;
	protected int[] m_arrSelectWeapon;
	protected Dictionary<int, int[]> m_dictSelectPassiveSkill;
	protected int m_nCurEquipStone;
	protected SafeInteger m_nAvatarHead;
	protected SafeInteger m_nAvatarUpper;
	protected SafeInteger m_nAvatarLower;
	protected SafeInteger m_nAvatarHeadup;
	protected SafeInteger m_nAvatarNeck;
	protected SafeInteger m_nAvatarWrist;
	protected SafeInteger m_nAvatarBadge;
	protected SafeInteger m_nAvatarStone;
	protected int m_nLatestLevel;
	protected int m_nLastLevel;
	protected bool m_bUnLockLevel;
	protected List<int> m_ltLevelList;
	protected bool m_bFirstTimePlay;
	protected Dictionary<int, int> m_dictWorldMonsterKill;
	protected List<int> m_ltTitle;
	protected Dictionary<int, int> m_dictKillMonster;
	protected SafeInteger m_MVPCount;
	protected SafeInteger m_ReviveInCoopCount;
	protected SafeInteger m_DeadInCoopCount;
	protected string m_sNickName = string.Empty;
	protected SafeInteger m_nHunterLvl;
	protected SafeInteger m_nHunterExp;
	protected SafeInteger m_nHunterExpTotal;
	protected SafeInteger m_nCombatPower;
	protected int m_nRank;
	protected int m_nLastRank;
	protected SafeInteger m_nBeAdmired;
	protected SafeInteger m_nTitle;
	protected string m_sSignature = "Let's go hunting!";
	protected List<string> m_ltFriends;
	public byte[] m_Photo;
	public bool m_bInBlackName { get; set; }
	public bool m_bInWhiteName { get; set; }

	public bool m_bForcedLoadoutActive = false;

	private int[] m_forcedSelectWeapons;

	private Dictionary<int, int> m_forcedWeaponLevels;

	private Dictionary<int, int> m_forcedAvatarLevels;

	private int m_forcedAvatarHead, m_forcedAvatarUpper, m_forcedAvatarLower,

	m_forcedAvatarHeadup, m_forcedAvatarNeck, m_forcedAvatarWrist,

	m_forcedAvatarBadge, m_forcedAvatarStone;

	private int[] m_backupSelectWeapons;

	private Dictionary<int, int> m_backupWeaponLevels;

	private Dictionary<int, int> m_backupAvatarLevels;

	private int m_backupAvatarHead, m_backupAvatarUpper, m_backupAvatarLower,
	m_backupAvatarHeadup, m_backupAvatarNeck, m_backupAvatarWrist,
	m_backupAvatarBadge, m_backupAvatarStone;

	private int m_nDifficulty = 0;

	public string GameVersion
	{
		get { return m_sGameVersion; }
		set { m_sGameVersion = value; }
	}

	public bool isTutorial
	{
		get { return m_bTutorial; }
		set { m_bTutorial = value; }
	}

	public int nTutorialVillageState
	{
		get { return m_nTutorialVillageState; }
		set { m_nTutorialVillageState = value; }
	}

	public bool MusicSwitch
	{
		get { return m_bMusic; }
		set { m_bMusic = value; }
	}

	public bool SoundSwitch
	{
		get { return m_bSound; }
		set { m_bSound = value; }
	}

	public bool AutoAimSwitch
	{
		get { return m_bAutoAim; }
		set { m_bAutoAim = value; }
	}

	public int Gold
	{
		get { return m_nGold.Get(); }
	}

	public int Crystal
	{
		get { return m_nCrystal.Get(); }
	}

	public int CurCharID
	{
		get { return m_nCurCharID; }
		set { m_nCurCharID = value; }
	}

	public int CurEquipStone
	{
		get { return m_nCurEquipStone; }
		set { m_nCurEquipStone = value; }
	}

	public int LatestLevel
	{
		get { return m_nLatestLevel; }
		set { m_nLatestLevel = value; }
	}

	public int LastLevel
	{
		get { return m_nLastLevel; }
		set { m_nLastLevel = value; }
	}

	public float SceneProccess
	{
		get { return m_fSceneProccess; }
		set { m_fSceneProccess = value; }
	}

	public int StashLevel
	{
		get { return m_nStashLevel.Get(); }
		set { m_nStashLevel.Set(value); }
	}

	public int StashCount
	{
		get
		{
			int num = 0;
			foreach (int value in m_dictMaterials.Values)
			{
				num += value;
			}
			return num;
		}
	}

	public int StashCountMax
	{
		get
		{
			iGameData gameData = iGameApp.GetInstance().m_GameData;
			if (gameData == null)
			{
				return 0;
			}
			CStashCapacity stashCapacity = gameData.GetStashCapacity(StashLevel);
			if (stashCapacity == null)
			{
				return 0;
			}
			return stashCapacity.nCapacity;
		}
	}

	public int HighestCharLevel
	{
		get
		{
			int num = 0;
			foreach (CCharSaveInfo value in m_dictCharSaveInfo.Values)
			{
				if (num == 0 || num < value.nLevel)
				{
					num = value.nLevel;
				}
			}
			return num;
		}
	}

	public DateTime LastLoginTime
	{
		get { return m_lastLoginTime; }
		set { m_lastLoginTime = value; }
	}

	public int DailyRewardCount
	{
		get { return m_nDailyRewardCount; }
		set { m_nDailyRewardCount = value; }
	}

	public int DailyRewardHasGot
	{
		get { return m_nDailyRewardHasGot; }
		set { m_nDailyRewardHasGot = value; }
	}

	public bool isEvaluate
	{
		get { return m_bEvaluate; }
		set { m_bEvaluate = value; }
	}

	public int EnterAppCount
	{
		get { return m_nEnterAppCount; }
		set { m_nEnterAppCount = value; }
	}

	public int AvatarHead
	{
		get { return m_nAvatarHead.Get(); }
		set { m_nAvatarHead.Set(value); }
	}

	public int AvatarUpper
	{
		get { return m_nAvatarUpper.Get(); }
		set { m_nAvatarUpper.Set(value); }
	}

	public int AvatarLower
	{
		get { return m_nAvatarLower.Get(); }
		set { m_nAvatarLower.Set(value); }
	}

	public int AvatarWrist
	{
		get { return m_nAvatarWrist.Get(); }
		set { m_nAvatarWrist.Set(value); }
	}

	public int AvatarHeadup
	{
		get { return m_nAvatarHeadup.Get(); }
		set { m_nAvatarHeadup.Set(value); }
	}

	public int AvatarNeck
	{
		get { return m_nAvatarNeck.Get(); }
		set { m_nAvatarNeck.Set(value); }
	}

	public int AvatarBadge
	{
		get { return m_nAvatarBadge.Get(); }
		set { m_nAvatarBadge.Set(value); }
	}

	public int AvatarStone
	{
		get { return m_nAvatarStone.Get(); }
		set { m_nAvatarStone.Set(value); }
	}

	public bool isUnLockLevel
	{
		get { return m_bUnLockLevel; }
	}

	public string NickName
	{
		get { return m_sNickName; }
		set { m_sNickName = value; }
	}

	public int HunterLvl
	{
		get { return m_nHunterLvl.Get(); }
		set { m_nHunterLvl.Set(value); }
	}

	public int HunterExp
	{
		get { return m_nHunterExp.Get(); }
		set { m_nHunterExp.Set(value); }
	}

	public int HunterExpTotal
	{
		get { return m_nHunterExpTotal.Get(); }
		set { m_nHunterExpTotal.Set(value); }
	}

	public int CombatPower
	{
		get { return m_nCombatPower.Get(); }
		set { m_nCombatPower.Set(value); }
	}

	public int Rank
	{
		get { return m_nRank; }
		set { m_nRank = value; }
	}

	public int LastRank
	{
		get { return m_nLastRank; }
		set { m_nLastRank = value; }
	}

	public int BeAdmire
	{
		get { return m_nBeAdmired.Get(); }
		set { m_nBeAdmired.Set(value); }
	}

	public int Title
	{
		get { return m_nTitle.Get(); }
		set { m_nTitle.Set(value); }
	}

	public string Signature
	{
		get { return m_sSignature; }
		set { m_sSignature = value; }
	}

	public int MVPCount
	{
		get { return m_MVPCount.Get(); }
		set { m_MVPCount.Set(value); }
	}

	public int ReviveInCoopCount
	{
		get { return m_ReviveInCoopCount.Get(); }
		set { m_ReviveInCoopCount.Set(value); }
	}

	public int DeadInCoopCount
	{
		get { return m_DeadInCoopCount.Get(); }
		set { m_DeadInCoopCount.Set(value); }
	}

	public int GetBoxCount(int charID)
	{
		var info = GetCharacter(charID);
		return info != null ? info.nBoxCount : 0;
	}

	public void SetBoxCount(int charID, int count)
	{
		var info = GetCharacter(charID);
		if (info != null) info.nBoxCount = count;
	}

	public void AddBox(int charID, int count = 1)
	{
		var info = GetCharacter(charID);
		if (info != null) info.nBoxCount += count;
	}

	public bool UseBox(int charID)
	{
		var info = GetCharacter(charID);
		if (info != null && info.nBoxCount > 0)
		{
			info.nBoxCount--;
			return true;
		}
		return false;
	}

	public int Difficulty
	{
		get { return m_nDifficulty; }
		set { m_nDifficulty = value; }
	}

	public int CurrentSlot
	{
		get { return m_nCurrentSlot; }
		set
		{
			if (value < 0 || value >= MAX_SLOTS)
				throw new ArgumentOutOfRangeException("Slot must be between 0 and 4");
			if (value != m_nCurrentSlot)
			{
				if (m_bSlotLoaded)
					SaveCurrentSlot();
				m_nCurrentSlot = value;
				m_bSlotLoaded = false;
			}
		}
	}

	public iDataCenter()
	{
		m_nGold = new SafeInteger();
		m_nCrystal = new SafeInteger();
		m_ltCrystalInBackground = new List<CCrystalInBackground>();
		m_nStashLevel = new SafeInteger();
		m_dictMaterials = new Dictionary<int, int>();
		m_dictWeapon = new Dictionary<int, int>();
		m_dictEquipStone = new Dictionary<int, int>();
		m_dictPassiveSkill = new Dictionary<int, int>();
		m_dictCharSaveInfo = new Dictionary<int, CCharSaveInfo>();
		m_dictSkill = new Dictionary<int, int>();
		m_dictAvatar = new Dictionary<int, int>();
		m_nAvatarHead = new SafeInteger();
		m_nAvatarUpper = new SafeInteger();
		m_nAvatarLower = new SafeInteger();
		m_nAvatarHeadup = new SafeInteger();
		m_nAvatarNeck = new SafeInteger();
		m_nAvatarWrist = new SafeInteger();
		m_nAvatarBadge = new SafeInteger();
		m_nAvatarStone = new SafeInteger();
		m_dictWeaponSign = new Dictionary<int, int>();
		m_dictEquipStoneSign = new Dictionary<int, int>();
		m_dictSkillSign = new Dictionary<int, int>();
		m_dictCharacterSign = new Dictionary<int, int>();
		m_dictAvatarSign = new Dictionary<int, int>();
		m_arrSelectWeapon = new int[3] { 2, 1, -1 };
		m_dictSelectPassiveSkill = new Dictionary<int, int[]>();
		m_ltLevelSaveInfo = new List<CLevelSaveInfo>();
		m_ltLevelList = new List<int>();
		for (int i = 1001; i <= 20010; i++)
		{
			m_ltLevelList.Add(i);
		}
		m_ltUnlockSign = new List<CUnlockSign>();
		m_dictAchievementData = new Dictionary<int, CAchievementData>();
		m_ltFreeWeapon = new List<int>();
		m_ltDailyTask = new List<int>();
		m_dictWorldMonsterKill = new Dictionary<int, int>();
		m_nHunterLvl = new SafeInteger();
		m_nHunterExp = new SafeInteger();
		m_nHunterExpTotal = new SafeInteger();
		m_nCombatPower = new SafeInteger();
		m_nBeAdmired = new SafeInteger();
		m_nTitle = new SafeInteger();
		m_ltFriends = new List<string>();
		m_ltTitle = new List<int>();
		m_dictKillMonster = new Dictionary<int, int>();
		m_MVPCount = new SafeInteger();
		m_ReviveInCoopCount = new SafeInteger();
		m_DeadInCoopCount = new SafeInteger();
		Clear();
	}

	public void Clear()
	{
		m_bMusic = true;
		m_bSound = true;
		m_bAutoAim = true;
		m_nGold.Set(100);
		m_nCrystal.Set(50);
		m_ltCrystalInBackground.Clear();
		m_nStashLevel.Set(1);
		m_nCrystalTotalGain = 0;
		m_nCrystalTotalConsume = 0;
		m_dictMaterials.Clear();
		m_dictWeapon.Clear();
		m_dictEquipStone.Clear();
		m_dictPassiveSkill.Clear();
		m_dictCharSaveInfo.Clear();
		m_dictSkill.Clear();
		m_dictAvatar.Clear();
		m_nAvatarHead.Set(-1);
		m_nAvatarUpper.Set(-1);
		m_nAvatarLower.Set(-1);
		m_nAvatarHeadup.Set(-1);
		m_nAvatarNeck.Set(-1);
		m_nAvatarWrist.Set(-1);
		m_nAvatarBadge.Set(-1);
		m_nAvatarStone.Set(-1);
		m_ltLevelSaveInfo.Clear();
		m_ltFreeWeapon.Clear();
		m_dictAchievementData.Clear();
		m_dictWeaponSign.Clear();
		m_dictEquipStoneSign.Clear();
		m_dictSkillSign.Clear();
		m_dictCharacterSign.Clear();
		m_dictAvatarSign.Clear();
		m_nCurCharID = 1;
		m_arrSelectWeapon = new int[3] { 2, 1, -1 };
		m_dictSelectPassiveSkill.Clear();
		m_nCurEquipStone = 0;
		m_nLatestLevel = 1001;
		m_bUnLockLevel = false;
		m_ltLevelSaveInfo.Clear();
		m_bFirstTimePlay = false;
		m_fSceneProccess = 0f;
		m_bTutorial = false;
		m_nTutorialVillageState = 28;
		m_bEvaluate = false;
		m_nEnterAppCount = 0;
		m_ltUnlockSign.Clear();
		m_dictAchievementData.Clear();
		m_ltFreeWeapon.Clear();
		m_ltDailyTask.Clear();
		m_nDailyRewardCount = 1;
		m_nDailyRewardHasGot = 0;
		m_lastLoginTime = new DateTime(1970, 1, 1);
		m_dictWorldMonsterKill.Clear();
		m_sNickName = string.Empty;
		m_nHunterLvl.Set(1);
		m_nHunterExp.Set(0);
		m_nHunterExpTotal.Set(0);
		m_nBeAdmired.Set(0);
		m_nRank = 0;
		m_nLastRank = 0;
		m_nBeAdmired.Set(0);
		m_sSignature = "Let's go hunting!";
		m_ltFriends.Clear();
		m_ltTitle.Clear();
		m_ltTitle.Add(1);
		m_nTitle.Set(1);
		m_dictKillMonster.Clear();
		m_MVPCount.Set(0);
		m_ReviveInCoopCount.Set(0);
		m_DeadInCoopCount.Set(0);
		m_bInBlackName = false;
		m_bInWhiteName = false;
		m_nDifficulty = 1;
	}

	private string GetSavePath(string fileName)
	{
		return System.IO.Path.Combine(Application.persistentDataPath, fileName);
	}

	private string GetCurrentPath()
	{
		return GetSavePath(string.Format(SAVE_FILE_FORMAT, m_nCurrentSlot));
	}

	private string GetBackupPath()
	{
		return GetSavePath(string.Format(BACKUP_FILE_FORMAT, m_nCurrentSlot));
	}

	private string GetTempPath()
	{
		return GetSavePath(string.Format(TEMP_FILE_FORMAT, m_nCurrentSlot));
	}

	public bool SlotExists(int slot)
	{
		if (slot < 0 || slot >= MAX_SLOTS) return false;
		string path = GetSavePath(string.Format(SAVE_FILE_FORMAT, slot));
		return File.Exists(path);
	}

	public void DeleteSlot(int slot)
	{
		if (slot < 0 || slot >= MAX_SLOTS) return;
		string basePath = GetSavePath(string.Format("gamedata_slot{0}", slot));
		string[] files = { basePath + ".json", basePath + ".json.bak", basePath + ".json.tmp" };
		foreach (string f in files)
		{
			if (File.Exists(f)) File.Delete(f);
		}
		if (slot == m_nCurrentSlot)
		{
			Clear();
			m_bSlotLoaded = false;
		}
	}

	public void CreateNewSlot(int slot, int difficulty = 1)
	{
		if (slot < 0 || slot >= MAX_SLOTS) return;
		CurrentSlot = slot;
		Clear();
		m_nDifficulty = difficulty;
		m_bFirstTimePlay = true;
		m_nTutorialVillageState = -1;
		SetCharacter(1, 1, 0);
		SetCharacter(6, 1, 0);
		SetWeaponLevel(1, 1);
		SetWeaponLevel(2, 1);
		m_bSlotLoaded = true;
		Save();
	}

	public void SwitchToSlot(int slot)
	{
		if (slot < 0 || slot >= MAX_SLOTS) return;
		if (slot == m_nCurrentSlot && m_bSlotLoaded) return;
		CurrentSlot = slot;
		Load();
	}

	public void SaveCurrentSlot()
	{
		if (!m_bSlotLoaded) return;
		Save();
	}

	[Serializable]
	private class SaveData
	{
		public string version;
		public string gameversion;
		public int gold;
		public int crystal;
		public int stashlevel;
		public int latestlevel;
		public int lastlevel;
		public bool isunlocklevel;
		public float proccess;
		public int crystaltotalgain;
		public int crystaltotalconsume;
		public bool isMusic;
		public bool isSound;
		public bool isTutorial;
		public int tutorialVillageState;
		public bool isEvaluate;
		public int enterappcount;
		public int dailyrewardcount;
		public int dailyrewardhasgot;
		public string nickname;
		public int hunterlvl;
		public int hunterexp;
		public int hunterexptotal;
		public int combatpower;
		public int rank;
		public int beadmired;
		public int title;
		public string signature;
		public int deadincoop;
		public int reviveincoop;
		public int mvpincoop;
		public bool isinblackname;
		public bool isinwhitename;
		public string photo;
		public string killmonster;
		public string worldmonsterkill;
		public string lastlogintime;
		public string dailytask;
		public List<LevelSaveInfoData> passedlevel;
		public CharacterData character;
		public WeaponData weapon;
		public AvatarData avatar;
		public SkillData skill;
		public EquipStoneData equipstone;
		public List<MaterialData> materials;
		public List<UnlockSignData> unlocksign;
		public List<CrystalInBackgroundData> crystalinbackground;
		public List<AchievementData> achievement;
		public List<int> freeweapon;
		public List<string> friends;
		public List<int> titles;
		public int difficulty = -1;
	}

	public List<SaveSlotInfo> GetSaveSlotsInfo()
	{
		List<SaveSlotInfo> list = new List<SaveSlotInfo>();
		for (int i = 0; i < MAX_SLOTS; i++)
		{
			SaveSlotInfo info = new SaveSlotInfo();
			info.slotIndex = i;
			info.exists = SlotExists(i);
			info.difficulty = 0;
			if (info.exists)
			{
				SaveData data = null;
				string path = GetSavePath(string.Format(SAVE_FILE_FORMAT, i));
				string decrypted = string.Empty;

				if (TryReadEncryptedFile(path, ref decrypted))
				{
					try { data = JsonUtility.FromJson<SaveData>(decrypted); }
					catch { data = null; }
				}
				if (data != null && IsValidSaveData(data))
				{
					info.difficulty = (data.difficulty == -1) ? 1 : data.difficulty;
					info.hunterLevel = data.hunterlvl;
					info.latestLevel = data.latestlevel;
					info.lastPlayed = GetLastModifiedTime(i);
					info.gold = data.gold;
					info.crystals = data.crystal;
					info.mapProgress = data.proccess;
					int charID = data.character.select;
					iCharacterCenter charCenter = iGameApp.GetInstance().m_GameData.GetCharacterCenter();
					CCharacterInfo charInfo = charCenter?.Get(charID);
					if (charInfo != null)
					{
						CCharacterInfoLevel levelInfo = charInfo.Get(1);
						info.characterName = levelInfo != null ? levelInfo.sName : "Hunter";
					}
					else
					{
						info.characterName = data.nickname ?? "Hunter";
					}
				}
				else
				{
					info.exists = false;
				}
			}
			list.Add(info);
		}
		return list;
	}

	private DateTime GetLastModifiedTime(int slot)
	{
		string path = GetSavePath(string.Format(SAVE_FILE_FORMAT, slot));
		if (File.Exists(path))
			return File.GetLastWriteTime(path);
		return DateTime.MinValue;
	}

	public struct SaveSlotInfo
	{
		public int slotIndex;
		public bool exists;
		public string characterName;
		public int hunterLevel;
		public int latestLevel;
		public DateTime lastPlayed;
		public float mapProgress;
		public int gold;
		public int crystals;
		public int difficulty;
	}

	[Serializable]
	private class LevelSaveInfoData
	{
		public int id;
		public bool isignorecg;
	}

	[Serializable]
	private class CharacterData
	{
		public int select;
		public List<CharacterNode> nodes;
	}

	[Serializable]
	private class CharacterNode
	{
		public int id;
		public int level;
		public int exp;
		public int boxcount;
	}

	[Serializable]
	private class WeaponData
	{
		public List<int> select;
		public List<WeaponNode> nodes;
	}

	[Serializable]
	private class WeaponNode
	{
		public int id;
		public int level;
	}

	[Serializable]
	private class AvatarData
	{
		public int avatarhead;
		public int avatarupper;
		public int avatarlower;
		public int avatarheadup;
		public int avatarneck;
		public int avatarwrist;
		public int avatarbadge;
		public int avatarstone;
		public List<AvatarNode> nodes;
	}

	[Serializable]
	private class AvatarNode
	{
		public int id;
		public int level;
	}

	[Serializable]
	private class SkillData
	{
		public List<SelectPassiveSkill> selectnodes;
		public List<SkillNode> nodes;
		public List<SkillNode2> nodes2;
	}

	[Serializable]
	private class SelectPassiveSkill
	{
		public int charid;
		public List<int> select;
	}

	[Serializable]
	private class SkillNode
	{
		public int id;
		public int level;
	}

	[Serializable]
	private class SkillNode2
	{
		public int id;
		public int level;
	}

	[Serializable]
	private class EquipStoneData
	{
		public int select;
		public List<EquipStoneNode> nodes;
	}

	[Serializable]
	private class EquipStoneNode
	{
		public int id;
		public int level;
	}

	[Serializable]
	private class MaterialData
	{
		public int id;
		public int count;
	}

	[Serializable]
	private class UnlockSignData
	{
		public int type;
		public int id;
	}

	[Serializable]
	private class CrystalInBackgroundData
	{
		public string combinekey;
		public float money;
		public int crystal;
	}

	[Serializable]
	private class AchievementData
	{
		public int id;
		public int state;
		public int value;
		public List<bool> isgotreward;
	}

	private bool TryReadEncryptedFile(string path, ref string decryptedContent)
	{
		decryptedContent = string.Empty;

		if (!File.Exists(path))
		{
			return false;
		}
		try
		{
			string encrypted = File.ReadAllText(path);
			if (string.IsNullOrEmpty(encrypted))
			{
				return false;
			}
			decryptedContent = XXTEAUtils.Decrypt(encrypted, iServerConfigData.GetInstance().m_sServerInfoKey);
			return !string.IsNullOrEmpty(decryptedContent);
		}
		catch
		{
			return false;
		}
	}

	private bool TryLoadFromJson(string path, out SaveData data)
	{
		data = null;
		
		string jsonText = string.Empty;
		if (!TryReadEncryptedFile(path, ref jsonText))
		{
			return false;
		}
		try
		{
			data = JsonUtility.FromJson<SaveData>(jsonText);
			return data != null;
		}
		catch
		{
			data = null;
			return false;
		}
	}

	private bool IsValidSaveData(SaveData data)
	{
		if (data == null)
		{
			return false;
		}
		if (data.character == null || data.weapon == null || data.skill == null || 
			data.equipstone == null || data.materials == null)
		{
			return false;
		}
		return true;
	}

	private bool IsSeverelyDifferent(SaveData current, SaveData backup)
	{
		if (!IsValidSaveData(current) || !IsValidSaveData(backup))
		{
			return false;
		}
		
		int currentSections = 0;
		int backupSections = 0;
		if (current.character != null && current.character.nodes != null) currentSections++;
		if (current.weapon != null && current.weapon.nodes != null) currentSections++;
		if (current.skill != null) currentSections++;
		if (current.equipstone != null && current.equipstone.nodes != null) currentSections++;
		if (current.materials != null) currentSections++;
		if (backup.character != null && backup.character.nodes != null) backupSections++;
		if (backup.weapon != null && backup.weapon.nodes != null) backupSections++;
		if (backup.skill != null) backupSections++;
		if (backup.equipstone != null && backup.equipstone.nodes != null) backupSections++;
		if (backup.materials != null) backupSections++;
		int diff = Math.Abs(currentSections - backupSections);
		if (diff >= 3)
		{
			return true;
		}
		if (currentSections < 3 && backupSections >= 3)
		{
			return true;
		}
		return false;
	}

	private void RestoreBackupToCurrent()
	{
		string currentPath = GetCurrentPath();
		string backupPath = GetBackupPath();
		if (!File.Exists(backupPath))
		{
			return;
		}
		try
		{
			string backupEncrypted = File.ReadAllText(backupPath);
			if (string.IsNullOrEmpty(backupEncrypted))
			{
				return;
			}
			File.WriteAllText(currentPath, backupEncrypted);
			Debug.Log("[iDataCenter] Restored save from backup");
		}
		catch (Exception ex)
		{
			Debug.LogWarning("[iDataCenter] Failed to restore backup: " + ex.Message);
		}
	}

	private void SaveEncryptedAtomic(string jsonText)
	{
		string currentPath = GetCurrentPath();
		string backupPath = GetBackupPath();
		string tempPath = GetTempPath();
		string encrypted = XXTEAUtils.Encrypt(jsonText, iServerConfigData.GetInstance().m_sServerInfoKey);
		try
		{
			File.WriteAllText(tempPath, encrypted);
		}
		catch (Exception ex)
		{
			Debug.LogError("[iDataCenter] Failed to write temp file: " + ex.Message);
			return;
		}
		SaveData tempData;
		if (!TryLoadFromJson(tempPath, out tempData) || !IsValidSaveData(tempData))
		{
			Debug.LogError("[iDataCenter] Temp file validation failed, aborting save");
			return;
		}
		SaveData currentData;
		bool currentValid = TryLoadFromJson(currentPath, out currentData) && IsValidSaveData(currentData);
		try
		{
			if (currentValid && File.Exists(currentPath))
			{
				File.Replace(tempPath, currentPath, backupPath, true);
			}
			else
			{
				if (File.Exists(currentPath))
				{
					File.Delete(currentPath);
				}
				File.Move(tempPath, currentPath);
			}
		}
		catch (Exception ex)
		{
			Debug.LogError("[iDataCenter] Atomic save failed: " + ex.Message);
			iGameApp.PendingPopupMessage = "Save failed. Your data should be safe, though you may have lost any very recently saved data.";
			iGameApp.GetInstance().RequestReload();
			try
			{
				if (File.Exists(currentPath))
				{
					File.Delete(currentPath);
				}
				File.Move(tempPath, currentPath);
				Debug.Log("[iDataCenter] Fallback save succeeded");
			}
			catch (Exception ex2)
			{
				Debug.LogError("[iDataCenter] Fallback save also failed: " + ex2.Message);
			}
		}
	}

	public bool Load()
	{
		SaveData currentData;
		SaveData backupData;
		bool currentOk = TryLoadFromJson(GetCurrentPath(), out currentData) && IsValidSaveData(currentData);
		bool backupOk = TryLoadFromJson(GetBackupPath(), out backupData) && IsValidSaveData(backupData);
		SaveData chosenData = null;
		if (currentOk && backupOk)
		{
			if (IsSeverelyDifferent(currentData, backupData))
			{
				Debug.LogWarning("[iDataCenter] Current and backup differ, using backup");
				chosenData = backupData;
				RestoreBackupToCurrent();
				iGameApp.PendingPopupMessage = "Your save seems to have corrupted, so it was rolled back.";
			}
			else
			{
				chosenData = currentData;
			}
		}
		else if (currentOk)
		{
			chosenData = currentData;
		}
		else if (backupOk)
		{
			Debug.LogWarning("[iDataCenter] Current save corrupted, using backup");
			chosenData = backupData;
			RestoreBackupToCurrent();
			iGameApp.PendingPopupMessage = "Your save seems to have corrupted, so it was rolled back.";
		}
		else
		{
			Debug.Log("[iDataCenter] No valid save found in slot " + m_nCurrentSlot);
			return false;
		}
		if (chosenData != null)
		{
			LoadFromData(chosenData);
			m_bSlotLoaded = true;
			return true;
		}
		return false;
	}
	
	public void LoadData(string content)
	{
		if (string.IsNullOrEmpty(content))
		{
			Clear();
			m_bFirstTimePlay = true;
			m_nTutorialVillageState = -1;
			SetCharacter(1, 1, 0);
			SetCharacter(6, 1, 0);
			SetWeaponLevel(1, 1);
			SetWeaponLevel(2, 1);
			Save();
			return;
		}
		try
		{
			SaveData data = JsonUtility.FromJson<SaveData>(content);
			if (data != null && IsValidSaveData(data))
			{
				LoadFromData(data);
			}
			else
			{
				Clear();
				m_bFirstTimePlay = true;
				m_nTutorialVillageState = -1;
				SetCharacter(1, 1, 0);
				SetCharacter(6, 1, 0);
				SetWeaponLevel(1, 1);
				SetWeaponLevel(2, 1);
				Save();
			}
		}
		catch
		{
			Clear();
			m_bFirstTimePlay = true;
			m_nTutorialVillageState = -1;
			SetCharacter(1, 1, 0);
			SetCharacter(6, 1, 0);
			SetWeaponLevel(1, 1);
			SetWeaponLevel(2, 1);
			Save();
		}
	}
	
	public string Pack()
	{
		try
		{
			SaveData data = BuildSaveData();
			string json = JsonUtility.ToJson(data);
			string zippedContent = string.Empty;
			MyUtils.ZipString(json, ref zippedContent);
			return zippedContent;
		}
		catch (Exception ex)
		{
			Debug.LogError("[iDataCenter] Pack failed: " + ex.Message);
			return string.Empty;
		}
	}

	public bool UnPack(string sData)
	{
		try
		{
			if (string.IsNullOrEmpty(sData))
			{
				return false;
			}
			string unzippedContent = string.Empty;
			MyUtils.UnZipString(sData, ref unzippedContent);
			LoadData(unzippedContent);
			return true;
		}
		catch (Exception ex)
		{
			Debug.LogError("[iDataCenter] UnPack failed: " + ex.Message);
			return false;
		}
	}

	private SaveData BuildSaveData()
	{
		SaveData data = new SaveData();
		data.version = m_sSaveVersion;
		data.gameversion = m_sGameVersion;
		data.gold = m_nGold.Get();
		data.crystal = m_nCrystal.Get();
		data.stashlevel = m_nStashLevel.Get();
		data.latestlevel = m_nLatestLevel;
		data.lastlevel = m_nLastLevel;
		data.isunlocklevel = m_bUnLockLevel;
		data.proccess = m_fSceneProccess;
		data.crystaltotalgain = m_nCrystalTotalGain;
		data.crystaltotalconsume = m_nCrystalTotalConsume;
		data.isMusic = m_bMusic;
		data.isSound = m_bSound;
		data.isTutorial = m_bTutorial;
		data.tutorialVillageState = m_nTutorialVillageState;
		data.isEvaluate = m_bEvaluate;
		data.enterappcount = m_nEnterAppCount;
		data.dailyrewardcount = m_nDailyRewardCount;
		data.dailyrewardhasgot = m_nDailyRewardHasGot;
		data.nickname = m_sNickName;
		data.hunterlvl = m_nHunterLvl.Get();
		data.hunterexp = m_nHunterExp.Get();
		data.hunterexptotal = m_nHunterExpTotal.Get();
		data.combatpower = m_nCombatPower.Get();
		data.rank = m_nRank;
		data.beadmired = m_nBeAdmired.Get();
		data.title = m_nTitle.Get();
		data.signature = m_sSignature;
		data.deadincoop = m_DeadInCoopCount.Get();
		data.reviveincoop = m_ReviveInCoopCount.Get();
		data.mvpincoop = m_MVPCount.Get();
		data.isinblackname = m_bInBlackName;
		data.isinwhitename = m_bInWhiteName;
		data.difficulty = m_nDifficulty;
		if (m_Photo != null)
		{
			data.photo = Convert.ToBase64String(m_Photo);
		}
		string killMonsterStr = string.Empty;
		foreach (KeyValuePair<int, int> item in m_dictKillMonster)
		{
			killMonsterStr = ((killMonsterStr.Length >= 1)
				? (killMonsterStr + "," + item.Key + "," + item.Value)
				: (item.Key + "," + item.Value));
		}
		data.killmonster = killMonsterStr;
		string worldMonsterStr = string.Empty;
		foreach (KeyValuePair<int, int> item2 in m_dictWorldMonsterKill)
		{
			worldMonsterStr = ((worldMonsterStr.Length >= 1)
				? (worldMonsterStr + "," + item2.Key + "," + item2.Value)
				: (item2.Key + "," + item2.Value));
		}
		data.worldmonsterkill = worldMonsterStr;
		string lastLoginStr = m_lastLoginTime.Year + "," + m_lastLoginTime.Month + "," + m_lastLoginTime.Day + "," +
		                      m_lastLoginTime.Hour + "," + m_lastLoginTime.Minute + "," + m_lastLoginTime.Second;
		data.lastlogintime = lastLoginStr;
		string dailyTaskStr = string.Empty;
		for (int i = 0; i < m_ltDailyTask.Count; i++)
		{
			dailyTaskStr = ((i != 0) ? (dailyTaskStr + "," + m_ltDailyTask[i]) : m_ltDailyTask[i].ToString());
		}
		data.dailytask = dailyTaskStr;
		data.passedlevel = new List<LevelSaveInfoData>();
		foreach (CLevelSaveInfo levelInfo in m_ltLevelSaveInfo)
		{
			LevelSaveInfoData levelData = new LevelSaveInfoData();
			levelData.id = levelInfo.nID;
			levelData.isignorecg = levelInfo.isIgnoreCG;
			data.passedlevel.Add(levelData);
		}
		data.character = new CharacterData();
		data.character.select = m_nCurCharID;
		data.character.nodes = new List<CharacterNode>();
		foreach (CCharSaveInfo charInfo in m_dictCharSaveInfo.Values)
		{
			CharacterNode node = new CharacterNode();
			node.id = charInfo.nID;
			node.level = charInfo.nLevel;
			node.exp = charInfo.nExp;
			node.boxcount = charInfo.nBoxCount;
			data.character.nodes.Add(node);
		}
		data.weapon = new WeaponData();
		data.weapon.select = new List<int>();
		foreach (int weapon in m_arrSelectWeapon)
		{
			data.weapon.select.Add(weapon);
		}
		data.weapon.nodes = new List<WeaponNode>();
		foreach (KeyValuePair<int, int> item3 in m_dictWeapon)
		{
			WeaponNode node = new WeaponNode();
			node.id = item3.Key;
			node.level = item3.Value;
			data.weapon.nodes.Add(node);
		}
		data.avatar = new AvatarData();
		data.avatar.avatarhead = AvatarHead;
		data.avatar.avatarupper = AvatarUpper;
		data.avatar.avatarlower = AvatarLower;
		data.avatar.avatarheadup = AvatarHeadup;
		data.avatar.avatarneck = AvatarNeck;
		data.avatar.avatarwrist = AvatarWrist;
		data.avatar.avatarbadge = AvatarBadge;
		data.avatar.avatarstone = AvatarStone;
		data.avatar.nodes = new List<AvatarNode>();
		foreach (KeyValuePair<int, int> item4 in m_dictAvatar)
		{
			AvatarNode node = new AvatarNode();
			node.id = item4.Key;
			node.level = item4.Value;
			data.avatar.nodes.Add(node);
		}
		data.skill = new SkillData();
		data.skill.selectnodes = new List<SelectPassiveSkill>();
		foreach (KeyValuePair<int, int[]> item5 in m_dictSelectPassiveSkill)
		{
			SelectPassiveSkill selectSkill = new SelectPassiveSkill();
			selectSkill.charid = item5.Key;
			selectSkill.select = new List<int>();
			foreach (int skillId in item5.Value)
			{
				selectSkill.select.Add(skillId);
			}
			data.skill.selectnodes.Add(selectSkill);
		}
		data.skill.nodes = new List<SkillNode>();
		foreach (KeyValuePair<int, int> item6 in m_dictPassiveSkill)
		{
			SkillNode node = new SkillNode();
			node.id = item6.Key;
			node.level = item6.Value;
			data.skill.nodes.Add(node);
		}
		data.skill.nodes2 = new List<SkillNode2>();
		foreach (KeyValuePair<int, int> item7 in m_dictSkill)
		{
			SkillNode2 node = new SkillNode2();
			node.id = item7.Key;
			node.level = item7.Value;
			data.skill.nodes2.Add(node);
		}
		data.equipstone = new EquipStoneData();
		data.equipstone.select = m_nCurEquipStone;
		data.equipstone.nodes = new List<EquipStoneNode>();
		foreach (KeyValuePair<int, int> item8 in m_dictEquipStone)
		{
			EquipStoneNode node = new EquipStoneNode();
			node.id = item8.Key;
			node.level = item8.Value;
			data.equipstone.nodes.Add(node);
		}
		data.materials = new List<MaterialData>();
		foreach (KeyValuePair<int, int> item9 in m_dictMaterials)
		{
			if (item9.Value != 0)
			{
				MaterialData mat = new MaterialData();
				mat.id = item9.Key;
				mat.count = item9.Value;
				data.materials.Add(mat);
			}
		}
		data.unlocksign = new List<UnlockSignData>();
		foreach (CUnlockSign sign in m_ltUnlockSign)
		{
			UnlockSignData signData = new UnlockSignData();
			signData.type = sign.m_nType;
			signData.id = sign.m_nID;
			data.unlocksign.Add(signData);
		}
		data.crystalinbackground = new List<CrystalInBackgroundData>();
		foreach (CCrystalInBackground crystal in m_ltCrystalInBackground)
		{
			CrystalInBackgroundData crystalData = new CrystalInBackgroundData();
			crystalData.combinekey = crystal.m_sCombineKey;
			crystalData.money = crystal.m_fMoney;
			crystalData.crystal = crystal.m_nCrystal.Get();
			data.crystalinbackground.Add(crystalData);
		}
		data.achievement = new List<AchievementData>();
		foreach (CAchievementData achi in m_dictAchievementData.Values)
		{
			AchievementData achiData = new AchievementData();
			achiData.id = achi.nID;
			achiData.state = achi.nState;
			achiData.value = achi.nCurValue;
			achiData.isgotreward = new List<bool>();
			for (int j = 0; j < 3; j++)
			{
				achiData.isgotreward.Add(achi.IsGotReward(j));
			}
			data.achievement.Add(achiData);
		}

		data.freeweapon = new List<int>();
		foreach (int weaponId in m_ltFreeWeapon)
		{
			data.freeweapon.Add(weaponId);
		}

		data.friends = new List<string>();
		foreach (string friend in m_ltFriends)
		{
			data.friends.Add(friend);
		}

		data.titles = new List<int>();
		foreach (int titleId in m_ltTitle)
		{
			data.titles.Add(titleId);
		}

		return data;
	}

	private void LoadFromData(SaveData data)
	{
		if (data == null) return;
		m_dictMaterials.Clear();
		m_dictWeapon.Clear();
		m_dictEquipStone.Clear();
		m_dictPassiveSkill.Clear();
		m_dictCharSaveInfo.Clear();
		m_dictSkill.Clear();
		m_dictAvatar.Clear();
		m_dictWeaponSign.Clear();
		m_dictEquipStoneSign.Clear();
		m_dictSkillSign.Clear();
		m_dictCharacterSign.Clear();
		m_dictAvatarSign.Clear();
		m_dictSelectPassiveSkill.Clear();
		m_ltLevelSaveInfo.Clear();
		m_ltFreeWeapon.Clear();
		m_dictAchievementData.Clear();
		m_ltUnlockSign.Clear();
		m_ltCrystalInBackground.Clear();
		m_dictKillMonster.Clear();
		m_dictWorldMonsterKill.Clear();
		m_ltTitle.Clear();
		m_ltFriends.Clear();
		m_ltDailyTask.Clear();
		m_ltLevelList.Clear();
		if (!string.IsNullOrEmpty(data.gameversion))
		{
			m_sGameVersion = data.gameversion;
		}
		m_nGold.Set(data.gold);
		m_nCrystal.Set(data.crystal);
		m_nStashLevel.Set(data.stashlevel);
		m_nLatestLevel = data.latestlevel;
		m_nLastLevel = data.lastlevel;
		m_bUnLockLevel = data.isunlocklevel;
		m_fSceneProccess = data.proccess;
		m_nCrystalTotalGain = data.crystaltotalgain;
		m_nCrystalTotalConsume = data.crystaltotalconsume;
		m_bMusic = data.isMusic;
		m_bSound = data.isSound;
		m_bTutorial = data.isTutorial;
		m_nTutorialVillageState = data.tutorialVillageState;
		m_bEvaluate = data.isEvaluate;
		m_nEnterAppCount = data.enterappcount;
		m_nDailyRewardCount = data.dailyrewardcount;
		m_nDailyRewardHasGot = data.dailyrewardhasgot;
		m_sNickName = data.nickname ?? string.Empty;
		m_nHunterLvl.Set(data.hunterlvl);
		m_nHunterExp.Set(data.hunterexp);
		m_nHunterExpTotal.Set(data.hunterexptotal);
		m_nCombatPower.Set(data.combatpower);
		m_nRank = data.rank;
		m_nBeAdmired.Set(data.beadmired);
		int titleVal = data.title;
		if (titleVal <= 0) titleVal = 1;
		m_nTitle.Set(titleVal);
		m_sSignature = data.signature ?? "Let's go hunting!";
		m_DeadInCoopCount.Set(data.deadincoop);
		m_ReviveInCoopCount.Set(data.reviveincoop);
		m_MVPCount.Set(data.mvpincoop);
		m_bInBlackName = data.isinblackname;
		m_bInWhiteName = data.isinwhitename;
		m_nDifficulty = (data.difficulty == -1) ? 1 : data.difficulty;
		if (!string.IsNullOrEmpty(data.photo))
		{
			try
			{
				m_Photo = Convert.FromBase64String(data.photo);
			}
			catch { m_Photo = null; }
		}
		if (!string.IsNullOrEmpty(data.killmonster))
		{
			m_dictKillMonster.Clear();
			string[] array = data.killmonster.Split(',');
			if (array != null && array.Length > 0)
			{
				for (int i = 0; i < array.Length / 2; i++)
				{
					SetkillMonster(int.Parse(array[i]), int.Parse(array[i + 1]));
				}
			}
		}
		if (!string.IsNullOrEmpty(data.worldmonsterkill))
		{
			m_dictWorldMonsterKill.Clear();
			string[] array = data.worldmonsterkill.Split(',');
			if (array != null && array.Length > 0)
			{
				for (int j = 0; j < array.Length / 2; j++)
				{
					AddWorldMonsterKill(int.Parse(array[j]), int.Parse(array[j + 1]));
				}
			}
		}
		if (!string.IsNullOrEmpty(data.lastlogintime))
		{
			string[] array = data.lastlogintime.Split(',');
			if (array != null && array.Length == 6)
			{
				m_lastLoginTime = new DateTime(int.Parse(array[0]), int.Parse(array[1]), int.Parse(array[2]), 
					int.Parse(array[3]), int.Parse(array[4]), int.Parse(array[5]));
			}
		}
		if (!string.IsNullOrEmpty(data.dailytask))
		{
			m_ltDailyTask.Clear();
			string[] array = data.dailytask.Split(',');
			if (array != null)
			{
				for (int k = 0; k < array.Length; k++)
				{
					m_ltDailyTask.Add(int.Parse(array[k]));
				}
			}
		}
		if (data.passedlevel != null)
		{
			m_ltLevelSaveInfo.Clear();
			foreach (var levelData in data.passedlevel)
			{
				CLevelSaveInfo info = new CLevelSaveInfo();
				info.nID = levelData.id;
				info.isIgnoreCG = levelData.isignorecg;
				m_ltLevelSaveInfo.Add(info);
			}
		}
		if (data.character != null)
		{
			m_nCurCharID = data.character.select;
			if (data.character.nodes != null)
			{
				foreach (var node in data.character.nodes)
				{
					SetCharacter(node.id, node.level, node.exp);
					CCharSaveInfo info = GetCharacter(node.id);
					if (info != null) info.nBoxCount = node.boxcount;
				}
			}
		}
		if (data.weapon != null)
		{
			if (data.weapon.select != null)
			{
				for (int l = 0; l < data.weapon.select.Count && l < m_arrSelectWeapon.Length; l++)
				{
					m_arrSelectWeapon[l] = data.weapon.select[l];
				}
			}
			if (data.weapon.nodes != null)
			{
				foreach (var node in data.weapon.nodes)
				{
					SetWeaponLevel(node.id, node.level);
				}
			}
		}
		if (data.avatar != null)
		{
			AvatarHead = data.avatar.avatarhead;
			AvatarUpper = data.avatar.avatarupper;
			AvatarLower = data.avatar.avatarlower;
			AvatarHeadup = data.avatar.avatarheadup;
			AvatarNeck = data.avatar.avatarneck;
			AvatarWrist = data.avatar.avatarwrist;
			AvatarBadge = data.avatar.avatarbadge;
			AvatarStone = data.avatar.avatarstone;
			if (data.avatar.nodes != null)
			{
				foreach (var node in data.avatar.nodes)
				{
					SetAvatar(node.id, node.level);
				}
			}
		}
		if (data.skill != null)
		{
			if (data.skill.selectnodes != null)
			{
				foreach (var selectNode in data.skill.selectnodes)
				{
					if (selectNode.select != null)
					{
						for (int m = 0; m < selectNode.select.Count; m++)
						{
							SetSelectPassiveSkill(selectNode.charid, m, selectNode.select[m]);
						}
					}
				}
			}
			if (data.skill.nodes != null)
			{
				foreach (var node in data.skill.nodes)
				{
					SetPassiveSkill(node.id, node.level);
				}
			}
			if (data.skill.nodes2 != null)
			{
				foreach (var node in data.skill.nodes2)
				{
					SetSkill(node.id, node.level);
				}
			}
		}
		if (data.equipstone != null)
		{
			m_nCurEquipStone = data.equipstone.select;
			if (data.equipstone.nodes != null)
			{
				foreach (var node in data.equipstone.nodes)
				{
					SetEquipStone(node.id, node.level);
				}
			}
		}
		if (data.materials != null)
		{
			foreach (var mat in data.materials)
			{
				SetMaterialNum(mat.id, mat.count);
			}
		}
		if (data.unlocksign != null)
		{
			m_ltUnlockSign.Clear();
			foreach (var sign in data.unlocksign)
			{
				AddUnlockSign(sign.type, sign.id);
			}
		}
		if (data.crystalinbackground != null)
		{
			m_ltCrystalInBackground.Clear();
			foreach (var crystalData in data.crystalinbackground)
			{
				CCrystalInBackground crystal = new CCrystalInBackground();
				crystal.m_sCombineKey = crystalData.combinekey;
				crystal.m_fMoney = crystalData.money;
				crystal.m_nCrystal.Set(crystalData.crystal);
				m_ltCrystalInBackground.Add(crystal);
			}
		}
		if (data.achievement != null)
		{
			m_dictAchievementData.Clear();
			foreach (var achiData in data.achievement)
			{
				CAchievementData achievement = new CAchievementData();
				achievement.nID = achiData.id;
				achievement.nState = achiData.state;
				achievement.nCurValue = achiData.value;
				if (achiData.isgotreward != null)
				{
					for (int n = 0; n < achiData.isgotreward.Count && n < 3; n++)
					{
						achievement.SetGotReward(n, achiData.isgotreward[n]);
					}
				}
				AddAchiData(achievement.nID, achievement);
			}
		}
		if (data.freeweapon != null)
		{
			m_ltFreeWeapon.Clear();
			foreach (int weaponId in data.freeweapon)
			{
				AddFreeWeapon(weaponId);
			}
		}
		if (data.friends != null)
		{
			m_ltFriends.Clear();
			foreach (string friend in data.friends)
			{
				AddFriend(friend);
			}
		}
		if (data.titles != null)
		{
			m_ltTitle.Clear();
			m_ltTitle.Add(1);
			foreach (int titleId in data.titles)
			{
				if (!m_ltTitle.Contains(titleId))
				{
					m_ltTitle.Add(titleId);
				}
			}
		}
	}

	public void Save()
	{
		bool wasForced = m_bForcedLoadoutActive;
		try
		{
			if (wasForced)
			{
				RestoreOriginal();
			}
			SaveInternal();
		}
		finally
		{
			if (wasForced)
			{
				ApplyForcedValues();
				m_bForcedLoadoutActive = true;
			}
		}
	}

	private void SaveInternal()
	{
		try
		{
			SaveData data = BuildSaveData();
			string json = JsonUtility.ToJson(data, true);
			SaveEncryptedAtomic(json);
		}
		catch (Exception ex)
		{
			Debug.LogError("[iDataCenter] Failed to save: " + ex.Message);
			iGameApp.PendingPopupMessage = "Save failed. Your data should be safe, though you may have lost any very recently saved data.";
			iGameApp.GetInstance().RequestReload();
		}
	}

	public List<int> GetLevelList()
	{
		return m_ltLevelList;
	}

	public List<CLevelSaveInfo> GetLevelSaveInfoData()
	{
		return m_ltLevelSaveInfo;
	}

	public List<int> GetDailyTask()
	{
		return m_ltDailyTask;
	}

	public void AddUnlockSign(int type, int id)
	{
		CUnlockSign cUnlockSign = new CUnlockSign();
		cUnlockSign.m_nType = type;
		cUnlockSign.m_nID = id;
		m_ltUnlockSign.Add(cUnlockSign);
	}

	public List<CUnlockSign> GetUnlockSignList()
	{
		return m_ltUnlockSign;
	}

	public Dictionary<int, int> GetMaterialData()
	{
		return m_dictMaterials;
	}

	public Dictionary<int, int> GetWeaponData()
	{
		return m_dictWeapon;
	}

	public Dictionary<int, int> GetEquipStoneData()
	{
		return m_dictEquipStone;
	}

	public CCharSaveInfo GetCharacter(int nCharID)
	{
		if (!m_dictCharSaveInfo.ContainsKey(nCharID))
		{
			return null;
		}
		return m_dictCharSaveInfo[nCharID];
	}

	public Dictionary<int, int> GetPassiveSkillData()
	{
		return m_dictPassiveSkill;
	}

	public bool GetPassiveSkill(int nSkillID, ref int nSkillLevel)
	{
		if (!m_dictPassiveSkill.ContainsKey(nSkillID))
		{
			return false;
		}
		nSkillLevel = m_dictPassiveSkill[nSkillID];
		return true;
	}

	public bool GetSkill(int nSkillID, ref int nSkillLevel)
	{
		if (!m_dictSkill.ContainsKey(nSkillID))
		{
			nSkillLevel = 1;
			return true;
		}
		nSkillLevel = m_dictSkill[nSkillID];
		return true;
	}

	public int GetPassiveSkillCount()
	{
		if (m_dictPassiveSkill == null)
		{
			return 0;
		}
		int num = 0;
		foreach (int value in m_dictPassiveSkill.Values)
		{
			if (value > 0)
			{
				num++;
			}
		}
		return num;
	}

	public int GetWeaponCount()
	{
		if (m_dictWeapon == null)
		{
			return 0;
		}
		int num = 0;
		foreach (int value in m_dictWeapon.Values)
		{
			if (value > 0)
			{
				num++;
			}
		}
		return num;
	}

	public bool GetEquipStone(int nItemID, ref int nItemLevel)
	{
		if (!m_dictEquipStone.ContainsKey(nItemID))
		{
			return false;
		}
		nItemLevel = m_dictEquipStone[nItemID];
		return true;
	}

	public bool GetWeaponLevel(int nWeaponID, ref int nLevel)
	{
		if (!m_dictWeapon.ContainsKey(nWeaponID))
		{
			return false;
		}
		nLevel = m_dictWeapon[nWeaponID];
		return true;
	}

	public int GetSelectWeapon(int nIndex)
	{
		if (nIndex < 0 || nIndex >= m_arrSelectWeapon.Length)
		{
			return -1;
		}
		return m_arrSelectWeapon[nIndex];
	}

	public bool HasSelectWeapon(int nWeaponID)
	{
		for (int i = 0; i < m_arrSelectWeapon.Length; i++)
		{
			if (m_arrSelectWeapon[i] != -1 && m_arrSelectWeapon[i] == nWeaponID)
			{
				return true;
			}
		}
		return false;
	}

	public int GetSelectPassiveSkill(int nCharID, int nIndex)
	{
		if (!m_dictSelectPassiveSkill.ContainsKey(nCharID))
		{
			return -1;
		}
		int[] array = m_dictSelectPassiveSkill[nCharID];
		if (nIndex < 0 || nIndex >= array.Length)
		{
			return -1;
		}
		return array[nIndex];
	}

	public bool HasSelectPassiveSkill(int nCharID, int nSkillID)
	{
		if (!m_dictSelectPassiveSkill.ContainsKey(nCharID))
		{
			return false;
		}
		int[] array = m_dictSelectPassiveSkill[nCharID];
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != -1 && array[i] == nSkillID)
			{
				return true;
			}
		}
		return false;
	}

	public Dictionary<int, int> GetWeaponSignData()
	{
		return m_dictWeaponSign;
	}

	public Dictionary<int, int> GetAvatarSignData()
	{
		return m_dictAvatarSign;
	}

	public Dictionary<int, int> GetSkillSignData()
	{
		return m_dictSkillSign;
	}

	public Dictionary<int, int> GetEquipStoneSignData()
	{
		return m_dictEquipStoneSign;
	}

	public Dictionary<int, int> GetCharacterSignData()
	{
		return m_dictCharacterSign;
	}

	public int GetMaterialNum(int nItemID)
	{
		if (!m_dictMaterials.ContainsKey(nItemID))
		{
			return -1;
		}
		return m_dictMaterials[nItemID];
	}

	public void AddMaterialNum(int nItemID, int nCount)
	{
		if (nItemID != -1)
		{
			if (!m_dictMaterials.ContainsKey(nItemID))
			{
				m_dictMaterials.Add(nItemID, nCount);
				return;
			}
			Dictionary<int, int> dictMaterials;
			Dictionary<int, int> dictionary = (dictMaterials = m_dictMaterials);
			int key;
			int key2 = (key = nItemID);
			key = dictMaterials[key];
			dictionary[key2] = key + nCount;
		}
	}

	public void DelMaterial(int nItemID)
	{
		if (m_dictMaterials.ContainsKey(nItemID))
		{
			m_dictMaterials.Remove(nItemID);
		}
	}

	public void SetMaterialNum(int nItemID, int nCount)
	{
		if (nItemID != -1)
		{
			if (!m_dictMaterials.ContainsKey(nItemID))
			{
				m_dictMaterials.Add(nItemID, nCount);
			}
			else
			{
				m_dictMaterials[nItemID] = nCount;
			}
		}
	}

	public int CheckStashVolume(int nCount)
	{
		int stashCountMax = StashCountMax;
		int stashCount = StashCount;
		if (stashCount + nCount > stashCountMax)
		{
			return stashCountMax - stashCount;
		}
		return nCount;
	}

	public void SetWeaponLevel(int nWeaponID, int nWeaponLevel)
	{
		if (!m_dictWeapon.ContainsKey(nWeaponID))
		{
			m_dictWeapon.Add(nWeaponID, nWeaponLevel);
		}
		else
		{
			m_dictWeapon[nWeaponID] = nWeaponLevel;
		}
	}

	public void UnlockWeapon(int nWeaponID)
	{
		if (!m_dictWeapon.ContainsKey(nWeaponID))
		{
			m_dictWeapon.Add(nWeaponID, -1);
		}
	}

	public void SetCharacter(int nCharID, int nLevel, int nExp)
	{
		if (!m_dictCharSaveInfo.ContainsKey(nCharID))
		{
			m_dictCharSaveInfo.Add(nCharID, new CCharSaveInfo(nCharID));
		}
		m_dictCharSaveInfo[nCharID].nLevel = nLevel;
		m_dictCharSaveInfo[nCharID].nExp = nExp;
	}

	public void UnlockCharacter(int nCharID)
	{
		if (!m_dictCharSaveInfo.ContainsKey(nCharID))
		{
			m_dictCharSaveInfo.Add(nCharID, new CCharSaveInfo(nCharID));
			m_dictCharSaveInfo[nCharID].nLevel = -1;
			m_dictCharSaveInfo[nCharID].nExp = 0;
		}
	}

	public void SetPassiveSkill(int nSkillID, int nLevel)
	{
		if (!m_dictPassiveSkill.ContainsKey(nSkillID))
		{
			m_dictPassiveSkill.Add(nSkillID, nLevel);
		}
		m_dictPassiveSkill[nSkillID] = nLevel;
	}

	public void UnlockPassiveSkill(int nSkillID)
	{
		if (!m_dictPassiveSkill.ContainsKey(nSkillID))
		{
			m_dictPassiveSkill.Add(nSkillID, -1);
		}
	}

	public void SetSkill(int nSkillID, int nLevel)
	{
		if (!m_dictSkill.ContainsKey(nSkillID))
		{
			m_dictSkill.Add(nSkillID, nLevel);
		}
		m_dictSkill[nSkillID] = nLevel;
	}

	public void SetEquipStone(int nItemID, int nLevel)
	{
		if (!m_dictEquipStone.ContainsKey(nItemID))
		{
			m_dictEquipStone.Add(nItemID, nLevel);
		}
		else
		{
			m_dictEquipStone[nItemID] = nLevel;
		}
	}

	public void UnlockEquipStone(int nItemID)
	{
		if (!m_dictEquipStone.ContainsKey(nItemID))
		{
			m_dictEquipStone.Add(nItemID, -1);
		}
	}

	public void AddGold(int nGold)
	{
		int num = m_nGold.Get();
		num += nGold;
		if (num < 0)
		{
			num = 0;
		}
		m_nGold.Set(num);
	}

	public void AddCrystal(int nCrystal)
	{
		int num = m_nCrystal.Get();
		int num2 = num + nCrystal;
		if (num2 < 0)
		{
			num2 = 0;
		}
		m_nCrystal.Set(num2);
		if (nCrystal > 0)
		{
			m_nCrystalTotalGain += nCrystal;
		}
		if (nCrystal < 0)
		{
			if (num2 == 0)
			{
				m_nCrystalTotalConsume += num;
			}
			else
			{
				m_nCrystalTotalConsume += nCrystal;
			}
		}
	}

	public void AddCrystalInBackground(int nCrystal, float fMoney, string sCombineKey)
	{
		foreach (CCrystalInBackground item in m_ltCrystalInBackground)
		{
			if (item.m_sCombineKey == sCombineKey)
			{
				return;
			}
		}
		CCrystalInBackground cCrystalInBackground = new CCrystalInBackground();
		cCrystalInBackground.m_sCombineKey = sCombineKey;
		cCrystalInBackground.m_fMoney = fMoney;
		cCrystalInBackground.m_nCrystal.Set(nCrystal);
		m_ltCrystalInBackground.Add(cCrystalInBackground);
	}

	public void ClearCrystalInBackground()
	{
		m_ltCrystalInBackground.Clear();
	}

	public void AddHunterExp(int nHunterExp)
	{
		HunterExpTotal += nHunterExp;
		if (HunterExpTotal < 0)
		{
			HunterExpTotal = 0;
		}
	}

	public void SetSelectWeapon(int nIndex, int nWeaponID)
	{
		if (nIndex >= 0 && nIndex < m_arrSelectWeapon.Length)
		{
			m_arrSelectWeapon[nIndex] = nWeaponID;
		}
	}

	public void SetSelectPassiveSkill(int nCharID, int nIndex, int nPassiveSkillID)
	{
		if (!m_dictSelectPassiveSkill.ContainsKey(nCharID))
		{
			m_dictSelectPassiveSkill.Add(nCharID, new int[3] { -1, -1, -1 });
		}
		int[] array = m_dictSelectPassiveSkill[nCharID];
		if (nIndex >= 0 && nIndex < array.Length)
		{
			array[nIndex] = nPassiveSkillID;
		}
	}

	public void UnlockNewLevelPrepare()
	{
		m_bUnLockLevel = true;
	}

	public void UnlockNewLevelConfirm(int nNewLevel)
	{
		m_bUnLockLevel = false;
		m_nLatestLevel = nNewLevel;
	}

	public bool GetWeaponSign(int nWeaponID, ref int nSignState)
	{
		if (!m_dictWeaponSign.ContainsKey(nWeaponID))
		{
			return false;
		}
		nSignState = m_dictWeaponSign[nWeaponID];
		return true;
	}

	public bool GetAvatarSign(int nAvatarID, ref int nSignState)
	{
		if (!m_dictAvatarSign.ContainsKey(nAvatarID))
		{
			return false;
		}
		nSignState = m_dictAvatarSign[nAvatarID];
		return true;
	}

	public void SetWeaponSign(int nWeaponID, int nSignState)
	{
		if (!m_dictWeaponSign.ContainsKey(nWeaponID))
		{
			m_dictWeaponSign.Add(nWeaponID, nSignState);
		}
		else
		{
			m_dictWeaponSign[nWeaponID] = nSignState;
		}
	}

	public void SetAvatarSign(int nAvatarID, int nSignState)
	{
		if (!m_dictAvatarSign.ContainsKey(nAvatarID))
		{
			m_dictAvatarSign.Add(nAvatarID, nSignState);
		}
		else
		{
			m_dictAvatarSign[nAvatarID] = nSignState;
		}
	}

	public bool GetEquipStoneSign(int nID, ref int nSignState)
	{
		if (!m_dictEquipStoneSign.ContainsKey(nID))
		{
			return false;
		}
		nSignState = m_dictEquipStoneSign[nID];
		return true;
	}

	public void SetEquipStoneSign(int nID, int nSignState)
	{
		if (!m_dictEquipStoneSign.ContainsKey(nID))
		{
			m_dictEquipStoneSign.Add(nID, nSignState);
		}
		else
		{
			m_dictEquipStoneSign[nID] = nSignState;
		}
	}

	public bool GetSkillSign(int nID, ref int nSignState)
	{
		if (!m_dictSkillSign.ContainsKey(nID))
		{
			return false;
		}
		nSignState = m_dictSkillSign[nID];
		return true;
	}

	public void SetSkillSign(int nID, int nSignState)
	{
		if (!m_dictSkillSign.ContainsKey(nID))
		{
			m_dictSkillSign.Add(nID, nSignState);
		}
		else
		{
			m_dictSkillSign[nID] = nSignState;
		}
	}

	public bool GetCharacterSign(int nID, ref int nSignState)
	{
		if (!m_dictCharacterSign.ContainsKey(nID))
		{
			return false;
		}
		nSignState = m_dictCharacterSign[nID];
		return true;
	}

	public void SetCharacterSign(int nID, int nSignState)
	{
		if (!m_dictCharacterSign.ContainsKey(nID))
		{
			m_dictCharacterSign.Add(nID, nSignState);
		}
		else
		{
			m_dictCharacterSign[nID] = nSignState;
		}
	}

	public bool IsLevelPassed(int nLevel)
	{
		foreach (CLevelSaveInfo item in m_ltLevelSaveInfo)
		{
			if (item.nID == nLevel)
			{
				return true;
			}
		}
		return false;
	}

	public void SetPassedLevel(int nLevel)
	{
		foreach (CLevelSaveInfo item in m_ltLevelSaveInfo)
		{
			if (item.nID == nLevel)
			{
				return;
			}
		}
		CLevelSaveInfo cLevelSaveInfo = new CLevelSaveInfo();
		cLevelSaveInfo.nID = nLevel;
		cLevelSaveInfo.isIgnoreCG = true;
		m_ltLevelSaveInfo.Add(cLevelSaveInfo);
	}

	public void ClearPassedLevel()
	{
		m_ltLevelSaveInfo.Clear();
	}

	public void SetLevelIgnoreCG(int nLevel, bool bIgnore)
	{
		foreach (CLevelSaveInfo item in m_ltLevelSaveInfo)
		{
			if (item.nID != nLevel)
			{
				continue;
			}
			item.isIgnoreCG = bIgnore;
			break;
		}
	}

	public bool IsLevelIgnoreCG(int nLevel)
	{
		foreach (CLevelSaveInfo item in m_ltLevelSaveInfo)
		{
			if (item.nID == nLevel)
			{
				return item.isIgnoreCG;
			}
		}
		return false;
	}

	public void AddAchiData(int nID, CAchievementData data)
	{
		if (!m_dictAchievementData.ContainsKey(nID))
		{
			m_dictAchievementData.Add(nID, data);
		}
	}

	public CAchievementData GetAchiData(int nID)
	{
		if (!m_dictAchievementData.ContainsKey(nID))
		{
			return null;
		}
		return m_dictAchievementData[nID];
	}

	public Dictionary<int, CAchievementData> GetAchiDataData()
	{
		return m_dictAchievementData;
	}

	public void AddFreeWeapon(int nWeaponID)
	{
		if (!m_ltFreeWeapon.Contains(nWeaponID))
		{
			m_ltFreeWeapon.Add(nWeaponID);
		}
	}

	public void DelFreeWeapon(int nWeaponID)
	{
		if (m_ltFreeWeapon.Contains(nWeaponID))
		{
			m_ltFreeWeapon.Remove(nWeaponID);
		}
	}

	public bool IsFreeWeaponID(int nWeaponID)
	{
		if (!m_ltFreeWeapon.Contains(nWeaponID))
		{
			return false;
		}
		return true;
	}

	private void ApplyForcedValues()
	{
		for (int i = 0; i < 3; i++) m_arrSelectWeapon[i] = m_forcedSelectWeapons[i];
		m_dictWeapon.Clear();
		foreach (var kvp in m_forcedWeaponLevels) m_dictWeapon[kvp.Key] = kvp.Value;
		AvatarHead = m_forcedAvatarHead;
		AvatarUpper = m_forcedAvatarUpper;
		AvatarLower = m_forcedAvatarLower;
		AvatarHeadup = m_forcedAvatarHeadup;
		AvatarNeck = m_forcedAvatarNeck;
		AvatarWrist = m_forcedAvatarWrist;
		AvatarBadge = m_forcedAvatarBadge;
		AvatarStone = m_forcedAvatarStone;
		m_dictAvatar.Clear();
		if (m_forcedAvatarLevels != null)
			foreach (var kvp in m_forcedAvatarLevels) m_dictAvatar[kvp.Key] = kvp.Value;
	}

	private void RestoreOriginal()
	{
		Array.Copy(m_backupSelectWeapons, m_arrSelectWeapon, 3);
		m_dictWeapon.Clear();
		foreach (var kvp in m_backupWeaponLevels) m_dictWeapon[kvp.Key] = kvp.Value;
		AvatarHead = m_backupAvatarHead;
		AvatarUpper = m_backupAvatarUpper;
		AvatarLower = m_backupAvatarLower;
		AvatarHeadup = m_backupAvatarHeadup;
		AvatarNeck = m_backupAvatarNeck;
		AvatarWrist = m_backupAvatarWrist;
		AvatarBadge = m_backupAvatarBadge;
		AvatarStone = m_backupAvatarStone;
		m_dictAvatar.Clear();
		foreach (var kvp in m_backupAvatarLevels) m_dictAvatar[kvp.Key] = kvp.Value;
	}

	public bool IsForcedLoadoutActive() => m_bForcedLoadoutActive;

	public void BeginForcedLoadout(int[] forcedSelectWeapons,
			Dictionary<int, int> forcedWeaponLevels,
			int forcedAvatarHead, int forcedAvatarUpper,
			int forcedAvatarLower, int forcedAvatarHeadup,
			int forcedAvatarNeck, int forcedAvatarWrist,
			int forcedAvatarBadge, int forcedAvatarStone,
			Dictionary<int, int> forcedAvatarLevels = null)
	{
		if (m_bForcedLoadoutActive) EndForcedLoadout();
		m_backupSelectWeapons = new int[3];
		Array.Copy(m_arrSelectWeapon, m_backupSelectWeapons, 3);
		m_backupWeaponLevels = new Dictionary<int, int>(m_dictWeapon);
		m_backupAvatarLevels = new Dictionary<int, int>(m_dictAvatar);
		m_backupAvatarHead = AvatarHead;
		m_backupAvatarUpper = AvatarUpper;
		m_backupAvatarLower = AvatarLower;
		m_backupAvatarHeadup = AvatarHeadup;
		m_backupAvatarNeck = AvatarNeck;
		m_backupAvatarWrist = AvatarWrist;
		m_backupAvatarBadge = AvatarBadge;
		m_backupAvatarStone = AvatarStone;
		m_forcedSelectWeapons = (int[])forcedSelectWeapons.Clone();
		m_forcedWeaponLevels = new Dictionary<int, int>(forcedWeaponLevels);
		m_forcedAvatarLevels = forcedAvatarLevels != null ? new Dictionary<int, int>(forcedAvatarLevels) : null;
		m_forcedAvatarHead = forcedAvatarHead;
		m_forcedAvatarUpper = forcedAvatarUpper;
		m_forcedAvatarLower = forcedAvatarLower;
		m_forcedAvatarHeadup = forcedAvatarHeadup;
		m_forcedAvatarNeck = forcedAvatarNeck;
		m_forcedAvatarWrist = forcedAvatarWrist;
		m_forcedAvatarBadge = forcedAvatarBadge;
		m_forcedAvatarStone = forcedAvatarStone;
		ApplyForcedValues();
		m_bForcedLoadoutActive = true;
	}

	public void EndForcedLoadout()
	{
		if (!m_bForcedLoadoutActive) return;
		Array.Copy(m_backupSelectWeapons, m_arrSelectWeapon, 3);
		m_dictWeapon.Clear();
		foreach (var kvp in m_backupWeaponLevels) m_dictWeapon[kvp.Key] = kvp.Value;
		AvatarHead = m_backupAvatarHead;
		AvatarUpper = m_backupAvatarUpper;
		AvatarLower = m_backupAvatarLower;
		AvatarHeadup = m_backupAvatarHeadup;
		AvatarNeck = m_backupAvatarNeck;
		AvatarWrist = m_backupAvatarWrist;
		AvatarBadge = m_backupAvatarBadge;
		AvatarStone = m_backupAvatarStone;
		m_dictAvatar.Clear();
		foreach (var kvp in m_backupAvatarLevels) m_dictAvatar[kvp.Key] = kvp.Value;
		m_forcedSelectWeapons = null;
		m_forcedWeaponLevels = null;
		m_forcedAvatarLevels = null;
		m_bForcedLoadoutActive = false;
	}

	public void RefreshServerDateTime(DateTime now)
	{
		iGameState gameState = iGameApp.GetInstance().m_GameState;
		if (gameState != null)
		{
			#if UNITY_EDITOR
			Debug.Log(string.Concat("lastday is ", m_lastLoginTime, " today is ", now));
			#endif
			gameState.m_nDaysFromLastLogin = CalcPassedDays(m_lastLoginTime, now);
			gameState.m_DayOfWeek = now.DayOfWeek;
			m_lastLoginTime = now;
			if (gameState.m_nDaysFromLastLogin > 0)
			{
				RefreshDailyRewardCount(gameState.m_nDaysFromLastLogin);
				RefreshDailyTask(now.DayOfWeek);
				m_dictWorldMonsterKill.Clear();
			}
		}
	}

	public void RefreshDailyRewardCount(int nPassDays)
	{
		#if UNITY_EDITOR
		Debug.Log("days from lastlogin = " + nPassDays);
		#endif
		if (nPassDays < 1)
		{
			return;
		}
		if (m_nDailyRewardCount == 0)
		{
			m_nDailyRewardCount = 1;
			m_nDailyRewardHasGot = 0;
		}
		else if (nPassDays == 1)
		{
			m_nDailyRewardCount++;
			if (m_nDailyRewardCount > 7)
			{
				m_nDailyRewardCount = 1;
				m_nDailyRewardHasGot = 0;
			}
		}
		else if (nPassDays > 1)
		{
			m_nDailyRewardCount = 1;
			m_nDailyRewardHasGot = 0;
		}
	}

	public void RefreshDailyTask(DayOfWeek dayofweek)
	{
		#if UNITY_EDITOR
		Debug.Log("today is " + dayofweek);
		#endif
		List<int> list = new List<int>();
		foreach (CAchievementData value in m_dictAchievementData.Values)
		{
			bool flag = false;
			for (int i = 0; i < m_ltDailyTask.Count; i++)
			{
				if (value.nID == m_ltDailyTask[i])
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				value.Reset();
				list.Add(value.nID);
			}
		}
		m_ltDailyTask.Clear();
		foreach (int item in list)
		{
			m_dictAchievementData.Remove(item);
		}
		iGameData gameData = iGameApp.GetInstance().m_GameData;
		if (gameData == null)
		{
			Debug.LogError("gamedata is null/..");
			return;
		}
		iAchievementCenter achievementCenter = gameData.GetAchievementCenter();
		if (achievementCenter == null)
		{
			Debug.LogError("gachievement is null/..");
			return;
		}
		iDailyTaskCenter dailyTaskCenter = gameData.GetDailyTaskCenter();
		if (dailyTaskCenter == null)
		{
			Debug.LogError("dauily Task is null/..");
			return;
		}
		CDailyTaskInfo cDailyTaskInfo = dailyTaskCenter.Get((int)dayofweek);
		if (cDailyTaskInfo == null || cDailyTaskInfo.ltTask == null || cDailyTaskInfo.ltTask.Count < 1)
		{
			Debug.LogError("dailyrask is null/..");
			return;
		}
		List<CAchievementInfo> dailyAchievementList = achievementCenter.GetDailyAchievementList();
		if (dailyAchievementList == null || dailyAchievementList.Count < 1)
		{
			return;
		}
		foreach (int item2 in cDailyTaskInfo.ltTask)
		{
			List<CAchievementInfo> list2 = new List<CAchievementInfo>();
			foreach (CAchievementInfo item3 in dailyAchievementList)
			{
				if (item3 != null && item2 == item3.nType)
				{
					list2.Add(item3);
				}
			}
			if (list2.Count >= 1)
			{
				int index = UnityEngine.Random.Range(0, list2.Count);
				m_ltDailyTask.Add(list2[index].nID);
				dailyAchievementList.Remove(list2[index]);
			}
		}
		foreach (CAchievementData value2 in m_dictAchievementData.Values)
		{
			bool flag2 = false;
			for (int j = 0; j < m_ltDailyTask.Count; j++)
			{
				if (value2.nID == m_ltDailyTask[j])
				{
					flag2 = true;
					break;
				}
			}
			if (flag2)
			{
				value2.Reset();
			}
		}
	}

	protected int CalcPassedDays(DateTime date1, DateTime date2)
	{
		DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0);
		TimeSpan timeSpan = date1 - dateTime;
		return (date2 - dateTime).Days - timeSpan.Days;
	}

	public int GetWorldMonsterKill(int nMobID)
	{
		if (!m_dictWorldMonsterKill.ContainsKey(nMobID))
		{
			return 0;
		}
		return m_dictWorldMonsterKill[nMobID];
	}

	public void SetWorldMonsterKill(int nMobID, int nNum)
	{
		if (!m_dictWorldMonsterKill.ContainsKey(nMobID))
		{
			m_dictWorldMonsterKill.Add(nMobID, nNum);
		}
		else
		{
			m_dictWorldMonsterKill[nMobID] = nNum;
		}
	}

	public void AddWorldMonsterKill(int nMobID, int nNum)
	{
		SetWorldMonsterKill(nMobID, GetWorldMonsterKill(nMobID) + nNum);
	}

	public bool GenerateNameCard(ref CNameCardInfo namecardinfo)
	{
		if (namecardinfo == null)
		{
			return false;
		}
		namecardinfo.m_sID = iServerSaveData.GetInstance().CurDeviceId;
		namecardinfo.m_sGCAccount = iServerSaveData.GetInstance().CurGameCenterId;
		namecardinfo.m_sNickName = m_sNickName;
		namecardinfo.m_nTitle = m_nTitle.Get();
		namecardinfo.m_nHunterLvl = m_nHunterLvl.Get();
		namecardinfo.m_nHunterExp = m_nHunterExp.Get();
		namecardinfo.m_nCombatPower = CombatPower;
		namecardinfo.m_nRank = m_nRank;
		namecardinfo.m_nBeAdmired = m_nBeAdmired.Get();
		namecardinfo.m_nGold = m_nGold.Get();
		namecardinfo.m_nCrystal = m_nCrystal.Get();
		namecardinfo.m_fSceneProccess = m_fSceneProccess;
		namecardinfo.m_sSignature = m_sSignature;
		namecardinfo.SetPhoto(m_Photo);
		namecardinfo.m_NCPack.roleid = m_nCurCharID;
		namecardinfo.m_NCPack.head = AvatarHead;
		namecardinfo.m_NCPack.upper = AvatarUpper;
		namecardinfo.m_NCPack.lower = AvatarLower;
		namecardinfo.m_NCPack.headup = AvatarHeadup;
		namecardinfo.m_NCPack.neck = AvatarNeck;
		namecardinfo.m_NCPack.bracelet = AvatarWrist;
		namecardinfo.m_NCPack.weapon = m_arrSelectWeapon;
		return true;
	}

	public void AddFriend(string sId)
	{
		if (sId != null && sId.Length >= 1 && !m_ltFriends.Contains(sId))
		{
			m_ltFriends.Add(sId);
		}
	}

	public void DelFriend(string sId)
	{
		if (sId != null && sId.Length >= 1)
		{
			m_ltFriends.Remove(sId);
		}
	}

	public List<string> GetFriends()
	{
		m_ltFriends = m_ltFriends.Distinct().ToList();
		return m_ltFriends;
	}

	public void ClearFriends()
	{
		m_ltFriends.Clear();
	}

	public bool IsFriend(string sId)
	{
		if (sId == null || sId.Length < 1)
		{
			return false;
		}
		if (m_ltFriends.Contains(sId))
		{
			return true;
		}
		return false;
	}

	public void AddTitle(int nID)
	{
		if (!m_ltTitle.Contains(nID))
		{
			m_ltTitle.Add(nID);
		}
	}

	public void DelTitle(int nID)
	{
		if (m_ltTitle.Contains(nID))
		{
			m_ltTitle.Remove(nID);
		}
	}

	public bool GetTitle(int nID)
	{
		return m_ltTitle.Contains(nID);
	}

	public List<int> GetTitleList()
	{
		return m_ltTitle;
	}

	public void AddKillMonster(int nID, int nCount = 1)
	{
		if (!m_dictKillMonster.ContainsKey(nID))
		{
			m_dictKillMonster.Add(nID, nCount);
			return;
		}
		Dictionary<int, int> dictKillMonster;
		Dictionary<int, int> dictionary = (dictKillMonster = m_dictKillMonster);
		int key;
		int key2 = (key = nID);
		key = dictKillMonster[key];
		dictionary[key2] = key + nCount;
	}

	public void SetkillMonster(int nID, int nCount)
	{
		if (!m_dictKillMonster.ContainsKey(nID))
		{
			m_dictKillMonster.Add(nID, nCount);
		}
		else
		{
			m_dictKillMonster[nID] = nCount;
		}
	}

	public int GetKillMonster(int nID)
	{
		if (!m_dictKillMonster.ContainsKey(nID))
		{
			return 0;
		}
		return m_dictKillMonster[nID];
	}

	public Dictionary<int, int> GetAvatarData()
	{
		return m_dictAvatar;
	}

	public bool GetAvatar(int avatarid, ref int avatarlevel)
	{
		if (!m_dictAvatar.ContainsKey(avatarid))
		{
			return false;
		}
		avatarlevel = m_dictAvatar[avatarid];
		return true;
	}

	public void SetAvatar(int avatarid, int avatarlevel)
	{
		if (!m_dictAvatar.ContainsKey(avatarid))
		{
			m_dictAvatar.Add(avatarid, avatarlevel);
		}
		else
		{
			m_dictAvatar[avatarid] = avatarlevel;
		}
	}

	public void SetPhoto(Texture2D texture)
	{
		if (!(texture == null))
		{
			Texture2D texture2D = texture;
			if (texture2D.width > 40 || texture2D.height > 40)
			{
				texture2D = gyLoadImage.Resize(texture2D, 40, 40);
			}
			m_Photo = texture2D.EncodeToPNG();
		}
	}

	public void SetPhoto(byte[] photo)
	{
		if (photo == null)
		{
			return;
		}
		try
		{
			Texture2D texture2D = new Texture2D(40, 40);
			texture2D.LoadImage(photo);
			if (texture2D.width > 40 || texture2D.height > 40)
			{
				texture2D = gyLoadImage.Resize(texture2D, 40, 40);
			}
			m_Photo = texture2D.EncodeToPNG();
		}
		catch
		{
			m_Photo = null;
		}
	}

	public byte[] GetPhoto()
	{
		return m_Photo;
	}

	public List<CCrystalInBackground> GetCrystalInBackground()
	{
		return m_ltCrystalInBackground;
	}
	
	public void ClampToLimits()
	{
		if (m_nGold.Get() > 150000)
			m_nGold.Set(150000);

		if (m_nCrystal.Get() > 2000)
			m_nCrystal.Set(2000);
	}
}