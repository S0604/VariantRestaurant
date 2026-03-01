using UnityEngine;
using UnityEngine.UI;

public class Fridge : MonoBehaviour
{
    public MenuItem supplyBoxItem;
    public Transform iconSpawnPoint;
    public GameObject iconPrefab;

    [Header("Highlight")]
    [SerializeField] private StationHighlighter_SwapOutlineMat highlighter;

    private bool playerInRange = false;
    private bool highlightState = false;

    private void Awake()
    {
        if (!highlighter)
            highlighter = GetComponentInChildren<StationHighlighter_SwapOutlineMat>(true);
    }

    private void Update()
    {
        if (!playerInRange)
            return;

        RefreshHighlight();

        if (Input.GetKeyDown(KeyCode.E))
        {
            TrySupplyBox();
            // 互動後背包會變不空，立即刷新一次高亮狀態
            RefreshHighlight();
        }
    }

    private void RefreshHighlight()
    {
        bool shouldHighlight = false;

        var inv = InventoryManager.Instance;
        if (playerInRange && inv != null)
        {
            // 背包為空才亮
            shouldHighlight = inv.GetItemCount() == 0;
        }

        if (highlighter != null && highlightState != shouldHighlight)
        {
            highlightState = shouldHighlight;
            highlighter.SetHighlight(shouldHighlight);
        }
    }

    private void TrySupplyBox()
    {
        var inv = InventoryManager.Instance;
        if (inv == null) return;

        if (inv.GetItemCount() > 0)
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
        inv.ClearInventory();
        inv.AddItem(itemInstance);

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
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        RefreshHighlight();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        // 離開範圍一定不亮
        if (highlighter != null && highlightState)
        {
            highlightState = false;
            highlighter.SetHighlight(false);
        }
    }
}