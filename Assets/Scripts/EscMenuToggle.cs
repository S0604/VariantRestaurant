using UnityEngine;

public class EscMenuToggle : MonoBehaviour
{
    [Header("主選單根面板")]
    [SerializeField] private GameObject optionsMenu;

    [Header("Esc 開啟時預設顯示的最上層面板")]
    [SerializeField] private GameObject defaultTopPanel;

    [Header("存讀檔子面板控制")]
    [SerializeField] private SaveLoadMenuUI saveLoadMenuUI;

    [Header("滑鼠設定")]
    [SerializeField] private bool hideCursorWhenClosed = false;

    [Header("Esc 開啟時是否強制回到最上層")]
    [SerializeField] private bool forceShowDefaultTopPanelOnOpen = true;

    private bool isOpen = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscapeInput();
        }
    }

    private void HandleEscapeInput()
    {
        if (!isOpen)
        {
            OpenMenu();
            return;
        }

        if (saveLoadMenuUI != null && saveLoadMenuUI.IsSubPanelOpen)
        {
            saveLoadMenuUI.CloseAllSubPanels();

            if (forceShowDefaultTopPanelOnOpen)
            {
                ShowDefaultTopPanel();
            }

            return;
        }

        CloseMenu();
    }

    public void ToggleMenu()
    {
        if (isOpen)
        {
            CloseMenu();
        }
        else
        {
            OpenMenu();
        }
    }

    public void OpenMenu()
    {
        isOpen = true;

        if (optionsMenu != null)
        {
            optionsMenu.SetActive(true);
        }

        if (saveLoadMenuUI != null)
        {
            saveLoadMenuUI.CloseAllSubPanels();
        }

        if (forceShowDefaultTopPanelOnOpen)
        {
            ShowDefaultTopPanel();
        }

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseMenu()
    {
        isOpen = false;

        if (saveLoadMenuUI != null)
        {
            saveLoadMenuUI.CloseAllSubPanels();
        }

        if (optionsMenu != null)
        {
            optionsMenu.SetActive(false);
        }

        Time.timeScale = 1f;
        Cursor.lockState = hideCursorWhenClosed ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !hideCursorWhenClosed;
    }

    public void OpenSavePanel()
    {
        if (saveLoadMenuUI != null)
        {
            saveLoadMenuUI.OpenSavePanel();
        }
    }

    public void OpenLoadPanel()
    {
        if (saveLoadMenuUI != null)
        {
            saveLoadMenuUI.OpenLoadPanel();
        }
    }

    public void ShowDefaultTopPanel()
    {
        if (optionsMenu == null)
            return;

        DeactivateAllChildren(optionsMenu);

        if (defaultTopPanel != null)
        {
            defaultTopPanel.SetActive(true);
        }
    }

    private void DeactivateAllChildren(GameObject parent)
    {
        Transform parentTransform = parent.transform;

        for (int i = 0; i < parentTransform.childCount; i++)
        {
            parentTransform.GetChild(i).gameObject.SetActive(false);
        }
    }
}