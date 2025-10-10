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

    private Vector3 lastTargetPos = Vector3.zero;
    private Vector3 movementOffset = Vector3.zero;
    private float movementOffsetIntensity = 0.175f;
    private float movementOffsetSmoothing = 4.5f;

    public new void Awake()
    {
        base.Awake();
        m_AudioListenerCamera = GetComponent<AudioListener>();
        m_AudioListenerCamera.enabled = false;
        m_fSrcYaw = 0f;
        m_fDstYaw = m_fSrcYaw;
        m_fSrcPitch = 0f;
        m_fDstPitch = m_fSrcPitch;
        m_fRateYaw = 1f;
        m_fRatePitch = 1f;
        base.enabled = false;
        m_CameraController.enabled = false;
        base.enabled = true;
        m_CameraController.enabled = true;
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
        SetPitch(28f);
        m_v3Camera_Offset_Near = camera_offset_block;
        m_v3Camera_Offset_Cur = camera_offset_normal;
        m_Target = target;
        m_fMaxCameraDis = Vector3.Distance(camera_offset_normal, camera_offset_block);
        m_fCurCameraDis = m_fMaxCameraDis;
        m_fSmoothSpeed = 1f;
        Quaternion quaternion = Quaternion.Euler(0f - m_fPitch, m_fYaw, 0f);
        m_CameraController.Position = m_Target.Pos + quaternion * camera_offset_normal;
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

        if (m_fRateYaw < 1f)
        {
            m_fYaw = MyUtils.Lerp(m_fYaw, m_fDstYaw, m_fRateYaw);
            m_fRateYaw += 2f * Time.deltaTime;
        }
        if (m_fRatePitch < 1f)
        {
            m_fPitch = MyUtils.Lerp(m_fPitch, m_fDstPitch, m_fRatePitch);
            m_fRatePitch += 2f * Time.deltaTime;
        }
        
        Vector3 moveDir = Vector3.zero;
        float dt = Mathf.Max(0.0001f, Time.deltaTime);
        Vector3 velocity = Vector3.zero;
        bool hasVelocity = false;
        var targetObj = m_Target;
        if (targetObj != null)
        {
            var prop = targetObj.GetType().GetProperty("Velocity");
            if (prop != null)
            {
                object val = prop.GetValue(targetObj, null);
                if (val is Vector3)
                {
                    velocity = (Vector3)val;
                    hasVelocity = true;
                }
            }
            if (!hasVelocity)
            {
                var field = targetObj.GetType().GetField("m_Velocity");
                if (field == null) field = targetObj.GetType().GetField("velocity");
                if (field != null)
                {
                    object fval = field.GetValue(targetObj);
                    if (fval is Vector3)
                    {
                        velocity = (Vector3)fval;
                        hasVelocity = true;
                    }
                }
            }
        }

        if (!hasVelocity)
        {
            velocity = (m_Target.Pos - lastTargetPos) / dt;
        }
        
        Vector3 horizontalVel = velocity;
        horizontalVel.y = 0f;

        if (horizontalVel.sqrMagnitude > 0.0001f)
        {
            moveDir = horizontalVel.normalized;
        }
        else
        {
            moveDir = Vector3.zero;
        }
        
        Vector3 desiredMovementOffset = moveDir * movementOffsetIntensity;
        movementOffset = Vector3.Lerp(movementOffset, desiredMovementOffset, 1f - Mathf.Exp(-movementOffsetSmoothing * dt));
        lastTargetPos = m_Target.Pos;
        Quaternion baseRot = Quaternion.Euler(-m_fPitch, m_fYaw, 0f);
        m_v3Camera_Offset_Cur = Vector3.Lerp(
            m_v3Camera_Offset_Cur,
            m_v3Camera_Offset_Far,
            m_fSmoothSpeed * Time.deltaTime
        );

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
            m_fCurCameraDis += m_fSmoothSpeed * Time.deltaTime;
            if (m_fCurCameraDis > m_fMaxCameraDis)
                m_fCurCameraDis = m_fMaxCameraDis;
        }
        Vector3 shiftedNearPt = nearPt + movementOffset;
        Vector3 shiftedFarPt  = farPt + movementOffset;

        Vector3 finalPos = Vector3.Lerp(shiftedNearPt, shiftedFarPt, m_fCurCameraDis / m_fMaxCameraDis);
        Vector3 lookDir = (lookPt + movementOffset) - finalPos;
        if (lookDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
            m_CameraController.Rotation = Quaternion.Slerp(
                m_CameraController.Rotation,
                targetRot,
                52.5f * Time.deltaTime
            );
        }
        m_CameraController.Position = finalPos;

        Vector3 charForward = Quaternion.Euler(0f, m_fYaw, 0f) * Vector3.forward;
        if (charForward.sqrMagnitude > 0.0001f)
        {
            Quaternion charRot = Quaternion.LookRotation(charForward, Vector3.up);
            m_Target.transform.rotation = Quaternion.Slerp(
                m_Target.transform.rotation,
                charRot,
                52.5f * Time.deltaTime
            );
        }
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

    public float GetYaw()
    {
        return m_fDstYaw;
    }

    public float GetPitch()
    {
        return m_fDstPitch;
    }

    public void SetViewMelee(bool on)
    {
        m_fSmoothSpeed = 4f;
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
