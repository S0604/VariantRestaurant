using UnityEngine;
using UnityEngine.UI;

public class Fridge : MonoBehaviour
{
    public MenuItem supplyBoxItem;
    public Transform iconSpawnPoint;
    public GameObject iconPrefab;

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
        if (InventoryManager.Instance == null) return;

        if (InventoryManager.Instance.GetItemCount() > 0)
        {
            Debug.Log("背包必須為空才能領取補給箱！");
            return;
        }

        if (supplyBoxItem == null)
        {
            Debug.LogWarning("[Fridge] supplyBoxItem 未設定。");
            return;
        }

        MenuItem itemInstance = Instantiate(supplyBoxItem);
        InventoryManager.Instance.ClearInventory();
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
}
