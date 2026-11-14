using System.Collections.Generic;
using UnityEngine;

public class Popup_TutorialHall : MonoBehaviour
{
    public GameObject go_popup;

    public TUILabel label_page_count;

    public List<GameObject> tutorial_pages = new List<GameObject>();

    private int current_page = 0;

    public int CurrentPage { get { return current_page; } }

    public int PageCount   { get { return tutorial_pages != null ? tutorial_pages.Count : 0; } }

    public bool IsFirstPage { get { return current_page <= 0; } }

    public bool IsLastPage  { get { return PageCount == 0 || current_page >= PageCount - 1; } }

    public void Show()
    {
        base.transform.localPosition = new Vector3(0f, 0f, base.transform.localPosition.z);

        if (go_popup != null && go_popup.GetComponent<Animation>() != null)
        {
            go_popup.GetComponent<Animation>().Play();
        }
        current_page = 0;
        RefreshPages();
    }

    public void Hide()
    {
        base.transform.localPosition = new Vector3(0f, -1000f, base.transform.localPosition.z);
    }

    public void Next()
    {
        if (current_page < PageCount - 1)
        {
            current_page++;
            RefreshPages();
        }
    }

    public void Previous()
    {
        if (current_page > 0)
        {
            current_page--;
            RefreshPages();
        }
    }

    private void RefreshPages()
    {
        if (tutorial_pages == null) return;

        for (int i = 0; i < tutorial_pages.Count; i++)
        {
            if (tutorial_pages[i] != null)
                tutorial_pages[i].SetActive(i == current_page);
        }
        if (label_page_count != null)
        {
            int display_page = PageCount == 0 ? 0 : current_page + 1;
            label_page_count.Text = display_page + "/" + PageCount;
        }
    }
}