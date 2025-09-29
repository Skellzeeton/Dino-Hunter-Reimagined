using UnityEngine;

public class CControlWindows : CControlBase
{
	public CControlWindows()
	{
		m_GameScene = iGameApp.GetInstance().m_GameScene;
		m_GameUI = m_GameScene.GetGameUI();
		m_GameUI.RegisterEvent_Windows();
	}

	public override void Initialize()
	{
		base.Initialize();
		mouseSensitivity = PlayerPrefs.GetFloat("mouseSensitivity", 1f);
		if (mouseSensitivity < minSensitivity)
		{
			mouseSensitivity = 1f;
			PlayerPrefs.SetFloat("mouseSensitivity", mouseSensitivity);
		}
	}

	//private bool m_TutorialActive = false;
	
	private float mouseSensitivity;
	private const float minSensitivity = 0.5f;
	private const float maxSensitivity = 5f;


	private void SetPause(bool pause)
	{
		if (m_GameScene != null)
		{
			m_GameScene.SetPause(pause);
		}
	}


	public override void Update(float deltaTime)
	{
		if (m_GameScene == null)
		{
			return;
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (m_GameScene.GameStatus == iGameSceneBase.kGameStatus.Gameing)
			{
				Cursor.lockState = CursorLockMode.None;
				Cursor.visible = true;
				m_GameScene.SetGamePause(true);
			}
			else if (m_GameScene.GameStatus == iGameSceneBase.kGameStatus.Pause)
				m_GameScene.SetGamePause(false);
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
		/*if (Input.GetKeyDown(KeyCode.T))
		{
			if (!m_TutorialActive)
			{
				CSoundScene.GetInstance().StopBGM();
				m_User.SetFire(false);
				m_User.MoveStop();
				Time.timeScale = 0f;
				m_GameScene.StartTutorial();
				m_TutorialActive = true;
			}
			else
			{
				CSoundScene.GetInstance().PlayBGM();
				Time.timeScale = 1f;
				m_GameScene.FinishTutorial();
				m_TutorialActive = false;
			}
		}*/
		if (Input.GetKeyDown(KeyCode.O))
		{
			mouseSensitivity = Mathf.Clamp(mouseSensitivity + 0.25f, minSensitivity, maxSensitivity);
			PlayerPrefs.SetFloat("mouseSensitivity", mouseSensitivity);
			PlayerPrefs.Save();
			Debug.Log("Sensitivity increased: " + mouseSensitivity);
		}
		else if (Input.GetKeyDown(KeyCode.I))
		{
			mouseSensitivity = Mathf.Clamp(mouseSensitivity - 0.25f, minSensitivity, maxSensitivity);
			PlayerPrefs.SetFloat("mouseSensitivity", mouseSensitivity);
			PlayerPrefs.Save();
			Debug.Log("Sensitivity decreased: " + mouseSensitivity);
		}
		if (m_GameScene.GameStatus == iGameSceneBase.kGameStatus.CutScene && Input.GetKeyDown(KeyCode.Space))
			CCameraRoam.GetInstance().Stop();
		if (m_User == null || (m_GameScene.GameStatus != iGameSceneBase.kGameStatus.Gameing && m_GameScene.GameStatus != iGameSceneBase.kGameStatus.GameOver_ShowTime))
		{
			return;
		}
		Vector2 zero = Vector2.zero;
		if (m_User.IsCanMove())
		{
			if (Input.GetKey(KeyCode.W))
			{
				if (Input.GetKey(KeyCode.UpArrow))
				{
					return;
				}
				zero.y += 1f;
			}
			if (Input.GetKey(KeyCode.UpArrow))
			{
				if (Input.GetKey(KeyCode.W))
				{
					return;
				}
				zero.y += 1f;
			}
			if (Input.GetKey(KeyCode.S))
			{
				if (Input.GetKey(KeyCode.DownArrow))
				{
					return;
				}
				zero.y += -1f;
			}
			if (Input.GetKey(KeyCode.DownArrow))
			{
				if (Input.GetKey(KeyCode.S))
				{
					return;
				}
				zero.y += -1f;
			}
			if (Input.GetKey(KeyCode.A))
			{
				if (Input.GetKey(KeyCode.LeftArrow))
				{
					return;
				}
				zero.x += -1f;
			}
			if (Input.GetKey(KeyCode.LeftArrow))
			{
				if (Input.GetKey(KeyCode.A))
				{
					return;
				}
				zero.x += -1f;
			}
			if (Input.GetKey(KeyCode.D))
			{
				if (Input.GetKey(KeyCode.RightArrow))
				{
					return;
				}
				zero.x += 1f;
			}
			if (Input.GetKey(KeyCode.RightArrow))
			{
				if (Input.GetKey(KeyCode.D))
				{
					return;
				}
				zero.x += 1f;
			}
			if (zero.x != 0f && zero.y != 0f)
			{
				zero.x *= 0.707f;
				zero.y *= 0.707f;
			}
		}
		if (zero == Vector2.zero)
		{
			m_User.MoveStop();
		}
		else
		{
			m_User.MoveByCompass(zero.x, zero.y);
			Ray ray = m_Camera.ScreenPointToRay(m_GameState.ScreenCenter, 0f);
			m_User.LookAt(ray.GetPoint(1000f));

		}
		if (Screen.lockCursor)
		{
			float axis = Input.GetAxis("Mouse X");
			if (axis != 0f)
			{
				m_Camera.Yaw(axis * 270f * Time.deltaTime * mouseSensitivity);
				if (m_User.IsCanAim())
				{
					m_User.SetYaw(m_Camera.GetYaw());
					Ray ray = m_Camera.ScreenPointToRay(m_GameState.ScreenCenter, 0f);
					m_User.LookAt(ray.GetPoint(1000f));
				}
			}
			float axis2 = Input.GetAxis("Mouse Y");
			if (axis2 != 0f)
			{
				m_Camera.Pitch(axis2 * 270f * Time.deltaTime * mouseSensitivity);
			}
			if (Input.GetMouseButton(1))
			{
				if (m_User.IsCanAim() && (axis != 0f || axis2 != 0f))
				{

				}
				if (Mathf.Abs(axis) > 0.1f || Mathf.Abs(axis2) > 0.1f)
				{
					m_GameScene.AssistAim_Stop();
				}
				else if (m_User.IsFire() && !m_GameScene.IsAssistAim())
				{
					m_GameScene.AssistAim_Start();
				}
			}
			else
			{
				if (!m_User.IsFire() && m_GameScene.IsAssistAim())
				{
					m_GameScene.AssistAim_Stop();
				}
			}
		}
		if (Input.GetKeyDown(KeyCode.R))
		{
			if (m_User.IsCanAttack() && !m_User.IsSkillCD())
			{
				m_User.UseSkill(m_User.SkillID, m_User.SkillLevel);
			}
		}
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			if (m_GameScene.CurGameLevelInfo.m_bLimitMelee)
			{
				return;
			}
			int curWeaponIndex = m_User.CurWeaponIndex;
			int num = curWeaponIndex = 0;
			while (num != curWeaponIndex && m_GameState.GetWeapon(num) == null)
			{
				num--;
				if (num < 0)
				{
					num = 0;
				}
			}
			
			m_User.SwitchWeapon(num);
			CUISound.GetInstance().Play("UI_Weapon_change");
		}
		if (Input.GetKeyDown(KeyCode.Q))
		{
			if (m_GameScene.CurGameLevelInfo.m_bLimitMelee)
			{
				return;
			}
			int curWeaponIndex = m_User.CurWeaponIndex;
			int num = curWeaponIndex - 1;
			while (num != curWeaponIndex && m_GameState.GetWeapon(num) == null)
			{
				num--;
				if (num < 0)
				{
					num = 2;
				}
			}
			
			m_User.SwitchWeapon(num);
			CUISound.GetInstance().Play("UI_Weapon_change");
		}
		if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			if (m_GameScene.CurGameLevelInfo.m_bLimitMelee)
			{
				return;
			}
			int curWeaponIndex2 = m_User.CurWeaponIndex;
			int num2 = curWeaponIndex2 = 1;
			while (num2 != curWeaponIndex2 && m_GameState.GetWeapon(num2) == null)
			{
				num2++;
				if (num2 >= 1)
				{
					num2 = 0;
				}
			}
			m_User.SwitchWeapon(num2);
			CUISound.GetInstance().Play("UI_Weapon_change");
		}
		if (Input.GetKeyDown(KeyCode.E))
		{
			if (m_GameScene.CurGameLevelInfo.m_bLimitMelee)
			{
				return;
			}
			int curWeaponIndex2 = m_User.CurWeaponIndex;
			int num2 = curWeaponIndex2 + 1;
			while (num2 != curWeaponIndex2 && m_GameState.GetWeapon(num2) == null)
			{
				num2++;
				if (num2 >= 3)
				{
					num2 = 0;
				}
			}
			m_User.SwitchWeapon(num2);
			CUISound.GetInstance().Play("UI_Weapon_change");
		}
		if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			if (m_GameScene.CurGameLevelInfo.m_bLimitMelee)
			{
				return;
			}
			int curWeaponIndex3 = m_User.CurWeaponIndex;
			int num3 = curWeaponIndex3 = 2;
			while (num3 != curWeaponIndex3 && m_GameState.GetWeapon(num3) == null)
			{
				num3++;
				if (num3 >= 2)
				{
					num3 = 0;
				}
			}
			m_User.SwitchWeapon(num3);
			CUISound.GetInstance().Play("UI_Weapon_change");
		}
		if (Input.GetKeyDown(KeyCode.Alpha9))
		{
			Debug.Log("press 9 key");
			m_GameScene.FinishGame(true);
			if (CGameNetManager.GetInstance().IsConnected())
			{
				CGameNetSender.GetInstance().SendMsg_GAME_OVER(true);
			}
		}
		if (!Input.GetKeyDown(KeyCode.Alpha0))
		{
			return;
		}
		Debug.Log("press 0 key");
		foreach (CCharMob item in m_GameScene.GetMobEnumerator())
		{
			CCharBoss cCharBoss = item as CCharBoss;
			if (cCharBoss != null)
			{
				cCharBoss.SetReadyToBlack(true, 2000f);
			}
		}
	}

	public override void LateUpdate(float deltaTime)
	{
	}
}
