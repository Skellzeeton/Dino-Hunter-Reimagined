using UnityEngine;

public class SaveSlotEntry : MonoBehaviour
{
    public TUILabel labelSlotInfo;
    public TUIButton btnLoad;
    public TUIButton btnDelete;
    public int slotIndex;
    private PopupSaveManager parentManager;
    private bool slotExists = false;

    void Awake()
    {
        parentManager = GetComponentInParent<PopupSaveManager>();
    }

    public void OnLoadClick(TUIControl control, int event_type, float wparam, float lparam, object data)
    {
        if (event_type == 3 && parentManager != null)
        {
            if (slotExists)
                parentManager.OnLoadSlot(slotIndex);
            else
                parentManager.OnCreateSlot(slotIndex);
        }
    }

    public void OnDeleteClick(TUIControl control, int event_type, float wparam, float lparam, object data)
    {
        if (event_type == 3 && parentManager != null)
            parentManager.OnDeleteSlot(slotIndex);
    }

    public void UpdateSlotInfo(iDataCenter.SaveSlotInfo info, int currentSlot = -1)
    {
        slotExists = info.exists;
        if (labelSlotInfo != null)
        {
            if (info.exists)
            {
                string difficultyText = info.difficulty == 1 ? "Hard" : "Normal";
                string progressText = info.mapProgress.ToString("F1") + "%";
                labelSlotInfo.Text = string.Format("Slot {0}\n{1}\nChar: {2}\nMap: {3}\nGold: {4}\nCrystals: {5}",
                        info.slotIndex + 1,
                        difficultyText,
                        info.characterName,
                        progressText,
                        info.gold,
                        info.crystals);
            }
            else
            {
                labelSlotInfo.Text = string.Format("Slot {0}\n(EMPTY)", info.slotIndex + 1);
            }
        }
        if (btnLoad != null)
        {
            btnLoad.gameObject.SetActive(true);
            TUILabel btnLabel = btnLoad.GetComponentInChildren<TUILabel>();
            if (btnLabel != null)
            {
                if (currentSlot == info.slotIndex && info.exists)
                    btnLabel.Text = "Loaded";
                else
                    btnLabel.Text = info.exists ? "Load" : "Create";
            }
        }
        if (btnDelete != null)
            btnDelete.gameObject.SetActive(info.exists);
        }
    }
