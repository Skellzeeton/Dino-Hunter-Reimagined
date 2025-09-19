using UnityEngine;

public class iCameraTrail : iCamera
{
	public CCharBase m_Target;

	public static Vector3 camera_offset_normal = new Vector3(1.2f, 2.2f, -2.6f);
	public static Vector3 camera_offset_shoot = new Vector3(1f, 2f, -1.2f);
	public static Vector3 camera_offset_melee = new Vector3(2f, 4f, -5f);
	public static Vector3 camera_offset_block = new Vector3(0f, 2f, 0f);
	public static Vector3 camera_lookat = new Vector3(1f, 0f, 5f);

	protected AudioListener m_AudioListenerCamera;

	protected AudioListener m_AudioListenerTarget;

	protected Vector3 m_v3Camera_Offset_Near;

	protected Vector3 m_v3Camera_Offset_Far;

	protected Vector3 m_v3Camera_Offset_Cur;

	protected float m_fSmoothSpeed;

	protected float m_fCurCameraDis;

	protected float m_fMaxCameraDis;

	public new void Awake()
	{
		base.Awake();
		m_AudioListenerCamera = GetComponent<AudioListener>();
		m_AudioListenerCamera.enabled = false;
	}

	public void Initialize(CCharBase target, bool bMeleeView = false)
	{
		if (m_AudioListenerTarget == null)
		{
			m_AudioListenerTarget = target.gameObject.AddComponent<AudioListener>();
		}
		SwitchToTargetListener();
		ShootMode(false);
		SetViewMelee(bMeleeView);
		SetPitch(30f);
		m_v3Camera_Offset_Near = camera_offset_block;
		m_v3Camera_Offset_Cur = camera_offset_normal;
		m_Target = target;
		m_fMaxCameraDis = Vector3.Distance(camera_offset_normal, camera_offset_block);
		m_fCurCameraDis = m_fMaxCameraDis;
		m_fSmoothSpeed = 1f;
		Quaternion quaternion = Quaternion.Euler(0f - m_fPitch, m_fYaw, 0f);
		m_CameraController.Position = m_Target.Pos + quaternion * camera_offset_normal;
	}

	public void Destroy()
	{
		m_Target = null;
	}

	public new void Update()
	{
		if (!m_bActive || m_Target == null)
		{
			return;
		}
		Quaternion quaternion = Quaternion.Euler(0f - m_fPitch, m_fYaw, 0f);
		Quaternion quaternion2 = Quaternion.Euler(0f, m_fYaw, 0f);
		m_CameraController.Rotation = quaternion;
		m_v3Camera_Offset_Cur = Vector3.Lerp(m_v3Camera_Offset_Cur, m_v3Camera_Offset_Far, m_fSmoothSpeed * Time.deltaTime);
		m_fMaxCameraDis = Vector3.Distance(m_v3Camera_Offset_Cur, m_v3Camera_Offset_Near);
		Vector3 vector = m_Target.Pos + quaternion * camera_lookat;
		Vector3 vector2 = m_Target.Pos + quaternion * m_v3Camera_Offset_Near;
		Vector3 vector3 = m_Target.Pos + quaternion * m_v3Camera_Offset_Cur;
		float num = Vector3.Distance(vector2, vector3);
		Vector3 direction = (vector3 - vector2) / num;
		RaycastHit hitInfo;
		if (Physics.Raycast(vector2, direction, out hitInfo, num + 0.3f, -1610612736))
		{
			m_fCurCameraDis = Vector3.Distance(vector2, hitInfo.point) - 0.3f;
		}
		else
		{
			m_fCurCameraDis += m_fSmoothSpeed * Time.deltaTime;
			if (m_fCurCameraDis > m_fMaxCameraDis)
			{
				m_fCurCameraDis = m_fMaxCameraDis;
			}
		}
		Vector3 vector4 = Vector3.Lerp(vector2, vector3, m_fCurCameraDis / m_fMaxCameraDis);
		Vector3 forward = base.transform.forward;
		base.transform.forward = vector - vector4;
		m_CameraController.Rotation = base.transform.rotation;
		m_CameraController.Position = vector4;
		base.transform.forward = forward;
	}

	public void Yaw(float angle)
	{
		m_fYaw += angle;
		MyUtils.LimitAngle(ref m_fYaw, m_fYawMin, m_fYawMax);
	}

	public void SetYaw(float angle)
	{
		m_fYaw = angle;
		MyUtils.LimitAngle(ref m_fYaw, m_fYawMin, m_fYawMax);
	}

	public void Pitch(float angle)
	{
		m_fPitch += angle;
		MyUtils.LimitAngle(ref m_fPitch, m_fPitchMin, m_fPitchMax);
	}

	public void SetPitch(float angle)
	{
		m_fPitch = angle;
		MyUtils.LimitAngle(ref m_fPitch, m_fPitchMin, m_fPitchMax);
	}

	public float GetYaw()
	{
		return m_fYaw;
	}

	public float GetPitch()
	{
		return m_fPitch;
	}

	public void SetViewMelee(bool on)
	{
		m_fSmoothSpeed = 5f;
		if (on)
		{
			m_v3Camera_Offset_Far = camera_offset_melee;
		}
		else
		{
			m_v3Camera_Offset_Far = camera_offset_normal;
		}
	}

	public void ShootMode(bool on)
	{
		if (on)
		{
			m_fSmoothSpeed = 8f;
			m_v3Camera_Offset_Far = camera_offset_shoot;
		}
		else
		{
			m_fSmoothSpeed = 5f;
			m_v3Camera_Offset_Far = camera_offset_normal;
		}
	}

	public void SwitchToTargetListener()
	{
		if (m_AudioListenerTarget != null)
		{
			m_AudioListenerTarget.enabled = true;
		}
		m_CameraController.ActiveListener(false);
	}

	public void SwitchToCameraListener()
	{
		if (m_AudioListenerTarget != null)
		{
			m_AudioListenerTarget.enabled = false;
		}
		m_CameraController.ActiveListener(true);
	}
}