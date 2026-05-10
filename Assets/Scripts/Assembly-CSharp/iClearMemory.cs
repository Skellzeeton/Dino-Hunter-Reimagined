using System;
using System.Collections;
using UnityEngine;

public class iClearMemory : MonoBehaviour
{
	private void Awake()
	{
		DontDestroyOnLoad(base.gameObject);
	}

	public void ClearMemory()
	{
		ClearImmidately();
	}

	protected IEnumerator Clear()
	{
		GC.Collect();
		yield return Resources.UnloadUnusedAssets();
	}

	protected void ClearImmidately()
	{
		GC.Collect();
		Resources.UnloadUnusedAssets();
	}
}
