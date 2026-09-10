using UnityEngine;

public class iItemDynamic : iItem
{
    public float fAbsorbDistance;
    public float fAbsorbSpeed = 20f;

    private float m_fAbsorbDelayTimer = 0f;
    private const float m_fAbsorbDelayThreshold = 5f;

    public GameObject m_GroundEffect;

    protected Rigidbody m_Rigidbody;
    private bool m_bHasRigidbody;
    private bool m_bHasCollider;
    private Vector3 m_v3GroundEffectLocalPos;

    protected bool m_bBump;
    protected float m_fBumpSpeed = 3f;
    protected float m_fBumpGravity = 10f;
    protected float m_fBumpDamping;
    protected float m_fBumpCurSpeed;
    protected float m_fBumpSrcHeight;
    protected float m_fFloorHeight;
    protected bool m_bAbsorb;

    private new void Awake()
    {
        base.Awake();

        m_Rigidbody = GetComponent<Rigidbody>();
        m_bHasRigidbody = (m_Rigidbody != null);
        m_bHasCollider  = (m_Collider  != null);   // <-- was never assigned before

        if (m_Collider != null)
            m_Collider.isTrigger = false;

        m_bBump = false;

        if (m_GroundEffect != null)
        {
            m_v3GroundEffectLocalPos = m_GroundEffect.transform.localPosition; // <-- was never assigned before
            m_GroundEffect.SetActiveRecursive(false);
        }

        m_bAbsorb = false;
    }

    private void Update()
    {
        m_fAbsorbDelayTimer += Time.deltaTime;

        if (!m_bAbsorb && m_fAbsorbDelayTimer >= m_fAbsorbDelayThreshold)
        {
            m_bAbsorb = true;
            m_bBump = false;

            if (m_GroundEffect != null)
                m_GroundEffect.SetActiveRecursive(false);   // was Object.Destroy

            if (m_Collider != null)
                m_Collider.enabled = false;

            if (m_Rigidbody != null)
            {
                m_Rigidbody.Sleep();
                m_Rigidbody.isKinematic = true;
            }
        }

        float deltaTime = Time.deltaTime;
        if (m_bBump)
        {
            Vector3 position = m_Transform.position;
            position.y += m_fBumpCurSpeed * deltaTime;
            m_fBumpCurSpeed -= m_fBumpGravity * deltaTime;

            if (position.y <= m_fBumpSrcHeight)
            {
                position.y = m_fBumpSrcHeight;
                m_fBumpCurSpeed = m_fBumpSpeed * (1f - m_fBumpDamping);
                if (m_fBumpDamping < 0.2f) m_fBumpDamping += 0.2f;
            }
            m_Transform.position = position;

            if (fAbsorbDistance > 0f)
            {
                CCharUser user = m_GameScene.GetUser();
                if (user != null && Vector3.Distance(user.Pos, m_Transform.position) <= fAbsorbDistance)
                {
                    if (m_GroundEffect != null)
                        m_GroundEffect.SetActiveRecursive(false);

                    m_bAbsorb = true;
                    m_bBump = false;
                    if (m_Collider != null) m_Collider.enabled = false;
                }
            }
        }

        if (!m_bAbsorb) return;

        CCharUser user2 = m_GameScene.GetUser();
        if (user2 == null)
        {
            Destroy();
            return;
        }

        Vector3 vector = user2.GetBone(2).position - m_Transform.position;
        float num = fAbsorbSpeed * deltaTime;
        if (num >= vector.magnitude)
        {
            if (ToughItem(user2)) Destroy();
        }
        else
        {
            m_Transform.position += vector.normalized * num;
        }
    }

    private void FixedUpdate()
    {
        if (m_GameScene == null)
            m_GameScene = iGameApp.GetInstance().m_GameScene;

        // Guard on !isKinematic — once we land, isKinematic becomes true and we never
        // re-enter this block for the rest of this item's life. That kills the land-sound
        // spam and the ground-effect spam.
        if (m_Rigidbody != null && !m_Rigidbody.isKinematic && m_GameScene != null &&
        base.transform.position.y <= m_fFloorHeight + m_Entity.transform.localPosition.y &&
        m_Rigidbody.linearVelocity.y > -0.2f && m_Rigidbody.linearVelocity.y < 0.2f)
        {
            m_Rigidbody.Sleep();
            m_Rigidbody.isKinematic = true;
            if (m_Collider != null) m_Collider.isTrigger = true;

            if (m_GroundEffect != null)
            {
                m_GroundEffect.SetActiveRecursive(true);
                m_GroundEffect.transform.parent = null;
                m_GroundEffect.transform.position = new Vector3(
                        m_GroundEffect.transform.position.x,
                        m_fFloorHeight + 0.01f,
                        m_GroundEffect.transform.position.z);
            }

            Vector3 position = base.transform.position;
            position.y = m_fFloorHeight;
            base.transform.position = position;
            m_bBump = true;
            m_fBumpSrcHeight = position.y;
            m_fBumpCurSpeed = m_fBumpSpeed;
            m_fBumpDamping = 0f;
            if (!string.IsNullOrEmpty(sLandAudio)) PlayItemAudio(sLandAudio);
        }
    }

    public override void Initialize(int nItemID, bool bAbsorb)
    {
        base.Initialize(nItemID, bAbsorb);

        // ---- Reset every runtime state field ----
        m_bBump = false;
        m_bAbsorb = false;
        m_fAbsorbDelayTimer = 0f;
        m_fBumpCurSpeed = 0f;
        m_fBumpDamping = 0f;
        m_fBumpSrcHeight = 0f;
        m_fFloorHeight = 0f;

        base.transform.rotation = Quaternion.identity;

        // Restore Rigidbody if it ever went missing.
        if (m_bHasRigidbody && m_Rigidbody == null)
        {
            m_Rigidbody = GetComponent<Rigidbody>();
            if (m_Rigidbody == null)
                m_Rigidbody = gameObject.AddComponent<Rigidbody>();
        }

        // ---- CRITICAL FIX ----
        // Un-kinematic FIRST, then zero velocities. Setting velocity on a kinematic body
        // is unsupported (hence the warnings) and — more importantly — it silently fails,
        // so the body keeps its stale velocity from the previous life and the item
        // tumbles on reuse. This is the root cause of both the warnings and the rotation.
        if (m_Rigidbody != null)
        {
            m_Rigidbody.isKinematic = false;
            m_Rigidbody.linearVelocity  = Vector3.zero;
            m_Rigidbody.angularVelocity = Vector3.zero;
            m_Rigidbody.WakeUp();
        }

        if (m_bHasCollider && m_Collider == null)
            m_Collider = GetComponent<Collider>();

        if (m_Collider != null)
        {
            m_Collider.enabled = true;
            m_Collider.isTrigger = false;
        }

        if (m_GroundEffect != null)
        {
            m_GroundEffect.transform.SetParent(base.transform, false);
            m_GroundEffect.transform.localPosition = m_v3GroundEffectLocalPos;
            m_GroundEffect.SetActiveRecursive(false);
        }

        Vector3 position = base.transform.position;
        position.y += 100f;
        RaycastHit hitInfo;
        if (Physics.Raycast(new Ray(position, Vector3.down), out hitInfo, 1000f, 536870912))
        {
            m_fFloorHeight = hitInfo.point.y;
        }
        else
        {
            bAbsorb = true;
        }

        if (bAbsorb)
        {
            if (m_Rigidbody != null)
            {
                m_Rigidbody.Sleep();
                m_Rigidbody.isKinematic = true;
            }
            if (m_Collider != null)
                m_Collider.isTrigger = true;

            m_bAbsorb = true;
        }
    }

    public override void Clear()
    {
        base.Clear();
        // Do NOT null m_GroundEffect — keep the reference so it can be reused.
        if (m_GroundEffect != null)
            m_GroundEffect.SetActiveRecursive(false);
    }

    public override void AddForce(Vector3 v3Force)
    {
        if (m_Rigidbody != null)
            m_Rigidbody.AddForce(v3Force);
    }

    public void ForceAbsorb(float overrideSpeed)
    {
        fAbsorbSpeed = overrideSpeed;
        BeginAbsorbCleanup();
    }

    private void BeginAbsorbCleanup()
    {
        if (m_GroundEffect != null)
        {
            Object.Destroy(m_GroundEffect);
            m_GroundEffect = null;
        }
        m_bAbsorb = true;
        m_bBump = false;
        if (m_Rigidbody != null)
        {
            try
            {
                m_Rigidbody.Sleep();
            }
            catch { }
            Object.Destroy(m_Rigidbody);
            m_Rigidbody = null;
        }
        if (m_Collider != null)
        {
            m_Collider.isTrigger = true;
        }
    }
}
