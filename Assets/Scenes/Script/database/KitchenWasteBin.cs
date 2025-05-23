using UnityEngine;

public class KitchenWasteBin : MonoBehaviour
{
    private bool isPlayerNearby = false;
    private Inventory playerInventory;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            playerInventory = other.GetComponent<Inventory>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            playerInventory = null;
        }
    }

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E) && playerInventory != null)
        {
            playerInventory.ClearItems();
            Debug.Log("廚餘桶已清空玩家背包！");
        }
    }
}
