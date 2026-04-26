using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotButtonUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text slotNameText;
    [SerializeField] private TMP_Text saveTypeText;
    [SerializeField] private TMP_Text saveTimeText;
    [SerializeField] private TMP_Text sceneNameText;
    [SerializeField] private Button button;

    private int slotIndex;
    private bool isAutoSave;
    private string fileName;
    private SaveLoadMenuUI parentUI;

    public void Setup(SaveSlotMetaData meta, bool loadMode, SaveLoadMenuUI owner)
    {
        slotIndex = meta.slotIndex;
        isAutoSave = meta.isAutoSave;
        fileName = meta.fileName;
        parentUI = owner;

        if (slotNameText != null)
            slotNameText.text = meta.displayName;

        if (saveTypeText != null)
            saveTypeText.text = meta.isAutoSave ? "類型: 自動存檔" : "類型: 手動存檔";

        if (saveTimeText != null)
            saveTimeText.text = $"時間: {meta.saveTime}";

        if (sceneNameText != null)
            sceneNameText.text = $"場景: {meta.sceneName}";

        if (button != null)
        {
            button.onClick.RemoveAllListeners();

            if (loadMode)
            {
                bool canLoad = owner != null && owner.CanUseLoadSlots();
                button.interactable = meta.hasData && canLoad;
                button.onClick.AddListener(OnClickLoad);
            }
            else
            {
                bool canSave = owner != null && owner.CanUseSaveSlots();
                button.interactable = !meta.isAutoSave && canSave;
                button.onClick.AddListener(OnClickSave);
            }
        }
    }

    private void OnClickSave()
    {
        if (parentUI != null)
        {
            parentUI.HandleSaveSlotClick(slotIndex, isAutoSave, fileName);
        }
    }

    private void OnClickLoad()
    {
        if (parentUI != null)
        {
            parentUI.HandleLoadSlotClick(slotIndex, isAutoSave, fileName);
        }
    }
}