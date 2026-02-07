using UnityEngine;

public class KitchenWasteBin : MonoBehaviour
{
    private bool isPlayerNearby = false;

    [Header("補給箱 UI 清除設定")]
    public Transform iconSpawnPoint;  // 指向補給箱 UI 生成位置

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
            ClearSupplyUI();
            Debug.Log("廚餘桶已清空玩家背包與補給 UI！");
        }
    }

    private void ClearSupplyUI()
    {
        if (iconSpawnPoint != null)
        {
            foreach (Transform child in iconSpawnPoint)
            {
                Destroy(child.gameObject); // 移除所有補給圖示
            }
        }
    }
}
