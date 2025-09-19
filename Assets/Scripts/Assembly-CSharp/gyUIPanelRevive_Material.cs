using UnityEngine;

public class gyUIPanelRevive_Material : MonoBehaviour
{
	public GameObject mTitleText;

	public GameObject mStatisticsBackground;

	public gyUIItem[] arrItem;

	public gyUILifeBar mLifeBar;

	public GameObject mReviveButton;

	protected bool m_bShow;

	protected int m_nStep;

	protected float m_fStepCount;

	protected float m_fTime;

	protected float m_fTimeCount;

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
		if (!(m_fTimeCount < m_fTime))
		{
			return;
		}
		m_fTimeCount += deltaTime;
		if (mLifeBar != null)
		{
			mLifeBar.SetValue(1f - m_fTimeCount / m_fTime);
		}
		if (m_fTimeCount >= m_fTime)
		{
			mLifeBar.gameObject.SetActiveRecursive(false);
			if (mReviveButton != null)
			{
				mReviveButton.SetActiveRecursive(false);
			}
		}
	}

	public void Show(bool bShow)
	{
		m_bShow = bShow;
		base.gameObject.active = bShow;
		if (bShow)
		{
			base.transform.localPosition = Vector3.zero;
			return;
		}
		ShowStatistcs(bShow);
		base.transform.localPosition = new Vector3(10000f, 10000f, 10000f);
	}

	public void ShowStatistcs(bool bShow)
	{
		mTitleText.SetActiveRecursive(bShow);
		mStatisticsBackground.SetActiveRecursive(bShow);
	}

	public void ShowItem(int nIndex, bool bShow)
	{
		if (nIndex >= 0 && nIndex < arrItem.Length)
		{
			arrItem[nIndex].gameObject.SetActiveRecursive(bShow);
		}
	}

	public void SetIcon(int nIndex, UIAtlas atlas)
	{
		if (nIndex >= 0 && nIndex < arrItem.Length)
		{
			arrItem[nIndex].SetIcon(atlas);
		}
	}

	public void SetCount(int nIndex, int nCount)
	{
		if (nIndex >= 0 && nIndex < arrItem.Length)
		{
			arrItem[nIndex].SetCount(nCount);
		}
	}

	public void SetReviveTime(float fTime)
	{
		if (!(mLifeBar == null) && !(mReviveButton == null))
		{
			m_fTime = fTime;
			m_fTimeCount = 0f;
			mLifeBar.gameObject.SetActiveRecursive(true);
			mLifeBar.InitValue(1f);
			mReviveButton.SetActiveRecursive(true);
		}
	}
}
