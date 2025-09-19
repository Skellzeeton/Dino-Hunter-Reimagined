using UnityEngine;

public class gyUIPanelMissionSuccessLevelUp : MonoBehaviour
{
    public GameObject mLightBase;
    public GameObject mLightAnim;
    public GameObject mTitleText;
    public GameObject mTitleIcon;
    public GameObject mStatisticsBackground;

    public UILabel mLabel1_1;
    public UILabel mLabel1_2;
    public UILabel mLabel1_3;
    public UILabel mLabel1_3_1;
    //public UISprite mLabel1_3;
    public UILabel mLabel1_4;

    public UILabel mLabel2_1;
    public UILabel mLabel2_2;
    public UILabel mLabel2_3;
    public UILabel mLabel2_3_1;
    //public UISprite mLabel2_3;
    public UILabel mLabel2_4;

    protected bool m_bShow;
    protected int m_nStep;
    protected float m_fStepCount;

    private void Awake()
    {
        m_bShow = false;
        gameObject.SetActive(false);
    }

    private void Start()
    {
    }

    private void Update()
    {
        if (!m_bShow)
            return;

        float deltaTime = Time.deltaTime;

        switch (m_nStep)
        {
            case 0:
                m_fStepCount -= deltaTime;
                if (m_fStepCount <= 0f)
                {
                    CUISound.GetInstance().Play("UI_Bump");
                    mLabel1_1.gameObject.SetActive(true);
                    m_nStep = 1;
                    m_fStepCount = 0.3f;
                }
                break;
            case 1:
                m_fStepCount -= deltaTime;
                if (m_fStepCount <= 0f)
                {
                    CUISound.GetInstance().Play("UI_Bump");
                    mLabel1_2.gameObject.SetActive(true);
                    m_nStep = 2;
                    m_fStepCount = 0.3f;
                }
                break;
            case 2:
                m_fStepCount -= deltaTime;
                if (m_fStepCount <= 0f)
                {
                    mLabel1_3.gameObject.SetActive(true);
                    m_nStep = 3;
                    m_fStepCount = 0.1f;
                }
                break;
            case 3:
                m_fStepCount -= deltaTime;
                if (m_fStepCount <= 0f)
                {
                    CUISound.GetInstance().Play("UI_Click");
                    mLabel1_3_1.gameObject.SetActive(true);
                    m_nStep = 4;
                    m_fStepCount = 0.3f;
                }
                break;
            case 4:
                m_fStepCount -= deltaTime;
                if (m_fStepCount <= 0f)
                {
                    CUISound.GetInstance().Play("UI_Exp_plus");
                    mLabel1_4.gameObject.SetActive(true);
                    m_nStep = 5;
                    m_fStepCount = 1f;
                }
                break;
            case 5:
                m_fStepCount -= deltaTime;
                if (m_fStepCount <= 0f)
                {
                    CUISound.GetInstance().Play("UI_Bump");
                    mLabel2_1.gameObject.SetActive(true);
                    m_nStep = 6;
                    m_fStepCount = 0.3f;
                }
                break;
            case 6:
                m_fStepCount -= deltaTime;
                if (m_fStepCount <= 0f)
                {
                    CUISound.GetInstance().Play("UI_Bump");
                    mLabel2_2.gameObject.SetActive(true);
                    m_nStep = 7;
                    m_fStepCount = 0.3f;
                }
                break;
            case 7:
                m_fStepCount -= deltaTime;
                if (m_fStepCount <= 0f)
                {
                    mLabel2_3.gameObject.SetActive(true);
                    m_nStep = 8;
                    m_fStepCount = 0.1f;
                }
                break;
            case 8:
                m_fStepCount -= deltaTime;
                if (m_fStepCount <= 0f)
                {
                    CUISound.GetInstance().Play("UI_Click");
                    mLabel2_3_1.gameObject.SetActive(true);
                    m_nStep = 9;
                    m_fStepCount = 0.3f;
                }
                break;
            case 9:
                m_fStepCount -= deltaTime;
                if (m_fStepCount <= 0f)
                {
                    CUISound.GetInstance().Play("UI_Exp_plus");
                    mLabel2_4.gameObject.SetActive(true);
                    m_nStep = 10;
                    m_fStepCount = 1f;
                }
                break;
        }
    }

    public void Show(bool bShow)
    {
        m_bShow = bShow;
        gameObject.SetActive(bShow);

        mLightBase.SetActive(bShow);
        mLightAnim.SetActive(bShow);
        mTitleText.SetActive(bShow);
        mTitleIcon.SetActive(bShow);
        mStatisticsBackground.SetActive(bShow);

        if (bShow)
        {
            transform.localPosition = new Vector3(0f, 0f, transform.localPosition.z);
            m_nStep = 0;
            m_fStepCount = 0.5f;

            //mLabel1_1.gameObject.SetActive(true);
            //mLabel2_1.gameObject.SetActive(true);
        }
        else
        {
            transform.localPosition = new Vector3(10000f, 10000f, transform.localPosition.z);
        }
    }

    public void Go()
    {
        mLabel1_1.gameObject.SetActive(false);
        mLabel1_2.gameObject.SetActive(false);
        mLabel1_3.gameObject.SetActive(false);
        mLabel1_3_1.gameObject.SetActive(false);
        mLabel1_4.gameObject.SetActive(false);
        mLabel2_1.gameObject.SetActive(false);
        mLabel2_2.gameObject.SetActive(false);
        mLabel2_3.gameObject.SetActive(false);
        mLabel2_3_1.gameObject.SetActive(false);
        mLabel2_4.gameObject.SetActive(false);
        
        /*CUISound.GetInstance().Play("UI_Bump");
        mLabel1_1.gameObject.SetActive(true);
        mLabel2_1.gameObject.SetActive(true);
        m_nStep = 0;
        m_fStepCount = 0.5f;
        m_bShow = true;*/
    }

    public void Stop()
    {
        CUISound.GetInstance().Play("UI_Bump");

        mLabel1_1.gameObject.SetActive(true);
        mLabel1_2.gameObject.SetActive(true);
        mLabel1_3.gameObject.SetActive(true);
        mLabel1_3_1.gameObject.SetActive(true);
        mLabel1_4.gameObject.SetActive(true);
        mLabel2_1.gameObject.SetActive(true);
        mLabel2_2.gameObject.SetActive(true);
        mLabel2_3.gameObject.SetActive(true);
        mLabel2_3_1.gameObject.SetActive(true);
        mLabel2_4.gameObject.SetActive(true);

        m_nStep = 10;
        m_bShow = true;
    }

    public bool IsAnim()
    {
        return m_nStep < 10;
    }

    public void SetLevelContext(int from, int to)
    {
        if (mLabel1_2 != null && mLabel1_4 != null)
        {
            mLabel1_2.text = from.ToString();
            mLabel1_4.text = "[11FF00]" + to;
        }
    }

    public void SetHPContext(int from, int to)
    {
        if (mLabel2_2 != null && mLabel2_4 != null)
        {
            mLabel2_2.text = from.ToString();
            mLabel2_4.text = "[11FF00]" + to;
        }
    }
}
