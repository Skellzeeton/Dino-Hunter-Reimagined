using System.Collections.Generic;

public class CCharMobStegosaurus : CCharBoss
{
    protected Dictionary<string, int> m_dictPartID;

    public override void InitAnimData()
    {
        m_AnimData.Add(new CAnimInfo(kAnimEnum.Idle, "Idle01"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.MoveForward, "Forward01"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Rush, "Forward01"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Attack, "Attack02"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Dead, "Death01"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_DeadHeadShoot, "Death01"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Hurt, "Damage_body01"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtFront, "Damage_body01"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtBehind, "Damage_body01"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_ShowTime, "Roar01"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_1, "Attack02"));
    }

    public override void InitAudioData()
    {
        m_AudioData.Add(kAudioEnum.HitBody, "Fx_Impact_body");
    }

    public override void InitMob(int nMobID, int nMobLevel)
    {
        base.InitMob(nMobID, nMobLevel);
    }
}
