using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [SerializeField] private int maxSlots = 2;
    [SerializeField] private InventoryUI inventoryUI;

    private List<MenuItem> items = new List<MenuItem>();
    public IReadOnlyList<MenuItem> Items => items;

    public static event System.Action<List<MenuItem>> OnInventoryChanged;
    private List<MenuItem> inventoryItems = new List<MenuItem>();


    public MenuItem GarbageItem; // 設定垃圾 MenuItem

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // === 核心功能 ===

    public bool AddItem(MenuItem newItem)
    {
        if (items.Count >= maxSlots)
        {
            Debug.Log("背包已滿，無法加入新物品");
            return false;
        }

        NotifyInventoryChanged();
        items.Add(newItem);
        inventoryUI?.UpdateUI(items);
        return true;
    }

    public bool RemoveItem(MenuItem item)
    {
        if (items.Remove(item))
        {
            inventoryUI?.UpdateUI(items);
            NotifyInventoryChanged();
            return true;
        }
        return false;
    }

    public void ClearItems()
    {
        items.Clear();
        inventoryUI?.UpdateUI(items);
        Debug.Log("玩家背包已清空");
        NotifyInventoryChanged();
    }

    public bool HasGarbage()
    {
        return items.Exists(item => item == GarbageItem || item.grade == BaseMinigame.DishGrade.Fail);
    }

    private void NotifyInventoryChanged()
    {
        OnInventoryChanged?.Invoke(new List<MenuItem>(inventoryItems));
    }

    // === 為了相容舊程式碼新增的 API ===

    public void ClearInventory() => ClearItems();  // 舊方法對應

    public List<MenuItem> GetItems() => new List<MenuItem>(items);  // 回傳可修改副本

    public List<MenuItem> GetAllItems() => GetItems(); // 同上，另名而已

    public int GetItemCount() => items.Count;

}
