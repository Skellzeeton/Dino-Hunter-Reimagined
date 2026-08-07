using UnityEngine;

public class gyUISkillButton : MonoBehaviour
{
	public UIFilledSprite mMask;
	public UISprite mIcon;
	public UISprite mIconAnim;
	public UISprite mMutiplyFlag;
	public UILabel cooldownLabel;
	protected bool m_bCD;
	protected float m_fTime;
	protected float m_fTimeCount;
	private float m_fLastSoundPlayTime;
	public bool m_bPause { get; set; }

	private void Awake()
	{
		mMask.fillAmount = 0f;
		mMutiplyFlag.enabled = false;
		m_bPause = false;
		m_fLastSoundPlayTime = 0f;
		if (cooldownLabel != null)
		{
			cooldownLabel.text = "";
			cooldownLabel.gameObject.SetActiveRecursive(false);
		}
	}

	private void Update()
	{
		if (m_bCD && !m_bPause)
		{
			m_fTimeCount += Time.deltaTime;
			if (m_fTimeCount >= m_fTime)
			{
				FinishCD();
			}
			else
			{
				float remaining = m_fTime - m_fTimeCount;
				mMask.fillAmount = 1f - m_fTimeCount / m_fTime;
				if (cooldownLabel != null && cooldownLabel.gameObject.activeSelf)
				{
					if (remaining <= 5f)
					{
						if (Time.time - m_fLastSoundPlayTime >= 0.1f)
						{
							CUISound.GetInstance().Play("UI_Skill_cd");
							m_fLastSoundPlayTime = Time.time;
						}
						cooldownLabel.text = "[FF0000]" + remaining.ToString("0.0") + "s[-]";
					}
					else
					{
						cooldownLabel.text = remaining.ToString("0.0") + "s";
					}
				}
			}
		}
	}

	public void SetIcon(string str)
	{
		if (mMask == null || mIcon == null) return;
		mMask.spriteName = str;
		mIcon.spriteName = str;
		mIconAnim.spriteName = str;
	}

	public void SetCD(float fTime)
	{
		m_bCD = true;
		m_fTime = fTime;
		m_fTimeCount = 0f;
		mMask.fillAmount = 1f;
		m_fLastSoundPlayTime = 0f;
		if (cooldownLabel != null)
		{
			cooldownLabel.gameObject.SetActive(true);
			float remaining = m_fTime;
			if (remaining <= 5f)
			{
				CUISound.GetInstance().Play("UI_Skill_cd");
				m_fLastSoundPlayTime = Time.time;
				cooldownLabel.text = "[FF0000]" + remaining.ToString("0.0") + "s[-]";
			}
			else
			{
				cooldownLabel.text = remaining.ToString("0.0") + "s";
			}
		}
	}

	public void FinishCD()
	{
		m_bCD = false;
		mMask.fillAmount = 0f;
		m_fLastSoundPlayTime = 0f;
		TweenAlpha tweenAlpha = TweenAlpha.Begin(mIconAnim.gameObject, 0.5f, 0f);
		tweenAlpha.from = 1f;
		tweenAlpha.to = 0f;
		TweenScale tweenScale = TweenScale.Begin(mIconAnim.gameObject, 0.5f, Vector3.zero);
		tweenScale.from = mIcon.transform.localScale;
		tweenScale.to = tweenScale.from * 2f;
		if (cooldownLabel != null)
		{
			cooldownLabel.text = "";
			cooldownLabel.gameObject.SetActive(false);
		}
		CUISound.GetInstance().Stop("UI_Skill_cd");
		CUISound.GetInstance().Play("UI_Skill_ready");
	}

	public void SetMutiplyFlag(bool bShow)
	{
		if (mMutiplyFlag == null) return;
		mMutiplyFlag.enabled = bShow;
	}
}