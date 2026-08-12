public class CCharMobAnkylosaur : CCharMob
{
	public override void InitAnimData()
	{
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Idle, "Anky_Idle"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.MoveForward, "Anky_Run"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Moribund, "Anky_Changeball"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Moribunding, "Anky_Changeball_Idle"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.MoribundBack, "Anky_Changeball_back"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.MoribundDeath, "Anky_Changeball_death"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Attack, "Anky_Attack01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Dead, "Anky_Death01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_DeadHeadShoot, "Anky_Death01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_DeadHitFly, "Anky_Death02"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Hurt, "Anky_Damage_body"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtFront, "Anky_Damage_body"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtBehind, "Anky_Damage_body"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Roar, "Anky_Roar"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Rush, "Anky_Run"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_1, "Anky_Attack01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_2, "Anky_Attack02"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_3, "Anky_Attack03"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_4, "Anky_Under"));
	}

	public override void InitAudioData()
	{
		m_AudioData.Add(kAudioEnum.HitBody, "Fx_Impact_body");
	}
}
