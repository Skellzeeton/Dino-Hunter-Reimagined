using System;
using System.Collections.Generic;
using UnityEngine;

public class CWeaponShotgun : CWeaponBase
{
	protected override void OnEquip(CCharPlayer player)
	{
		RefreshBulletUI(player);
	}

	protected override void OnFire(CCharPlayer player)
	{
		if (!player.IsCanAttack())
		{
			return;
		}
		if (base.IsBulletEmpty)
		{
			player.PlayAudio("Weapon_nobullet_gun");
			Stop(player);
			return;
		}

		ConsumeBullet(player);
		ShowFireLight(true);

		if (base.m_GameScene.IsMyself(player))
		{
			iGameUIBase gameUI = base.m_GameScene.GetGameUI();
			if (gameUI != null)
			{
				gameUI.ExpandAimCross();
			}
		}

		float actionLen = player.GetActionLen(kAnimEnum.Attack);
		if (actionLen > m_fFireInterval)
		{
			actionLen = player.PlayAnimMix(kAnimEnum.Attack, WrapMode.ClampForever, actionLen / m_fFireInterval);
		}
		else
		{
			actionLen = player.PlayAnimMix(kAnimEnum.Attack, WrapMode.ClampForever, 1f);
		}

		// range (fValue) and cone angle (fValue2)
		float fValue = 10000f;
		float fValue2 = 0f;
		m_pWeaponLvlInfo.GetAtkModeValue(0, ref fValue);
		m_pWeaponLvlInfo.GetAtkModeValue(1, ref fValue2);

		// CAMERA RAY: where is the crosshair pointing?
		Ray camRay = Camera.main.ScreenPointToRay(m_GameState.GetScreenCenterV3());
		RaycastHit aimHit;
		Vector3 targetPoint;
		int aimMask = -1610612736; // your original shotgun mask

		if (Physics.Raycast(camRay, out aimHit, fValue, aimMask))
		{
			targetPoint = aimHit.point;
			base.m_GameScene.AddHitEffect(aimHit.point, aimHit.normal, m_pWeaponLvlInfo.nHit);
		}
		else
		{
			targetPoint = camRay.origin + camRay.direction * fValue;
		}

		// MUZZLE: start of shotgun spread
		Transform muzzleTf = player.GetShootMouseTf();
		Vector3 muzzlePos = muzzleTf.position;

		// CHECK: obstruction between camera and muzzle (so player can't shoot through a wall that sits between camera and gun)
		Vector3 camToMuzzleDir = (muzzlePos - camRay.origin).normalized;
		float camToMuzzleDist = Vector3.Distance(camRay.origin, muzzlePos);
		RaycastHit camToMuzzleHit;
		if (Physics.Raycast(new Ray(camRay.origin, camToMuzzleDir), out camToMuzzleHit, camToMuzzleDist, aimMask))
		{
			// Move muzzle start point to just in front of the obstruction so we don't start inside the wall.
			muzzlePos = camToMuzzleHit.point - camToMuzzleDir * 0.05f;
		}

		// ALIGNMENT GATING: if the camera aim and muzzle→target differ too much, fall back to camera forward
		Vector3 cameraForward = camRay.direction.normalized;
		Vector3 muzzleToTarget = (targetPoint - muzzlePos).normalized;
		float camAlignment = Vector3.Dot(cameraForward, muzzleToTarget);
		if (camAlignment < 0.75f) // adjust threshold if you want wider/narrower acceptance
		{
			targetPoint = muzzlePos + cameraForward * fValue;
			muzzleToTarget = (targetPoint - muzzlePos).normalized;
		}

		// Fire effect at muzzle (shotgun blast)
		base.m_GameScene.AddFireEffect(muzzleTf, muzzleToTarget, m_pWeaponLvlInfo.nFire, 2f);
		player.PlayAudio(m_pWeaponLvlInfo.sAudioFire);

		// Now apply AOE: iterate mobs that are within range and within cone, but only apply hit if there's LOS from muzzle
		Dictionary<int, CCharMob> mobData = base.m_GameScene.GetMobData();
		foreach (CCharMob mob in new List<CCharMob>(mobData.Values))
		{
			if (mob == null || mob.isDead)
				continue;

			// distance test (squared for perf)
			Vector3 vecToMob = mob.Pos - player.Pos;
			float distSqr = vecToMob.sqrMagnitude;
			if (distSqr > fValue * fValue)
				continue;

			// If close, check forward/back only if fValue2 > 0 (original behavior)
			if (distSqr < 2f)
			{
				if (fValue2 > 0f)
				{
					vecToMob.y = 0f;
					if (Vector3.Dot(player.Dir2D, vecToMob.normalized) <= 0f)
					{
						continue;
					}
				}
			}
			else if (fValue2 > 0f)
			{
				vecToMob.y = 0f;
				// cone check using cos(angle/2)
				if (Vector3.Dot(player.Dir2D, vecToMob.normalized) < Mathf.Cos(fValue2 * ((float)Math.PI / 180f) / 2f))
				{
					continue;
				}
			}

			// compute the "blood" hit position (targeting visible body region)
			Vector3 dirToMob = mob.Pos - player.Pos;
			Vector3 bloodPos = mob.GetBloodPos(player.GetUpBodyPos() + new Vector3(0f, 0.7f, 0f), dirToMob);

			// NEW: LOS check from muzzle to bloodPos so AOE can't hit through walls/objects.
			// Use same mask as main aim so walls/props will block the shotgun pellet AOE.
			Vector3 muzzleToBlood = (bloodPos - muzzlePos);
			float muzzleToBloodDist = muzzleToBlood.magnitude;
			if (muzzleToBloodDist <= 0.001f)
			{
				// extremely close — allow
			}
			else
			{
				RaycastHit losHit;
				if (Physics.Raycast(new Ray(muzzlePos, muzzleToBlood.normalized), out losHit, muzzleToBloodDist, aimMask))
				{
					// if the ray hit something but not the mob (or the mob's colliders), skip this mob.
					Transform troot = losHit.transform.root;
					if (troot != mob.transform.root)
					{
						// blocked by world geometry (wall/prop) — don't hit this mob
						continue;
					}
					// else the hit is the mob (or its child) -> allow
				}
			}

			// OK: we have LOS and passed distance/cone checks — apply hit FX and damage
			CCharBoss cCharBoss = mob as CCharBoss;
			if (cCharBoss != null && cCharBoss.isInBlack)
			{
				base.m_GameScene.AddHitEffect(bloodPos, dirToMob, 1953);
			}
			else
			{
				switch (m_pWeaponLvlInfo.nHit)
				{
					case 1103:
						base.m_GameScene.AddHitEffect(bloodPos, dirToMob, 1100);
						break;
					case 1104:
						base.m_GameScene.AddHitEffect(bloodPos, dirToMob, 1101);
						break;
					case 1105:
						base.m_GameScene.AddHitEffect(bloodPos, dirToMob, 1102);
						break;
					default:
						base.m_GameScene.AddHitEffect(bloodPos, dirToMob, 1110);
						break;
				}
			}

			base.m_GameScene.ShakeCamera(0.2f, 0.15f);

			if (!base.isNetPlayerShoot)
			{
				OnHitMob(player, mob, bloodPos, dirToMob, string.Empty);
			}

			mob.PlayAudio(kAudioEnum.HitBody);
			switch (m_pWeaponLvlInfo.nElementType)
			{
				case 1:
					mob.PlayAudio("Fx_Impact_fire");
					break;
				case 3:
					mob.PlayAudio("Fx_Impact_freeze");
					break;
				case 2:
					mob.PlayAudio("Fx_Impact_electric");
					break;
			}
		}
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
		mob.SetLifeBarParam(1f);
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
		if (num2 > UnityEngine.Random.Range(1f, 100f))
		{
			num *= 1f + num3 / 100f;
			bCritical = true;
		}
		float num4 = mob.CalcProtect();
		num *= 1f - num4 / 100f;
		if (num < 1f)
		{
			num = 1f;
		}
		base.m_GameScene.AddMyDamage(num, mob.CurHP);
		mob.OnHit(0f - num, m_pWeaponLvlInfo, string.Empty);
		base.m_GameScene.AddDamageText(num, hitpos, bCritical);
		base.m_GameScene.AddHitEffect(hitpos, Vector3.forward, 1116);
		iGameLogic.HitInfo hitinfo = new iGameLogic.HitInfo();
		hitinfo.v3HitDir = hitdir;
		hitinfo.v3HitPos = hitpos;
		m_GameLogic = base.m_GameScene.GetGameLogic();
		if (m_GameLogic != null)
		{
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
