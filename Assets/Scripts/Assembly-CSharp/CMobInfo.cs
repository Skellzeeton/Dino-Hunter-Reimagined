using System.Collections.Generic;

public class CMobInfo
{
    public int nID;
    private int m_nMaxLevel = -1;
    private string m_sScaleType = "exponential";
    public Dictionary<int, CMobInfoLevel> m_dictMobInfoLevel;
    private int m_nMinBoost = 0;
    private int m_nMaxBoost = 0;

    public CMobInfo()
    {
        m_dictMobInfoLevel = new Dictionary<int, CMobInfoLevel>();
    }

    public void Add(int nLevel, CMobInfoLevel mobinfolevel)
    {
        if (!m_dictMobInfoLevel.ContainsKey(nLevel))
        {
            m_dictMobInfoLevel.Add(nLevel, mobinfolevel);
        }
    }

    public CMobInfoLevel Get(int nLevel)
    {
        if (!m_dictMobInfoLevel.ContainsKey(nLevel))
        {
            return null;
        }
        return m_dictMobInfoLevel[nLevel];
    }
    
    public void SetMinBoost(int minBoost)
    {
        m_nMinBoost = minBoost;
    }
    
    public int GetMinBoost()
    {
        return m_nMinBoost;
    }
    
    public void SetMaxBoost(int maxBoost)
    {
        m_nMaxBoost = maxBoost;
    }
    
    public int GetMaxBoost()
    {
        return m_nMaxBoost;
    }
	
    public void SetMaxLevel(int maxLevel)
    {
        m_nMaxLevel = maxLevel;
    }
	
    public int GetMaxLevel()
    {
        return m_nMaxLevel;
    }
	
    public void SetScaleType(string scaleType)
    {
        if (scaleType == "exponential" || scaleType == "linear")
        {
            m_sScaleType = scaleType;
        }
    }
	
    public string GetScaleType()
    {
        return m_sScaleType;
    }
	
    public bool HasBaseData()
    {
        return m_dictMobInfoLevel.ContainsKey(1);
    }
}