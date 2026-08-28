using UnityEngine;
using System.Collections.Generic;

public class PopupSaveManager : MonoBehaviour
{
    public Transform panel;

    public List<SaveSlotEntry> slotEntries;

    public TUILabel infoLabel;

    public TUIButton btnBack;

    private bool isShowing = false;

    private Vector3 originalPosition;

    void Awake()
    {
        originalPosition = base.transform.localPosition;
        base.transform.localPosition = new Vector3(originalPosition.x, 1000f, originalPosition.z);
    }

    void Start()
    {
        if (slotEntries.Count != 5)
            Debug.LogWarning("PopupSaveManager: Not all 5 slot entries are assigned!");
    }

    public void Show()
    {
        base.GetComponent<Animation>().Play();
        base.transform.localPosition = originalPosition;
        gameObject.SetActive(true);
        isShowing = true;
        RefreshUI();
    }

    public void Hide()
    {
        base.transform.localPosition = new Vector3(originalPosition.x, 1000f, originalPosition.z);
        gameObject.SetActive(false);
        isShowing = false;
    }

    public bool IsShowing() => isShowing;

    public void RefreshUI()
    {
        iDataCenter dc = iGameApp.GetInstance().m_GameData.GetDataCenter();
        if (dc == null) return;
        var slots = dc.GetSaveSlotsInfo();
        int existingCount = 0;
        int currentSlot = dc.CurrentSlot;
        for (int i = 0; i < slotEntries.Count && i < slots.Count; i++)
        {
            if (slotEntries[i] != null)
            {
                slotEntries[i].slotIndex = i;
                slotEntries[i].UpdateSlotInfo(slots[i], currentSlot);
                if (slots[i].exists)
                    existingCount++;
            }
        }
        if (infoLabel != null)
            infoLabel.Text = existingCount == 0 ? "No saves found. Create a new game." : "";
    }

    public void OnLoadSlot(int slotIndex)
    {
        iDataCenter dc = iGameApp.GetInstance().m_GameData.GetDataCenter();
        if (dc == null) return;
        dc.SwitchToSlot(slotIndex);
        SettingsManager.Instance.LastSaveSlot = slotIndex;
        RefreshUI();
    }

    public void OnCreateSlot(int slotIndex)
    {
        iDataCenter dc = iGameApp.GetInstance().m_GameData.GetDataCenter();
        if (dc == null) return;
        var slots = dc.GetSaveSlotsInfo();
        if (slotIndex < 0 || slotIndex >= slots.Count || slots[slotIndex].exists)
        {
            Debug.LogWarning("Slot " + slotIndex + " is not empty!");
            return;
        }
        dc.CreateNewSlot(slotIndex);
        dc.SwitchToSlot(slotIndex);
        SettingsManager.Instance.LastSaveSlot = slotIndex;
        RefreshUI();
    }

    public void OnDeleteSlot(int slotIndex)
    {
        iDataCenter dc = iGameApp.GetInstance().m_GameData.GetDataCenter();
        if (dc == null) return;
        dc.DeleteSlot(slotIndex);
        if (SettingsManager.Instance.LastSaveSlot == slotIndex)
            SettingsManager.Instance.LastSaveSlot = 0;
        RefreshUI();
    }

    public void OnNewGameClick(TUIControl control, int event_type, float wparam, float lparam, object data)
    {
        if (event_type == 3) OnNewGame();
    }

    public void OnNewGame()
    {
        iDataCenter dc = iGameApp.GetInstance().m_GameData.GetDataCenter();
        if (dc == null) return;
        var slots = dc.GetSaveSlotsInfo();
        int emptySlot = -1;
        for (int i = 0; i < slots.Count; i++)
        {
            if (!slots[i].exists) { emptySlot = i; break; }
        }
        if (emptySlot == -1)
        {
            Debug.LogWarning("All save slots are full!");
            return;
        }
        dc.CreateNewSlot(emptySlot);
        dc.SwitchToSlot(emptySlot);
        RefreshUI();
    }

    public void OnBackClick(TUIControl control, int event_type, float wparam, float lparam, object data)
    {
        if (event_type == 3) OnBack();
    }

    public void OnBack()
    {
        iDataCenter dc = iGameApp.GetInstance().m_GameData.GetDataCenter();
        if (dc == null || !dc.m_bSlotLoaded)
        {
            Debug.LogWarning("No save loaded! Please select or create a save.");
            return;
        }
        Hide();
        Scene_Main main = FindObjectOfType<Scene_Main>();
        if (main != null) main.OnSaveLoaded();
    }
}