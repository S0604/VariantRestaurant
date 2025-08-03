using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class OrderItem
{
    public MenuItem menuItem;
    public bool isCompleted;
}

public class CustomerOrder : MonoBehaviour
{
    public List<OrderItem> selectedItems = new List<OrderItem>();
    public bool IsOrderReady { get; private set; } = false;
    private BaseMinigame baseMinigame;

    void Start()
    {
        baseMinigame = FindObjectOfType<BaseMinigame>(); // 或透過其他方式取得參考
    }

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
                quantities.Add(3); // 全部分給一種
            }
            else
            {
                int firstCount = Random.Range(1, 3); // 1 或 2
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
                        isCompleted = false
                    });
                }
            }
        }
        else
        {
            // === 普通顧客：A×1、A×2、A×1 + B×1（最多兩份） ===
            int totalDishes = Random.Range(1, 3); // 1 或 2 份
            int dishTypes = totalDishes == 1 ? 1 : Random.Range(1, 3); // 如果只要一份，必為一種；若兩份，有可能是 A×2 或 A×1+B×1

            List<MenuItem> selectedMenuItems = shuffled.Take(dishTypes).ToList();
            List<int> quantities = new List<int>();

            if (dishTypes == 1)
            {
                quantities.Add(totalDishes); // A×1 或 A×2
            }
            else
            {
                quantities.Add(1); // A×1
                quantities.Add(1); // B×1
            }

            for (int i = 0; i < selectedMenuItems.Count; i++)
            {
                for (int j = 0; j < quantities[i]; j++)
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

    // 提交物品時呼叫，標記符合 itemTag 的項目為完成
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
                break; // 只提交一個
            }
        }
        return found;
    }

    // 判斷訂單是否完成（所有項目都完成）
    public bool IsOrderComplete()
    {
        return selectedItems.All(item => item.isCompleted);
    }
}
