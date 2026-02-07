using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class OrderItem
{
    public MenuItem menuItem;
    public bool isCompleted;
}
[System.Serializable]
public class FixedOrderEntry
{
    public MenuItem menuItem;
    public int quantity = 1;
}

public class CustomerOrder : MonoBehaviour
{
    [Header("固定菜單設定 (Inspector 可拖拽)")]
    public List<FixedOrderEntry> fixedOrder = new List<FixedOrderEntry>();

    public List<OrderItem> selectedItems = new List<OrderItem>();
    public bool IsOrderReady { get; private set; } = false;
    private BaseMinigame baseMinigame;

    void Start()
    {
        baseMinigame = FindObjectOfType<BaseMinigame>();
    }

    /// <summary>
    /// 生成訂單，若 Inspector 固定菜單非空則使用固定
    /// </summary>
    /// <param name="database">菜單資料庫，隨機生成用</param>
    /// <param name="isSpecialCustomer">特殊顧客標記，隨機生成用</param>
    public void GenerateOrder(MenuDatabase database, bool isSpecialCustomer)
    {
        selectedItems.Clear();
        IsOrderReady = false;

        // 如果 Inspector 固定菜單有設定，優先使用
        if (fixedOrder != null && fixedOrder.Count > 0)
        {
            foreach (var entry in fixedOrder)
            {
                if (entry.menuItem == null || entry.quantity < 1) continue;

                for (int i = 0; i < entry.quantity; i++)
                {
                    selectedItems.Add(new OrderItem
                    {
                        menuItem = entry.menuItem,
                        isCompleted = false
                    });
                }
            }

            IsOrderReady = true;
            return;
        }

        // === 隨機生成邏輯 ===
        if (database == null || database.allMenuItems.Length < 1)
        {
            Debug.LogWarning("MenuDatabase 為空或菜色不足！");
            return;
        }

        List<MenuItem> shuffled = database.allMenuItems.OrderBy(x => Random.value).ToList();

        if (isSpecialCustomer)
        {
            int totalDishes = 3;
            int dishTypes = Random.Range(1, 3);
            List<MenuItem> selectedMenuItems = shuffled.Take(dishTypes).ToList();
            List<int> dishQuantities = new List<int>();

            if (dishTypes == 1)
            {
                dishQuantities.Add(3);
            }
            else
            {
                int firstCount = Random.Range(1, 3);
                dishQuantities.Add(firstCount);
                dishQuantities.Add(totalDishes - firstCount);
            }

            for (int i = 0; i < selectedMenuItems.Count; i++)
            {
                for (int j = 0; j < dishQuantities[i]; j++)
                {
                    selectedItems.Add(new OrderItem
                    {
                        menuItem = selectedMenuItems[i],
                        isCompleted = false
                    });
                }
            }
        }
        else
        {
            int totalDishes = Random.Range(1, 3);
            int dishTypes = totalDishes == 1 ? 1 : Random.Range(1, 3);
            List<MenuItem> selectedMenuItems = shuffled.Take(dishTypes).ToList();
            List<int> dishQuantities = new List<int>();

            if (dishTypes == 1) dishQuantities.Add(totalDishes);
            else { dishQuantities.Add(1); dishQuantities.Add(1); }

            for (int i = 0; i < selectedMenuItems.Count; i++)
            {
                for (int j = 0; j < dishQuantities[i]; j++)
                {
                    selectedItems.Add(new OrderItem
                    {
                        menuItem = selectedMenuItems[i],
                        isCompleted = false
                    });
                }
            }
        }

        IsOrderReady = true;
    }

    public bool SubmitItem(MenuItem submittedItem)
    {
        if (submittedItem.grade == BaseMinigame.DishGrade.Fail ||
            submittedItem == BaseMinigame.CurrentInstance.garbageItem)
        {
            Debug.Log("Garbage cannot be submitted.");
            return false;
        }

        bool found = false;
        foreach (var orderItem in selectedItems)
        {
            if (orderItem.menuItem.itemTag == submittedItem.itemTag && !orderItem.isCompleted)
            {
                orderItem.isCompleted = true;
                found = true;
                break;
            }
        }
        return found;
    }

    public bool IsOrderComplete()
    {
        return selectedItems.All(item => item.isCompleted);
    }
}
