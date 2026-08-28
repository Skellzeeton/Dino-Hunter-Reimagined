using UnityEngine;
using System;

public class PopupConfirmDelete : MonoBehaviour
{
    public TUILabel messageLabel;
    public TUIButton confirmBtn;
    public TUIButton cancelBtn;
    public GameObject popup;

    private int slotIndex;
    private Action<int> onConfirmCallback;
    private Vector3 originalPosition;
    private bool isShowing = false;

    void Awake()
    {
        originalPosition = transform.localPosition;
        transform.localPosition = new Vector3(originalPosition.x, 1000f, originalPosition.z);
        gameObject.SetActive(false);
    }

    public void Show(int slotIndex, Action<int> onConfirm)
    {
        this.slotIndex = slotIndex;
        this.onConfirmCallback = onConfirm;
        if (messageLabel != null)
            messageLabel.Text = string.Format("Delete save slot {0}?\nThis action cannot be undone.\nPress the X in the upper right to cancel.", slotIndex + 1);
        transform.localPosition = originalPosition;
        gameObject.SetActive(true);
        isShowing = true;
        if (popup.GetComponent<Animation>() != null)
        {
            popup.GetComponent<Animation>().Play();
        }
    }

    public void Hide()
    {
        transform.localPosition = new Vector3(originalPosition.x, 1000f, originalPosition.z);
        gameObject.SetActive(false);
        isShowing = false;
        onConfirmCallback = null;
    }

    public bool IsShowing() => isShowing;

    public void OnConfirmClick(TUIControl control, int event_type, float wparam, float lparam, object data)
    {
        if (event_type == 3)
        {
            onConfirmCallback?.Invoke(slotIndex);
            Hide();
        }
    }

    public void OnCancelClick(TUIControl control, int event_type, float wparam, float lparam, object data)
    {
        if (event_type == 3)
        {
            Hide();
        }
    }
}
