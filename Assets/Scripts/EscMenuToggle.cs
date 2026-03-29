using UnityEngine;

public class EscMenuToggle : MonoBehaviour
{
    [Header("主選單面板")]
    [SerializeField] private GameObject optionsMenu;

    [Header("存讀檔子面板控制")]
    [SerializeField] private SaveLoadMenuUI saveLoadMenuUI;

    [Header("滑鼠設定")]
    [SerializeField] private bool hideCursorWhenClosed = false;

    private bool isOpen = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (saveLoadMenuUI != null && saveLoadMenuUI.IsSubPanelOpen)
            {
                saveLoadMenuUI.CloseAllSubPanels();
                return;
            }

            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        isOpen = !isOpen;

        if (optionsMenu != null)
        {
            optionsMenu.SetActive(isOpen);
        }

        Time.timeScale = isOpen ? 0f : 1f;

        if (isOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = hideCursorWhenClosed ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !hideCursorWhenClosed;
        }
    }

    public void CloseMenu()
    {
        isOpen = false;

        if (optionsMenu != null)
        {
            optionsMenu.SetActive(false);
        }

        if (saveLoadMenuUI != null)
        {
            saveLoadMenuUI.CloseAllSubPanels();
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
}