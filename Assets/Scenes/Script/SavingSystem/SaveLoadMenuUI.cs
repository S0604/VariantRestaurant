using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SaveLoadMenuUI : MonoBehaviour
{
    [Header("面板")]
    [SerializeField] private GameObject savePanel;
    [SerializeField] private GameObject loadPanel;

    [Header("存檔槽生成位置")]
    [SerializeField] private Transform saveSlotContainer;

    [Header("讀檔槽生成位置 (Scroll View Content)")]
    [SerializeField] private Transform loadSlotContainer;

    [Header("槽位 Prefab")]
    [SerializeField] private SaveSlotButtonUI slotButtonPrefab;

    [Header("提示文字")]
    [SerializeField] private TMP_Text saveBlockedHintText;

    [Header("ESC 選單控制")]
    [SerializeField] private EscMenuToggle escMenuToggle;

    [Header("設定")]
    [SerializeField] private bool includeAutoSaveInLoadPanel = true;

    private SaveManager saveManager;

    public bool IsSubPanelOpen
    {
        get
        {
            bool saveOpen = savePanel != null && savePanel.activeSelf;
            bool loadOpen = loadPanel != null && loadPanel.activeSelf;
            return saveOpen || loadOpen;
        }
    }

    private void Awake()
    {
        saveManager = SaveManager.Instance;
        CloseAllSubPanels();
        UpdateSaveBlockedHint(false);

        if (escMenuToggle == null)
        {
            escMenuToggle = GetComponentInParent<EscMenuToggle>(true);
        }
    }

    public bool CanUseSaveSlots()
    {
        EnsureSaveManager();

        if (saveManager == null)
            return false;

        return saveManager.CanSaveNow();
    }

    public bool CanUseLoadSlots()
    {
        EnsureSaveManager();

        if (saveManager == null)
            return false;

        return saveManager.CanLoadNow();
    }

    public string GetSaveBlockedReason()
    {
        EnsureSaveManager();

        if (saveManager == null)
            return "找不到 SaveManager";

        return saveManager.GetSaveBlockedReason();
    }

    public void OpenSavePanel()
    {
        EnsureSaveManager();

        if (savePanel != null)
            savePanel.SetActive(true);

        if (loadPanel != null)
            loadPanel.SetActive(false);

        RefreshSavePanel();
    }

    public void OpenLoadPanel()
    {
        EnsureSaveManager();

        if (savePanel != null)
            savePanel.SetActive(false);

        if (loadPanel != null)
            loadPanel.SetActive(true);

        UpdateSaveBlockedHint(false);
        RefreshLoadPanel();
    }

    public void CloseAllSubPanels()
    {
        if (savePanel != null)
            savePanel.SetActive(false);

        if (loadPanel != null)
            loadPanel.SetActive(false);

        UpdateSaveBlockedHint(false);
    }

    public void RefreshSavePanel()
    {
        EnsureSaveManager();

        if (saveManager == null || saveSlotContainer == null || slotButtonPrefab == null)
            return;

        ClearChildren(saveSlotContainer);

        bool canSaveNow = saveManager.CanSaveNow();
        UpdateSaveBlockedHint(!canSaveNow);

        for (int i = 1; i <= saveManager.ManualSlotCount; i++)
        {
            SaveSlotMetaData meta = saveManager.GetManualSlotMetaData(i);
            SaveSlotButtonUI slotUI = Instantiate(slotButtonPrefab, saveSlotContainer);
            slotUI.Setup(meta, false, this);
        }
    }

    public void RefreshLoadPanel()
    {
        EnsureSaveManager();

        if (saveManager == null || loadSlotContainer == null || slotButtonPrefab == null)
            return;

        ClearChildren(loadSlotContainer);
        UpdateSaveBlockedHint(false);

        List<SaveSlotMetaData> slots = saveManager.GetAllLoadableSlotsSorted(includeAutoSaveInLoadPanel);

        for (int i = 0; i < slots.Count; i++)
        {
            SaveSlotButtonUI slotUI = Instantiate(slotButtonPrefab, loadSlotContainer);
            slotUI.Setup(slots[i], true, this);
        }
    }

    public void HandleSaveSlotClick(int slotIndex, bool isAutoSave, string fileName)
    {
        EnsureSaveManager();

        if (saveManager == null)
            return;

        if (!saveManager.CanSaveNow())
        {
            UpdateSaveBlockedHint(true);
            Debug.Log($"[SaveLoadMenuUI] {saveManager.GetSaveBlockedReason()}");
            RefreshSavePanel();
            return;
        }

        if (!isAutoSave)
        {
            saveManager.SaveSlot(slotIndex);
        }

        RefreshSavePanel();
        RefreshLoadPanel();
        CloseWholeEscMenu();
    }

    public void HandleLoadSlotClick(int slotIndex, bool isAutoSave, string fileName)
    {
        EnsureSaveManager();

        if (saveManager == null)
            return;

        if (isAutoSave)
        {
            saveManager.LoadAutoByFileName(fileName);
        }
        else
        {
            saveManager.LoadSlot(slotIndex);
        }

        CloseWholeEscMenu();
    }

    private void CloseWholeEscMenu()
    {
        if (escMenuToggle != null)
        {
            escMenuToggle.CloseMenu();
        }
        else
        {
            CloseAllSubPanels();
        }
    }

    private void UpdateSaveBlockedHint(bool show)
    {
        if (saveBlockedHintText == null)
            return;

        if (show)
        {
            saveBlockedHintText.gameObject.SetActive(true);
            saveBlockedHintText.text = GetSaveBlockedReason();
        }
        else
        {
            saveBlockedHintText.gameObject.SetActive(false);
        }
    }

    private void EnsureSaveManager()
    {
        if (saveManager == null)
            saveManager = SaveManager.Instance;
    }

    private void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }
}