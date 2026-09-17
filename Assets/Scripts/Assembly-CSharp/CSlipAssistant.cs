using UnityEngine;

public class CSlipAssistant
{
	public float m_fCurFrameYaw;
	public float m_fCurFramePitch;

	protected float m_fTransTime;
	protected float m_fCurRateSpeed;
	protected float m_fCurRate;
	protected float m_fResetTime;
	protected float m_fResetTimeCount;
	protected float m_fLastSlipTime;
	protected float m_fResetDistance;

	private const float kMaxYawSpeed = 300f;
	private const float kMaxPitchSpeed = 75f;

	public void Init(float fTransTime, float fResetTime, float fResetDistance)
	{
		m_fTransTime = fTransTime;
		m_fResetTime = fResetTime;
		m_fResetDistance = fResetDistance;
		m_fCurRateSpeed = 1f / m_fTransTime;
		m_fCurRate = 0f;
	}

	public bool Slip(Vector2 v2Delta)
	{
		if (v2Delta == Vector2.zero)
		{
			m_fCurFrameYaw = 0f;
			m_fCurFramePitch = 0f;
			m_fCurRate = 0f;
			m_fResetTimeCount = 0f;
			return false;
		}
		if (m_fCurRate < 1f)
		{
			m_fCurRate += m_fCurRateSpeed * Time.deltaTime;
			if (m_fCurRate > 1f) m_fCurRate = 1f;
		}
		float x = Mathf.Clamp(v2Delta.x / (float)Screen.width,  -1f, 1f);
		float y = Mathf.Clamp(v2Delta.y / (float)Screen.height, -1f, 1f);
		float sensitivityMultiplier = SettingsManager.SensitivityMultiplier;
		float maxYaw   = kMaxYawSpeed   * sensitivityMultiplier;
		float maxPitch = kMaxPitchSpeed * sensitivityMultiplier;
		m_fCurFrameYaw   = x * maxYaw   * m_fCurRate;
		m_fCurFramePitch = y * maxPitch * m_fCurRate;
		return true;
	}
}