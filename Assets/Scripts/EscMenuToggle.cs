using UnityEngine;

public class EscMenuToggle : MonoBehaviour
{
    [Header("ESC 選單顯示根物件")]
    [SerializeField] private GameObject menuVisualRoot;

    [Header("Esc 開啟時預設顯示的最上層面板")]
    [SerializeField] private GameObject defaultTopPanel;

    [Header("存讀檔子面板控制")]
    [SerializeField] private SaveLoadMenuUI saveLoadMenuUI;

    [Header("滑鼠設定")]
    [SerializeField] private bool hideCursorWhenClosed = false;

    [Header("Esc 開啟時是否強制回到最上層")]
    [SerializeField] private bool forceShowDefaultTopPanelOnOpen = true;

    private bool isOpen = false;

    private void Awake()
    {
        if (saveLoadMenuUI == null)
        {
            saveLoadMenuUI = GetComponentInChildren<SaveLoadMenuUI>(true);
        }

        if (menuVisualRoot != null)
        {
            menuVisualRoot.SetActive(false);
        }

        isOpen = false;
    }

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
            CloseMenu();
        else
            OpenMenu();
    }

    public void OpenMenu()
    {
        isOpen = true;

        if (menuVisualRoot != null)
        {
            menuVisualRoot.SetActive(true);
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

        if (menuVisualRoot != null)
        {
            menuVisualRoot.SetActive(false);
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
        if (defaultTopPanel == null)
            return;

        Transform panelParent = defaultTopPanel.transform.parent;
        if (panelParent == null)
        {
            defaultTopPanel.SetActive(true);
            return;
        }

        for (int i = 0; i < panelParent.childCount; i++)
        {
            panelParent.GetChild(i).gameObject.SetActive(false);
        }

        defaultTopPanel.SetActive(true);
    }

    public bool IsMenuOpen()
    {
        return isOpen;
    }
}