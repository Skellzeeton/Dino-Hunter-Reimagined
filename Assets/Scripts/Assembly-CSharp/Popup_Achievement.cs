using UnityEngine;

public class Popup_Achievement : MonoBehaviour
{
    public GameObject go_popup;
    public AchievementScrollList chievement_scrolllist;
    private TUIControl btn_take_achievement;
    public TUIMeshSprite img_scroll2d_bg;

    private void Start()
    {
        if (img_scroll2d_bg != null)
        {
            img_scroll2d_bg.gameObject.layer = 9;
        }
    }

    private void Update()
    {
    }

    public void DoCreateAchievement(TUIAchievementInfo m_info, GameObject m_go_invoke)
    {
        if (chievement_scrolllist != null)
        {
            chievement_scrolllist.DoCreate(m_info, m_go_invoke);
        }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        transform.localPosition = new Vector3(0f, 0f, transform.localPosition.z);

        if (go_popup != null)
        {
            Animation anim = go_popup.GetComponent<Animation>();
            if (anim != null)
            {
                anim.Play();
            }
        }
    }

    public void Hide()
    {
        transform.localPosition = new Vector3(0f, -10000f, transform.localPosition.z);
        gameObject.SetActive(false);
    }

    public void SetTakeAchievementBtn(TUIControl m_control)
    {
        btn_take_achievement = m_control;
    }

    public void AfterTakeAchievement()
    {
        if (chievement_scrolllist != null)
        {
            chievement_scrolllist.AfterTakeAchievement(btn_take_achievement);
        }
    }

    public TUIAchievementRewardInfo TakeAchievement()
    {
        if (chievement_scrolllist != null)
        {
            return chievement_scrolllist.TakeAchievement(btn_take_achievement);
        }
        return null;
    }
}