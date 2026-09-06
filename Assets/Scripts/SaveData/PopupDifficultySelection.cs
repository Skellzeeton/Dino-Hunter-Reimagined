using UnityEngine;

public class PopupDifficultySelection : MonoBehaviour
{
    private System.Action<int> onSelected;

    void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Show(System.Action<int> callback)
    {
        onSelected = callback;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
        onSelected = null;
    }

    public void SelectNormal()
    {
        if (onSelected != null)
            onSelected(0);
        Hide();
    }

    public void SelectHard()
    {
        if (onSelected != null)
            onSelected(1);
        Hide();
    }
}