using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveSlotButtonUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text slotNameText;
    [SerializeField] private TMP_Text saveTimeText;
    [SerializeField] private TMP_Text sceneNameText;
    [SerializeField] private Button button;

    private int slotIndex;
    private bool isAutoSave;
    private bool isLoadMode;
    private SaveLoadMenuUI parentUI;

    public void Setup(SaveSlotMetaData meta, bool loadMode, SaveLoadMenuUI owner)
    {
        slotIndex = meta.slotIndex;
        isAutoSave = meta.isAutoSave;
        isLoadMode = loadMode;
        parentUI = owner;

        if (slotNameText != null)
            slotNameText.text = meta.displayName;

        if (saveTimeText != null)
            saveTimeText.text = meta.saveTime;

        if (sceneNameText != null)
            sceneNameText.text = $"³õ´º: {meta.sceneName}";

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
                button.interactable = true;
                button.onClick.AddListener(OnClickSave);
            }
        }
    }

    private void OnClickSave()
    {
        if (parentUI != null)
        {
            parentUI.HandleSaveSlotClick(slotIndex, isAutoSave);
        }
    }

    private void OnClickLoad()
    {
        if (parentUI != null)
        {
            parentUI.HandleLoadSlotClick(slotIndex, isAutoSave);
        }
    }
}