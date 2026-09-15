using System.Collections.Generic;
using gyEvent;
using UnityEngine;

public class CCharBoss : CCharMob
{
	public const float kBlackTriggerHPRatio = 0.1f;

	public class CBodyPart
	{
		public int nPartID;
		public List<kAnimEnum> m_ltAnim;
		public float m_fDmgRate;

		public kAnimEnum GetAnimRadnom()
		{
			if (m_ltAnim == null || m_ltAnim.Count < 1)
			{
				return kAnimEnum.None;
			}
			return m_ltAnim[Random.Range(0, m_ltAnim.Count)];
		}
	}

	protected Dictionary<int, CBodyPart> m_dictBodyPart;

	protected float m_bossHardinessCur;

	protected float m_bossHardinessMax;

	protected kAnimEnum m_lastHurtAnim = kAnimEnum.None;

	protected CAIManagerInfo m_curAIManager;

	protected List<CAITriggerInfo> m_curTriggerList;

	protected List<CAITriggerInfo> m_tmpTriggerList;

	protected CAITriggerInfo m_curTrigger;

	protected float m_fLifeTime;

	protected int m_nChangeAI;

	protected List<GameObject> m_ltBodyEffect;

	protected bool m_bInBlack;

	protected float m_fCurBlackLife;

	protected float m_fMaxBlackLife;

	protected Renderer m_BlackGearRenderer;

	protected bool m_bBlackThresholdHit;

	public int ChangeAI
	{
		get
		{
			return m_nChangeAI;
		}
		set
		{
			m_nChangeAI = value;
		}
	}

	public bool isInBlack
	{
		get
		{
			return m_bInBlack;
		}
	}

	public Dictionary<int, CBodyPart> GetBodyPart()
	{
		return m_dictBodyPart;
	}

	public new void Awake()
	{
		base.Awake();
		m_nType = kCharType.Boss;
		m_dictBodyPart = new Dictionary<int, CBodyPart>();
		m_curAIManager = null;
		m_curTriggerList = new List<CAITriggerInfo>();
		m_tmpTriggerList = new List<CAITriggerInfo>();
		m_curTrigger = null;
		m_fLifeTime = 0f;
		m_bShowTime = false;
		m_ltBodyEffect = new List<GameObject>();
		m_nChangeAI = -1;
		m_bBlackThresholdHit = false;
	}

	public new void Start()
	{
		base.Start();
	}

	public new void Update()
	{
		base.Update();
		m_fLifeTime += Time.deltaTime;
		UpdateAITrigger(Time.deltaTime);
		if (m_nChangeAI <= 0)
		{
			return;
		}
		m_bShowTime = false;
		SetAI(m_nChangeAI);
		if (m_GameScene.m_MGManager != null)
		{
			CEventManager eventManager = m_GameScene.m_MGManager.GetEventManager();
			if (eventManager != null)
			{
				eventManager.Trigger(new EventCondition_MobByWave(GenerateWaveID, GenerateSequence, 2, m_nChangeAI));
				eventManager.Trigger(new EventCondition_MobByID(ID, 2, m_nChangeAI));
			}
		}
		m_nChangeAI = -1;
	}

	public override void InitMob(int nMobID, int nMobLevel)
	{
		base.InitMob(nMobID, nMobLevel);
		if (m_GameScene.CurGameLevelInfo != null && m_GameScene.CurGameLevelInfo.sSceneName == "SceneSnow")
		{
			iStepEffectLeft component = m_ModelEntity.GetComponent<iStepEffectLeft>();
			if (component != null)
			{
				component.nPrefabID = 1914;
			}
			iStepEffectRight component2 = m_ModelEntity.GetComponent<iStepEffectRight>();
			if (component2 != null)
			{
				component2.nPrefabID = 1914;
			}
		}
		if (m_BlackGear != null)
		{
			m_BlackGear.gameObject.SetActive(false);
			m_BlackGearRenderer = m_BlackGear.GetComponent<Renderer>();
		}
	}

	public override void Destroy()
	{
		ClearBodyEffect();
		base.Destroy();
	}

	protected virtual float GetHardinessContributionMultiplier(int nPartID)
	{
		switch (nPartID)
		{
			case 1:
				return 1.1f;
			case 3:
				return 0.95f;
			case 2:
			default:
				return 1.0f;
		}
	}

	protected virtual kAnimEnum GetHurtAnimForPart(int nPartID)
	{
		switch (nPartID)
		{
			case 1:
				return ResolveHurtAnim(kAnimEnum.Mob_Hurt_Head);
			case 3:
				return ResolveHurtAnim(kAnimEnum.Mob_Hurt_Leg);
			case 2:
			default:
				return kAnimEnum.Mob_Hurt;
		}
	}

	protected kAnimEnum ResolveHurtAnim(kAnimEnum preferred)
	{
		if (IsHurtAnimValid(preferred))
		{
			return preferred;
		}
		return kAnimEnum.Mob_Hurt;
	}

	protected bool IsHurtAnimValid(kAnimEnum anim)
	{
		if (anim == kAnimEnum.None)
		{
			return false;
		}
		if (m_AnimManager == null)
		{
			return false;
		}
		return m_AnimManager.GetAnimLen(anim) > 0f;
	}

	public override void InitHardiness(int nMobID, int nMobLevel)
	{
		CMobInfoLevel mobInfo = m_GameData.GetMobInfo(nMobID, nMobLevel);
		if (mobInfo == null)
		{
			return;
		}
		m_dictBodyPart.Clear();
		float baseHardiness = mobInfo.fHardiness;
		if (baseHardiness <= 0f && mobInfo.ltHardinessInfo != null && mobInfo.ltHardinessInfo.Count > 0)
		{
			baseHardiness = mobInfo.ltHardinessInfo[0].fHardiness;
		}
		m_bossHardinessMax = baseHardiness;
		m_bossHardinessCur = baseHardiness;
		if (mobInfo.ltHardinessInfo != null && mobInfo.ltHardinessInfo.Count > 0)
		{
			foreach (CHardinessInfo item in mobInfo.ltHardinessInfo)
			{
				CBodyPart part = new CBodyPart();
				part.nPartID = item.nPartID;
				part.m_ltAnim = new List<kAnimEnum>();
				part.m_ltAnim.Add(GetHurtAnimForPart(item.nPartID));
				part.m_fDmgRate = 100f;
				m_dictBodyPart[item.nPartID] = part;
			}
		}
		else
		{
			for (int i = 1; i <= 3; i++)
			{
				CBodyPart part = new CBodyPart();
				part.nPartID = i;
				part.m_ltAnim = new List<kAnimEnum>();
				part.m_ltAnim.Add(GetHurtAnimForPart(i));
				part.m_fDmgRate = 100f;
				m_dictBodyPart[i] = part;
			}
		}
	}

	public override void OnDead(kDeadMode nDeathMode)
	{
		base.OnDead(nDeathMode);
		if (m_GameData.m_DataCenter != null)
		{
			m_GameData.m_DataCenter.AddKillMonster(ID);
		}
	}

	protected bool AddHardinessValue(CBodyPart info, float fValue)
	{
		if (info == null)
		{
			return false;
		}
		float multiplier = GetHardinessContributionMultiplier(info.nPartID);
		float change = fValue * (info.m_fDmgRate / 100f) * multiplier;
		m_bossHardinessCur += change;
		kAnimEnum newAnim = info.GetAnimRadnom();
		if (newAnim != kAnimEnum.None)
		{
			m_HurtAnim = newAnim;
		}
		bool broken = false;
		if (m_bossHardinessCur <= 0f)
		{
			m_bossHardinessCur = m_bossHardinessMax;
			broken = true;
		}
		if (m_bossHardinessCur > m_bossHardinessMax)
		{
			m_bossHardinessCur = m_bossHardinessMax;
		}
		return broken;
	}

	protected void UpdateAITrigger(float deltaTime)
	{
		if (m_nChangeAI != -1 || m_curTriggerList == null)
		{
			return;
		}
		m_tmpTriggerList.Clear();
		foreach (CAITriggerInfo curTrigger in m_curTriggerList)
		{
			if (curTrigger.nAI == m_nCurAIID)
			{
				continue;
			}
			switch (curTrigger.nType)
			{
				case 1:
					if (MyUtils.Compare(curTrigger.nValue, curTrigger.nOprate, m_fLifeTime, 0f) && (m_curTrigger == null || m_curTrigger.nPriority < curTrigger.nPriority))
					{
						m_tmpTriggerList.Add(curTrigger);
					}
					break;
				case 2:
					if (MyUtils.Compare(curTrigger.nValue, curTrigger.nOprate, m_fHP, m_fHPMax) && (m_curTrigger == null || m_curTrigger.nPriority < curTrigger.nPriority))
					{
						m_tmpTriggerList.Add(curTrigger);
					}
					break;
			}
		}
		if (m_tmpTriggerList.Count == 0)
		{
			return;
		}
		if (m_tmpTriggerList.Count == 1)
		{
			m_curTrigger = m_tmpTriggerList[0];
		}
		else
		{
			for (int i = 0; i < m_tmpTriggerList.Count; i++)
			{
				if (i == 0)
				{
					m_curTrigger = m_tmpTriggerList[i];
				}
				else if (m_curTrigger.nPriority < m_tmpTriggerList[i].nPriority)
				{
					m_curTrigger = m_tmpTriggerList[i];
				}
			}
		}
		if (m_curTrigger != null)
		{
			m_nChangeAI = m_curTrigger.nAI;
		}
	}

	public override void InitAI(int nAIManager)
	{
		base.InitAI(nAIManager);
		CAIManagerInfo aIManagerInfo = m_GameData.GetAIManagerInfo(nAIManager);
		if (aIManagerInfo == null)
		{
			return;
		}
		m_curTriggerList.Clear();
		foreach (CAITriggerInfo item in aIManagerInfo.ltAITrigger)
		{
			m_curTriggerList.Add(item);
		}
	}

	protected override void OnEnterAI(int nLastAI, int nAI)
	{
		CAIInfo aIInfo = m_GameData.GetAIInfo(nAI);
		if (aIInfo == null)
		{
			return;
		}
		ClearBodyEffect();
		for (int i = 0; i < aIInfo.ltEffect.Count; i++)
		{
			switch (aIInfo.ltEffect[i])
			{
				case 1500:
					AddBodyEffect(1500, GetBone(8));
					AddBodyEffect(1500, GetBone(9));
					break;
				case 1501:
					AddBodyEffect(1501, GetBone(2));
					break;
			}
		}
	}

	protected void ClearBodyEffect()
	{
		foreach (GameObject item in m_ltBodyEffect)
		{
			Object.Destroy(item);
		}
		m_ltBodyEffect.Clear();
	}

	protected void AddBodyEffect(int nPrefabID, Transform node)
	{
		if (node == null)
		{
			return;
		}
		GameObject gameObject = PrefabManager.Get(nPrefabID);
		if (!(gameObject == null))
		{
			GameObject gameObject2 = (GameObject)Object.Instantiate(gameObject);
			if (!(gameObject2 == null))
			{
				gameObject2.transform.parent = node;
				gameObject2.transform.localPosition = Vector3.zero;
				gameObject2.transform.localRotation = Quaternion.identity;
				m_ltBodyEffect.Add(gameObject2);
			}
		}
	}

	public void SetReadyToBlack(bool bReadyToBlack, float fBlackLife = 0f)
	{
		m_bReadyToBlack = bReadyToBlack;
		m_fReadyToBlackLife = fBlackLife;
		if (bReadyToBlack)
		{
			SetBlack(true, fBlackLife);
		}
	}

	public override void ResetMob()
	{
		base.ResetMob();
		m_bInBlack = false;
		m_bBlackThresholdHit = false;
		m_bReadyToBlack = false;
		m_fCurBlackLife = 0f;
		m_fMaxBlackLife = 0f;
		m_nChangeAI = -1;
		m_fLifeTime = 0f;
		m_bossHardinessCur = 0f;
		m_bossHardinessMax = 0f;
		m_lastHurtAnim = kAnimEnum.None;
		ClearBodyEffect();
		m_curTriggerList.Clear();
		m_tmpTriggerList.Clear();
		m_curTrigger = null;
		DisableBlackGear();
	}

	public override void AddHP(float fHP)
	{
		base.AddHP(fHP);
		TryTriggerBlack();
	}

	public void DisableBlackGear()
	{
		if (m_BlackGear != null)
			m_BlackGear.gameObject.SetActive(false);
	}

	protected void TryTriggerBlack()
	{
		if (m_bBlackThresholdHit || isInBlack || m_bReadyToBlack || MaxHP <= 0f)
		{
			return;
		}
		if (CurHP / MaxHP > kBlackTriggerHPRatio)
		{
			return;
		}
		CMobInfoLevel curMobInfo = GetMobInfo();
		if (curMobInfo == null)
		{
			return;
		}
		if (!curMobInfo.nHasArmor && !m_GameScene.m_bMutiplyGame)
		{
			return;
		}
		m_bBlackThresholdHit = true;
		ResetAI();
	}

	public void SetBlack(bool bBlack, float fBlackLife = 0f)
	{
		if (m_bInBlack == bBlack)
		{
			return;
		}
		m_bInBlack = bBlack;
		m_fCurBlackLife = fBlackLife;
		m_fMaxBlackLife = fBlackLife;
		if (bBlack)
		{
			SetLifeBarStyle(1, 1f);
			if (m_BlackGear != null)
			{
				m_BlackGear.gameObject.SetActive(true);
				CUISound.GetInstance().Play("UI_Armor_activate");
			}
			if (m_GameScene.m_nBlackMonsterCount == 0)
			{
				iGameUIBase gameUI = m_GameScene.GetGameUI();
				if (gameUI != null)
				{
					gameUI.ShowBlackWarning(true);
				}
			}
			m_GameScene.m_nBlackMonsterCount++;
			return;
		}
		m_bBlackThresholdHit = false;
		SetLifeBarStyle(0, CurHP / MaxHP);
		if (m_BlackGear != null)
		{
			m_BlackGear.gameObject.SetActive(false);
			CUISound.GetInstance().Play("UI_Armor_deactivate");
			CUISound.GetInstance().Play("UI_Armor_destruction");
		}
		Transform bone = GetBone(2);
		if (bone != null)
		{
			m_GameScene.AddEffect(bone.position, Dir2D, 2f, 1952);
		}
		m_GameScene.m_nBlackMonsterCount--;
		if (m_GameScene.m_nBlackMonsterCount < 0)
		{
			m_GameScene.m_nBlackMonsterCount = 0;
		}
		if (m_GameScene.m_nBlackMonsterCount == 0)
		{
			iGameUIBase gameUI2 = m_GameScene.GetGameUI();
			if (gameUI2 != null)
			{
				gameUI2.ShowBlackWarning(false);
			}
		}
	}

	public void AddBlackDmg(float fDmg)
	{
		if (m_bInBlack)
		{
			m_fCurBlackLife += fDmg;
			if (m_LifeBar != null)
			{
				m_LifeBar.SetLife(m_fCurBlackLife / m_fMaxBlackLife);
			}
			if (m_fCurBlackLife <= 0f)
			{
				SetBlack(false);
			}
			else if (m_fCurBlackLife > m_fMaxBlackLife)
			{
				m_fCurBlackLife = m_fMaxBlackLife;
			}
		}
	}
}