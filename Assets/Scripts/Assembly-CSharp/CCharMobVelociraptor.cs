public class CCharMobVelociraptor : CCharMob
{
	public override void InitAnimData()
	{
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Idle, "idle"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.MoveForward, "run"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Attack, "attack01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Dead, "death01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_DeadHeadShoot, "death01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_DeadHitFly, "death02"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Hurt, "damage"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtFront, "damage"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtBehind, "damage"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Roar, "Roar"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Rush, "run"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_ShowTime, "Appearance"));
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
