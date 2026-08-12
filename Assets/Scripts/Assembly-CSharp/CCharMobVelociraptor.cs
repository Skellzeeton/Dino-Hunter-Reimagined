public class CCharMobVelociraptor : CCharMob
{
	public override void InitAnimData()
	{
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Idle, "Velo_idle"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.MoveForward, "Velo_run"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Attack, "Velo_attack01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Dead, "Velo_death01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_DeadHeadShoot, "Velo_death01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_DeadHitFly, "Velo_death02"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Hurt, "Velo_damage"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtFront, "Velo_damage"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtBehind, "Velo_damage"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Roar, "Velo_Roar"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Rush, "Velo_run"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_ShowTime, "Velo_Appearance"));
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
