using UnityEngine;

public class gyUIGamePauseDialog : MonoBehaviour
{
	public gyUISwitch mMusicSwitch;

	public gyUISwitch mSoundSwitch;

	public UILabel mTaskDesc;

	public GameObject mbtnClose;

	public GameObject mbtnVillage;

	public GameObject mbtnContinue;

	protected bool m_bShow;

	private void Awake()
	{
		Show(false);
	}

	public void Show(bool bShow)
	{
		m_bShow = bShow;
		base.gameObject.SetActiveRecursive(bShow);
		TweenScale tweenScale = TweenScale.Begin(base.gameObject, 0.4f, Vector3.one);
		tweenScale.from = Vector3.zero;
		tweenScale.to = Vector3.one;
		if (bShow)
		{
			base.transform.localPosition = new Vector3(0f, 0f, base.transform.localPosition.z);
		}
		else
		{
			base.transform.localPosition = new Vector3(10000f, 10000f, base.transform.localPosition.z);
		}
	}
}
