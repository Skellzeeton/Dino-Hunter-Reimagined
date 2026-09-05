using UnityEngine;

public class gyUIPanelTool : gyUICellPanel
{
	public float fRemainTime = 2f;

	protected bool m_bMoveIn;
	protected float m_fTimeCount;
	protected Collider[] m_arrCollider;

	public UISprite mIconBox;
	public UILabel m_CountLabel;

	public GameObject mBlockObject;

	public bool IsMoveIn
	{
		get { return m_bMoveIn; }
	}

	public new void Awake()
	{
		base.Awake();
		m_bMoveIn = false;
		if (m_arrCell != null && m_arrCell.Length > 0)
		{
			m_arrCollider = new Collider[m_arrCell.Length];
			for (int i = 0; i < m_arrCell.Length; i++)
			{
				m_arrCollider[i] = m_arrCell[i].transform.GetComponent<Collider>();
				if (m_arrCollider[i] != null)
					m_arrCollider[i].enabled = true;
			}
		}
		RegisterOnClickCell(OnClickToolPanel);
	}

	private void Start()
	{
		MoveOut();
	}

	private void Update()
	{
		if (m_bMoveIn)
		{
			m_fTimeCount += Time.deltaTime;
			if (m_fTimeCount >= fRemainTime)
				MoveOut();
		}
		UpdateBlockState();
	}

	public void MoveIn()
	{
		UpdateCount();
		m_bMoveIn = true;
		m_fTimeCount = 0f;
		TweenPosition tween = TweenPosition.Begin(gameObject, 0.5f, Vector3.zero);
		tween.to = new Vector3(0f, 27f, 0f);
		tween.method = UITweener.Method.BounceIn;
	}

	public void MoveOut()
	{
		m_bMoveIn = false;
		TweenPosition tween = TweenPosition.Begin(gameObject, 0.5f, Vector3.zero);
		tween.to = new Vector3(0f, -10f, 0f);
		tween.method = UITweener.Method.EaseOut;
	}

	public void OnClickToolPanel(int nIndex)
	{
		if (!m_bMoveIn)
		{
			MoveIn();
			return;
		}
		iGameSceneBase scene = iGameApp.GetInstance().m_GameScene as iGameSceneBase;
		if (scene == null) return;
		if (scene.isTutorialStage)
		{
			return;
		}
		if (scene.GameStatus != iGameSceneBase.kGameStatus.Gameing) return;
		CCharUser user = scene.GetUser();
		if (user == null) return;
		if (user.m_bBoxUsedThisLevel) return;
		if (user.CurHP >= user.MaxHP) return;
		iDataCenter dataCenter = iGameApp.GetInstance().m_GameData?.GetDataCenter();
		if (dataCenter == null) return;
		int charID = user.ID;
		if (dataCenter.UseBox(charID))
		{
			float healAmount = user.MaxHP - user.CurHP;
			user.AddHP(healAmount);
			user.m_bBoxUsedThisLevel = true;
			UpdateCount();
			MoveOut();
			CUISound.GetInstance().Play("UI_Heal_Use");
		}
		else
		{
			return;
		}
	}

	public void UpdateCount()
	{
		if (m_CountLabel == null) return;

		iGameSceneBase scene = iGameApp.GetInstance().m_GameScene as iGameSceneBase;
		if (scene == null) return;
		CCharUser user = scene.GetUser();
		if (user == null) return;
		iDataCenter dataCenter = iGameApp.GetInstance().m_GameData?.GetDataCenter();
		int count = dataCenter?.GetBoxCount(user.ID) ?? 0;
		m_CountLabel.text = count.ToString();
		UpdateBlockState();
	}

	private void UpdateBlockState()
	{
		if (mBlockObject == null) return;
		iGameSceneBase scene = iGameApp.GetInstance().m_GameScene as iGameSceneBase;
		if (scene == null)
		{
			mBlockObject.SetActive(false);
			return;
		}
		iGameUIBase gameUI = scene.GetGameUI();
		bool tutorialPanelActive = (gameUI != null && gameUI.UIManager != null &&
		gameUI.UIManager.mTutorialsPanel != null &&
		gameUI.UIManager.mTutorialsPanel.gameObject.activeSelf);
		if (tutorialPanelActive)
		{
			mBlockObject.SetActive(false);
			return;
		}
		CCharUser user = scene.GetUser();
		bool blocked = false;
		if (scene.GameStatus != iGameSceneBase.kGameStatus.Gameing)
			blocked = true;
		else if (scene.isTutorialStage)
			blocked = true;
		else if (user.m_bBoxUsedThisLevel)
			blocked = true;
		else if (user.CurHP >= user.MaxHP)
			blocked = true;
		iDataCenter dataCenter = iGameApp.GetInstance().m_GameData?.GetDataCenter();
		int count = dataCenter?.GetBoxCount(user.ID) ?? 0;
		if (count == 0)
		{
			if (!blocked)
				mBlockObject.SetActive(false);
			else
				mBlockObject.SetActive(true);
		}
		else
		{
			mBlockObject.SetActive(blocked);
		}
	}
}