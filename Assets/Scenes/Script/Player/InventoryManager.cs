using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    // UI image slots（由 Inspector 指派）
    public Image[] itemSlots = new Image[2];

    // 實際持有的物品
    private List<MenuItem> heldItems = new List<MenuItem>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 加入物品
    public bool AddItem(MenuItem item)
    {
        if (heldItems.Count >= 2)
        {
            Debug.Log("物品欄已滿");
            return false;
        }

        heldItems.Add(item);
        UpdateUI();
        return true;
    }

    // 移除物品（可依據需求擴充：依 index 或 type）
    public void RemoveItem(MenuItem item)
    {
        if (heldItems.Contains(item))
        {
            heldItems.Remove(item);
            UpdateUI();
        }
    }
    public List<MenuItem> GetHeldItems()
    {
        return heldItems;
    }

    public void RemoveItemByTag(string tag)
    {
        for (int i = 0; i < heldItems.Count; i++)
        {
            if (heldItems[i].itemTag == tag)
            {
                heldItems.RemoveAt(i);
                UpdateUI();
                return;
            }
        }
    }


    // 更新 UI 顯示
    private void UpdateUI()
    {
        for (int i = 0; i < itemSlots.Length; i++)
        {
            if (i < heldItems.Count)
            {
                itemSlots[i].sprite = heldItems[i].itemImage;
                itemSlots[i].enabled = true;
            }
            else
            {
                itemSlots[i].sprite = null;
                itemSlots[i].enabled = false;
            }
        }
    }

    // 清空全部物品（例如提交後）
    public void ClearItems()
    {
        heldItems.Clear();
        UpdateUI();
    }
}
