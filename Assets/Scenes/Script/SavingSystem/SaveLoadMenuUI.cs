using System.Collections.Generic;
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

        RefreshLoadPanel();
    }

    public void CloseAllSubPanels()
    {
        if (savePanel != null)
            savePanel.SetActive(false);

        if (loadPanel != null)
            loadPanel.SetActive(false);
    }

    public void RefreshSavePanel()
    {
        EnsureSaveManager();

        if (saveManager == null || saveSlotContainer == null || slotButtonPrefab == null)
            return;

        ClearChildren(saveSlotContainer);

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

        List<SaveSlotMetaData> slots = saveManager.GetAllLoadableSlotsSorted(includeAutoSaveInLoadPanel);

        for (int i = 0; i < slots.Count; i++)
        {
            SaveSlotButtonUI slotUI = Instantiate(slotButtonPrefab, loadSlotContainer);
            slotUI.Setup(slots[i], true, this);
        }
    }

    public void HandleSaveSlotClick(int slotIndex, bool isAutoSave)
    {
        EnsureSaveManager();

        if (saveManager == null)
            return;

        if (isAutoSave)
        {
            saveManager.SaveAuto();
        }
        else
        {
            saveManager.SaveSlot(slotIndex);
        }

        RefreshSavePanel();
        RefreshLoadPanel();
    }

    public void HandleLoadSlotClick(int slotIndex, bool isAutoSave)
    {
        EnsureSaveManager();

        if (saveManager == null)
            return;

        if (isAutoSave)
        {
            saveManager.LoadAuto();
        }
        else
        {
            saveManager.LoadSlot(slotIndex);
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