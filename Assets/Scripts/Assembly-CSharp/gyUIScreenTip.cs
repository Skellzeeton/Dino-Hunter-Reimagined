using UnityEngine;

public class gyUIScreenTip : MonoBehaviour {
	public float m_fCloseDistance = 10f;

	public GameObject m_Actor;

	public GameObject m_Target;

	public GameObject m_Item;

	public GameObject m_Arrow;

	protected bool m_bActive;

	protected Transform mTransform;

	protected UISprite m_SpriteItem;

	protected UISprite m_SpriteArrow;

	protected gyUIAnimationBump m_AnimItem;

	protected gyUIAnimationHop m_AnimArrow;

	protected bool m_bForceHidden;

	public void SetForceHidden(bool bHidden)
	{
		if (m_bForceHidden == bHidden)
			return;
		m_bForceHidden = bHidden;
		base.gameObject.SetActiveRecursive(m_bActive && !m_bForceHidden);
	}

	public bool isActive
	{
		get
		{
			return m_bActive;
		}
		set
		{
			m_bActive = value;
			base.gameObject.SetActiveRecursive(m_bActive);
		}
	}

	private void Awake()
	{
		m_bActive = false;
		mTransform = base.transform;
		m_fCloseDistance *= m_fCloseDistance;
		m_SpriteItem = m_Item.GetComponentInChildren<UISprite>();
		m_SpriteArrow = m_Arrow.GetComponentInChildren<UISprite>();
		m_AnimItem = m_Item.GetComponentInChildren<gyUIAnimationBump>();
		m_AnimArrow = m_Arrow.GetComponentInChildren<gyUIAnimationHop>();
	}

	private void Update()
	{
		if (!m_bActive || m_Actor == null || m_Target == null)
			return;
		Vector3 forward = m_Actor.transform.forward;
		forward.y = 0f;
		Vector3 toTarget = m_Target.transform.position - m_Actor.transform.position;
		Vector3 toTargetDir = toTarget.normalized;
		toTargetDir.y = 0f;
		bool isInFront = Vector3.Dot(forward, toTargetDir) > 0f;
		Vector3 screenPos = Camera.main.WorldToScreenPoint(m_Target.transform.position);
		float halfW = Screen.width * 0.5f;
		float halfH = Screen.height * 0.5f;
		Vector2 localPos = new Vector2(screenPos.x - halfW, screenPos.y - halfH);
		float tipTopExtent = 0f;
		if (m_Arrow != null)
			tipTopExtent += m_Arrow.transform.localScale.x;
		if (m_Item != null)
			tipTopExtent += m_Item.transform.localScale.x + m_Item.transform.localScale.y * 0.5f;
		float tipHalfWidth = 0f;
		if (m_Item != null)
			tipHalfWidth = m_Item.transform.localScale.x * 0.5f;
		if (m_Arrow != null)
			tipHalfWidth = Mathf.Max(tipHalfWidth, m_Arrow.transform.localScale.x * 0.5f);
		float screenTop    = Screen.height - tipTopExtent;
		float screenLeft   = tipHalfWidth;
		float screenRight  = Screen.width - tipHalfWidth;
		bool isOnScreen =
		screenPos.z > 0f &&
		screenPos.x >= screenLeft && screenPos.x <= screenRight &&
		screenPos.y >= 0f        && screenPos.y <= screenTop;
		if (isOnScreen && isInFront)
		{
			SetPos_InScreen(localPos);
		}
		else
		{
			Vector2 dir;
			if (screenPos.z > 0f)
			{
				dir = localPos.sqrMagnitude > 0.0001f ? localPos.normalized : Vector2.up;
			}
			else
			{
				Vector2 flipped = new Vector2(-localPos.x, -localPos.y);
				if (flipped.sqrMagnitude < 0.0001f)
				{
					Vector3 fwd3 = Camera.main.transform.forward;
					flipped = new Vector2(-fwd3.x, -fwd3.z);
				}
				dir = flipped.sqrMagnitude > 0.0001f ? flipped.normalized : Vector2.up;
			}
			SetPos_OutScreen(dir);
		}
	}

	public void Initialize(GameObject actor, GameObject target)
	{
		m_Actor = actor;
		m_Target = target;
		mTransform.localScale = Vector3.one;
		isActive = true;
	}

	public void Clear()
	{
		m_Actor = null;
		m_Target = null;
	}

	public void SetIcon(string sIcon)
	{
		if (!(m_SpriteItem == null) && m_SpriteItem != null)
		{
			m_SpriteItem.spriteName = sIcon;
		}
	}

	protected void SetPos_OutScreen(Vector2 dir)
	{
		if (dir.sqrMagnitude < 0.0001f)
			dir = Vector2.up;
		dir = dir.normalized;
		float halfW = Screen.width * 0.5f;
		float halfH = Screen.height * 0.5f;
		float absX = Mathf.Abs(dir.x);
		float absY = Mathf.Abs(dir.y);
		float scale;
		if (absX * halfH > absY * halfW)
			scale = halfW / absX;
		else
			scale = halfH / absY;
		Vector2 pos = dir * scale;
		base.transform.localPosition = pos;
		m_Arrow.transform.right = dir;
		m_Arrow.transform.localPosition = -m_Arrow.transform.right * m_Arrow.transform.localScale.x / 2f;
		m_Item.transform.localPosition = -m_Arrow.transform.right * (m_Arrow.transform.localScale.x + m_Item.transform.localScale.x / 2f);
		if (m_AnimArrow != null)
			m_AnimArrow.Stop();
		if (m_AnimItem != null)
			m_AnimItem.Stop();
	}

	protected void SetPos_InScreen(Vector2 v2ScreenPos)
	{
		base.transform.localPosition = v2ScreenPos;
		m_Arrow.transform.right = -Vector2.up;
		m_Arrow.transform.localPosition = -m_Arrow.transform.right * m_Arrow.transform.localScale.x;
		m_Item.transform.localPosition = -m_Arrow.transform.right * (m_Arrow.transform.localScale.x + m_Item.transform.localScale.x);
		if (m_AnimArrow != null && !m_AnimArrow.isActive)
		{
			m_AnimArrow.Go(-m_Arrow.transform.up);
		}
		if (m_AnimItem != null && !m_AnimItem.isActive)
		{
			m_AnimItem.Go();
		}
	}

	protected bool IsInScreen(Vector3 v3WorldPos, ref Vector2 v2ScreenPos)
	{
		Vector3 vector = Camera.main.WorldToScreenPoint(v3WorldPos);
		if (vector.x >= 0f && vector.x <= (float)Screen.width && vector.y >= 0f && vector.y <= (float)Screen.height)
		{
			v2ScreenPos = new Vector2(vector.x - (float)(Screen.width / 2), vector.y - (float)(Screen.height / 2));
			return true;
		}
		return false;
	}
}
