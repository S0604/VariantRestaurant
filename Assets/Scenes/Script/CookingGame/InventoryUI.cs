
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public GameObject itemImagePrefab;
    public Transform itemContainer;

    private List<GameObject> spawnedImages = new List<GameObject>();

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

        foreach (var img in spawnedImages)
        {
            Destroy(img);
        }
        spawnedImages.Clear();

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
                }
            }

            RectTransform rt = newImageObj.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = new Vector2(i * 100f, 0);
            }

            spawnedImages.Add(newImageObj);
        }

        Debug.Log($"更新 UI：顯示 {visibleItems.Count} 個可視物品");
    }
}
