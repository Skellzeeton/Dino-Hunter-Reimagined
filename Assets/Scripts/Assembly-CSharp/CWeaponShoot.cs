using UnityEngine;

public class CWeaponShoot : CWeaponBase
{
	protected override void OnEquip(CCharPlayer player)
	{
		RefreshBulletUI(player);
	}

protected override void OnFire(CCharPlayer player)
{
    if (!player.IsCanAttack())
        return;

    if (base.IsBulletEmpty)
    {
        if (m_pWeaponLvlInfo != null)
        {
            switch (m_pWeaponLvlInfo.nType)
            {
                case 2:
                    player.PlayAudio("Weapon_nobullet_gun");
                    break;
                case 0:
                    player.PlayAudio("Weapon_nobullet_crossbow");
                    break;
            }
        }
        Stop(player);
        return;
    }

    ConsumeBullet(player);
    ShowFireLight(true);

    if (m_GameScene.IsMyself(player))
    {
        iGameUIBase ui = m_GameScene.GetGameUI();
        if (ui != null) ui.ExpandAimCross();
    }

    player.PlayAnimMix(kAnimEnum.Attack, WrapMode.ClampForever, 1f);

    float maxDist = 10000f;
    m_pWeaponLvlInfo.GetAtkModeValue(0, ref maxDist);
    Transform muzzle = player.GetShootMouseTf();
    Vector3 muzzlePos = muzzle.position;
    Ray camRay = Camera.main.ScreenPointToRay(m_GameState.GetScreenCenterV3());
    RaycastHit aimHit;

    Vector3 targetPoint;

    if (Physics.Raycast(camRay, out aimHit, maxDist, -1543503872))
    {
        targetPoint = aimHit.point;
    }
    else
    {
        targetPoint = camRay.origin + camRay.direction * maxDist;
    }

    Ray camToMuzzle = new Ray(camRay.origin, (muzzlePos - camRay.origin).normalized);
    RaycastHit obstruction;
    float muzzleDistance = Vector3.Distance(camRay.origin, muzzlePos);

    if (Physics.Raycast(camToMuzzle, out obstruction, muzzleDistance, -1543503872))
    {
	    muzzlePos = obstruction.point - camToMuzzle.direction * 0.1f;
    }
    Vector3 cameraForward = camRay.direction.normalized;
    Vector3 muzzleToTarget = (targetPoint - muzzlePos).normalized;
    float camAlignment = Vector3.Dot(cameraForward, muzzleToTarget);
    if (camAlignment < 0.75f)
    {
	    targetPoint = muzzlePos + cameraForward * maxDist;
    }
    Vector3 shootDir = (targetPoint - muzzlePos).normalized;
    Ray weaponRay = new Ray(muzzlePos, shootDir);
    RaycastHit hit;
    if (!Physics.Raycast(weaponRay, out hit, maxDist, -1543503872))
    {
        m_GameScene.AddFireEffect(muzzle, shootDir, m_pWeaponLvlInfo.nFire, 2f);
        player.PlayAudio(m_pWeaponLvlInfo.sAudioFire);
        return;
    }
    float travelDist = (hit.point - muzzlePos).magnitude;
    if (travelDist > 2f)
    {
        m_GameScene.AddBulletTrack(muzzlePos, hit.point, m_pWeaponLvlInfo.nBullet);
    }
    m_GameScene.AddFireEffect(muzzle, shootDir, m_pWeaponLvlInfo.nFire, 2f);
    player.PlayAudio(m_pWeaponLvlInfo.sAudioFire);
    if (hit.transform.gameObject.layer == 31 || hit.transform.gameObject.layer == 29)
    {
        m_GameScene.AddHitEffect(hit.point, hit.normal, m_pWeaponLvlInfo.nHit);
        return;
    }
    if (hit.transform.gameObject.layer == 26)
    {
        CCharMob mob = hit.transform.root.GetComponent<CCharMob>();
        if (mob != null && !mob.isDead)
        {
            if (!base.isNetPlayerShoot)
                OnHitMob(player, mob, hit.point, hit.normal, hit.transform.name);

            mob.PlayAudio(kAudioEnum.HitBody);

            switch (m_pWeaponLvlInfo.nElementType)
            {
                case 1: mob.PlayAudio("Fx_Impact_fire"); break;
                case 2: mob.PlayAudio("Fx_Impact_electric"); break;
                case 3: mob.PlayAudio("Fx_Impact_freeze"); break;
            }

            if (mob is CCharBoss && ((CCharBoss)mob).isInBlack)
            {
                m_GameScene.AddHitEffect(hit.point, hit.normal, 1953);
            }
            else
            {
                m_GameScene.AddHitEffect(hit.point, hit.normal, m_pWeaponLvlInfo.nHit);
            }
        }
    }
}


	protected override void OnStop(CCharPlayer player)
	{
	}

	protected override void OnUpdate(CCharPlayer player, float deltaTime)
	{
		if (m_fFireIntervalCount < m_fFireInterval)
		{
			m_fFireIntervalCount += deltaTime;
			if (m_fFireIntervalCount < m_fFireInterval)
			{
				return;
			}
		}
		if (m_bFire)
		{
			m_fFireIntervalCount = 0f;
			OnFire(player);
		}
	}

	protected override void OnHitMob(CCharPlayer player, CCharMob mob, Vector3 hitpos, Vector3 hitdir, string sBodyPart = "")
	{
		mob.SetLifeBarParam(3f);
		iGameUIBase gameUI = m_GameScene.GetGameUI();
		if (gameUI != null)
		{
			gameUI.ShootLifeBar(mob.UID);
		}
		CCharBoss cCharBoss = mob as CCharBoss;
		if (cCharBoss != null && cCharBoss.isInBlack)
		{
			cCharBoss.AddBlackDmg(-1f);
			base.m_GameScene.AddDamageText(1f, hitpos);
			if (CGameNetManager.GetInstance().IsConnected() && base.m_GameScene.IsMyself(player))
			{
				CGameNetSender.GetInstance().SendMsg_BATTLE_DAMAGE_MOB(mob.UID, 1f, true);
			}
			return;
		}
		float num = player.CalcWeaponDamage(m_pWeaponLvlInfo);
		float num2 = player.CalcCritical(m_pWeaponLvlInfo);
		float num3 = player.CalcCriticalDmg(m_pWeaponLvlInfo);
		bool bCritical = false;
		if (num2 > Random.Range(1f, 100f))
		{
			num *= 1f + num3 / 100f;
			bCritical = true;
		}
		float elementValue = m_pWeaponLvlInfo.GetElementValue(mob.ID);
		if (elementValue != 0f)
		{
			num *= 1f + elementValue / 100f;
		}
		float num4 = mob.CalcProtect();
		num *= 1f - num4 / 100f;
		if (num < 1f)
		{
			num = 1f;
		}
		base.m_GameScene.AddMyDamage(num, mob.CurHP);
		mob.OnHit(0f - num, m_pWeaponLvlInfo, sBodyPart);
		base.m_GameScene.AddDamageText(num, hitpos, bCritical);
		base.m_GameScene.AddHitEffect(hitpos, Vector3.forward, 1115);
		m_GameLogic = base.m_GameScene.GetGameLogic();
		if (m_GameLogic != null)
		{
			iGameLogic.HitInfo hitinfo = new iGameLogic.HitInfo();
			hitinfo.v3HitDir = mob.Pos - player.Pos;
			hitinfo.v3HitPos = hitpos;
			m_GameLogic.CaculateFunc(player, mob, m_pWeaponLvlInfo.arrFunc, m_pWeaponLvlInfo.arrValueX, m_pWeaponLvlInfo.arrValueY, ref hitinfo);
			m_GameLogic.ltDamageInfo.Add(num);
			m_GameLogic.m_fTotalDmg += num;
		}
		if (CGameNetManager.GetInstance().IsConnected() && base.m_GameScene.IsMyself(player))
		{
			CGameNetSender.GetInstance().SendMsg_BATTLE_DAMAGE_MOB(mob.UID, m_GameLogic.m_fTotalDmg);
		}
		if (!mob.isDead)
		{
			return;
		}
		CMobInfoLevel mobInfo = mob.GetMobInfo();
		if (mobInfo != null)
		{
			int num5 = 0;
			num5 = ((!base.m_GameScene.m_bMutiplyGame) ? mobInfo.nExp : MyUtils.formula_monsterexp(mobInfo.nExp, mob.Level));
			float value = player.Property.GetValue(kProEnum.Char_IncreaseExp);
			if (value > 0f)
			{
				num5 = (int)((float)num5 * (1f + value / 100f));
			}
			player.AddExp(num5);
			base.m_GameScene.AddExpText(num5, hitpos);
		}
	}
}
