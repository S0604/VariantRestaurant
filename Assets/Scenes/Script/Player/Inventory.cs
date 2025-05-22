using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public int maxSlots = 2;
    public List<MenuItem> items = new List<MenuItem>(); // 改用你的 MenuItem
    public Image[] slotImages;

    public void AddItem(MenuItem newItem)
    {
        if (items.Count >= maxSlots)
        {
            Debug.Log("物品欄已滿");
            return;
        }
        items.Add(newItem);
        UpdateUI();
    }

    public void RemoveItem(int index)
    {
        if (index < items.Count)
        {
            items.RemoveAt(index);
            UpdateUI();
        }
    }

    void UpdateUI()
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (i < items.Count && items[i] != null)
            {
                slotImages[i].sprite = items[i].itemImage; // 改用 itemImage
                slotImages[i].color = Color.white;
            }
            else
            {
                slotImages[i].sprite = null;
                slotImages[i].color = new Color(1, 1, 1, 0);
            }
        }
    }
    public bool RemoveItem(MenuItem item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
            UpdateUI();
            return true;
        }
        return false;
    }
    public void ClearItems()
    {
        items.Clear();
        UpdateUI();
    }

}

