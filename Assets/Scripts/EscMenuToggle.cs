using UnityEngine;

public class EscMenuToggle : MonoBehaviour
{
    [Header("指向你的選項面板 (Panel)")]
    public GameObject optionsMenu;

    // 如果關閉時需要隱藏滑鼠，改成 false（依你的遊戲需求）
    public bool hideCursorWhenClosed = false;

    private bool isOpen = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        isOpen = !isOpen;

        optionsMenu.SetActive(isOpen);

        // 暫停或恢復遊戲
        Time.timeScale = isOpen ? 0f : 1f;

        // 滑鼠控制
        if (isOpen)
        {
            Cursor.lockState = CursorLockMode.None; // 滑鼠自由活動
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
        optionsMenu.SetActive(false);
        Time.timeScale = 1f;

        Cursor.lockState = hideCursorWhenClosed ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !hideCursorWhenClosed;
    }
}
