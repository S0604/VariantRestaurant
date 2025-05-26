using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SubmitPoint : MonoBehaviour
{
    public float interactRange = 3f;
    public Inventory playerInventory;
    public CustomerQueueManager queueManager;
    public Transform playerTransform;

    void Update()
    {
        if (playerTransform == null || playerInventory == null || queueManager == null)
            return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        if (distance <= interactRange && Input.GetKeyDown(KeyCode.E))
        {
            TrySubmitOrder();
        }
    }

    void TrySubmitOrder()
    {
        var queue = queueManager.GetCurrentQueue();
        if (queue.Count == 0)
        {
            Debug.Log("目前沒有顧客等待訂單");
            return;
        }

        Customer firstCustomer = queue[0];
        var order = firstCustomer.GetComponent<CustomerOrder>();
        if (order == null || !order.IsOrderReady)
        {
            Debug.Log("第一位顧客尚未準備好訂單");
            return;
        }

        if (order.IsOrderComplete())
        {
            Debug.Log("訂單已完成，顧客即將離開");
            return;
        }

        // 找出玩家物品欄中所有符合訂單需求的物品
        var possibleItems = new List<MenuItem>();
        foreach (var playerItem in playerInventory.items)
        {
            if (order.selectedItems.Any(item =>
                item.menuItem.itemTag == playerItem.itemTag && !item.isCompleted))
            {
                possibleItems.Add(playerItem);
            }
        }

        if (possibleItems.Count == 0)
        {
            Debug.Log("物品欄中沒有顧客需要的餐點，無法提交");
            return;
        }

        // 一次提交所有符合條件的餐點
        int submittedCount = 0;
        foreach (var playerItem in possibleItems)
        {
            // 找到訂單中所有未完成的對應餐點，標記為完成
            foreach (var orderItem in order.selectedItems)
            {
                if (orderItem.menuItem.itemTag == playerItem.itemTag && !orderItem.isCompleted)
                {
                    orderItem.isCompleted = true;
                    submittedCount++;
                    break; // 只標記一個
                }
            }
        }

        // 從玩家物品欄移除所有已提交的餐點
        for (int i = 0; i < submittedCount; i++)
        {
            playerInventory.RemoveItem(possibleItems[i]);
        }

        Debug.Log($"提交了 {submittedCount} 份餐點給顧客");

        // UI 刷新
        OrderDisplayManager.Instance.UpdateOrderDisplayImagesForCustomer(firstCustomer);

        if (order.IsOrderComplete())
        {
            Debug.Log("顧客訂單完成，顧客即將離開");
            StartCoroutine(DelayCustomerLeave(firstCustomer));
        }
    }

    IEnumerator DelayCustomerLeave(Customer customer)
    {
        yield return new WaitForSeconds(0.5f); // 延遲0.5秒
        customer.ReceiveOrder();
        queueManager.LeaveQueue(customer);
    }
}
