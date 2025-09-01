using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [SerializeField] private int maxSlots = 2;
    [SerializeField] private InventoryUI inventoryUI;

    private List<MenuItem> items = new List<MenuItem>();
    public IReadOnlyList<MenuItem> Items => items;

    public static event System.Action<List<MenuItem>> OnInventoryChanged;
    private List<MenuItem> inventoryItems = new List<MenuItem>();

    public MenuItem GarbageItem; // �]�w�U�� MenuItem

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // === �֤ߥ\�� ===

    public bool AddItem(MenuItem newItem)
    {
        if (items.Count >= maxSlots)
        {
            Debug.Log("�I�]�w���A�L�k�[�J�s���~");
            return false;
        }

        items.Add(newItem);
        NotifyInventoryChanged();
        Debug.Log($"�[�J�s���~�G{newItem.name}");
        return true;
    }

    public void AddItemFromTexture(Texture2D texture, string itemName)
    {
        if (items.Count >= maxSlots)
        {
            Debug.Log("�I�]�w���A�L�k�[�J�s�Ϥ����~");
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

        Debug.Log("�[�J�I�Ϫ��~�G" + itemName);
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
        Debug.Log("���a�I�]�w�M��");
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

    // === ���F�ۮe�µ{���X�s�W�� API ===

    public void ClearInventory() => ClearItems();

    public List<MenuItem> GetItems() => new List<MenuItem>(items);

    public List<MenuItem> GetAllItems() => GetItems();

    public int GetItemCount() => items.Count;
}
