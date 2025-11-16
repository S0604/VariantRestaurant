using UnityEngine;

public class KitchenWasteBin : MonoBehaviour
{
    [Header("補給箱 UI 清除設定")]
    public Transform iconSpawnPoint;

    private bool isPlayerNearby = false;

    /* 🔒 只播一次 6_3 */
    private static bool hasClearedOnce = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerNearby = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) isPlayerNearby = false;
    }

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            InventoryManager.Instance.ClearInventory();
            ClearSupplyUI();


            Debug.Log("廚餘桶已清空玩家背包與補給 UI！");
        }
    }

    void ClearSupplyUI()
    {
        if (iconSpawnPoint != null)
            foreach (Transform t in iconSpawnPoint) Destroy(t.gameObject);
    }
}