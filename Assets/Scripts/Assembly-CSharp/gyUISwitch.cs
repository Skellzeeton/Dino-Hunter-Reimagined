using UnityEngine;

public class gyUISwitch : MonoBehaviour
{
	public UISprite mIconOn;

	public UISprite mIconOff;

	private void Awake()
	{
		Switch(false);
	}



	public void Switch(bool on)
	{
		if (mIconOn != null)
		{
			mIconOn.gameObject.SetActiveRecursive(on);
		}
		if (mIconOff != null)
		{
			mIconOff.gameObject.SetActiveRecursive(!on);
		}
	}
}
