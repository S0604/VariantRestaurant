using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [Header("Prefab & Container")]
    public GameObject itemImagePrefab;
    public Transform itemContainer;

    [Header("Display Size (UI，不改圖片解析度)")]
    [Tooltip("每個物品在 UI 上顯示的固定寬高（像素），不會被父物件撐滿")]
    public Vector2 itemBoxSize = new Vector2(128f, 128f);

    [Tooltip("是否套用固定顯示尺寸（關閉則沿用 prefab 尺寸）")]
    public bool useFixedDisplaySize = true;

    [Tooltip("保持貼圖比例；開啟時會信封置中，關閉則可能被撐滿而變形")]
    public bool preserveAspect = true;

    [Header("Layout（不使用 LayoutGroup 時）")]
    [Tooltip("未使用 LayoutGroup 時，兩個格子之間的水平間距")]
    public float slotSpacing = 100f;

    private readonly List<GameObject> spawnedImages = new List<GameObject>();

    private void OnEnable()
    {
        InventoryManager.OnInventoryChanged += UpdateUI;
    }

    private void OnDisable()
    {
        InventoryManager.OnInventoryChanged -= UpdateUI;
    }

    public void UpdateUI(List<MenuItem> items)
    {
        if (itemImagePrefab == null || itemContainer == null)
        {
            Debug.LogError("Prefab 或 Container 沒有設定！");
            return;
        }

        // 清理舊的
        foreach (var img in spawnedImages) Destroy(img);
        spawnedImages.Clear();

        // 過濾可視項
        List<MenuItem> visibleItems = items.FindAll(item => item.itemTag != "SupplyBox");

        for (int i = 0; i < Mathf.Min(2, visibleItems.Count); i++)
        {
            MenuItem item = visibleItems[i];
            GameObject newImageObj = Instantiate(itemImagePrefab, itemContainer);
            Image imageComponent = newImageObj.GetComponent<Image>();

            if (imageComponent != null && item != null)
            {
                if (item.itemImage == null)
                {
                    Debug.LogWarning($"物品 {item.itemName} 沒有圖片 (itemImage 為 null)");
                }
                else
                {
                    imageComponent.sprite = item.itemImage;
                    imageComponent.color = Color.white;
                    imageComponent.type = Image.Type.Simple;
                    imageComponent.preserveAspect = preserveAspect; // 不變形
                    // 不要 SetNativeSize —— 我們要固定顯示尺寸
                }
            }

            RectTransform rt = newImageObj.GetComponent<RectTransform>();
            if (rt != null)
            {
                if (useFixedDisplaySize)
                {
                    // 固定顯示寬高（UI 尺寸）
                    rt.sizeDelta = itemBoxSize;

                    // 若容器上有 LayoutGroup，避免被覆寫尺寸
                    var le = newImageObj.GetComponent<LayoutElement>();
                    if (le == null) le = newImageObj.AddComponent<LayoutElement>();
                    le.preferredWidth = itemBoxSize.x;
                    le.preferredHeight = itemBoxSize.y;
                    le.minWidth = itemBoxSize.x;
                    le.minHeight = itemBoxSize.y;
                    le.flexibleWidth = 0;
                    le.flexibleHeight = 0;
                }

                // 如果沒有用 LayoutGroup，就用簡單的水平位移排兩格
                rt.anchoredPosition = new Vector2(i * slotSpacing, 0);
            }

            spawnedImages.Add(newImageObj);
        }

        Debug.Log($"更新 UI：顯示 {visibleItems.Count} 個可視物品");
    }
}
