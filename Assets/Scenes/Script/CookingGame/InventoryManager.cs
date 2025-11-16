using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;

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
    private static bool firstItemEverGot = false;   // 第一次獲得「任何」物品
    private static bool firstDishGot = false;   // 第一次非垃圾料理（保留原功能）
    private static bool firstGarbageGot = false;   // 第一次拿垃圾
    private static bool firstFries002Got = false;   // 第一次拿 Fries 002
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

        /* 1. 第一次獲得「任何物品」→ 只解鎖 Get garbage（不播對話）*/
        if (!firstItemEverGot)
        {
            firstItemEverGot = true;
            TutorialProgressManager.Instance?.CompleteEvent("Get garbage");
            // 不播對話，只解鎖
        }

        /* 2. 第一次拿「Fries & 002」→ 對話8 + 依序生成4顧客（只一次）*/
        if (!firstFries002Got && newItem.itemName == "Fries" && newItem.itemTag == "002")
        {
            firstFries002Got = true;
            /* ===== 對話8 結束瞬間 → 依序生成4顧客（只一次）===== */
            StartCoroutine(Spawn4CustomersAfterDialogue());
        }

        /* 3. 第一次拿「非垃圾料理」→ 保留對話6 + 解鎖 Successfully prepared the dish」*/
        if (!firstDishGot && !isGarbage)
        {
            firstDishGot = true;
            if (TutorialDialogueController.Instance != null)
                TutorialDialogueController.Instance.PlayChapter("6");
            if (TutorialProgressManager.Instance != null)
                TutorialProgressManager.Instance.CompleteEvent("Unlock French Fries");
        }

        /* 4. 第一次拿「垃圾」→ 只播 6_1（不解鎖，避免重複）*/
        if (!firstGarbageGot && newItem.itemName == "Garbage" && newItem.itemTag == "004")
        {
            firstGarbageGot = true;
            TutorialDialogueController.Instance?.PlayChapter("6_1");
            // 不再解鎖 Get garbage
        }
        /* 5. 背包滿 → 不再觸發任何事件或對話 */
        // 留空，什麼都不做
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

        CheckFirstItems(newItem);
    }

    public bool HasItemByTag(string tag) => items.Exists(item => item.itemTag == tag);

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

    /* ===== 對話8 結束 → 依序生成4顧客（只一次）===== */
    private IEnumerator Spawn4CustomersAfterDialogue()
    {
        // 等對話跑完（用 RealTime 版，不受 timeScale 影響）
        if (TutorialDialogueController.Instance != null)
            yield return TutorialDialogueController.Instance.PlaySingleChapter("8");

        // 依序生成4名（0→1→2→3）
        var spawner = FreeCustomerSpawner.Instance;
        if (spawner == null) yield break;

        for (int i = 0; i < 4; i++)
            spawner.SpawnCustomer(i);   // 強制依序 0→1→2→3
    }
}