using EventCenter;
using UnityEngine;

public class Scene_Main : MonoBehaviour
{
	public TUIFade m_fade;

	private float m_fade_in_time;

	private float m_fade_out_time;

	private bool do_fade_in;

	private bool is_fade_out;

	private bool do_fade_out;

	private string next_scene = "Scene_MainMenu";

	private int next_scene_id;

	private bool is_enter_level_scene;

	private bool sfx_open_now = true;

	private bool music_open_now = true;

	public TUILabel label_text;

	public PopupGlobal popup_warning;

	public PopupSaveManager savePopup;

	private bool m_bShowingSaveError = false;

	private bool connect_success;

	private ServerConnectFailType server_connect_fail;

	public GameObject prefab_popup_server;

	private PopupServer popup_server;

	public Transform tui_control;

	private bool didTheThing;

	private bool m_bSaveLoaded = false;

	private void Awake()
	{
		TUIDataServer.Instance().Initialize();
		label_text.Text = "touch to play";
	}

	private void Start()
	{
		global::EventCenter.EventCenter.Instance.Publish(this, new TUIEvent.SendEvent_SceneMain(TUIEvent.SceneMainEventType.TUIEvent_EnterInfo));
		if (music_open_now)
		{
			CUISound.GetInstance().Play("BGM_startscreen");
			if (CUISound.GetInstance().HasSound("BGM_theme"))
			{
				CUISound.GetInstance().Stop("BGM_theme");
			}
		}
		if (!string.IsNullOrEmpty(iGameApp.PendingPopupMessage))
		{
			m_bShowingSaveError = true;
			if (popup_warning != null)
				popup_warning.ShowPopupYes(iGameApp.PendingPopupMessage);
		}
		else
		{
			ShowSavePopup();
		}
	}

	private void Update()
	{
		if (Input2.touchCount > 0 && !didTheThing && !m_bShowingSaveError && m_bSaveLoaded &&
		!(savePopup != null && savePopup.IsShowing()))
		{
			ProceedToGame();
		}
		if (m_fade == null)
		{
			Debug.LogWarning("error!no found m_fade!");
		}
		m_fade_in_time += Time.deltaTime;
		if (m_fade_in_time >= m_fade.fadeInTime && !do_fade_in)
		{
			do_fade_in = true;
		}
		if (!is_fade_out)
		{
			return;
		}
		m_fade_out_time += Time.deltaTime;
		if (!(m_fade_out_time >= m_fade.fadeOutTime) || do_fade_out)
		{
			return;
		}
		do_fade_out = true;
		m_fade.SetFadeOutEnd();
		if (is_enter_level_scene)
		{
			CUISound.GetInstance().Stop("BGM_theme");
		}
		if (is_enter_level_scene)
		{
			TUIMappingInfo.SwitchSceneInt switchSceneInt = TUIMappingInfo.Instance().GetSwitchSceneInt();
			if (switchSceneInt != null)
			{
				switchSceneInt(next_scene_id);
			}
		}
		else
		{
			TUIMappingInfo.SwitchSceneStr switchSceneStr = TUIMappingInfo.Instance().GetSwitchSceneStr();
			if (switchSceneStr != null)
			{
				switchSceneStr(next_scene);
			}
		}
	}

	private void ShowSavePopup()
	{
		iDataCenter dc = iGameApp.GetInstance().m_GameData.GetDataCenter();
		if (dc == null) return;
		int lastSlot = SettingsManager.Instance.LastSaveSlot;
		var slots = dc.GetSaveSlotsInfo();
		bool slotExists = (lastSlot >= 0 && lastSlot < slots.Count && slots[lastSlot].exists);
		if (slotExists)
		{
			dc.SwitchToSlot(lastSlot);
			if (savePopup != null)
				savePopup.Show();
			m_bSaveLoaded = true;
		}
		else
		{
			int targetSlot = -1;
			int existingCount = 0;
			for (int i = 0; i < slots.Count; i++)
			{
				if (slots[i].exists) existingCount++;
				if (!slots[i].exists && targetSlot == -1)
					targetSlot = i;
			}
			if (targetSlot == -1)
			{
				targetSlot = 0;
			}
			dc.CreateNewSlot(targetSlot);
			SettingsManager.Instance.LastSaveSlot = targetSlot;
			if (savePopup != null)
				savePopup.Show();
			m_bSaveLoaded = true;
			if (existingCount == 0)
			{
				iGameApp.PendingPopupMessage = "No save was detected, the game has created a fresh save for you.";
				if (popup_warning != null)
					popup_warning.ShowPopupYes(iGameApp.PendingPopupMessage);
			}
		}
		if (!string.IsNullOrEmpty(iGameApp.PendingPopupMessage) && popup_warning != null)
		{
			popup_warning.ShowPopupYes(iGameApp.PendingPopupMessage);
		}
	}

	private void OnDestroy()
	{
		if (savePopup != null)
			savePopup.Hide();
		global::EventCenter.EventCenter.Instance.Unregister<TUIEvent.BackEvent_SceneMain>(TUIEvent_SetUIInfo);
	}

	public void OnSaveLoaded()
	{
		m_bSaveLoaded = true;
		if (savePopup != null && savePopup.IsShowing())
			savePopup.Hide();
		iDataCenter dc = iGameApp.GetInstance().m_GameData.GetDataCenter();
		if (dc != null)
		{
			SettingsManager.Instance.LastSaveSlot = dc.CurrentSlot;
		}
	}

	private void ProceedToGame()
	{
		iDataCenter dc = iGameApp.GetInstance().m_GameData.GetDataCenter();
		if (dc != null)
		{
			bool isNewPlayer = (dc.nTutorialVillageState == (int)NewHelpState.None &&
			!dc.IsLevelPassed(1001));
			if (isNewPlayer)
			{
				iGameApp.GetInstance().m_GameState.GameLevel = 1001;
				is_enter_level_scene = true;
				next_scene_id = 2;
			}
			else
			{
				is_enter_level_scene = false;
				next_scene = "Scene_MainMenu";
			}
		}
		else
		{
			is_enter_level_scene = false;
			next_scene = "Scene_MainMenu";
		}
		is_fade_out = true;
		m_fade.FadeOut();
		CUISound.GetInstance().Play("UI_Entergame");
		CUISound.GetInstance().Play("UI_Button");
		CUISound.GetInstance().Play("BGM_theme");
		CUISound.GetInstance().Stop("BGM_startscreen");
		didTheThing = true;
	}

	public void TUIEvent_SetUIInfo(object sender, TUIEvent.BackEvent_SceneMain m_event)
	{
		is_enter_level_scene = m_event.GetControlSuccess();
		if (m_event.GetControlSuccess())
		{
			int wparam = m_event.GetWparam();
			next_scene_id = wparam;
			if (!is_fade_out)
			{
				is_fade_out = true;
				m_fade.FadeOut();
			}
			return;
		}
		int wparam2 = m_event.GetWparam();
		string sceneName = TUIMappingInfo.Instance().GetSceneName(wparam2);
		if (sceneName != string.Empty)
		{
			next_scene = sceneName;
		}
		else
		{
			next_scene = "Scene_MainMenu";

		}
		if (!is_fade_out)
		{
			is_fade_out = true;
			m_fade.FadeOut();
		}
	}

	public void TUIEvent_Enter(TUIControl control, int event_type, float wparam, float lparam, object data)
	{
		if (event_type == 3 && connect_success)
		{
			if (sfx_open_now)
			{
				CUISound.GetInstance().Play("UI_Entergame");
			}
			global::EventCenter.EventCenter.Instance.Publish(this, new TUIEvent.SendEvent_SceneMain(TUIEvent.SceneMainEventType.TUIEvent_EnterLevel));
		}
	}

	public void TUIEvent_CloseWarnning(TUIControl control, int event_type, float wparam, float lparam, object data)
	{
		if (event_type == 3)
		{
			if (sfx_open_now)
			{
				CUISound.GetInstance().Play("UI_Button");
			}
			if (!string.IsNullOrEmpty(iGameApp.PendingPopupMessage))
			{
				iGameApp.PendingPopupMessage = "";
				if (popup_warning != null)
					popup_warning.Hide();
				return;
			}
			if (m_bShowingSaveError)
			{
				m_bShowingSaveError = false;
				if (popup_warning != null)
					popup_warning.Hide();
				iDataCenter dataCenter = iGameApp.GetInstance().m_GameData.GetDataCenter();
				if (dataCenter != null)
				{
					dataCenter.Load();
				}
				return;
			}
			AndroidReturnPlugin.instance.ClearFunc(TUIEvent_CloseWarnning);
			if (server_connect_fail == ServerConnectFailType.NeedNet)
			{
				global::EventCenter.EventCenter.Instance.Publish(this, new TUIEvent.SendEvent_SceneMain(TUIEvent.SceneMainEventType.TUIEvent_ConnectAgain));
			}
			else if (server_connect_fail == ServerConnectFailType.NeedUpdate)
			{
				global::EventCenter.EventCenter.Instance.Publish(this, new TUIEvent.SendEvent_SceneMain(TUIEvent.SceneMainEventType.TUIEvent_GotoUpdate));
			}
			else if (server_connect_fail == ServerConnectFailType.FetchFailed)
			{
				global::EventCenter.EventCenter.Instance.Publish(this, new TUIEvent.SendEvent_SceneMain(TUIEvent.SceneMainEventType.TUIEvent_FetchFailed));
			}
			else if (server_connect_fail == ServerConnectFailType.GMUsing)
			{
				global::EventCenter.EventCenter.Instance.Publish(this, new TUIEvent.SendEvent_SceneMain(TUIEvent.SceneMainEventType.TUIEvent_GMUsing));
			}
			else if (server_connect_fail == ServerConnectFailType.ServerMaintain)
			{
				global::EventCenter.EventCenter.Instance.Publish(this, new TUIEvent.SendEvent_SceneMain(TUIEvent.SceneMainEventType.TUIEvent_ServerMaintain));
			}
			else
			{
				Debug.LogWarning("Unhandled server connect fail type: " + server_connect_fail);
			}
		}
	}

	public void TUIEvent_CloseSavePopup(TUIControl control, int event_type, float wparam, float lparam, object data)
	{
		if (event_type == 3)
		{
			if (sfx_open_now)
			{
				CUISound.GetInstance().Play("UI_Cancle");
			}
			if (savePopup != null)
			{
				iDataCenter dc = iGameApp.GetInstance().m_GameData.GetDataCenter();
				if (dc == null || !dc.m_bSlotLoaded)
				{
					Debug.LogWarning("No save loaded! Please select or create a save before closing.");
					return;
				}
				savePopup.OnBack();
			}
		}
	}

	public void TUIEvent_PopupServerOK(TUIControl control, int event_type, float wparam, float lparam, object data)
	{
		if (event_type == 3)
		{
			if (sfx_open_now)
			{
				CUISound.GetInstance().Play("UI_Button");
			}
			global::EventCenter.EventCenter.Instance.Publish(this, new TUIEvent.SendEvent_SceneMain(TUIEvent.SceneMainEventType.TUIEvent_PopupServerOK));
		}
	}

	public void TUIEvent_PopupServerCancle(TUIControl control, int event_type, float wparam, float lparam, object data)
	{
		if (event_type == 3)
		{
			if (sfx_open_now)
			{
				CUISound.GetInstance().Play("UI_Cancle");
			}
			global::EventCenter.EventCenter.Instance.Publish(this, new TUIEvent.SendEvent_SceneMain(TUIEvent.SceneMainEventType.TUIEvent_PopupServerCancle));
		}
	}

	public void TUIEvent_LoadSlot(TUIControl control, int event_type, float wparam, float lparam, object data)
	{
		if (event_type == 3)
		{
			if (sfx_open_now)
			{
				CUISound.GetInstance().Play("UI_Button");
			}
			int slot = ExtractSlotIndex(control);
			if (savePopup != null)
			{
				iDataCenter dc = iGameApp.GetInstance().m_GameData.GetDataCenter();
				if (dc != null)
				{
					var slots = dc.GetSaveSlotsInfo();
					if (slot >= 0 && slot < slots.Count)
					{
						if (slots[slot].exists)
							savePopup.OnLoadSlot(slot);
						else
							savePopup.OnCreateSlot(slot);
					}
				}
			}
		}
	}

	public void TUIEvent_DeleteSlot(TUIControl control, int event_type, float wparam, float lparam, object data)
	{
		if (event_type == 3)
		{
			if (sfx_open_now)
			{
				CUISound.GetInstance().Play("UI_Button");
			}

			int slot = ExtractSlotIndex(control);
			if (savePopup != null)
				savePopup.OnDeleteSlot(slot);
		}
	}

	private int ExtractSlotIndex(TUIControl control)
	{
		string name = control.name;
		int underscore = name.LastIndexOf('_');
		if (underscore >= 0 && int.TryParse(name.Substring(underscore + 1), out int slot))
			return slot;
		Transform parent = control.transform.parent;
		if (parent != null)
		{
			string parentName = parent.name;
			underscore = parentName.LastIndexOf('_');
			if (underscore >= 0 && int.TryParse(parentName.Substring(underscore + 1), out slot))
				return slot;
		}
		Debug.LogWarning("ExtractSlotIndex: Could not find slot index for control: " + control.name);
		return 0;
	}
}