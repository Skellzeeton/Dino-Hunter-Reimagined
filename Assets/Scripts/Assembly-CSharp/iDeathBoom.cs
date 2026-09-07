using UnityEngine;
using System.Collections.Generic;

public class iDeathBoom : _iAnimEventBase
{
	[System.Serializable]
	public class EffectOverridePair
	{
		public int originalID;
		public int overrideID;
	}

	[Header("Effect Overrides")]
	[Tooltip("Map an original effect ID to a replacement. Only matching IDs will be changed.")]
	public List<EffectOverridePair> effectOverrides = new List<EffectOverridePair>();

	public void iDeathBoom_PlayEffect(int nPrefabID)
	{
		int finalID = GetOverriddenID(nPrefabID);
		PlayEffect(finalID);
	}

	private int GetOverriddenID(int originalID)
	{
		foreach (var pair in effectOverrides)
		{
			if (pair.originalID == originalID)
				return pair.overrideID;
		}
		return originalID;
	}
}