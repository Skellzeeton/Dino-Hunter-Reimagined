using System.Collections.Generic;

public class CCharBossTyrannosaurus : CCharBoss
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
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Idle, "Tyran_idle"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.MoveForward, "Tyran_run"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.TurnLeft, "Tyran_left"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.TurnRight, "Tyran_right"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Attack, "Tyran_attack01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Dead, "Tyran_death"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_DeadFly, "Tyran_death"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_DeadHeadShoot, "Tyran_death"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Hurt, "Tyran_damage"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Hurt_Head, "Tyran_damage_head"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Hurt_Leg, "Tyran_damage_leg"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtFront, "Tyran_damage"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtBehind, "Tyran_damage"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_ShowTime, "Tyran_roar"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Roar, "Tyran_roar"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_MoveRoar, "Tyran_roar"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Rush, "Tyran_rush"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_1, "Tyran_attack01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_2, "Tyran_attack02"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_3, "Tyran_attack03"));
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
