using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class OrderItem
{
    public MenuItem menuItem;
    public bool isCompleted;

    // ★ 新增：記錄「實際交付」的料理等級（由 Inventory 交出去的那份決定）
    public BaseMinigame.DishGrade deliveredGrade = BaseMinigame.DishGrade.Fail;
}

public class CustomerOrder : MonoBehaviour
{
    public List<OrderItem> selectedItems = new List<OrderItem>();
    public bool IsOrderReady { get; private set; } = false;

    public void GenerateOrder(MenuDatabase database, bool isSpecialCustomer)
    {
        selectedItems.Clear();
        IsOrderReady = false;

        if (database == null || database.allMenuItems.Length < 1)
        {
            Debug.LogWarning("MenuDatabase 為空或菜色不足！");
            return;
        }

        List<MenuItem> shuffled = database.allMenuItems.OrderBy(x => Random.value).ToList();

        if (isSpecialCustomer)
        {
            // === 特殊顧客：A×3、A×2+B×1、A×1+B×2 ===
            int totalDishes = 3;
            int dishTypes = Random.Range(1, 3); // 1 或 2 種

            List<MenuItem> selectedMenuItems = shuffled.Take(dishTypes).ToList();
            List<int> quantities = new List<int>();

            if (dishTypes == 1)
            {
                quantities.Add(3);
            }
            else
            {
                int firstCount = Random.Range(1, 3);
                int secondCount = totalDishes - firstCount;
                quantities.Add(firstCount);
                quantities.Add(secondCount);
            }

            for (int i = 0; i < selectedMenuItems.Count; i++)
            {
                for (int j = 0; j < quantities[i]; j++)
                {
                    selectedItems.Add(new OrderItem
                    {
                        menuItem = selectedMenuItems[i],
                        isCompleted = false,
                        deliveredGrade = BaseMinigame.DishGrade.Fail
                    });
                }
            }
        }
        else
        {
            // === 普通顧客：A×1、A×2、A×1 + B×1（最多兩份） ===
            int totalDishes = Random.Range(1, 3); // 1 或 2 份
            int dishTypes = totalDishes == 1 ? 1 : Random.Range(1, 3);

            List<MenuItem> selectedMenuItems = shuffled.Take(dishTypes).ToList();
            List<int> quantities = new List<int>();

            if (dishTypes == 1)
            {
                quantities.Add(totalDishes);
            }
            else
            {
                quantities.Add(1);
                quantities.Add(1);
            }

            for (int i = 0; i < selectedMenuItems.Count; i++)
            {
                for (int j = 0; j < quantities[i]; j++)
                {
                    selectedItems.Add(new OrderItem
                    {
                        menuItem = selectedMenuItems[i],
                        isCompleted = false,
                        deliveredGrade = BaseMinigame.DishGrade.Fail
                    });
                }
            }
        }

        IsOrderReady = true;
    }

    // 提交物品時呼叫，標記符合 itemTag 的項目為完成，並記錄實際交付等級
    public bool SubmitItem(MenuItem submittedItem)
    {
        if (submittedItem == null) return false;

        // ★ 更安全：不要依賴 BaseMinigame.CurrentInstance（可能為 null）
        if (submittedItem.grade == BaseMinigame.DishGrade.Fail) return false;
        if (InventoryManager.Instance != null && InventoryManager.Instance.GarbageItem != null
            && submittedItem == InventoryManager.Instance.GarbageItem)
        {
            return false;
        }

        foreach (var orderItem in selectedItems)
        {
            if (orderItem.menuItem != null &&
                orderItem.menuItem.itemTag == submittedItem.itemTag &&
                !orderItem.isCompleted)
            {
                orderItem.isCompleted = true;

                // ★ 核心：記錄「實際交出去的」料理等級
                orderItem.deliveredGrade = submittedItem.grade;

                return true; // 只提交一個
            }
        }
        return false;
    }

    // 判斷訂單是否完成（所有項目都完成）
    public bool IsOrderComplete()
    {
        return selectedItems.All(item => item.isCompleted);
    }
}
