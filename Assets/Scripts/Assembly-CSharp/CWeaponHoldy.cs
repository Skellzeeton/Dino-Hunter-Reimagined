using System;
using System.Collections.Generic;
using UnityEngine;

public class CWeaponHoldy : CWeaponBase
{
    protected float m_fRadius;
    protected float m_fAngle;
    protected float m_fEffectTime;
    protected float m_fEffectTimeCount;
    protected GameObject m_FireEffect;
    protected ParticleSystem[] m_arrParticleSystem;

    protected override void OnEquip(CCharPlayer player)
    {
        if (m_FireEffect != null || m_pWeaponLvlInfo == null || player == null)
            return;

        RefreshBulletUI(player);
        GameObject prefab = PrefabManager.Get(m_pWeaponLvlInfo.nFire);
        if (prefab == null)
            return;

        m_FireEffect = UnityEngine.Object.Instantiate(prefab);
        if (m_FireEffect == null)
            return;

        m_FireEffect.transform.parent = player.GetShootMouseTf();
        m_FireEffect.transform.localPosition = Vector3.zero;
        m_FireEffect.transform.localEulerAngles = new Vector3(90f, 0f, 0f);

        m_arrParticleSystem = m_FireEffect.GetComponentsInChildren<ParticleSystem>(true);
        if (m_arrParticleSystem != null)
        {
            foreach (var ps in m_arrParticleSystem)
            {
                if (ps != null)
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
        }
    }

    protected override void OnDestroy()
    {
        if (m_FireEffect != null)
        {
            UnityEngine.Object.Destroy(m_FireEffect);
            m_FireEffect = null;
        }
    }

    protected override void OnFire(CCharPlayer player)
    {
        if (!player.IsCanAttack())
            return;

        m_fFireLightTime = 1.5f;
        player.PlayAnimMix(kAnimEnum.Attack, WrapMode.Loop, 1f);
        player.PlayAudio(m_pWeaponLvlInfo.sAudioFire);

        if (m_arrParticleSystem != null)
        {
            foreach (var ps in m_arrParticleSystem)
            {
                if (ps != null)
                    ps.Play(true);
            }
        }

        m_fRadius = 0f;
        m_fAngle = 0f;
        m_fEffectTime = 0.5f;
        m_pWeaponLvlInfo.GetAtkModeValue(0, ref m_fRadius);
        m_pWeaponLvlInfo.GetAtkModeValue(1, ref m_fAngle);
        m_pWeaponLvlInfo.GetAtkModeValue(2, ref m_fEffectTime);
        m_fEffectTimeCount = m_fEffectTime;
    }

    protected override void OnStop(CCharPlayer player)
    {
        player.StopAction(kAnimEnum.Attack);
        player.StopAudio(m_pWeaponLvlInfo.sAudioFire);

        if (m_pWeaponLvlInfo.nElementType == 3)
            player.PlayAudio("Weapon_ice_end");
        else
            player.PlayAudio("Weapon_flame_end");

        if (m_arrParticleSystem != null)
        {
            foreach (var ps in m_arrParticleSystem)
            {
                if (ps != null)
                    ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
            }
        }
    }

    protected override void OnUpdate(CCharPlayer player, float deltaTime)
    {
        if (m_fFireIntervalCount < m_fFireInterval)
            m_fFireIntervalCount += deltaTime;

        if (!m_bFire)
            return;

        m_fEffectTimeCount += deltaTime;
        if (m_fEffectTimeCount < m_fEffectTime)
            return;

        m_fEffectTimeCount = 0f;

        if (base.IsBulletEmpty)
        {
            player.PlayAudio("Weapon_nobullet_flamethrower");
            Stop(player);
            return;
        }

        ConsumeBullet(player);
        ShowFireLight(true);

        if (base.m_GameScene.IsMyself(player))
        {
            iGameUIBase gameUI = base.m_GameScene.GetGameUI();
            if (gameUI != null)
                gameUI.ExpandAimCross();
        }

        Dictionary<int, CCharMob> mobData = base.m_GameScene.GetMobData();
        foreach (CCharMob mob in mobData.Values)
        {
            if (mob.isDead)
                continue;

            Vector3 toMob = mob.Pos - player.Pos;
            if (toMob.sqrMagnitude > m_fRadius * m_fRadius)
                continue;

            if (m_fRadius < 2f && m_fAngle > 0f)
            {
                toMob.y = 0f;
                if (Vector3.Dot(player.Dir2D, toMob.normalized) <= 0f)
                    continue;
            }
            else if (m_fAngle > 0f)
            {
                toMob.y = 0f;
                if (Vector3.Dot(player.Dir2D, toMob.normalized) < Mathf.Cos(m_fAngle * Mathf.Deg2Rad / 2f))
                    continue;
            }

            Vector3 hitDir = mob.Pos - player.Pos;
            Vector3 hitPos = mob.GetBloodPos(player.GetUpBodyPos() + new Vector3(0f, 0.7f, 0f), hitDir);
            CCharBoss boss = mob as CCharBoss;

            if (boss != null && boss.isInBlack)
                base.m_GameScene.AddHitEffect(hitPos, hitDir, 1953);
            else
                base.m_GameScene.AddHitEffect(hitPos, hitDir, m_pWeaponLvlInfo.nHit);

            if (!base.isNetPlayerShoot)
                OnHitMob(player, mob, hitPos, hitDir, string.Empty);

            mob.PlayAudio(kAudioEnum.HitBody);
            switch (m_pWeaponLvlInfo.nElementType)
            {
                case 1: mob.PlayAudio("Fx_Impact_fire"); break;
                case 3: mob.PlayAudio("Fx_Impact_freeze"); break;
                case 2: mob.PlayAudio("Fx_Impact_electric"); break;
            }
        }
    }

    protected override void OnHitMob(CCharPlayer player, CCharMob mob, Vector3 hitpos, Vector3 hitdir, string sBodyPart = "")
    {
        mob.SetLifeBarParam(1f);
        CCharBoss boss = mob as CCharBoss;

        if (boss != null && boss.isInBlack)
        {
            boss.AddBlackDmg(-1f);
            base.m_GameScene.AddDamageText(1f, hitpos);
            if (CGameNetManager.GetInstance().IsConnected() && base.m_GameScene.IsMyself(player))
                CGameNetSender.GetInstance().SendMsg_BATTLE_DAMAGE_MOB(mob.UID, 1f, true);
            return;
        }

        float dmg = player.CalcWeaponDamage(m_pWeaponLvlInfo);
        float critChance = player.CalcCritical(m_pWeaponLvlInfo);
        float critBonus = player.CalcCriticalDmg(m_pWeaponLvlInfo);
        bool isCrit = false;

        if (critChance > UnityEngine.Random.Range(1f, 100f))
        {
            dmg *= 1f + critBonus / 100f;
            isCrit = true;
        }

        dmg *= 1f - mob.CalcProtect() / 100f;
        if (dmg < 1f) dmg = 1f;

        base.m_GameScene.AddMyDamage(dmg, mob.CurHP);
        mob.OnHit(-dmg, m_pWeaponLvlInfo, string.Empty);
        base.m_GameScene.AddDamageText(dmg, hitpos, isCrit);
        base.m_GameScene.AddHitEffect(hitpos, Vector3.forward, 1115);

        iGameLogic.HitInfo hitinfo = new iGameLogic.HitInfo { v3HitDir = hitdir, v3HitPos = hitpos };
        m_GameLogic = base.m_GameScene.GetGameLogic();

        if (m_GameLogic != null)
        {
            m_GameLogic.CaculateFunc(player, mob, m_pWeaponLvlInfo.arrFunc, m_pWeaponLvlInfo.arrValueX, m_pWeaponLvlInfo.arrValueY, ref hitinfo);
            m_GameLogic.ltDamageInfo.Add(dmg);
            m_GameLogic.m_fTotalDmg += dmg;
        }

        if (CGameNetManager.GetInstance().IsConnected() && base.m_GameScene.IsMyself(player))
            CGameNetSender.GetInstance().SendMsg_BATTLE_DAMAGE_MOB(mob.UID, m_GameLogic.m_fTotalDmg);

        if (!mob.isDead)
            return;

        CMobInfoLevel mobInfo = mob.GetMobInfo();
        if (mobInfo != null)
        {
            int exp = base.m_GameScene.m_bMutiplyGame ? MyUtils.formula_monsterexp(mobInfo.nExp, mob.Level) : mobInfo.nExp;
            float bonus = player.Property.GetValue(kProEnum.Char_IncreaseExp);
            if (bonus > 0f)
                exp = (int)(exp * (1f + bonus / 100f));
            player.AddExp(exp);
            base.m_GameScene.AddExpText(exp, hitinfo.v3HitPos);
        }
    }
}
