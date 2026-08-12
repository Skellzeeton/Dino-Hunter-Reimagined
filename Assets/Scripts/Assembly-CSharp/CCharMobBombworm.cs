public class CCharMobBombworm : CCharMob
{
	public override void InitAnimData()
	{
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Idle, "Bombworm_Idle"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.MoveForward, "Bombworm_Fly"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Attack, "Bombworm_Attack"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Dead, "Bombworm_Death"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_DeadFly, "Bombworm_Death"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_DeadHeadShoot, "Bombworm_Death"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_DeadHitFly, "Bombworm_Death"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Hurt, "Bombworm_Idle"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtFront, "Bombworm_Idle"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtBehind, "Bombworm_Idle"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Roar, "Bombworm_Idle"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Rush, "Bombworm_Fly"));
	}

	public override void InitAudioData()
	{
		m_AudioData.Add(kAudioEnum.HitBody, "Fx_Impact_body");
	}
}
