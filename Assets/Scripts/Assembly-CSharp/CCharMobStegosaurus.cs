using System.Collections.Generic;

public class CCharMobStegosaurus : CCharBoss
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
        m_dictPartID.Add("Bip01 Spine1", 2);
        m_dictPartID.Add("Bip01 L Foot", 3);
        m_dictPartID.Add("Bip01 R Foot", 3);
        m_dictPartID.Add("Bip01 L Hand", 3);
        m_dictPartID.Add("Bip01 R Hand", 3);
    }
    public override void InitAnimData()
    {
        m_AnimData.Add(new CAnimInfo(kAnimEnum.Idle, "Idle01"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.MoveForward, "Forward01"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Rush, "Forward01"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Attack, "Attack03"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Dead, "Death01"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_DeadHeadShoot, "Death01"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_Hurt, "Damage_body01"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtFront, "Damage_body01"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.BigHurtBehind, "Damage_body01"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.Mob_ShowTime, "Roar01"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_1, "Attack03"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_2, "Attack03"));
        m_AnimData.Add(new CAnimInfo(kAnimEnum.Skill_Action_3, "Attack03"));
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
