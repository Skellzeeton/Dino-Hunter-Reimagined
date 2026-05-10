using UnityEngine;

public class gyUITutorialsPanel : MonoBehaviour
{
	public GameObject mMask;

	public GameObject[] arrTutorials;


	public void Show(bool bShow)
	{
		base.gameObject.SetActiveRecursive(bShow);
		if (arrTutorials == null)
		{
			return;
		}
		for (int i = 0; i < arrTutorials.Length; i++)
		{
			if (arrTutorials[i] != null)
			{
				arrTutorials[i].SetActiveRecursive(bShow);
			}
		}
	}
}
