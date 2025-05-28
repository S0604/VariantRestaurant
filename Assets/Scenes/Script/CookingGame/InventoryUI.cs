using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public GameObject itemImagePrefab; // UI Image 的 prefab（需掛 Image）
    public Transform itemContainer;    // 放置圖片的 Panel/空物件

    private List<GameObject> spawnedImages = new List<GameObject>();

    private void OnEnable()
    {
        Debug.Log("InventoryUI OnEnable");
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

        // 清除現有的圖片
        foreach (var img in spawnedImages)
        {
            Destroy(img);
        }
        spawnedImages.Clear();

        // 顯示最多兩張圖片，由左而右排列
        for (int i = 0; i < Mathf.Min(2, items.Count); i++)
        {
            MenuItem item = items[i];
            GameObject newImageObj = Instantiate(itemImagePrefab, itemContainer);
            Image imageComponent = newImageObj.GetComponent<Image>();

            if (imageComponent != null && item != null)
            {
                // 轉換 DishGrade -> ItemGrade 並取得對應圖片
                ItemGrade convertedGrade = (ItemGrade)(int)item.grade;
                imageComponent.sprite = item.GetSpriteByGrade(convertedGrade);
                imageComponent.color = Color.white;
            }

            RectTransform rt = newImageObj.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = new Vector2(i * 100f, 0);
            }

            spawnedImages.Add(newImageObj);
        }

        Debug.Log($"更新 UI：收到 {items.Count} 個物品");
    }

}
