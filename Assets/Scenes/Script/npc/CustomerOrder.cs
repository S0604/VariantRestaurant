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

    public void GenerateOrder(MenuDatabase database, int minItems = 1, int maxItems = 2)
    {
        selectedItems.Clear();
        IsOrderReady = false;

        if (database == null || database.allMenuItems.Length == 0)
        {
            Debug.LogWarning("MenuDatabase 為空或未設定！");
            return;
        }

        int count = Random.Range(minItems, maxItems + 1);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, database.allMenuItems.Length);
            selectedItems.Add(new OrderItem
            {
                menuItem = database.allMenuItems[index],
                isCompleted = false
            });
        }

        IsOrderReady = true;
    }

    // 提交物品時呼叫，標記符合 itemTag 的項目為完成
    public bool SubmitItem(string itemTag)
    {
        bool found = false;
        foreach (var orderItem in selectedItems)
        {
            if (orderItem.menuItem.itemTag == itemTag && !orderItem.isCompleted)
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
