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
        Debug.Log("���a�I�]�w�M��");
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

    // === ���F�ۮe�µ{���X�s�W�� API ===

    public void ClearInventory() => ClearItems();  // �¤�k����

    public List<MenuItem> GetItems() => new List<MenuItem>(items);  // �^�ǥi�ק�ƥ�

    public List<MenuItem> GetAllItems() => GetItems(); // �P�W�A�t�W�Ӥw

    public int GetItemCount() => items.Count;

}
