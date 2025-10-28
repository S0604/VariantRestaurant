using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class TutorialOrderItem
{
    public MenuItem menuItem;   // 指定餐點
    public bool isCompleted = false;
}

public class TutorialCustomerOrder : MonoBehaviour
{
    [Header("教學用固定餐單")]
    public List<TutorialOrderItem> fixedItems = new List<TutorialOrderItem>();

    public bool IsOrderReady { get; private set; } = false;

    void Start()
    {
        // 啟動時直接準備好固定菜單
        if (fixedItems == null || fixedItems.Count == 0)
        {
            Debug.LogWarning("⚠️ 教學顧客尚未設定固定菜單項目！");
        }
        else
        {
            IsOrderReady = true;
        }
    }

    // 提交餐點時呼叫
    public bool SubmitItem(MenuItem submittedItem)
    {
        if (submittedItem == null)
        {
            Debug.LogWarning("❌ 提交的餐點為空！");
            return false;
        }

        if (submittedItem.grade == BaseMinigame.DishGrade.Fail ||
            submittedItem == BaseMinigame.CurrentInstance.garbageItem)
        {
            Debug.Log("🚫 錯誤或垃圾餐點無法提交。");
            return false;
        }

        // 嘗試找對應菜色
        foreach (var orderItem in fixedItems)
        {
            if (orderItem.menuItem != null &&
                orderItem.menuItem.itemTag == submittedItem.itemTag &&
                !orderItem.isCompleted)
            {
                orderItem.isCompleted = true;
                Debug.Log($"✅ 成功提交 {submittedItem.itemName}");
                return true;
            }
        }

        Debug.Log($"❌ 未找到對應的餐點：{submittedItem.itemName}");
        return false;
    }

    // 檢查是否所有餐點都完成
    public bool IsOrderComplete()
    {
        return fixedItems.All(item => item.isCompleted);
    }

    // 重新設定訂單（可在不同教學階段重用）
    public void ResetOrder(List<MenuItem> newMenuItems)
    {
        fixedItems.Clear();
        foreach (var menu in newMenuItems)
        {
            fixedItems.Add(new TutorialOrderItem { menuItem = menu, isCompleted = false });
        }
        IsOrderReady = true;
        Debug.Log("🔄 教學訂單已重設。");
    }
}
