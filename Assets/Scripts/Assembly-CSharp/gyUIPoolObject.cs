using UnityEngine;

public class gyUIPoolObject : MonoBehaviour
{
	protected Transform m_NodeFree;
	protected Transform m_NodePlay;
	protected bool      m_bFree;
	protected float     m_fBackTime;
	protected float     m_fBackTimeCount;

	protected Vector3   m_v3BaseScale;

	public bool isFree { get { return m_bFree; } }

	private void Update()
	{
		if (!(m_fBackTime <= 0f))
		{
			m_fBackTimeCount += Time.deltaTime;
			if (!(m_fBackTimeCount < m_fBackTime))
			{
				m_fBackTimeCount = 0f;
				m_fBackTime      = 0f;
				TakeBack(0f);
			}
		}
	}

	public void Initialize(Transform nodefree, Transform nodeplay)
	{
		m_NodeFree = nodefree;
		m_NodePlay = nodeplay;
		m_v3BaseScale = base.transform.localScale;
		TakeBack(0f);
	}

	public void GiveOut(float fTime = 0f)
	{
		m_bFree = false;
		Vector3 localScale = base.transform.localScale;
		base.transform.parent = m_NodePlay;
		base.transform.localScale = localScale;
		m_fBackTime = fTime;
		m_fBackTimeCount = 0f;
	}

	public void TakeBack(float fTime = 0f)
	{
		if (m_bFree) return;
		if (fTime <= 0f)
		{
			m_bFree = true;
			base.transform.parent        = m_NodeFree;
			base.transform.localScale    = m_v3BaseScale;
			base.transform.localPosition = Vector3.zero;
			base.transform.localRotation = Quaternion.identity;
			base.gameObject.SetActiveRecursive(false);
		}
		else if (m_fBackTime <= 0f)
		{
			m_fBackTime = fTime;
			m_fBackTimeCount = 0f;
		}
	}
}
