using UnityEngine;

public class Popup_Option : MonoBehaviour
{
	public TUIButtonPush btn_music;
	public TUIButtonPush btn_sfx;
	public TUISlider slider_sensitivity;
	public TUILabel  label_sensitivity;
	public TUISlider slider_music;
	public TUISlider slider_sfx;
	public TUILabel  label_music;
	public TUILabel  label_sfx;
	public Transform go_popup;

	private bool music_open_now = true;
	private bool sfx_open_now = true;

	public void SetOption(bool m_music_open, bool m_sfx_open)
	{
		music_open_now = m_music_open;
		sfx_open_now = m_sfx_open;
		btn_music.m_bPressed = !music_open_now;
		btn_music.Show();
		btn_sfx.m_bPressed = !sfx_open_now;
		btn_sfx.Show();
		RefreshSensitivity();
		RefreshMusicVolume();
		RefreshSFXVolume();
	}

	public void RefreshSensitivity()
	{
		float value = SettingsManager.Instance.MouseSensitivity;
		if (slider_sensitivity != null)
		{
			float t = Mathf.Clamp01(value / SettingsManager.MaxSensitivity);
			slider_sensitivity.sliderValue = t;
			slider_sensitivity.ForceVisualUpdate();
		}
		if (label_sensitivity != null)
			label_sensitivity.Text = value.ToString("0.###");
	}

	public void RefreshMusicVolume()
	{
		float value = SettingsManager.Instance.MusicVolume;
		if (slider_music != null)
		{
			slider_music.sliderValue = Mathf.Clamp01(value);
			slider_music.ForceVisualUpdate();
		}
		if (label_music != null)
			label_music.Text = Mathf.RoundToInt(value * 100) + "%";
	}

	public void RefreshSFXVolume()
	{
		float value = SettingsManager.Instance.SoundVolume;
		if (slider_sfx != null)
		{
			slider_sfx.sliderValue = Mathf.Clamp01(value);
			slider_sfx.ForceVisualUpdate();
		}
		if (label_sfx != null)
			label_sfx.Text = Mathf.RoundToInt(value * 100) + "%";
	}

	public void Show()
	{
		base.transform.localPosition = new Vector3(0f, 0f, base.transform.localPosition.z);
		base.transform.localPosition = new Vector3(0f, 0f, base.transform.localPosition.z);
		if (go_popup != null && go_popup.GetComponent<Animation>() != null)
			go_popup.GetComponent<Animation>().Play();
	}

	public void Hide()
	{
		base.transform.localPosition = new Vector3(0f, -1000f, base.transform.localPosition.z);
	}

	public void SetMusicNow()
	{
		music_open_now = !music_open_now;
		btn_music.m_bPressed = !music_open_now;
		btn_music.Show();
	}

	public void SetSFXNow()
	{
		sfx_open_now = !sfx_open_now;
		btn_sfx.m_bPressed = !sfx_open_now;
		btn_sfx.Show();
	}

	public bool GetMusicNow() => music_open_now;
	public bool GetSFXNow()   => sfx_open_now;

	public void RestoreOption()
	{
		btn_music.m_bPressed = !music_open_now;
		btn_music.Show();
		btn_sfx.m_bPressed = !sfx_open_now;
		btn_sfx.Show();
		RefreshSensitivity();
		RefreshMusicVolume();
		RefreshSFXVolume();
	}
}