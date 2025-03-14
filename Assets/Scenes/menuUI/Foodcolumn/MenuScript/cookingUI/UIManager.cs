using UnityEngine;

public class UIManager : MonoBehaviour
{
    public GameObject currentUI; // 当前显示的UI
    public GameObject nextUI; // 下一显示的UI

    void Update()
    {
        // 当用户点击屏幕时触发
        if (Input.GetMouseButtonDown(0)) // 0 表示左键点击
        {
            HideCurrentUI();
            ShowNextUI();
        }
    }

    void HideCurrentUI()
    {
        if (currentUI != null)
        {
            currentUI.SetActive(false); // 隐藏当前UI
        }
    }

    void ShowNextUI()
    {
        if (nextUI != null)
        {
            nextUI.SetActive(true); // 显示下一个UI
        }
    }
}
