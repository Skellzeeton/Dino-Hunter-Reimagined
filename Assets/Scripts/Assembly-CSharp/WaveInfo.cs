using System.Collections.Generic;
using UnityEngine;

public class WaveInfo
{
	public int nID;
	public int nOverride = -1;
	public int nEventType;
	public List<int> ltEventParam;
	public bool bEventLoop;
	public bool m_bRandom;
	public int m_nMobCount;
	public float m_fDelayTime;
	public float m_fInterval;
	public int m_nNumAtOnce;
	public int m_nLoop;
	public List<WaveMobInfo> m_ltWaveMobInfo;
	public string sCutScene;
	public string sCutSceneContent;
	public string sCutSceneAmbience;
	public string sCutSceneBGM;
	public int nDefaultMobLevel;

	public WaveInfo()
	{
		nOverride = -1;
		m_bRandom = true;
		m_nMobCount = 0;
		m_fDelayTime = 0f;
		m_fInterval = 0.1f;
		m_nNumAtOnce = 1;
		m_nLoop = -1;
		m_ltWaveMobInfo = new List<WaveMobInfo>();
		sCutScene = string.Empty;
		sCutSceneContent = string.Empty;
		sCutSceneBGM = string.Empty;
		sCutSceneAmbience = string.Empty;
		nDefaultMobLevel = 1;
	}

	public bool IsPredefinedWave()
	{
		if (!m_bRandom) return true;
		if (m_ltWaveMobInfo == null || m_ltWaveMobInfo.Count == 0) return false;
		return m_nMobCount <= 0;
	}

	public WaveMobInfo GetWaveMobInfo(int nIndex)
	{
		if (m_ltWaveMobInfo == null || nIndex < 0 || nIndex >= m_ltWaveMobInfo.Count)
			return null;
		return m_ltWaveMobInfo[nIndex];
	}

	public WaveMobInfo GetWaveMobInfoRandom()
	{
		if (m_ltWaveMobInfo == null || m_ltWaveMobInfo.Count < 1)
			return null;
		if (m_ltWaveMobInfo.Count == 1)
			return m_ltWaveMobInfo[0];
		int totalWeight = 0;
		for (int i = 0; i < m_ltWaveMobInfo.Count; i++)
			totalWeight += m_ltWaveMobInfo[i].nRandom;
		if (totalWeight <= 0)
			return m_ltWaveMobInfo[Random.Range(0, m_ltWaveMobInfo.Count)];
		int roll = Random.Range(1, totalWeight + 1);
		int cumulative = 0;
		for (int i = 0; i < m_ltWaveMobInfo.Count; i++)
		{
			cumulative += m_ltWaveMobInfo[i].nRandom;
			if (roll <= cumulative)
				return m_ltWaveMobInfo[i];
		}
		return m_ltWaveMobInfo[m_ltWaveMobInfo.Count - 1];
	}

	public int GetWaveMobInfoCount()
	{
		return m_ltWaveMobInfo?.Count ?? 0;
	}

	public int GetWaveMobCount()
	{
		if (IsPredefinedWave())
			return m_ltWaveMobInfo?.Count ?? 0;
		return m_nMobCount;
	}
}