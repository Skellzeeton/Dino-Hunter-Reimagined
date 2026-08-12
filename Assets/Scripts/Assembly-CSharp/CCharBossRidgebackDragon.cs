using System.Collections.Generic;

public class CCharBossRidgebackDragon : CCharBoss
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
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Idle, "Spino_Idle01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.MoveForward, "Spino_Run01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.TurnLeft, "Spino_Left_Ratation01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.TurnRight, "Spino_Right_Ratation01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Attack, "Spino_Attack00"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Dead, "Spino_Death01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_DeadFly, "Spino_Death01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_DeadHeadShoot, "Spino_Death01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Hurt, "Spino_Damage_body01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Hurt_Head, "Spino_Head_Damage01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Hurt_Leg, "Spino_Damage_foot01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtFront, "Spino_Head_Damage01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtBehind, "Spino_Head_Damage01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_ShowTime, "Spino_Roar01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Rush, "Spino_Run01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_1, "Spino_Attack01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_2, "Spino_Attack02"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_3, "Spino_Attack03"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_4, "Spino_Attack03_1"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_5, "Spino_Attack03_3"));
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
