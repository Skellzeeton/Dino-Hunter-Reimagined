using gyTaskSystem;
using UnityEngine;

public class iGameTaskUICollect : iGameTaskUIBase
{
	public UISprite mBackground;
	public UIFilledSprite mSpritePrecents;
	public UISprite m_TargetIcon;
	public UILabel m_CurNum;
	public UILabel m_MaxNum;
	public UILabel m_Slash;

	public UILabel m_LabelLife;

	protected CTaskCollection m_TaskCollect;
	protected int m_nIndex;

	private void Awake()
	{
		base.Height = 25f;
		if (base.Height < m_TargetIcon.transform.localScale.y)
			base.Height = m_TargetIcon.transform.localScale.y;
		if (base.Height < m_CurNum.transform.localScale.y)
			base.Height = m_CurNum.transform.localScale.y;
		if (base.Height < m_MaxNum.transform.localScale.y)
			base.Height = m_MaxNum.transform.localScale.y;
		if (m_LabelLife != null && base.Height < m_LabelLife.transform.localScale.y)
			base.Height = m_LabelLife.transform.localScale.y;
		ShowLifeBar(false);
	}

	private void Update()
	{
		float deltaTime = Time.deltaTime;
	}

	public void Initialize(CTaskBase taskbase, int nIndex)
	{
		base.Initialize(taskbase);
		m_nIndex = nIndex;
	}

	public void SetIcon(string sIcon)
	{
		if (m_TargetIcon != null)
			m_TargetIcon.spriteName = sIcon;
	}

	public void SetCurNum(int nNum)
	{
		if (m_CurNum != null)
			m_CurNum.text = nNum.ToString();
	}

	public void SetMaxNum(int nNum)
	{
		if (m_MaxNum != null)
			m_MaxNum.text = nNum.ToString();
	}

	public void SetPrecents(float fPrecents)
	{
		if (mSpritePrecents != null)
			mSpritePrecents.fillAmount = fPrecents;
	}

	public void SetLife(float fCurLife, float fMaxLife)
	{
		if (m_LabelLife == null)
			return;
		m_LabelLife.text = Mathf.CeilToInt(fCurLife) + "/" + Mathf.CeilToInt(fMaxLife);
	}

	public override void Show(bool bShow)
	{
		if (m_TargetIcon != null)
			m_TargetIcon.gameObject.SetActiveRecursive(bShow);
		if (m_CurNum != null)
			m_CurNum.gameObject.SetActiveRecursive(bShow);
		if (m_Slash != null)
			m_Slash.gameObject.SetActiveRecursive(bShow);
		if (m_MaxNum != null)
			m_MaxNum.gameObject.SetActiveRecursive(bShow);
		if (!bShow)
			ShowLifeBar(bShow);
	}

	public override void InitTask(CTaskBase taskbase)
	{
		m_TaskCollect = m_curTaskBase as CTaskCollection;
		if (m_TaskCollect != null &&
		m_nIndex >= 0 &&
		m_nIndex < m_TaskCollect.m_ltCollections.Count)
		{
			CTaskCollection.CCollect c = m_TaskCollect.m_ltCollections[m_nIndex];
			SetCurNum(c.nCurCount);
			SetMaxNum(c.nMaxCount);
			SetPrecents(c.m_fMaxLife > 0f ? c.m_fCurLife / c.m_fMaxLife : 0f);
			SetLife(c.m_fCurLife, c.m_fMaxLife);
		}
	}

	public void ShowLifeBar(bool bShow)
	{
		if (mBackground != null)
			mBackground.gameObject.SetActiveRecursive(bShow);
		if (mSpritePrecents != null)
			mSpritePrecents.gameObject.SetActiveRecursive(bShow);
		if (m_LabelLife != null)
			m_LabelLife.gameObject.SetActiveRecursive(bShow);
	}

	public bool IsLifeBarShow()
	{
		if (mBackground != null)
			return mBackground.gameObject.active;
		return false;
	}
}