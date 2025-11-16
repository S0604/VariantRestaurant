using UnityEngine;
using UnityEngine.UI;

public class Fridge : MonoBehaviour
{
    [Header("補給箱道具")] public MenuItem supplyBoxItem;        // ScriptableObject
    [Header("UI 生成點")] public Transform iconSpawnPoint;      // 圖示掛點
    [Header("圖示預製")] public GameObject iconPrefab;          // Image prefab
    [Header("發放數量")] public int supplyAmount = 1;           // 可升級

    /* ===== 首次標記 ===== */
    private static bool hasReceivedSupplyOnce = false;          // 第一次領補給
    private static bool hasPlayedNotEmptyOnce = false;          // 非空手只播一次 14-2

    private bool playerInRange = false;

    /* ---------- 更新 ---------- */
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
            TrySupplyBox();
    }

    /* ---------- 核心邏輯 ---------- */
    private void TrySupplyBox()
    {
        /* 1. 背包非空 → 只播 14-2（不解鎖，可重複按但只播一次）*/
        if (InventoryManager.Instance.GetItemCount() > 0)
        {
            if (!hasPlayedNotEmptyOnce)
            {
                hasPlayedNotEmptyOnce = true;
                if (TutorialDialogueController.Instance != null)
                    TutorialDialogueController.Instance.PlayChapter("14_2");
                Debug.Log("背包非空，播 14-2");
            }
            return;                   // 不發放補給
        }

        /* 2. 空手 → 發放 + 第一次播 14-1（不解鎖，可自加）*/
        MenuItem itemInstance = Instantiate(supplyBoxItem);
        InventoryManager.Instance.ClearInventory();   // 確保只拿這份
        InventoryManager.Instance.AddItem(itemInstance);

        if (!hasReceivedSupplyOnce)
        {
            hasReceivedSupplyOnce = true;
            if (TutorialDialogueController.Instance != null)
                TutorialDialogueController.Instance.PlayChapter("14_1");

            Debug.Log("第一次領補給，播 14-1");
        }

        /* 第一次領補給 → 只播 14-1*/

        SpawnSupplyIcon(itemInstance);
    }

    /* ---------- UI 圖示 ---------- */
    private void SpawnSupplyIcon(MenuItem item)
    {
        if (iconPrefab == null || iconSpawnPoint == null || item == null) return;

        GameObject iconObj = Instantiate(iconPrefab, iconSpawnPoint);
        Image img = iconObj.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = item.itemImage;
            img.color = Color.white;
        }
    }

    /* ---------- 觸發區 ---------- */
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player")) playerInRange = false;
    }

    /* ---------- 升級接口 ---------- */
    public void UpgradeSupplyAmount(int amount)
    {
        supplyAmount += amount;
        Debug.Log($"冰箱補給數量升級為：{supplyAmount}");
    }
}