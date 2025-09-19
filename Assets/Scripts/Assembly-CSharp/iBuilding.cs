using System.Collections.Generic;
using BehaviorTree;
using gyTaskSystem;
using UnityEngine;

public class iBuilding : MonoBehaviour
{
    public List<iBuildingUnit> m_ltBudingUnit;
    public List<Transform> m_ltAttackPoint;
    public float[] arrLifeRate = new float[3] { 100f, 50f, 0f };
    public int m_nLifeState;
    public float m_fLife;
    public float m_fLifeMax;

    protected TAudioController m_AudioController;

    public bool IsBroken
    {
        get { return m_nLifeState == arrLifeRate.Length - 1; }
    }
    
    private int m_nPrevLifeState = -1;
    

    private void Awake()
    {
        m_AudioController = GetComponent<TAudioController>();
        if (m_AudioController == null)
        {
            m_AudioController = gameObject.AddComponent<TAudioController>();
        }

        m_nLifeState = 0;

        foreach (Transform item in transform)
        {
            if (item.name == "attackpoint")
            {
                if (m_ltAttackPoint == null)
                    m_ltAttackPoint = new List<Transform>();
                m_ltAttackPoint.Add(item);
            }
            else if (item.name == "ZhalanUnit")
            {
                iBuildingUnit component = item.GetComponent<iBuildingUnit>();
                if (component != null)
                {
                    if (m_ltBudingUnit == null)
                        m_ltBudingUnit = new List<iBuildingUnit>();
                    m_ltBudingUnit.Add(component);
                }
            }
        }
    }

    public void Initialize(float fLifeMax)
    {
        m_fLife = fLifeMax;
        m_fLifeMax = fLifeMax;
    }
    public void SetHP(float fLife)
    {
        m_fLife = fLife;
        int newLifeState = arrLifeRate.Length - 1;
        float percent = m_fLife / m_fLifeMax * 100f;

        for (int i = 0; i < arrLifeRate.Length - 1; i++)
        {
            if (percent <= arrLifeRate[i] && percent > arrLifeRate[i + 1])
            {
                newLifeState = i;
                break;
            }
        }

        // Play impact sound when entering life state 1 or 2
        if (newLifeState != m_nLifeState)
        {
            if (newLifeState == 1)
            {
                PlayAudio("Mat_Fence_crash");
                Debug.Log("Barricade entered life state " + newLifeState + ", playing impact sound.");
            }
            if (newLifeState == 2)
            {
                PlayAudio("Mat_Fence_destroy");
                Debug.Log("Barricade entered life state " + newLifeState + ", playing impact sound.");
            }
        }

        m_nLifeState = newLifeState;
        SetModel(m_nLifeState);
    }



    public void AddHP(float fDmg)
    {
        m_fLife += fDmg;
        if (m_fLife > m_fLifeMax) m_fLife = m_fLifeMax;
        else if (m_fLife <= 0f) m_fLife = 0f;

        SetHP(m_fLife);

        if (fDmg < 0f)
        {
            PlayColorAnim(new Color(1f, 0f, 0f, 1f), new Color(1f, 1f, 1f, 1f));
        }
    }

    public Vector3 GetRandomPoint()
    {
        if (m_ltAttackPoint == null || m_ltAttackPoint.Count == 0)
            return transform.position;

        return m_ltAttackPoint[Random.Range(0, m_ltAttackPoint.Count)].position;
    }

    public List<Vector3> GetAttackPoints()
    {
        List<Vector3> points = new List<Vector3>();
        if (m_ltAttackPoint != null)
        {
            for (int i = 0; i < m_ltAttackPoint.Count; i++)
            {
                if (m_ltAttackPoint[i] != null)
                {
                    points.Add(m_ltAttackPoint[i].position);
                }
            }
        }
        return points;
    }

    protected void SetModel(int nIndex)
    {
        if (m_ltBudingUnit == null) return;
        for (int i = 0; i < m_ltBudingUnit.Count; i++)
        {
            if (m_ltBudingUnit[i] != null)
            {
                m_ltBudingUnit[i].SetModel(nIndex);
            }
        }
    }

    protected void PlayColorAnim(Color src, Color dst)
    {
        if (m_ltBudingUnit == null) return;
        for (int i = 0; i < m_ltBudingUnit.Count; i++)
        {
            if (m_ltBudingUnit[i] != null)
            {
                m_ltBudingUnit[i].PlayColorAnim(src, dst);
            }
        }
    }

    public void PlayAudio(string sName)
    {
        if (m_AudioController != null)
        {
            m_AudioController.PlayAudio(sName);
        }
    }

    public void StopAudio(string sName)
    {
        if (m_AudioController != null)
        {
            m_AudioController.StopAudio(sName);
        }
    }
}

