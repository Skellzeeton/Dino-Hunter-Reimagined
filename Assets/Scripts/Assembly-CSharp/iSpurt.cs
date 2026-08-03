using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EffectPair
{
	public int normalID;
	public int eliteID;
}

public class iSpurt : _iAnimEventBase
{
	public enum EffectMode
	{
		Normal,
		Elite
	}

	[Header("Effect Mapping")]
	public List<EffectPair> effectPairs = new List<EffectPair>();

	[Header("Mode Selection")]
	public EffectMode mode = EffectMode.Normal;

	public void iSpurt_PlayEffect(int nPrefabID)
	{
		int idToUse = nPrefabID;

		if (mode == EffectMode.Elite)
		{
			bool found = false;
			foreach (var pair in effectPairs)
			{
				if (pair.normalID == nPrefabID)
				{
					idToUse = pair.eliteID;
					found = true;
					break;
				}
			}
			if (!found)
			{
				idToUse = nPrefabID;
				Debug.LogWarning("No elite mapping found for normal ID " + nPrefabID + ", using normal ID.");
			}
		}
		else
		{
			idToUse = nPrefabID;
		}

		PlayEffect(idToUse);
	}

	protected override void TransformRefresh(GameObject o)
	{
		base.TransformRefresh(o);
		o.transform.forward = m_Node.up;
		o.transform.position = o.transform.position + o.transform.forward * 0.2f;
	}
}