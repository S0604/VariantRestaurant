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
                Debug.Log("�w�M�ũҦ��Ʋz�����P�e���I");

                //�D�ʳq�� Manager �������
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
