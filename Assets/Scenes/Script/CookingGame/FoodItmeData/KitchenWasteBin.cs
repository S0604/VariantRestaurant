using UnityEngine;

public class KitchenWasteBin : MonoBehaviour
{
    private bool isPlayerNearby = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }

    private void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            InventoryManager.Instance.ClearInventory();
            Debug.Log("廚餘桶已清空玩家背包！");
        }
    }
}
