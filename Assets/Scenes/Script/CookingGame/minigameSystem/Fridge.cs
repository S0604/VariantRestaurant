using UnityEngine;
using UnityEngine.UI;

public class Fridge : MonoBehaviour
{
    public MenuItem supplyBoxItem;            // 生成的補給箱物品（ScriptableObject）
    public Transform iconSpawnPoint;          // 補給圖示生成位置
    public GameObject iconPrefab;             // 圖示用的 Image prefab

    public int supplyAmount = 3;              // 可被升級，實際補多少能量
    private bool playerInRange = false;

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            TrySupplyBox();
        }
    }

    private void TrySupplyBox()
    {
        if (InventoryManager.Instance.GetItemCount() > 0)
        {
            Debug.Log("背包必須為空才能領取補給箱！");
            return;
        }

        MenuItem itemInstance = Instantiate(supplyBoxItem);
        InventoryManager.Instance.ClearInventory();  // 保險做法
        InventoryManager.Instance.AddItem(itemInstance);
        Debug.Log("已領取補給箱，佔據整個背包");

        SpawnSupplyIcon(itemInstance);
    }

    private void SpawnSupplyIcon(MenuItem item)
    {
        if (iconPrefab == null || iconSpawnPoint == null || item == null)
        {
            Debug.LogWarning("Fridge 補給圖示尚未正確設定");
            return;
        }

        GameObject iconObj = Instantiate(iconPrefab, iconSpawnPoint);
        Image img = iconObj.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = item.itemImage;
            img.color = Color.white;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }

    public void UpgradeSupplyAmount(int amount)
    {
        supplyAmount += amount;
        Debug.Log($"冰箱補給量升級為：{supplyAmount}");
    }
}
