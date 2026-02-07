using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("背包設定")]
    [SerializeField] private int maxSlots = 2;
    [SerializeField] private InventoryUI inventoryUI;
    [SerializeField] private MenuItem GarbageItem; // Inspector 拖入垃圾模板

    private List<MenuItem> items = new List<MenuItem>();
    public IReadOnlyList<MenuItem> Items => items;

    public static event System.Action<List<MenuItem>> OnInventoryChanged;
    public static event System.Action<MenuItem> OnItemAdded;

    /* ===== 首次標記 ===== */
    private static bool firstItemEverGot = false;
    private static bool firstDishGot = false;
    private static bool firstGarbageGot = false;
    private static bool firstFries002Got = false;
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

    /* ===== 新增物品 ===== */
    public bool AddItem(MenuItem newItem)
    {
        if (items.Count >= maxSlots)
        {
            Debug.Log("背包已滿，無法放入新道具");
            return false;
        }

        items.Add(newItem);
        NotifyInventoryChanged();
        OnItemAdded?.Invoke(newItem);
        Debug.Log($"放入新道具：{newItem.name}");

        CheckFirstItems(newItem);
        return true;
    }

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
        OnItemAdded?.Invoke(newItem);
        Debug.Log("放入拍照道具：" + newItem.itemName);

        CheckFirstItems(newItem);
    }

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
        OnItemAdded?.Invoke(newItem);

        CheckFirstItems(newItem);
    }

    /* ===== 檢查首次物品事件 ===== */
    private void CheckFirstItems(MenuItem newItem)
    {
        bool isGarbage = newItem.grade == BaseMinigame.DishGrade.Fail || newItem == GarbageItem;

        // 第一次獲得任何物品 → 解鎖 Get garbage
        if (!firstItemEverGot)
        {
            firstItemEverGot = true;
            TutorialProgressManager.Instance?.CompleteEvent("Get garbage");
        }

        // 第一次拿 Fries 002 → 對話8 + 生成4顧客
        if (!firstFries002Got && newItem.itemName == "Fries" && newItem.itemTag == "002")
        {
            firstFries002Got = true;
            StartCoroutine(Spawn4CustomersAfterDialogue());
        }

        // 第一次拿非垃圾料理 → 對話6 + Unlock French Fries
        if (!firstDishGot && !isGarbage)
        {
            firstDishGot = true;
            TutorialDialogueController.Instance?.PlayChapter("6");
            TutorialProgressManager.Instance?.CompleteEvent("Unlock French Fries");
        }

        // 第一次拿垃圾 → 播 6_1
        if (!firstGarbageGot && newItem.itemName == "Garbage" && newItem.itemTag == "004")
        {
            firstGarbageGot = true;
            TutorialDialogueController.Instance?.PlayChapter("6_1");
        }
    }

    /* ===== 移除物品 ===== */
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

    /* ===== 查詢 ===== */
    public bool HasItem(MenuItem item) => items.Contains(item);
    public bool HasItemByTag(string tag) => items.Exists(i => i.itemTag == tag);
    public bool HasGarbage() => items.Exists(i => i == GarbageItem || i.grade == BaseMinigame.DishGrade.Fail);
    public List<MenuItem> GetItems() => new List<MenuItem>(items);
    public int GetItemCount() => items.Count;
    public void ClearInventory() => ClearItems();

    private void NotifyInventoryChanged() => OnInventoryChanged?.Invoke(new List<MenuItem>(items));

    /* ===== 對話8結束後生成4顧客 ===== */
    private IEnumerator Spawn4CustomersAfterDialogue()
    {
        if (TutorialDialogueController.Instance != null)
            yield return TutorialDialogueController.Instance.PlaySingleChapter("8");

        var spawner = FreeCustomerSpawner.Instance;
        if (spawner == null) yield break;

        for (int i = 0; i < 4; i++)
            spawner.SpawnCustomer(i);
    }
}
