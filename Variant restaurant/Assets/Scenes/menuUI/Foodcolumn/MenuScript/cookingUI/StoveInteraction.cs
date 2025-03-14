using UnityEngine;
using UnityEngine.UI;

public class StoveInteraction : MonoBehaviour
{
    public GameObject interactionUI; // 互动提示 UI
    public GameObject cookingMenuUI; // 烹饪菜单 UI
    private bool isPlayerNearby = false; // 玩家是否在范围内

    private void Start()
    {
        // 确保提示 UI 默认隐藏
        if (interactionUI != null)
            interactionUI.SetActive(false);

        if (cookingMenuUI != null)
            cookingMenuUI.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // 确保是 Player 进入
        {
            isPlayerNearby = true;
            if (interactionUI != null)
                interactionUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) // 确保是 Player 离开
        {
            isPlayerNearby = false;
            if (interactionUI != null)
                interactionUI.SetActive(false);
        }
    }

    private void Update()
    {
        // 如果玩家在范围内并按下 E 键
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (cookingMenuUI != null)
                cookingMenuUI.SetActive(!cookingMenuUI.activeSelf); // 显示/隐藏菜单
        }
    }
}
