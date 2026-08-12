public class CCharMobDilophosaurus : CCharMob
{
	public override void InitAnimData()
	{
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Idle, "Dilo_Idle01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.MoveForward, "Dilo_Run01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Attack, "Dilo_Attack00_1"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Dead, "Dilo_Death01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_DeadHitFly, "Dilo_Death02"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_DeadHeadShoot, "Death01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Hurt, "Dilo_Damage_body01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Hurt_Head, "Dilo_Head_Damage01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Hurt_Leg, "Dilo_Damage_foot01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtFront, "Dilo_Head_Damage01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtBehind, "Dilo_Damage_foot01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Roar, "Dilo_Roar01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_5, "Dilo_Attack00"));
	}
	public override void InitAudioData()
	{
		m_AudioData.Add(kAudioEnum.HitBody, "Fx_Impact_body");
	}
}
