using UnityEngine;

public class DumpStation : MonoBehaviour
{
    private bool playerInRange = false;
    private Player currentPlayer;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            if (currentPlayer != null && currentPlayer.isCooking == false)
            {
                BaseMinigame.ClearAllDishRecords();
                Debug.Log("已清空所有料理紀錄與畫面！");

                //主動通知 Manager 重建顯示
                if (MinigameManager.Instance != null)
                {
                    MinigameManager.Instance.RefreshDishDisplay();
                }
            }
        }
    }



    private void InteractDump()
    {
        if (MinigameManager.Instance != null)
        {
            Transform dishContainer = MinigameManager.Instance.dishDisplayContainer;
            BaseMinigame.ClearCompletedDishes(dishContainer);
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null)
        {
            currentPlayer = player;
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Player player = other.GetComponent<Player>();
        if (player != null && player == currentPlayer)
        {
            currentPlayer = null;
            playerInRange = false;
        }
    }
}
