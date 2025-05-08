using UnityEngine;

public class CookingStation : MonoBehaviour
{
    private bool playerInRange = false;

    [Tooltip("選擇這個站點對應的小遊戲類型，如 Burger、Fries、Drink")]
    public string minigameType = "Burger";  // 可自訂為 "Fries"、"Drink" 等

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log($"開始 {minigameType} 小遊戲！");
            MinigameManager.Instance.StartMinigame(minigameType, OnMinigameComplete);
        }
    }

    void OnMinigameComplete(bool success, int rank)
    {
        if (success)
        {
            Debug.Log($"{minigameType} 製作成功！完成度等級: {rank}");
        }
        else
        {
            Debug.Log($"{minigameType} 製作失敗，請再試一次！");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("玩家進入料理範圍");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("玩家離開料理範圍");
        }
    }
}
