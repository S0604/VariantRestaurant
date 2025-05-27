using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    public GameObject itemImagePrefab; // UI Image �� prefab�]�ݱ� Image�^
    public Transform itemContainer;    // ��m�Ϥ��� Panel/�Ū���

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
        // �M���{�����Ϥ�
        foreach (var img in spawnedImages)
        {
            Destroy(img);
        }
        spawnedImages.Clear();

        // ��̦ܳh��i�Ϥ��A�ѥ��ӥk�ƦC
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

            // �۰ʱƦC�]�p�G�S�� layout group�A�i��ʦ첾�^
            RectTransform rt = newImageObj.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = new Vector2(i * 100f, 0); // �C�i���Z 100�A�i�վ�
            }

            spawnedImages.Add(newImageObj);
        }
    }
}
