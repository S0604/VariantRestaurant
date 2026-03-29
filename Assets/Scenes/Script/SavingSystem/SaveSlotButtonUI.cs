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
    private bool isLoadMode;
    private string fileName;
    private SaveLoadMenuUI parentUI;

    public void Setup(SaveSlotMetaData meta, bool loadMode, SaveLoadMenuUI owner)
    {
        slotIndex = meta.slotIndex;
        isAutoSave = meta.isAutoSave;
        isLoadMode = loadMode;
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
                button.interactable = meta.hasData;
                button.onClick.AddListener(OnClickLoad);
            }
            else
            {
                button.interactable = !meta.isAutoSave;
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