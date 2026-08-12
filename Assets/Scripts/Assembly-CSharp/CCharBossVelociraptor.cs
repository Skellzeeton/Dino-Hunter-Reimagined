using System.Collections.Generic;

public class CCharBossVelociraptor : CCharBoss
{
	public class kPart
	{
		public const int Head = 1;

		public const int Body = 2;

		public const int Leg = 3;
	}

	protected Dictionary<string, int> m_dictPartID;

	public new void Awake()
	{
		base.Awake();
		m_dictPartID = new Dictionary<string, int>();
		m_dictPartID.Add("Bip01 Head", 1);
		m_dictPartID.Add("Bip01 Spine", 2);
		m_dictPartID.Add("Bip01 L Foot", 3);
		m_dictPartID.Add("Bip01 R Foot", 3);
	}

	public override void InitAnimData()
	{
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Idle, "Velo_idle"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.MoveForward, "Velo_run"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.TurnLeft, "Velo_left"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.TurnRight, "Velo_right"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Attack, "Velo_attack01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Dead, "Velo_death01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_DeadHeadShoot, "Velo_death01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Hurt, "Velo_damage"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtFront, "Velo_damage"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtBehind, "Velo_damage"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Roar, "Velo_Roar"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_ShowTime, "Velo_Roar"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Rush, "Velo_run"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_1, "Velo_attack03_1"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_2, "Velo_attack03_2"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_3, "Velo_attack03_3"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_4, "Velo_Ready"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_5, "Velo_left jump"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_6, "Velo_right jump"));
	}

	public override void InitAudioData()
	{
		m_AudioData.Add(kAudioEnum.HitBody, "Fx_Impact_body");
	}

	public override bool AddHardiness(float fDamage, string sBoneName = "")
	{
		bool result = false;
		if (sBoneName != string.Empty)
		{
			if (!m_dictPartID.ContainsKey(sBoneName))
			{
				return false;
			}
			int key = m_dictPartID[sBoneName];
			if (!m_dictBodyPart.ContainsKey(key))
			{
				return false;
			}
			result = AddHardinessValue(m_dictBodyPart[key], fDamage);
		}
		else
		{
			int count = m_dictBodyPart.Count;
			fDamage /= (float)count;
			foreach (CBodyPart value in m_dictBodyPart.Values)
			{
				if (AddHardinessValue(value, fDamage))
				{
					result = true;
				}
			}
		}
		return result;
	}
}
