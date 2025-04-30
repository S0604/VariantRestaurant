using UnityEngine;

public class CookingStation : MonoBehaviour
{
    private bool playerInRange = false;
    private CookingMinigame cookingMinigame;

    void Start()
    {
        // 這裡假設場景中有唯一一個 CookingMinigame
        cookingMinigame = FindObjectOfType<CookingMinigame>();
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("開始烹飪遊戲！");
            cookingMinigame.StartMinigame(OnCookingComplete);
        }
    }

    void OnCookingComplete(bool success, int rank)
    {
        if (success)
        {
            Debug.Log($"料理成功！完成度等級: {rank}");
        }
        else
        {
            Debug.Log("料理失敗，請再試一次！");
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
