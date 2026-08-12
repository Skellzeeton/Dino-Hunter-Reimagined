using System.Collections.Generic;

public class CCharBossPterodactylNew : CCharBoss
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
	}

	public override void InitAnimData_Ground()
	{
		m_AnimData.Cleanup();
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Idle, "Amphibious_Ground_Idle"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.MoveForward, "Amphibious_Ground_Walk"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.TurnLeft, "Amphibious_Ground_Turn_left"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.TurnRight, "Amphibious_Ground_Turn_right"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Rush, "Amphibious_Ground_Walk"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Attack, "Amphibious_Ground_Attack01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Dead, "Amphibious_Ground_Die"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_DeadHeadShoot, "Amphibious_Ground_Die"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Hurt, "Amphibious_Ground_Body_Damage"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtFront, "Amphibious_Ground_Body_Damage"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtBehind, "Amphibious_Ground_Body_Damage"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Roar, "Amphibious_Ground_Roar"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_ShowTime, "Amphibious_Ground_Roar"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_1, "Amphibious_Ground_Attack01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_2, "Amphibious_Ground_Attack02"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_3, "Amphibious_Ground_Attack03"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_4, "Amphibious_Ground_Attack04_01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_5, "Amphibious_SkyAttack402"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_6, "Amphibious_Ground_Attack04_03"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_7, "Amphibious_Ground_Take_off"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_11, "Amphibious_SkyAttack01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_12, "Amphibious_SkyAttack02"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_13, "Amphibious_SkyAttack0301"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_14, "Amphibious_SkyAttack0302"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_15, "Amphibious_SkyAttack0303"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_16, "Amphibious_SkyAttack0401"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_17, "Amphibious_SkyAttack0402"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_18, "Amphibious_SkyAttack0403"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_19, "Amphibious_Ground_Idle"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_20, "Amphibious_SkyIdle"));
	}

	public override void InitAnimData_Sky()
	{
		m_AnimData.Cleanup();
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Idle, "Amphibious_SkyIdle"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.MoveForward, "Amphibious_SkyFly"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Attack, "Amphibious_SkyAttack01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Dead, "Amphibious_SkyDie"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_DeadHeadShoot, "Amphibious_SkyDie"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Hurt, "Amphibious_SkyDamage"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtFront, "Amphibious_SkyDamage"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtBehind, "Amphibious_SkyDamage"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Glide, "Amphibious_SkyFly"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Roar, "Amphibious_SkyRoar"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_ShowTime, "Amphibious_SkyRoar"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_1, "Amphibious_Ground_Attack01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_2, "Amphibious_Ground_Attack02"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_3, "Amphibious_Ground_Attack03"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_4, "Amphibious_Ground_Attack04_01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_5, "Amphibious_Ground_Attack04_02"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_6, "Amphibious_Ground_Attack04_03"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_7, "Amphibious_Ground_Take_off"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_11, "Amphibious_SkyAttack01"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_12, "Amphibious_SkyAttack02"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_13, "Amphibious_SkyAttack0301"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_14, "Amphibious_SkyAttack0302"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_15, "Amphibious_SkyAttack0303"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_16, "Amphibious_SkyAttack0401"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_17, "Amphibious_SkyAttack0402"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_18, "Amphibious_SkyAttack0403"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_19, "Amphibious_Ground_Idle"));
		m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_20, "Amphibious_SkyIdle"));
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
