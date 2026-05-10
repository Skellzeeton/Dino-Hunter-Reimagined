using UnityEngine;

public class LevelPointCoop : MonoBehaviour
{
	public GameObject drop_sign;

	public void ShowDropSign(bool m_show)
	{
		if (drop_sign == null)
		{
			Debug.LogWarning("error!");
		}
		else
		{
			drop_sign.SetActiveRecursive(m_show);
		}
	}
}
