using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CMobInfoLevel
{
    public int nLevel;

    public int nRareType;

    public int nType;

    public int nModel;

    public string sName;

    public string sDesc;

    public string sIcon;

    public float fLife;
    
    public float fLifeMax;
	
    public float fMoveSpeed;
    
    public float fMoveSpeedMax;

    public float fMeleeRange = 5f;

    public float fDamage;
    
    public float fDamageMax;

    public float fHardiness;
    
    public float fHardinessMax;
    
    public bool bIgnoreKnock;
    
    public int nMinBoost = 1;
    
    public int nMaxBoost = 45;

    public bool nBypassAutoAI = false;

    public List<SkillComboRateInfo> ltSkill;

    public List<int> ltSkillPassive;

    public int nAIManagerID;

    public float fMoveSpeedRate = 1f;

    public float fRushSpeedRate = 1f;

    public bool isWaitRot;

    public int nGoldRate = 5;

    public int nGold;
    
    public int nGoldMax;
    
    public int nGoldBonusRate = 20;

    public int nExp;
    
    public int nExpMax;
    
    public int nCrystal = 1;
    
    public float nCrystalRate = 0.2f;

    public int nDropGroup;

    public int[] arrDropCount;

    public List<CHardinessInfo> ltHardinessInfo;

    public CMobInfoLevel()
    {
        ltSkill = new List<SkillComboRateInfo>();
        ltSkillPassive = new List<int>();
        ltHardinessInfo = new List<CHardinessInfo>();
        arrDropCount = new int[1] { -1 };
    }

    public int GetDropItemCount()
    {
        if (arrDropCount != null && arrDropCount.Length > 0)
            return arrDropCount[0];
        return -1;
    }
}