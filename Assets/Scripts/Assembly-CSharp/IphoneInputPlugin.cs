using System.Text.RegularExpressions;
using UnityEngine;

public class IphoneInputPlugin
{
	public delegate void OnEvent(string str);

	protected static IphoneInputPlugin m_Instance;

	protected OnEvent m_OnDone;

	protected TouchScreenKeyboard m_KeyBoard;

	protected int m_nLimitLength;

	protected Regex m_Regex;

	protected int m_nEnterCount;

	public string sValue { get; set; }

	private bool m_IsOpenOnPC;

	private bool m_DoneOnPC;

	private static bool IsPCPlatform
	{
		get { return !Application.isMobilePlatform; }
	}

	public IphoneInputPlugin()
	{
		sValue = string.Empty;
		m_OnDone = null;
		m_KeyBoard = null;
		m_nLimitLength = 0;
	}

	public static IphoneInputPlugin GetInstance()
	{
		if (m_Instance == null)
		{
			m_Instance = new IphoneInputPlugin();
		}
		return m_Instance;
	}

	public void Update(float deltaTime)
	{
		if (IsPCPlatform)
		{
			UpdatePC();
			return;
		}
		if (m_KeyBoard == null)
		{
			return;
		}
		bool isActive;
		bool isDone;
		try
		{
			isActive = m_KeyBoard.active;
			isDone = m_KeyBoard.done;
		}
		catch (System.NullReferenceException)
		{
			if (m_OnDone != null)
			{
				m_OnDone(sValue);
			}
			m_KeyBoard = null;
			return;
		}
		if (!isActive || isDone)
		{
			if (m_OnDone != null)
			{
				m_OnDone(sValue);
			}
			m_KeyBoard.active = false;
			m_KeyBoard = null;
			return;
		}
		if (m_KeyBoard.text.Length < 1)
		{
			sValue = string.Empty;
			return;
		}
		m_nEnterCount = 0;
		for (int i = 0; i < m_KeyBoard.text.Length; i++)
		{
			if (m_KeyBoard.text[i] == '\n')
			{
				m_nEnterCount++;
			}
		}
		if (m_nEnterCount > 2)
		{
			m_KeyBoard.text = sValue;
			return;
		}
		if (m_nLimitLength > 0 && m_KeyBoard.text.Length > m_nLimitLength)
		{
			m_KeyBoard.text = m_KeyBoard.text.Substring(0, m_nLimitLength);
		}
		if (m_KeyBoard.text != sValue)
		{
			if (!m_Regex.IsMatch(m_KeyBoard.text))
			{
				m_KeyBoard.text = sValue;
			}
			else
			{
				sValue = m_KeyBoard.text;
			}
		}
	}

	private void UpdatePC()
	{
		if (!m_IsOpenOnPC) return;
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			m_IsOpenOnPC = false;
			m_DoneOnPC = false;
			return;
		}
		string input = Input.inputString;
		for (int i = 0; i < input.Length; i++)
		{
			char c = input[i];
			if (c == '\b')
			{
				if (sValue.Length > 0)
				{
					sValue = sValue.Substring(0, sValue.Length - 1);
				}
				continue;
			}

			if (c == '\n' || c == '\r')
			{
				m_DoneOnPC = true;
				continue;
			}
			if (c < ' ')
			{
				continue;
			}
			if (m_nLimitLength > 0 && sValue.Length >= m_nLimitLength)
			{
				continue;
			}
			string candidate = sValue + c;
			if (m_Regex != null && !m_Regex.IsMatch(candidate))
			{
				continue;
			}
			sValue = candidate;
		}
		if (m_DoneOnPC)
		{
			m_IsOpenOnPC = false;
			m_DoneOnPC = false;
			OnEvent cb = m_OnDone;
			m_OnDone = null;
			if (cb != null) cb(sValue);
		}
	}

	public void Open(string title, string text, int limitlength, OnEvent ondone, TouchScreenKeyboardType keyboardtype = TouchScreenKeyboardType.Default, string regexstr = "^[\\w]+${0,12}", bool secure = false, bool autocorrection = true, bool multiline = false, bool alert = false)
	{
		m_nLimitLength = limitlength;
		m_OnDone = ondone;
		m_Regex = new Regex(regexstr);
		sValue = text;
		if (!m_Regex.IsMatch(sValue))
		{
			sValue = string.Empty;
		}
		if (IsPCPlatform)
		{
			m_IsOpenOnPC = true;
			m_DoneOnPC = false;
			return;
		}
		m_KeyBoard = TouchScreenKeyboard.Open(text, keyboardtype, autocorrection, multiline, secure, alert, title);
	}

	public bool IsOpen
	{
		get
		{
			if (IsPCPlatform)
				return m_IsOpenOnPC;
			return m_KeyBoard != null;
		}
	}

	public void Cancel()
	{
		m_OnDone = null;
		m_DoneOnPC = false;
		if (IsPCPlatform)
		{
			m_IsOpenOnPC = false;
			return;
		}
		if (m_KeyBoard != null)
		{
			m_KeyBoard.active = false;
			m_KeyBoard = null;
		}
	}
}