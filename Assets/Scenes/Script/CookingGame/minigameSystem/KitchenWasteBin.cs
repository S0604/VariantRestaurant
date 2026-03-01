using UnityEngine;

public class KitchenWasteBin : MonoBehaviour
{
    [Header("補給箱 UI 清除設定")]
    public Transform iconSpawnPoint;

    [Header("Highlight")]
    [SerializeField] private StationHighlighter_SwapOutlineMat highlighter;

    private bool isPlayerNearby = false;

    /* 🔒 只播一次 6_3 */
    private static bool hasClearedOnce = false;

    private void Awake()
    {
        if (!highlighter)
            highlighter = GetComponentInChildren<StationHighlighter_SwapOutlineMat>(true);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (highlighter) highlighter.SetHighlight(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (highlighter) highlighter.SetHighlight(false);
        }
    }

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (InventoryManager.Instance != null)
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