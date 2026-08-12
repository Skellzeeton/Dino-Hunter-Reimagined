public class CCharMobPterodactyl : CCharMob
{
	public override void InitAnimData()
	{
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Idle, "Ptero_idle"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.MoveForward, "Ptero_fly"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Attack, "Ptero_attack01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Dead, "Ptero_death01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_DeadFly, "Ptero_fly_death"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_DeadHeadShoot, "Ptero_fly_death"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_DeadHitFly, "Ptero_fly_death"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Hurt, "Ptero_damage"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtFront, "Ptero_damage"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtBehind, "Ptero_damage"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Glide, "Ptero_fly_circle"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_1, "Ptero_attack02_01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_2, "Ptero_attack02"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_3, "Ptero_attack05"));
	}

	public override void InitAudioData()
	{
		m_AudioData.Add(kAudioEnum.HitBody, "Fx_Impact_body");
	}
}
