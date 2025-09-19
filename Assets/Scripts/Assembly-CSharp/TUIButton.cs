using UnityEngine;

public class TUIButton : TUIControlImpl
{
	public GameObject m_NormalObj;

	public GameObject m_PressObj;

	public GameObject m_DisableObj;

	public GameObject m_NormalLabelObj;

	public GameObject m_PressLabelObj;

	public GameObject m_DisableLabelObj;

	public bool m_bDisable;

	public bool m_bPressed;

	protected int m_iFingerId = -1;

	public void Start()
	{
		Show();
	}

	public override void Reset()
	{
		base.Reset();
		m_bDisable = false;
		m_bPressed = false;
		m_iFingerId = -1;
		Show();
	}

	public override void Activate(bool bActive)
	{
		base.gameObject.SetActiveRecursive(bActive);
		if (bActive)
		{
			Show();
		}
	}

	public virtual void Disable(bool bValue)
	{
		m_bDisable = bValue;
		Show();
	}

	public virtual void Show()
	{
		if (m_bDisable)
		{
			if (null != m_NormalObj)
			{
				m_NormalObj.SetActiveRecursive(false);
			}
			if (null != m_PressObj)
			{
				m_PressObj.SetActiveRecursive(false);
			}
			if (null != m_DisableObj)
			{
				m_DisableObj.SetActiveRecursive(true);
			}
			if (null != m_NormalLabelObj)
			{
				m_NormalLabelObj.SetActiveRecursive(false);
			}
			if (null != m_PressLabelObj)
			{
				m_PressLabelObj.SetActiveRecursive(false);
			}
			if (null != m_DisableLabelObj)
			{
				m_DisableLabelObj.SetActiveRecursive(true);
			}
		}
		else if (m_bPressed)
		{
			if (null != m_NormalObj)
			{
				m_NormalObj.SetActiveRecursive(false);
			}
			if (null != m_PressObj)
			{
				m_PressObj.SetActiveRecursive(true);
				//CUISound.GetInstance().Play("UI_Drag");
			}
			if (null != m_DisableObj)
			{
				m_DisableObj.SetActiveRecursive(false);
			}
			if (null != m_NormalLabelObj)
			{
				m_NormalLabelObj.SetActiveRecursive(false);
			}
			if (null != m_PressLabelObj)
			{
				m_PressLabelObj.SetActiveRecursive(true);
			}
			if (null != m_DisableLabelObj)
			{
				m_DisableLabelObj.SetActiveRecursive(false);
			}
		}
		else
		{
			if (null != m_NormalObj)
			{
				m_NormalObj.SetActiveRecursive(true);
			}
			if (null != m_PressObj)
			{
				m_PressObj.SetActiveRecursive(false);
			}
			if (null != m_DisableObj)
			{
				m_DisableObj.SetActiveRecursive(false);
			}
			if (null != m_NormalLabelObj)
			{
				m_NormalLabelObj.SetActiveRecursive(true);
			}
			if (null != m_PressLabelObj)
			{
				m_PressLabelObj.SetActiveRecursive(false);
			}
			if (null != m_DisableLabelObj)
			{
				m_DisableLabelObj.SetActiveRecursive(false);
			}
		}
	}
}
