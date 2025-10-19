using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [SerializeField] private int maxSlots = 2;
    [SerializeField] private InventoryUI inventoryUI;

    private List<MenuItem> items = new List<MenuItem>();
    public IReadOnlyList<MenuItem> Items => items;

    public static event System.Action<List<MenuItem>> OnInventoryChanged;
    public static event System.Action<MenuItem> OnItemAdded;
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

        items.Add(newItem);
        NotifyInventoryChanged();

        // ★ 觸發單筆新增事件
        OnItemAdded?.Invoke(newItem);

        Debug.Log($"加入新物品：{newItem.name}");
        return true;
    }


    // ★ 新增：直接用 Sprite 建立一個臨時 MenuItem 加入（給 Burger 擷取）
    public void AddItemFromSprite(Sprite sprite, string itemName, string itemTag, BaseMinigame.DishGrade grade)
    {
        if (items.Count >= maxSlots)
        {
            Debug.Log("背包已滿，無法加入新物品（Sprite）");
            return;
        }

        var newItem = ScriptableObject.CreateInstance<MenuItem>();
        newItem.itemName = itemName;
        newItem.itemTag = itemTag;
        newItem.grade = grade;
        newItem.itemImage = sprite;

        items.Add(newItem);
        NotifyInventoryChanged();

        // ★ 觸發單筆新增事件
        OnItemAdded?.Invoke(newItem);

        Debug.Log($"加入物品（Sprite）：{itemName}");
    }

    public void AddItemFromTexture(Texture2D texture, string itemName)
    {
        if (items.Count >= maxSlots)
        {
            Debug.Log("背包已滿，無法加入新圖片物品");
            return;
        }

        MenuItem newItem = ScriptableObject.CreateInstance<MenuItem>();
        newItem.itemName = itemName;
        newItem.itemTag = "001";
        newItem.grade = BaseMinigame.DishGrade.Perfect;

        Rect rect = new Rect(0, 0, texture.width, texture.height);
        Vector2 pivot = new Vector2(0.5f, 0.5f);
        Sprite sprite = Sprite.Create(texture, rect, pivot);
        newItem.itemImage = sprite;

        items.Add(newItem);
        NotifyInventoryChanged();

        // ★ 觸發單筆新增事件
        OnItemAdded?.Invoke(newItem);

        Debug.Log("加入截圖物品：" + itemName);
    }

    public bool HasItemByTag(string tag)
    {
        return items.Exists(item => item.itemTag == tag);
    }

    public bool RemoveItemByTag(string tag)
    {
        var item = items.Find(i => i.itemTag == tag);
        if (item != null)
        {
            items.Remove(item);
            inventoryUI?.UpdateUI(items);
            NotifyInventoryChanged();
            return true;
        }
        return false;
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
        Debug.Log("玩家背包已清空");
        NotifyInventoryChanged();
    }

    public bool HasGarbage()
    {
        return items.Exists(item => item == GarbageItem || item.grade == BaseMinigame.DishGrade.Fail);
    }

    public bool HasItem(MenuItem item)
    {
        return items.Contains(item);
    }

    private void NotifyInventoryChanged()
    {
        OnInventoryChanged?.Invoke(new List<MenuItem>(items));
    }

    // === 為了相容舊程式碼新增的 API ===

    public void ClearInventory() => ClearItems();

    public List<MenuItem> GetItems() => new List<MenuItem>(items);

    public List<MenuItem> GetAllItems() => GetItems();

    public int GetItemCount() => items.Count;
}
