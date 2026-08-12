using System.Collections.Generic;

public class CCharBossTriceratops : CCharBoss
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
		m_dictPartID.Add("Bip01 L Hand", 3);
		m_dictPartID.Add("Bip01 R Hand", 3);
	}

	public override void InitAnimData()
	{
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Idle, "Trice_Idle01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.MoveForward, "Trice_Run01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.TurnLeft, "Trice_Left_Rotation01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.TurnRight, "Trice_Right_Rotation01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Attack, "Trice_Attack02"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Dead, "Trice_Death01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_DeadHeadShoot, "Trice_Death01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Hurt, "Trice_Damage_body01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtFront, "Trice_Damage_body01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtBehind, "Trice_Damage_body01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Roar, "Trice_Roar01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_ShowTime, "Trice_Roar01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_1, "Trice_Attack01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_2, "Trice_Attack00_1"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_3, "Trice_Attack00_2"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_4, "Trice_Attack00_3"));
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
