using System.Collections;
using EventCenter;
using LitJson;
using UnityEngine;

public class iWindowAccount : MonoBehaviour
{
	public KeyCode[] m_arrKeyCode;

	public string[] m_arrAccount;

	protected iGameData m_GameData
	{
		get
		{
			return iGameApp.GetInstance().m_GameData;
		}
	}

	protected iDataCenter m_DataCenter
	{
		get
		{
			if (m_GameData == null)
			{
				return null;
			}
			return m_GameData.GetDataCenter();
		}
	}
	
	protected void OnLoginSuccess()
	{
		iServerVerify.CServerConfigInfo serverConfigInfo = iServerVerify.GetInstance().GetServerConfigInfo();
		if (serverConfigInfo != null && serverConfigInfo.m_sServerMessage.Length > 0 && iServerSaveData.GetInstance().IsBackgroundRelogin)
		{
			if (!iGameApp.GetInstance().UpgradeVersion("3.1.7a"))
			{
				OnLoginFailed(iLoginManager.kFailedType.Timeout);
				return;
			}
			CMessageBoxScript.GetInstance().MessageBox(serverConfigInfo.m_sServerTitle, serverConfigInfo.m_sServerMessage, null, null, "OK");
			iTrinitiDataCollect.GetInstance().SetUserSymbol(iServerSaveData.GetInstance().CurDeviceId);
			iTrinitiDataCollect.GetInstance().SetUserName(m_DataCenter.NickName);
			if (iServerSaveData.GetInstance().m_bFirstRegister)
			{
				CTrinitiCollectManager.GetInstance().SendRegister();
			}
			CTrinitiCollectManager.GetInstance().SendLogin();
		}
		iGameState gameState = iGameApp.GetInstance().m_GameState;
		if (gameState != null)
		{
			gameState.m_bNeedAutoSaleUI = true;
		}
		iGameData gameData = iGameApp.GetInstance().m_GameData;
		if (gameData != null)
		{
			iDataCenter dataCenter = gameData.GetDataCenter();
			if (dataCenter != null)
			{
				for (int i = 0; i < m_arrAccount.Length; i++)
				{
					if (!(MyUtils.g_sWindowsAccount == m_arrAccount[i]) && !dataCenter.IsFriend(m_arrAccount[i]))
					{
						dataCenter.AddFriend(m_arrAccount[i]);
					}
				}
			}
		}
		global::EventCenter.EventCenter.Instance.Publish(this, new TUIEvent.BackEvent_SceneMain(TUIEvent.SceneMainEventType.TUIEvent_ConnectResult, true));
	}

	protected void OnLoginFailed(iLoginManager.kFailedType type)
	{
		switch (type)
		{
		case iLoginManager.kFailedType.VersionError:
			global::EventCenter.EventCenter.Instance.Publish(this, new TUIEvent.BackEvent_SceneMain(TUIEvent.SceneMainEventType.TUIEvent_ConnectResult, false, 2));
			break;
		case iLoginManager.kFailedType.ServerMaintain:
			global::EventCenter.EventCenter.Instance.Publish(this, new TUIEvent.BackEvent_SceneMain(TUIEvent.SceneMainEventType.TUIEvent_ConnectResult, false, 5));
			break;
		case iLoginManager.kFailedType.GameCenterChanged:
			CMessageBoxScript.GetInstance().MessageBox(string.Empty, "Game Center ID changed. Please re-login.", OnLoginFailedOnOK, null, "Reconnect");
			break;
		case iLoginManager.kFailedType.GMUsing:
			global::EventCenter.EventCenter.Instance.Publish(this, new TUIEvent.BackEvent_SceneMain(TUIEvent.SceneMainEventType.TUIEvent_ConnectResult, false, 4));
			break;
		default:
			global::EventCenter.EventCenter.Instance.Publish(this, new TUIEvent.BackEvent_SceneMain(TUIEvent.SceneMainEventType.TUIEvent_ConnectResult, false, 3));
			break;
		}
	}

	protected void OnLoginNetError()
	{
		global::EventCenter.EventCenter.Instance.Publish(this, new TUIEvent.BackEvent_SceneMain(TUIEvent.SceneMainEventType.TUIEvent_ConnectResult, false, 1));
	}

	protected void OnLoginFailedOnOK()
	{
		iGameApp.GetInstance().EnterScene("Scene_Main");
	}

	protected void CheckConfig()
	{
		iGameData gameData = iGameApp.GetInstance().m_GameData;
		if (gameData == null)
		{
			return;
		}
		foreach (WaveInfo value in gameData.m_MGCenter.GetData().Values)
		{
			foreach (WaveMobInfo item in value.m_ltWaveMobInfo)
			{
				if (gameData.m_MobCenter.Get(item.nID) == null)
				{
					Debug.LogWarning(value.nID + "'s mob " + item.nID + " is not exist");
				}
			}
		}
	}
}
