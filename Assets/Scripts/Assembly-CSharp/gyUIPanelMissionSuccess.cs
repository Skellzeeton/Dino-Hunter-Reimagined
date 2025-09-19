using UnityEngine;

public class gyUIPanelMissionSuccess : MonoBehaviour
{
	public GameObject mLightBase;

	public GameObject mLightAnim;

	public GameObject mTitleText;

	public GameObject mTitleIcon;

	public GameObject mStatisticsBackground;

	public gyUIResultExpBar m_CharExp;
	
	public GameObject mStatisticsContext1;

	public GameObject mStatisticsContext2;

	public GameObject mStatisticsContext3;
	
	public GameObject mStatisticsContext4;

	public GameObject mStatisticsContext5;
	
	public GameObject mStatisticsContext6;
	
	public GameObject mStatisticsContext7;
	
	public GameObject mStatisticsContext8;
	
	public GameObject mStatisticsContext9;
	
	public GameObject mStatisticsContext10;
	
	public GameObject mStatisticsContext11;
	
	public gyUIHopNumber mContext1;

	public gyUIHopNumber mContext2;

	public gyUIHopNumber mContext3;

	protected bool m_bShow;

	protected int m_nStep;

	protected float m_fStepCount;

	private void Awake()
	{
		m_bShow = false;
		base.gameObject.SetActiveRecursive(false);
	}

	private void Start()
	{
	}

	private void Update()
	{
		if (!m_bShow)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		switch (m_nStep)
		{
		case 0:
			m_fStepCount -= deltaTime;
			if (m_fStepCount <= 0f)
			{
				mLightAnim.SetActiveRecursive(true);
				mTitleText.SetActiveRecursive(true);
				TweenPosition tweenPosition = TweenPosition.Begin(mTitleText, 0.5f, Vector3.zero);
				tweenPosition.from = new Vector3(0f, 260f, 0f);
				tweenPosition.to = new Vector3(0f, 97f, 0f);
				tweenPosition.method = UITweener.Method.BounceIn;
				m_nStep = 1;
				m_fStepCount = 0.2f;
			}
			break;
		case 1:
			m_fStepCount -= deltaTime;
			if (m_fStepCount <= 0f)
			{
				mTitleIcon.SetActive(true);
				TweenPosition tweenPosition2 = TweenPosition.Begin(mTitleIcon, 0.5f, Vector3.zero);
				tweenPosition2.from = new Vector3(-120f, 260f, 0f);
				tweenPosition2.to = new Vector3(-120f, 83f, 0f);
				tweenPosition2.method = UITweener.Method.BounceIn;
				m_nStep = 2;
				m_fStepCount = 0.1f;
			}
			break;
		case 2:
			m_fStepCount -= deltaTime;
			if (m_fStepCount <= 0f)
			{
				mStatisticsBackground.SetActive(true);
				TweenPosition tweenPosition = TweenPosition.Begin(mStatisticsBackground, 0.1f, Vector3.zero);
				tweenPosition.from = new Vector3(0f, 260f, 0f);
				tweenPosition.to = new Vector3(0f, -12.28027f, 0f);
				tweenPosition.method = UITweener.Method.BounceIn;
				m_nStep = 3;
				m_fStepCount = 0.2f;
			}
			break;
		case 3:
			m_fStepCount -= deltaTime;
			if (m_fStepCount <= 0f)
			{
				CUISound.GetInstance().Play("UI_Bump");
				mStatisticsContext1.SetActive(true);
				m_nStep = 4;
				m_fStepCount = 0.2f;
			}
			break;
		case 4:
			m_fStepCount -= deltaTime;
			if (m_fStepCount <= 0f)
			{
				mStatisticsContext2.SetActive(true);
				m_nStep = 5;
				m_fStepCount = 0.2f;
			}
			break;
		case 5:
			m_fStepCount -= deltaTime;
			if (m_fStepCount <= 0f)
			{
				CUISound.GetInstance().Play("UI_Exp_plus");
				mStatisticsContext3.SetActive(true);
				mContext1.gameObject.SetActive(true);
				m_nStep = 6;
				m_fStepCount = 0.5f;
			}
			break;
		case 6:
			m_fStepCount -= deltaTime;
			if (m_fStepCount <= 0f)
			{
				CUISound.GetInstance().Play("UI_Bump");
				mStatisticsContext4.SetActive(true);
				m_nStep = 7;
				m_fStepCount = 0.2f;
			}
			break;
		case 7:
			m_fStepCount -= deltaTime;
			if (m_fStepCount <= 0f)
			{
				mStatisticsContext5.SetActive(true);
				m_nStep = 8;
				m_fStepCount = 0.2f;
			}
			break;
		case 8:
			m_fStepCount -= deltaTime;
			if (m_fStepCount <= 0f)
			{
				mStatisticsContext6.SetActive(true);
				m_nStep = 9;
				m_fStepCount = 0.2f;
			}
			break;
		case 9:
			m_fStepCount -= deltaTime;
			if (m_fStepCount <= 0f)
			{
				CUISound.GetInstance().Play("UI_Exp_plus");
				mStatisticsContext7.SetActive(true);
				mContext2.gameObject.SetActive(true);
				m_nStep = 10;
				m_fStepCount = 0.5f;
			}
			break;
		case 10:
			m_fStepCount -= deltaTime;
			if (m_fStepCount <= 0f)
			{
				CUISound.GetInstance().Play("UI_Bump");
				mStatisticsContext8.SetActive(true);
				m_nStep = 11;
				m_fStepCount = 0.1f;
			}
			break;
		case 11:
			m_fStepCount -= deltaTime;
			if (m_fStepCount <= 0f)
			{
				mStatisticsContext9.SetActive(true);
				m_nStep = 12;
				m_fStepCount = 0.2f;
			}
			break;
		case 12:
			m_fStepCount -= deltaTime;
			if (m_fStepCount <= 0f)
			{
				mStatisticsContext10.SetActive(true);
				m_nStep = 13;
				m_fStepCount = 0.2f;
			}
			break;
		case 13:
			m_fStepCount -= deltaTime;
			if (m_fStepCount <= 0f)
			{
				CUISound.GetInstance().Play("UI_Exp_plus");
				mStatisticsContext11.SetActive(true);
				mContext3.gameObject.SetActive(true);
				m_nStep = 14;
				m_fStepCount = 0.5f;
			}
			break;
		case 14:
			m_fStepCount -= deltaTime;
			if (m_fStepCount <= 0f)
			{
				CUISound.GetInstance().Play("UI_Exp_plus");
				m_CharExp.gameObject.SetActive(true);
				m_nStep = 15;
				m_fStepCount = 0.1f;
			}
			break;
		}
	}

	public void Show(bool bShow)
	{
		m_bShow = bShow;
		base.gameObject.SetActiveRecursive(bShow);
		mLightBase.SetActive(false);
		mLightAnim.SetActive(false);
		mTitleText.SetActive(false);
		mTitleIcon.SetActive(false);
		mStatisticsBackground.SetActive(false);
		mStatisticsContext1.SetActive(false);
		mStatisticsContext2.SetActive(false);
		mStatisticsContext3.SetActive(false);
		mStatisticsContext4.SetActive(false);
		mStatisticsContext5.SetActive(false);
		mStatisticsContext6.SetActive(false);
		mStatisticsContext7.SetActive(false);
		mStatisticsContext8.SetActive(false);
		mStatisticsContext9.SetActive(false);
		mStatisticsContext10.SetActive(false);
		mStatisticsContext11.SetActive(false);
		mContext1.gameObject.SetActive(false);
		mContext2.gameObject.SetActive(false);
		mContext3.gameObject.SetActive(false);
		m_CharExp.gameObject.SetActive(false);
		if (bShow)
		{
			base.transform.localPosition = new Vector3(0f, 0f, base.transform.localPosition.z);
		}
		else
		{
			base.transform.localPosition = new Vector3(10000f, 10000f, base.transform.localPosition.z);
		}
		if (bShow)
		{
			mLightBase.SetActive(true);
			TweenScale tweenScale = TweenScale.Begin(mLightBase, 0.5f, Vector3.zero);
			tweenScale.from = Vector3.zero;
			tweenScale.to = Vector3.one;
			tweenScale.method = UITweener.Method.EaseIn;
			m_nStep = 0;
			m_fStepCount = 0.5f;
		}
	}

	public void SetCharExp(int lstLevel, float lstRate, int curLevel, float curRate)
	{
		if (!(m_CharExp == null))
		{
			m_CharExp.BarValue = lstRate;
			m_CharExp.Level = lstLevel;
			if (lstLevel != curLevel || lstRate != curRate)
			{
				m_CharExp.SetAnimation(curRate, curLevel);
			}
		}
	}

	public void SetGainCrystal(int nValue)
	{
		if (!(mContext1 == null))
		{
			mContext1.Go(0f, nValue, Mathf.Clamp01((float)nValue / 300f) * 5f);
		}
	}

	public void SetGainGold(int nValue)
	{
		if (!(mContext2 == null))
		{
			mContext2.Go(0f, nValue, Mathf.Clamp01((float)nValue / 300f) * 5f);
		}
	}
	
	public void SetGainGoldEarned(int nValue)
	{
		if (!(mContext3 == null))
		{
			mContext3.Go(0f, nValue, Mathf.Clamp01((float)nValue / 300f) * 5f);
		}
	}

	public bool IsContextHop()
	{
		if (mContext1 == null || !mContext1.gameObject.active || !mContext1.isHop)
		{
			return false;
		}
		if (mContext2 == null || !mContext2.gameObject.active || !mContext2.isHop)
		{
			return false;
		}
		if (mContext3 == null || !mContext3.gameObject.active || !mContext3.isHop)
		{
			return false;
		}
		return true;
	}

	public void StopContextHop()
	{
		if (mContext1 != null)
		{
			mContext1.Stop();
		}
		if (mContext2 != null)
		{
			mContext2.Stop();
		}
		if (mContext3 != null)
		{
			mContext3.Stop();
		}
	}
}
