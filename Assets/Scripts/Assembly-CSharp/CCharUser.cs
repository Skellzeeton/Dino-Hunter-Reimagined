using gyAchievementSystem;
using gyEvent;
using gyTaskSystem;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class CCharUser : CCharPlayer
{
	protected float m_fCurSpeedMax;
	protected float m_fCurSpeedSideMax;
	protected float m_fCurSpeed;
	protected float m_fCurSpeedSide;
	protected kCharMoveState m_CharMoveState;
	protected Vector2 m_v2MoveDir;
	protected Vector2 m_v2Move;
	protected bool m_bUpdatePos;
	protected bool m_bUpdateDir;
	protected float m_fUpdatePosTime;
	protected float m_fUpdateDirTime;
	protected float m_fSkillCD;
	protected float m_fSkillCDcount;
	protected int m_nCurWeaponIndex;
	protected bool m_bCarryTaskItem;
	protected bool m_bWasMoving = false;
	protected float m_fAccelerationTimer = 0f;
	protected const float kAccelerationTime = 0.66f;
	protected float m_fTurnSlowMaxPercent = 0.2f;
	protected float m_fTurnAngularFull = 180f;
	protected float m_turnDeadzone = 2.25f;
	protected float m_turnCurve = 0.1f;
	protected float m_yawWeight = 0.3f;
	protected float m_moveWeight = 0.3f;
	private float m_lastYaw = 0f;
	private float m_prevTurnPenalty = 0f;
	private Vector2 m_lastMoveDir = Vector2.zero;
	private float m_angSpeedSmoothed = 0f;
	private float m_moveAngleSpeedSmoothed = 0f;
	protected float m_angSmoothRate = 8f;
	protected float m_moveSmoothRate = 8f;
	protected float m_fMoveInputPower = 0f;
	protected float m_fCurMoveFrameDistance = 0f;
	
	public CharacterController m_Controller { get; private set; }

	public override Vector3 ShootDir
	{
		get
		{
			iCameraTrail iCameraTrail2 = m_GameScene.GetCamera();
			if (iCameraTrail2 == null)
			{
				return Vector3.forward;
			}
			return iCameraTrail2.transform.forward;
		}
	}

	public float CurSkillCD
	{
		get
		{
			return m_fSkillCD;
		}
	}

	public int CurWeaponIndex
	{
		get
		{
			return m_nCurWeaponIndex;
		}
		set
		{
			m_nCurWeaponIndex = value;
		}
	}

	public new void Awake()
	{
		base.Awake();
		m_nType = kCharType.User;
		m_curMoveDir = kCharMoveDir.None;
		m_fCurSpeedMax = 0f;
		m_fCurSpeedSideMax = 0f;
		m_fCurSpeed = 0f;
		m_fCurSpeedSide = 0f;
		m_CharMoveState = kCharMoveState.None;
		m_v2MoveDir = Vector2.zero;
		m_v2Move = Vector2.zero;
		m_Controller = m_Model.GetComponent<CharacterController>();
		if (m_Controller == null)
		{
			m_Controller = m_Model.AddComponent<CharacterController>();
		}
		m_Controller.slopeLimit = 30f;
		m_Controller.stepOffset = 0.4f;
		m_Controller.center = new Vector3(0f, 1f, 0f);
		m_bCarryTaskItem = false;
		if (m_ModelTransform != null)
			m_lastYaw = m_ModelTransform.eulerAngles.y;
		else
			m_lastYaw = 0f;
		
		if (this.GetType().GetField("m_fYaw") != null)
		{
			try { m_lastYaw = (float)this.GetType().GetField("m_fYaw").GetValue(this); } catch { m_lastYaw = (m_ModelTransform != null ? m_ModelTransform.eulerAngles.y : 0f); }
		}
		else
		{
			m_lastYaw = (m_ModelTransform != null ? m_ModelTransform.eulerAngles.y : 0f);
		}

		m_lastMoveDir = Vector2.zero;
		m_angSpeedSmoothed = 0f;
		m_moveAngleSpeedSmoothed = 0f;

	}

	public new void Start()
	{
		base.Start();
	}

	public override void Destroy()
	{
		base.Destroy();
	}

	public new void Update()
	{
		float deltaTime = Time.deltaTime;
		base.Update();
		if (base.isDead)
		{
			return;
		}
		UpdateAnimation();
		if (m_curWeapon != null)
		{
			m_curWeapon.Update(this, deltaTime);
		}
		iGameUIBase gameUI = base.m_GameScene.GetGameUI();
		if (gameUI != null)
		{
			gyUISkillButton skillCDButton = gameUI.GetSkillCDButton();
			if (skillCDButton != null && base.m_GameScene.isPause != skillCDButton.m_bPause)
			{
				skillCDButton.m_bPause = base.m_GameScene.isPause;
			}
		}
		if (!base.m_GameScene.isPause && m_fSkillCDcount < m_fSkillCD)
		{
			m_fSkillCDcount += deltaTime;
			if (m_fSkillCDcount > m_fSkillCD)
			{
				m_fSkillCDcount = m_fSkillCD;
			}
		}
		if (!CGameNetManager.GetInstance().IsConnected())
		{
			return;
		}
		if (m_bUpdatePos)
		{
			m_fUpdatePosTime -= deltaTime;
			if (m_fUpdatePosTime <= 0f)
			{
				m_fUpdatePosTime = 0.2f;
				m_bUpdatePos = false;
				CGameNetSender.GetInstance().SendMsg_PLAYER_MOVE(base.Pos, base.Pos);
			}
		}
		if (!m_bUpdateDir)
		{
			return;
		}
		m_fUpdateDirTime -= deltaTime;
		if (m_fUpdateDirTime <= 0f)
		{
			m_fUpdateDirTime = 0.2f;
			m_bUpdateDir = false;
			RaycastHit hitInfo;
			if (Physics.Raycast(new Ray(GetShootMouse(), ShootDir), out hitInfo, 1000f, -1610612736))
			{
				CGameNetSender.GetInstance().SendMsg_PLAYER_AIM(hitInfo.point);
			}
		}
	}

public new void FixedUpdate()
{
    base.FixedUpdate();
    float deltaTime = Time.deltaTime;
    if (m_CharMoveState == kCharMoveState.None)
        return;
    float currentYaw = 0f;
    var t = this.GetType();
    var yawField = t.GetField("m_fYaw");
    if (yawField != null)
    {
        try { currentYaw = (float)yawField.GetValue(this); }
        catch { currentYaw = (m_ModelTransform != null ? m_ModelTransform.eulerAngles.y : 0f); }
    }
    else
    {
        currentYaw = (m_ModelTransform != null ? m_ModelTransform.eulerAngles.y : 0f);
    }
    float deltaYaw = Mathf.DeltaAngle(m_lastYaw, currentYaw);
    float yawAngSpeed = 0f;
    if (deltaTime > 0f) yawAngSpeed = Mathf.Abs(deltaYaw) / deltaTime;
    float yawLerp = Mathf.Clamp01(m_angSmoothRate * deltaTime);
    m_angSpeedSmoothed = Mathf.Lerp(m_angSpeedSmoothed, yawAngSpeed, yawLerp);
    Vector2 currentMoveDir = m_v2MoveDir;
    float moveAngleDeg = 0f;
    if (currentMoveDir.sqrMagnitude > 0.0001f && m_lastMoveDir.sqrMagnitude > 0.0001f)
    {
        moveAngleDeg = Vector2.Angle(m_lastMoveDir, currentMoveDir);
    }
    else
    {
        moveAngleDeg = 0f;
    }
    float moveAngSpeed = 0f;
    if (deltaTime > 0f) moveAngSpeed = Mathf.Abs(moveAngleDeg) / deltaTime;
    float moveLerp = Mathf.Clamp01(m_moveSmoothRate * deltaTime);
    m_moveAngleSpeedSmoothed = Mathf.Lerp(m_moveAngleSpeedSmoothed, moveAngSpeed, moveLerp);
    if (currentMoveDir.sqrMagnitude > 0.000001f)
        m_lastMoveDir = currentMoveDir;
    float effectiveAngSpeed = m_yawWeight * m_angSpeedSmoothed + m_moveWeight * m_moveAngleSpeedSmoothed;
    if (effectiveAngSpeed <= m_turnDeadzone)
    {
        effectiveAngSpeed = 0f;
    }
    else
    {
        effectiveAngSpeed = Mathf.Max(0f, effectiveAngSpeed - m_turnDeadzone);
    }
    float denom = Mathf.Max(0.0001f, (m_fTurnAngularFull - m_turnDeadzone));
    float rawRatio = Mathf.Clamp01(effectiveAngSpeed / denom);
    float curved = Mathf.Pow(rawRatio, m_turnCurve);
    float penalty = curved * m_fTurnSlowMaxPercent;
    float baseSpeed = m_Property.GetValue(kProEnum.MoveSpeed);
    if (baseSpeed < 0f) baseSpeed = 0f;
    float desiredMax = baseSpeed * (1f - penalty);
    if (penalty > m_prevTurnPenalty + 0.001f)
    {
        m_fCurSpeedMax = desiredMax;
        m_fCurSpeedSideMax = desiredMax;

        if (m_fCurSpeed > m_fCurSpeedMax) m_fCurSpeed = m_fCurSpeedMax;
        if (m_fCurSpeedSide > m_fCurSpeedSideMax) m_fCurSpeedSide = m_fCurSpeedSideMax;
    }
    else
    {
        m_fCurSpeedMax = desiredMax;
        m_fCurSpeedSideMax = desiredMax;

        float accelRate = (kAccelerationTime > 0f) ? (baseSpeed / kAccelerationTime) : baseSpeed;

        m_fCurSpeed = Mathf.MoveTowards(m_fCurSpeed, m_fCurSpeedMax, accelRate * deltaTime);
        m_fCurSpeedSide = Mathf.MoveTowards(m_fCurSpeedSide, m_fCurSpeedSideMax, accelRate * deltaTime);
    }
    m_prevTurnPenalty = penalty;
    m_lastYaw = currentYaw;
    float num = 0f;
    if (m_Property.GetValue(kProEnum.Char_MSEquip_Off) == 0f && m_curWeapon != null && m_curWeaponLvlInfo != null)
    {
	    num = m_curWeaponLvlInfo.fMSDownRateEquip;
	    if (m_curWeapon.IsFire())
	    {
		    num += m_curWeaponLvlInfo.fMSDownRateShoot;
	    }
    }
    float num2 = m_Property.GetValue(kProEnum.Char_MoveSpeedUp);
    if (num2 < 0f) num2 = 0f;
    float inputPower = Mathf.Clamp01(m_fMoveInputPower);
    float moveMultiplier = 1f - num;
    if (moveMultiplier < 0f)
	    moveMultiplier = 0f;
    float finalSpeed = (Mathf.Min(m_fCurSpeed, m_fCurSpeedSide) + num2);
    finalSpeed *= moveMultiplier;
    Vector2 finalMoveDir = m_v2MoveDir.normalized * inputPower;
    Vector2 velocity = finalMoveDir * finalSpeed;
    m_v2Move = velocity * deltaTime;
    m_fCurMoveFrameDistance = velocity.magnitude;
    Vector3 zero = Vector3.zero;
    zero += m_ModelTransform.forward * m_v2Move.y;
    zero += m_ModelTransform.right * m_v2Move.x;
    zero.y = -20f * deltaTime;
    CollisionFlags collisionFlags = m_Controller.Move(zero);
    m_bUpdatePos = true;
}



	public new void LateUpdate()
	{
		base.LateUpdate();
		m_ModelTransform.rotation = Quaternion.Euler(0f - m_fPitch, m_fYaw, m_fRoll);
	}

	private void OnControllerColliderHit(ControllerColliderHit hit)
	{
		iItem component = hit.transform.root.GetComponent<iItem>();
		if (!(component == null) && component.ToughItem(this))
		{
			component.Destroy();
		}
	}

	public void MoveByCompass(float fRateX, float fRateY)
	{
		Vector2 input = new Vector2(fRateX, fRateY);
		float inputPower = Mathf.Clamp01(input.magnitude);
		if (inputPower <= 0.001f)
		{
			MoveStop();
			return;
		}
		if (m_v2MoveDir.sqrMagnitude == 0f)
		{
			m_fAccelerationTimer = 0f;
			m_CharMoveState = kCharMoveState.Acc;
		}
		m_v2MoveDir = input.normalized;
		m_fMoveInputPower = inputPower;
		m_bWasMoving = true;
		UpdateMoveAnim(m_v2MoveDir);
	}

	
	public void MoveStop()
	{
		if (m_CharMoveState != kCharMoveState.None)
		{
			m_bUpdatePos = false;
			if (CGameNetManager.GetInstance().IsConnected())
			{
				CGameNetSender.GetInstance().SendMsg_PLAYER_MOVESTOP(base.Pos);
			}
		}
		m_CharMoveState = kCharMoveState.None;
		m_fCurSpeedMax = 0f;
		m_fCurSpeedSideMax = 0f;
		m_fCurSpeed = 0f;
		m_fCurSpeedSide = 0f;
		m_fMoveInputPower = 0f;
		m_fCurMoveFrameDistance = 0f;
		m_v2MoveDir = Vector2.zero;
		m_fAccelerationTimer = 0f;
		StopMoveAnim();
		m_bWasMoving = false;
	}


	protected void UpdateAnimation()
	{
		float step = m_AnimData.GetStep(kAnimEnum.MoveForward);
		if (step > 0f)
		{
			m_AnimManager.SetAnimSpeed(kAnimEnum.MoveForward, m_AnimManager.GetAnimLen(kAnimEnum.MoveForward) / (step / m_v2Move.magnitude));
		}
	}

	public override void InitChar(int nCharID, int nLevel, int nExp = 0, int nAvatarHead = -1, int nAvatarUpper = -1, int nAvatarLower = -1, int nAvatarHeadup = -1, int nAvatarNeck = -1, int nAvatarWrist = -1, int nAvatarBadge = -1, int nAvatarStone = -1)
	{
		iDataCenter dataCenter = base.m_GameData.GetDataCenter();
		if (dataCenter != null)
		{
			for (int i = 0; i < m_arrCarryPassiveSkill.Length; i++)
			{
				int selectPassiveSkill = dataCenter.GetSelectPassiveSkill(nCharID, i);
				if (selectPassiveSkill > 0)
				{
					int nSkillLevel = 0;
					if (dataCenter.GetPassiveSkill(selectPassiveSkill, ref nSkillLevel))
					{
						m_arrCarryPassiveSkill[i] = selectPassiveSkill;
						m_arrCarryPassiveSkillLevel[i] = nSkillLevel;
					}
				}
			}
			if (dataCenter.GetEquipStone(dataCenter.CurEquipStone, ref m_nEquipStoneLevel))
			{
				m_nEquipStone = dataCenter.CurEquipStone;
			}
		}
		base.InitChar(nCharID, nLevel, nExp, nAvatarHead, nAvatarUpper, nAvatarLower, nAvatarHeadup, nAvatarNeck, nAvatarWrist, nAvatarBadge, nAvatarStone);
		m_fSkillCD = m_curCharacterInfoLevel.fSkillCD;
		m_fSkillCDcount = m_fSkillCD;
		if (base.m_GameData.m_DataCenter != null)
		{
			base.m_GameData.m_DataCenter.GetSkill(m_nSkill, ref m_nSkillLevel);
		}
		iGameUIBase gameUI = base.m_GameScene.GetGameUI();
		if (gameUI != null && m_fHPMax != 0f)
		{
			gameUI.SetProtraitName(m_curCharacterInfoLevel.sName, base.UID);
			gameUI.SetProtraitIcon(m_curCharacterInfoLevel.sIcon, base.UID);
			gameUI.SetProtraitLife(m_fHP / m_fHPMax, base.UID);
			CSkillInfoLevel skillInfo = base.m_GameData.GetSkillInfo(m_nSkill, m_nSkillLevel);
			if (skillInfo != null)
			{
				gameUI.SetSkillIcon(skillInfo.sIcon);
				gameUI.SetSkillMutiplyFlag(skillInfo.m_bMutiply);
			}
		}
	}

	public void SwitchWeapon(int nIndex)
	{
		CWeaponBase weapon = base.m_GameState.GetWeapon(nIndex);
		if (weapon == null)
		{
			return;
		}
		CWeaponInfoLevel weaponInfo = base.m_GameData.GetWeaponInfo(weapon.ID, weapon.Level);
		if (weaponInfo == null)
		{
			return;
		}
		m_nCurWeaponIndex = nIndex;
		SetFire(false);
		UnEquipWeapon();
		base.m_GameState.SwitchWeapon(nIndex);
		EquipWeapon(weapon.ID, weapon.Level, base.m_GameState.GetCurrWeapon());
		iGameUIBase gameUI = base.m_GameScene.GetGameUI();
		if (gameUI != null)
		{
			if (weapon.CurWeaponLvlInfo != null)
			{
				gameUI.InitAimCross(weapon.CurWeaponLvlInfo.nType, weapon.CurWeaponLvlInfo.fPrecise);
			}
			gameUI.SetWeaponIcon(weapon.CurWeaponLvlInfo.sIcon);
			if (base.m_GameScene.m_nBlackMonsterCount > 0)
			{
				if (weaponInfo.nType != 1)
				{
					gameUI.ShowBlackWarning(true);
				}
				else
				{
					gameUI.ShowBlackWarning(false);
				}
			}
		}
		iCameraTrail iCameraTrail2 = base.m_GameScene.GetCamera();
		if (iCameraTrail2 != null)
		{
			iCameraTrail2.SetViewMelee(weaponInfo.nType == 1);
		}
		iGameUIBase gameUI2 = base.m_GameScene.GetGameUI();
		if (gameUI2 != null)
		{
			gameUI2.ShowShootUI(true);
		}
		if (CGameNetManager.GetInstance().IsConnected())
		{
			CGameNetSender.GetInstance().SendMsg_PLAYER_SWITCHWEAPON(weapon.ID, weapon.Level);
		}
	}
	
	public void Event_SwitchWeaponPrevious(GameObject go)
	{
		iGameSceneBase gameScene = iGameApp.GetInstance().m_GameScene as iGameSceneBase;
		if (gameScene == null) return;
		CCharUser user = gameScene.GetUser() as CCharUser;
		if (user == null) return;
		int weaponCount = user.m_GameState.GetWeaponCount();
		int currentIndex = user.CurWeaponIndex;
		int previousIndex = (currentIndex - 1 + weaponCount) % weaponCount;
		user.SwitchWeapon(previousIndex);
	}


	public void LookAt(Vector3 v3Point)
	{
		iCameraTrail iCameraTrail2 = base.m_GameScene.GetCamera();
		if (!(iCameraTrail2 == null))
		{
			Vector3 forward = iCameraTrail2.transform.forward;
			forward.y = 0f;
			if (!(Vector3.Dot(base.Dir2D, forward) <= 0.1f))
			{
				UpdateUpBody(v3Point - m_ManualSpine.position);
				iGameApp.GetInstance().SetGizmosLine("lookat", m_ManualSpine.position, v3Point, Color.green);
				m_bUpdateDir = true;
			}
		}
	}

	public void SetFire(bool bFire)
	{
		if (m_curWeapon == null)
		{
			return;
		}
		if (bFire)
		{
			m_curWeapon.Fire(this);
		}
		else
		{
			m_curWeapon.Stop(this);
		}
		if (m_curWeaponLvlInfo.nAttackMode == 2)
		{
			iCameraTrail iCameraTrail2 = base.m_GameScene.GetCamera();
			if (iCameraTrail2 != null)
			{
				iCameraTrail2.ShootMode(bFire);
			}
		}
		if (CGameNetManager.GetInstance().IsConnected() && (!bFire || !m_curWeapon.IsBulletEmpty))
		{
			CGameNetSender.GetInstance().SendMsg_PLAYER_SHOOT(bFire);
		}
	}

	public bool IsFire()
	{
		if (m_curWeapon == null)
		{
			return false;
		}
		return m_curWeapon.IsFire();
	}

	protected void UpdateMoveAnim(Vector2 v2Compass)
	{
		kCharMoveDir kCharMoveDir2 = kCharMoveDir.None;
		float num = Vector2.Dot(Vector2.up, v2Compass.normalized);
		kCharMoveDir2 = ((num > 0.78f) ? kCharMoveDir.Forward : ((num < -0.78f) ? kCharMoveDir.Back : ((v2Compass.x > 0f) ? ((num > 0.2f) ? kCharMoveDir.ForwardRight : ((!(num < -0.2f)) ? kCharMoveDir.Right : kCharMoveDir.BackRight)) : ((num > 0.2f) ? kCharMoveDir.ForwardLeft : ((!(num < -0.2f)) ? kCharMoveDir.Left : kCharMoveDir.BackLeft)))));
		SetMoveAnim(kCharMoveDir2);
	}

	public bool IsCanMove()
	{
		return !m_bBeatBack && !base.isDead && !m_bBumping && !m_bStun;
	}

	public override bool IsCanAttack()
	{
		return base.IsCanAttack();
	}

	public bool IsCanAim()
	{
		return !base.isDead && !m_bBumping && !m_bStun;
	}

	public override void BeatBack(Vector3 v3Dir, float fDis)
	{
		base.BeatBack(v3Dir, fDis);
		MoveStop();
		SetFire(false);
		base.m_GameScene.ShakeCamera(0.2f, 0.15f);
	}

	public override void SetStun(bool bStun, float fTime = 0f)
	{
		base.SetStun(bStun, fTime);
		if (bStun)
		{
			MoveStop();
			SetFire(false);
		}
	}

	public override void AddHP(float fHP)
	{
		base.AddHP(fHP);
		if (CGameNetManager.GetInstance().IsConnected())
		{
			CGameNetSender.GetInstance().SendMsg_BATTLE_DAMAGE_PLAYER(base.UID, base.CurHP, base.MaxHP);
		}
	}

	public override void OnDead(kDeadMode nDeathMode)
	{
		base.OnDead(nDeathMode);
		UnityEngine.Debug.Log(nDeathMode);
		MoveStop();
		base.m_GameScene.StartUserDeathCamera();
		iGameUIBase gameUI = base.m_GameScene.GetGameUI();
		if (gameUI != null)
		{
			gameUI.SetProtraitDeathFlag(true, base.UID);
		}
		if (CGameNetManager.GetInstance().IsConnected())
		{
			CGameNetSender.GetInstance().SendMsg_PLAYER_DEAD();
		}
		if (base.m_GameScene.m_bMutiplyGame && base.m_GameData.m_DataCenter != null)
		{
			base.m_GameData.m_DataCenter.DeadInCoopCount++;
		}
	}

	public override bool OnHit(float fDmg, CWeaponInfoLevel pWeaponLvlInfo = null, string sBodyPart = "")
	{
		if (base.m_GameScene.GameStatus != iGameSceneBase.kGameStatus.Gameing)
		{
			return false;
		}
		iGameUIBase gameUI = base.m_GameScene.GetGameUI();
		if (gameUI != null)
		{
			gameUI.ShowScreenBlood(true);
		}
		if (m_bCarryTaskItem && fDmg < 0f)
		{
			CTaskCollection cTaskCollection = base.m_GameScene.m_TaskManager.GetTask() as CTaskCollection;
			if (cTaskCollection != null)
			{
				cTaskCollection.AddDamage(m_nCarryItemID, fDmg);
			}
		}
		return base.OnHit(fDmg, pWeaponLvlInfo, sBodyPart);
	}

	public override void AddExp(int nExp)
	{
		if (m_curCharacterInfo == null || nExp == 0 || m_curCharacterInfo.IsMaxLevel(base.Level))
		{
			return;
		}
		m_nExp += nExp;
		iGameUIBase gameUI = base.m_GameScene.GetGameUI();
		bool flag = false;
		if (nExp <= 0)
		{
			if (m_nExp < 0)
			{
				m_nExp = 0;
			}
			if (gameUI != null)
			{
				gameUI.SetProtraitExp(0, Mathf.Clamp01((float)m_nExp / (float)m_curCharacterInfoLevel.nExp), base.UID);
			}
		}
		else
		{
			int nLevel = m_nLevel;
			LevelUp(ref m_nExp, ref m_nLevel);
			int num = m_nLevel - nLevel;
			if (gameUI != null)
			{
				gameUI.SetProtraitExp(num, Mathf.Clamp01((float)m_nExp / (float)m_curCharacterInfoLevel.nExp), base.UID);
				gameUI.SetProtraitLevel(m_nLevel.ToString(), base.UID);
				if (num > 0)
				{
					gameUI.PlayProtraitLevelAnim();
				}
			}
			if (num > 0)
			{
				InitChar(base.ID, base.Level, base.EXP, base.AvatarHead, base.AvatarUpper, base.AvatarLower, base.AvatarHeadup, base.AvatarNeck, base.AvatarWrist, base.AvatarBadge, base.AvatarStone);
				m_bUpdateProBuff = true;
				GameObject gameObject = base.m_GameScene.AddEffect(Vector3.zero, Vector3.one, 5f, 1300);
				gameObject.transform.parent = GetBone(3);
				gameObject.transform.localPosition = new Vector3(0f, 0.01f, 0f);
				gameObject.transform.localRotation = Quaternion.identity;
				base.m_GameState.isCheckUnLock = true;
				flag = true;
				if (base.CurCharInfoLevel.isMale)
				{
					PlayAudio("UI_Upgrade_Male");
				}
				else
				{
					PlayAudio("UI_Upgrade_Female");
				}
				if (CGameNetManager.GetInstance().IsConnected())
				{
					CGameNetSender.GetInstance().SendMsg_PLAYER_LEVELUP(base.Level);
				}
				CTrinitiCollectManager.GetInstance().SendCharLevel(base.ID, base.Level);
			}
		}
		iDataCenter dataCenter = base.m_GameData.GetDataCenter();
		if (dataCenter == null)
		{
			return;
		}
		dataCenter.SetCharacter(base.ID, base.Level, base.EXP);
		if (flag)
		{
			dataCenter.Save();
			if (base.Level == dataCenter.HighestCharLevel)
			{
				CAchievementManager.GetInstance().AddAchievement(1, new object[1] { base.Level });
			}
		}
	}

	public void AddGold(int nGold)
	{
	}

	public void LevelUp(ref int nExp, ref int nLevel)
	{
		CCharacterInfoLevel characterInfo = base.m_GameData.GetCharacterInfo(base.ID, nLevel);
		if (characterInfo == null)
		{
			return;
		}
		int nExp2 = characterInfo.nExp;
		if (nExp >= nExp2)
		{
			nExp -= nExp2;
			nLevel++;
			if (!m_curCharacterInfo.IsMaxLevel(nLevel))
			{
				LevelUp(ref nExp, ref nLevel);
			}
		}
	}

	public override void Revive(float fHP)
	{
		base.Revive(fHP);
		iGameUIBase gameUI = base.m_GameScene.GetGameUI();
		if (m_bCarryTaskItem && gameUI != null)
		{
			iGameTaskUICollectList iGameTaskUICollectList2 = gameUI.GetTaskUI() as iGameTaskUICollectList;
			if (iGameTaskUICollectList2 != null)
			{
				iGameTaskUICollectList2.ShowLifeBar(true, 0);
			}
		}
		if (gameUI != null)
		{
			gameUI.SetProtraitDeathFlag(false, base.UID);
		}
		if (base.m_GameScene.m_bMutiplyGame && base.m_GameData.m_DataCenter != null)
		{
			base.m_GameData.m_DataCenter.ReviveInCoopCount++;
		}
	}

	public override void TakeItem(int nItemID, GameObject ItemObj)
	{
		base.TakeItem(nItemID, ItemObj);
		base.m_GameScene.ShowItemScreenTip(false);
		base.m_GameScene.ShowTriggerEndScreenTip(true);
		CItemInfoLevel itemInfo = base.m_GameData.GetItemInfo(nItemID, 1);
		if (itemInfo != null && itemInfo.nType == 4)
		{
			m_bCarryTaskItem = true;
			iGameUIBase gameUI = base.m_GameScene.GetGameUI();
			if (gameUI != null)
			{
				iGameTaskUICollectList iGameTaskUICollectList2 = gameUI.GetTaskUI() as iGameTaskUICollectList;
				if (iGameTaskUICollectList2 != null)
				{
					if (iGameTaskUICollectList2.Task != null)
					{
						iGameTaskUICollectList2.Task.ResetHP();
					}
					iGameTaskUICollectList2.ShowLifeBar(true, 0);
				}
			}
		}
		if (base.m_GameScene.m_MGManager != null)
		{
			CEventManager eventManager = base.m_GameScene.m_MGManager.GetEventManager();
			if (eventManager != null)
			{
				eventManager.Trigger(new EventCondition_StealEgg_Take(nItemID, base.m_GameScene.GetStealItem(nItemID)));
			}
		}
	}

	public override void DropItem()
	{
		base.DropItem();
		base.m_GameScene.ShowItemScreenTip(true);
		base.m_GameScene.ShowTriggerEndScreenTip(false);
		if (!m_bCarryTaskItem)
		{
			return;
		}
		m_bCarryTaskItem = false;
		iGameUIBase gameUI = base.m_GameScene.GetGameUI();
		if (gameUI != null)
		{
			iGameTaskUICollectList iGameTaskUICollectList2 = gameUI.GetTaskUI() as iGameTaskUICollectList;
			if (iGameTaskUICollectList2 != null)
			{
				iGameTaskUICollectList2.ShowLifeBar(false, 0);
			}
		}
	}

	public override void UseSkill(int nSkill, int nSkillLevel)
	{
		base.UseSkill(nSkill, nSkillLevel);
		m_fSkillCD = m_curCharacterInfoLevel.fSkillCD;
		CSkillPro skillPro = m_Property.GetSkillPro(nSkill);
		if (skillPro != null)
		{
			m_fSkillCD -= skillPro.fCDDown;
		}
		float value = m_Property.GetValue(kProEnum.Skill_CD_Faster);
		float value2 = m_Property.GetValue(kProEnum.Skill_CD_Faster_Rate);
		m_fSkillCD = m_fSkillCD * (1f - value2 / 100f) - value;
		m_fSkillCDcount = 0f;
		iGameUIBase gameUI = base.m_GameScene.GetGameUI();
		if (gameUI != null)
		{
			gameUI.SetSkillCD(m_fSkillCD);
		}
		if (CGameNetManager.GetInstance().IsConnected())
		{
			CGameNetSender.GetInstance().SendMsg_PLAYER_USESKILL(nSkill, nSkillLevel);
		}
	}

	public bool IsSkillCD()
	{
		if (m_fSkillCDcount < m_fSkillCD)
		{
			return true;
		}
		return false;
	}

	public void ResetSkillCD()
	{
		m_fSkillCDcount = m_fSkillCD;
		iGameUIBase gameUI = base.m_GameScene.GetGameUI();
		if (gameUI != null)
		{
			gameUI.FinishSkillCD();
		}
	}
}