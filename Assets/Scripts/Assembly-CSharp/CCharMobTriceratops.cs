public class CCharMobTriceratops : CCharMob
{
	public override void InitAnimData()
	{
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Idle, "Trice_Idle01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.MoveForward, "Trice_Run01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Attack, "Trice_Attack02"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Dead, "Trice_Death01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_DeadHitFly, "Trice_Death02"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_DeadHeadShoot, "Trice_Death01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Hurt, "Trice_Damage_body01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtFront, "Trice_Damage_body01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtBehind, "Trice_Damage_body01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Roar, "Trice_Roar01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_1, "Trice_Attack01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_2, "Trice_Attack00_1"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_3, "Trice_Attack00_2"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_4, "Trice_Attack00_3"));
	}

	public override void InitAudioData()
	{
		m_AudioData.Add(kAudioEnum.HitBody, "Fx_Impact_body");
	}
}
