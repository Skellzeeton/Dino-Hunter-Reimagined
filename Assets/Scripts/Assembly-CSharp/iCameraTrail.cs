using UnityEngine;

public class iCameraTrail : iCamera
{
    public CCharBase m_Target;

    public static Vector3 camera_offset_normal = new Vector3(1.2f, 2.2f, -2.6f);
    public static Vector3 camera_offset_shoot  = new Vector3(1f,  2f, -1.2f);
    public static Vector3 camera_offset_melee  = new Vector3(2f,  4f, -5f);
    public static Vector3 camera_offset_block  = new Vector3(0f,  2f,  0f);
    public static Vector3 camera_lookat        = new Vector3(1f,  0f,  5f);

    protected AudioListener m_AudioListenerCamera;
    protected AudioListener m_AudioListenerTarget;

    protected Vector3 m_v3Camera_Offset_Near;
    protected Vector3 m_v3Camera_Offset_Far;
    protected Vector3 m_v3Camera_Offset_Cur;

    protected float m_fSmoothSpeed;
    protected float m_fCurCameraDis;
    protected float m_fMaxCameraDis;
    protected float m_fSrcYaw;
    protected float m_fDstYaw;
    protected float m_fSrcPitch;
    protected float m_fDstPitch;
    protected float m_fRateYaw;
    protected float m_fRatePitch;

    public float movementOffsetIntensity = 0.325f;
    public float movementOffsetSmoothing = 2f;
    public float rotationSmooth = 270f;

    private Vector3 lastTargetPos = Vector3.zero;
    private Vector3 movementOffset = Vector3.zero;

    public new void Awake()
    {
        base.Awake();
        m_AudioListenerCamera = GetComponent<AudioListener>();
        if (m_AudioListenerCamera != null)
            m_AudioListenerCamera.enabled = false;

        m_fSrcYaw = m_fDstYaw = 0f;
        m_fSrcPitch = m_fDstPitch = 0f;
        m_fRateYaw = m_fRatePitch = 1f;

        base.enabled = false;
        if (m_CameraController != null) m_CameraController.enabled = false;
        base.enabled = true;
        if (m_CameraController != null) m_CameraController.enabled = true;
    }

    public void Initialize(CCharBase target, bool bMeleeView = false)
    {
        if (target == null) return;

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

        Quaternion q = Quaternion.Euler(0f - m_fPitch, m_fYaw, 0f);
        if (m_CameraController != null)
            m_CameraController.Position = m_Target.Pos + q * camera_offset_normal;

        lastTargetPos = m_Target.Pos;
        movementOffset = Vector3.zero;
    }

    public void Destroy()
    {
        m_Target = null;
    }

    public new void LateUpdate()
    {
        if (!m_bActive || m_Target == null)
            return;

        float dt = Mathf.Max(0.0001f, Time.deltaTime);

        if (m_fRateYaw < 1f)
        {
            m_fYaw = MyUtils.Lerp(m_fYaw, m_fDstYaw, m_fRateYaw);
            m_fRateYaw += 2f * dt;
        }
        if (m_fRatePitch < 1f)
        {
            m_fPitch = MyUtils.Lerp(m_fPitch, m_fDstPitch, m_fRatePitch);
            m_fRatePitch += 2f * dt;
        }

        Vector3 velocity = (m_Target.Pos - lastTargetPos) / dt;
        lastTargetPos = m_Target.Pos;

        velocity.y = 0f;
        Vector3 moveDir = Vector3.zero;
        if (velocity.sqrMagnitude > 0.0001f)
            moveDir = velocity.normalized;

        Vector3 desiredMovementOffset = moveDir * movementOffsetIntensity;
        float alpha = 1f - Mathf.Exp(-movementOffsetSmoothing * dt);
        movementOffset = Vector3.Lerp(movementOffset, desiredMovementOffset, alpha);

        Quaternion baseRot = Quaternion.Euler(-m_fPitch, m_fYaw, 0f);
        m_v3Camera_Offset_Cur = Vector3.Lerp(m_v3Camera_Offset_Cur, m_v3Camera_Offset_Far, m_fSmoothSpeed * dt);

        m_fMaxCameraDis = Vector3.Distance(m_v3Camera_Offset_Cur, m_v3Camera_Offset_Near);
        Vector3 lookPt = m_Target.Pos + baseRot * camera_lookat;
        Vector3 nearPt = m_Target.Pos + baseRot * m_v3Camera_Offset_Near;
        Vector3 farPt  = m_Target.Pos + baseRot * m_v3Camera_Offset_Cur;

        float dist = Vector3.Distance(nearPt, farPt);
        Vector3 dir = dist > 0.0001f ? (farPt - nearPt).normalized : Vector3.forward;
        RaycastHit hit;
        if (Physics.Raycast(nearPt, dir, out hit, dist + 0.3f, -1610612736))
        {
            m_fCurCameraDis = Vector3.Distance(nearPt, hit.point) - 0.3f;
        }
        else
        {
            m_fCurCameraDis += m_fSmoothSpeed * dt;
            if (m_fCurCameraDis > m_fMaxCameraDis) m_fCurCameraDis = m_fMaxCameraDis;
        }

        Vector3 shiftedNearPt = nearPt + movementOffset;
        Vector3 shiftedFarPt  = farPt + movementOffset;
        Vector3 finalPos = Vector3.Lerp(shiftedNearPt, shiftedFarPt, m_fCurCameraDis / m_fMaxCameraDis);

        Vector3 lookDir = (lookPt + movementOffset) - finalPos;
        if (lookDir.sqrMagnitude > 0.0001f && m_CameraController != null)
        {
            Quaternion desiredRot = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
            float rotT = 1f - Mathf.Exp(-rotationSmooth * dt);
            m_CameraController.Rotation = Quaternion.Slerp(m_CameraController.Rotation, desiredRot, rotT);
        }

        if (m_CameraController != null)
            m_CameraController.Position = finalPos;

        // ✅ Character rotation removed — no transform.rotation applied to m_Target
    }

    public void Yaw(float angle)
    {
        m_fDstYaw += angle;
        if (MyUtils.LimitAngle(ref m_fDstYaw, m_fYawMin, m_fYawMax))
        {
            m_fYaw = m_fDstYaw;
            m_fRateYaw = 1f;
        }
        else
        {
            m_fRateYaw = 0f;
        }
    }

    public void SetYaw(float angle)
    {
        m_fDstYaw = angle;
        if (MyUtils.LimitAngle(ref m_fDstYaw, m_fYawMin, m_fYawMax))
        {
            m_fYaw = m_fDstYaw;
            m_fRateYaw = 1f;
        }
        else
        {
            m_fRateYaw = 0f;
        }
    }

    public void Pitch(float angle)
    {
        m_fDstPitch += angle;
        MyUtils.LimitAngle(ref m_fDstPitch, m_fPitchMin, m_fPitchMax);
        m_fPitch = m_fDstPitch;
        m_fRatePitch = 1f;
    }

    public void SetPitch(float angle)
    {
        m_fDstPitch = angle;
        MyUtils.LimitAngle(ref m_fDstPitch, m_fPitchMin, m_fPitchMax);
        m_fPitch = m_fDstPitch;
        m_fRatePitch = 1f;
    }

    public float GetYaw() { return m_fDstYaw; }
    public float GetPitch() { return m_fDstPitch; }

    public void SetViewMelee(bool on)
    {
        m_fSmoothSpeed = 4f;
        m_v3Camera_Offset_Far = on ? camera_offset_melee : camera_offset_normal;
    }

    public void ShootMode(bool on)
    {
        if (on)
        {
            m_fSmoothSpeed = 6f;
            m_v3Camera_Offset_Far = camera_offset_shoot;
        }
        else
        {
            m_fSmoothSpeed = 4f;
            m_v3Camera_Offset_Far = camera_offset_normal;
        }
    }


    public void SwitchToTargetListener()
    {
        if (m_AudioListenerTarget != null) m_AudioListenerTarget.enabled = true;
        if (m_CameraController != null) m_CameraController.ActiveListener(false);
    }

    public void SwitchToCameraListener()
    {
        if (m_AudioListenerTarget != null) m_AudioListenerTarget.enabled = false;
        if (m_CameraController != null) m_CameraController.ActiveListener(true);
    }
}
