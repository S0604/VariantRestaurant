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
        InventoryManager.OnInventoryChanged += UpdateUI;
    }

    private void OnDisable()
    {
        InventoryManager.OnInventoryChanged -= UpdateUI;
    }

    public void UpdateUI(List<MenuItem> items)
    {
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
                imageComponent.sprite = item.itemImage;
                imageComponent.color = Color.white;
            }

            // 自動排列（如果沒用 layout group，可手動位移）
            RectTransform rt = newImageObj.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = new Vector2(i * 100f, 0); // 每張間距 100，可調整
            }

            spawnedImages.Add(newImageObj);
        }
    }
}
