public class CCharMobDilophosaurus : CCharMob
{
	public override void InitAnimData()
	{
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Idle, "Idle01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.MoveForward, "Run01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Attack, "Attack00_1"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Dead, "Death01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_DeadHitFly, "Death02"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_DeadHeadShoot, "Death01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Hurt, "Dagame_body01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Hurt_Head, "Head_Damage01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Hurt_Leg, "Dagame_foot01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtFront, "Head_Damage01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtBehind, "Dagame_foot01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Roar, "Roar01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_5, "Attack00"));
	}
	public override void InitAudioData()
	{
		m_AudioData.Add(kAudioEnum.HitBody, "Fx_Impact_body");
	}
}
