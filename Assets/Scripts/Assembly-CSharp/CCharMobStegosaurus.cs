public class CCharMobStegosaurus : CCharMob
{
    public override void InitAnimData()
    {
        m_AnimData.Add(new CAnimInfo(kAnimEnum.Idle, "Stego_Idle01"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.MoveForward, "Stego_Forward01"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Rush, "Stego_Forward01"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Attack, "Stego_Attack02"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Dead, "Stego_Death01"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_DeadHeadShoot, "Stego_Death01"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Hurt, "Stego_Damage_body01"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtFront, "Stego_Damage_body01"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtBehind, "Stego_Damage_body01"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Roar, "Stego_Roar01"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_ShowTime, "Stego_Roar01"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_1, "Stego_Attack02"));
    }

    public override void InitAudioData()
    {
        m_AudioData.Add(kAudioEnum.HitBody, "Fx_Impact_body");
    }
}
