using System.Collections.Generic;

public class CCharBossDilphosaurus : CCharBoss
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
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Idle, "Dilo_Idle01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.MoveForward, "Dilo_Run01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.TurnLeft, "Dilo_Left_Rotation01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.TurnRight, "Dilo_Right_Rotation01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Attack, "Dilo_Attack00_1"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Dead, "Dilo_Death01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_DeadHeadShoot, "Dilo_Death01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Hurt, "Dilo_Damage_body01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Hurt_Head, "Dilo_Head_Damage01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Hurt_Leg, "Dilo_Damage_foot01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtFront, "Dilo_Head_Damage01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtBehind, "Dilo_Damage_foot01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Roar, "Dilo_Roar01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_ShowTime, "Dilo_Roar01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_1, "Dilo_Attack01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_2, "Dilo_Attack00_1"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_3, "Dilo_Attack01_left"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_4, "Dilo_Attack01_right"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_5, "Dilo_Attack01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_6, "Dilo_Attack_Pee"));
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
