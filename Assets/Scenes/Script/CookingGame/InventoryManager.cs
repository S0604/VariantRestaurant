<<<<<<< Updated upstream
=======
﻿using System.Collections.Generic;
>>>>>>> Stashed changes
using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [SerializeField] private int maxSlots = 2;
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private MenuItem GarbageItem;        // 在 Inspector 拖入垃圾模板

    private List<MenuItem> items = new List<MenuItem>();
    public IReadOnlyList<MenuItem> Items => items;

    public static event System.Action<List<MenuItem>> OnInventoryChanged;

    /* ===== 首次標記 ===== */
    private static bool firstDishGot = false;   // 第一次獲得「非垃圾料理」
    private static bool firstGarbageGot = false;  // 第一次獲得「垃圾」
    /* ==================== */

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /* 通用首次檢查 */
    private void CheckFirstItems(MenuItem newItem)
    {
        bool isGarbage = newItem.grade == BaseMinigame.DishGrade.Fail || newItem == GarbageItem;

        /* 第一次拿到「正常料理」→ 對話6 */
        if (!firstDishGot && !isGarbage)
        {
            firstDishGot = true;
            if (TutorialDialogueController.Instance != null)
                TutorialDialogueController.Instance.PlayChapter("6");
            return;   // 避免同帧又判到垃圾
        }

        /* 第一次拿到「垃圾」→ 對話6_1 */
        if (!firstGarbageGot && isGarbage)
        {
            firstGarbageGot = true;
            if (TutorialDialogueController.Instance != null)
                TutorialDialogueController.Instance.PlayChapter("6_1");
        }
    }

    /* --------------- 對外 API --------------- */

    public bool AddItem(MenuItem newItem)
    {
        if (items.Count >= maxSlots)
        {
            Debug.Log("背包已滿，無法放入新道具");
            return false;
        }

        items.Add(newItem);
        NotifyInventoryChanged();
        Debug.Log($"放入新道具：{newItem.name}");

        CheckFirstItems(newItem);   // ✅ 檢查第一次
        return true;
    }

<<<<<<< Updated upstream
=======
    /* 拍照專用 */
    public void AddItemFromSprite(Sprite sprite, string itemName, string itemTag, BaseMinigame.DishGrade grade)
    {
        if (items.Count >= maxSlots)
        {
            Debug.Log("背包已滿，無法放入新拍照道具");
            return;
        }

        MenuItem newItem = ScriptableObject.CreateInstance<MenuItem>();
        newItem.itemName = string.IsNullOrEmpty(itemName) ? "Burger" : itemName;
        newItem.itemTag = string.IsNullOrEmpty(itemTag) ? "Burger" : itemTag;
        newItem.grade = grade;
        newItem.itemImage = sprite;

        items.Add(newItem);
        NotifyInventoryChanged();
        Debug.Log("放入拍照道具：" + newItem.itemName);

        CheckFirstItems(newItem);   // ✅ 檢查第一次
    }

    /* 其他原有方法保持不動 */
>>>>>>> Stashed changes
    public void AddItemFromTexture(Texture2D texture, string itemName)
    {
        if (items.Count >= maxSlots) return;

        MenuItem newItem = ScriptableObject.CreateInstance<MenuItem>();
        newItem.itemName = itemName;
        newItem.itemTag = "001";
        newItem.grade = BaseMinigame.DishGrade.Perfect;

        Rect rect = new Rect(0, 0, texture.width, texture.height);
        Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
        newItem.itemImage = sprite;

        items.Add(newItem);
        NotifyInventoryChanged();

        CheckFirstItems(newItem);   // ✅ 檢查第一次
    }

<<<<<<< Updated upstream


    public bool HasItemByTag(string tag)
    {
        return items.Exists(item => item.itemTag == tag);
    }
=======
    public bool HasItemByTag(string tag) => items.Exists(item => item.itemTag == tag);
>>>>>>> Stashed changes

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
        Debug.Log("背包已清空");
        NotifyInventoryChanged();
    }

    public bool HasGarbage() => items.Exists(item => item == GarbageItem || item.grade == BaseMinigame.DishGrade.Fail);

    public bool HasItem(MenuItem item) => items.Contains(item);

    public List<MenuItem> GetItems() => new List<MenuItem>(items);

    public int GetItemCount() => items.Count;

    public void ClearInventory() => ClearItems();

    private void NotifyInventoryChanged() => OnInventoryChanged?.Invoke(new List<MenuItem>(items));
}