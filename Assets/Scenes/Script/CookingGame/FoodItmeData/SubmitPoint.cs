using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SubmitPoint : MonoBehaviour
{
    [Header("互動設定")]
    public float interactRange = 3f;
    public CustomerQueueManager queueManager;
    public Transform playerTransform;

    private Customer lastFirstCustomer = null; // 記錄上一位第一位顧客

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        RefreshReferences();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshReferences();
    }

    void RefreshReferences()
    {
        if (playerTransform == null)
        {
            var playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
            else
                Debug.LogWarning("SubmitPoint: 找不到玩家物件，請確認玩家有正確Tag設定");
        }

        if (queueManager == null)
        {
            queueManager = FindObjectOfType<CustomerQueueManager>();
            if (queueManager == null)
                Debug.LogWarning("SubmitPoint: 找不到 CustomerQueueManager");
        }
    }

    void Update()
    {
        if (playerTransform == null || queueManager == null || InventoryManager.Instance == null)
            return;

        // 每幀檢查第一位顧客是否變更
        CheckFirstCustomerForDialogue();

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        if (distance <= interactRange && Input.GetKeyDown(KeyCode.E))
        {
            TrySubmitOrder();
        }
    }

    private void CheckFirstCustomerForDialogue()
    {
        var queue = queueManager.GetCurrentQueue();
        if (queue.Count == 0)
        {
            lastFirstCustomer = null;
            return;
        }

        Customer currentFirst = queue[0];
        if (currentFirst != lastFirstCustomer)
        {
            lastFirstCustomer = currentFirst;

            // 第一位顧客變更，檢查他的訂單
            var order = currentFirst.GetComponent<CustomerOrder>();
            if (order != null && order.IsOrderReady)
            {
                bool hasDrink003 = order.selectedItems.Any(item =>
                    item.menuItem.itemName == "Drink" &&
                    item.menuItem.itemTag == "003");

                if (hasDrink003)
                {
                    Debug.Log("第一位顧客包含 Drink (003)，延遲 1 秒觸發對話 10");
                    StartCoroutine(PlayDialogueDelayed(1f)); // 延遲 1 秒
                }
            }
        }
    }

    // 延遲播放對話的 Coroutine
    private IEnumerator PlayDialogueDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        TutorialDialogueController.Instance.PlayChapter("10");
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

        var inventoryItems = InventoryManager.Instance.GetItems();
        var possibleItems = new List<MenuItem>();

        foreach (var item in inventoryItems)
        {
            if (order.selectedItems.Any(orderItem =>
                orderItem.menuItem.itemTag == item.itemTag && !orderItem.isCompleted))
            {
                possibleItems.Add(item);
            }
        }

        if (possibleItems.Count == 0)
        {
            Debug.Log("物品欄中沒有顧客需要的餐點，無法提交");
            return;
        }

        int submittedCount = 0;
        foreach (var item in possibleItems)
        {
            foreach (var orderItem in order.selectedItems)
            {
                if (orderItem.menuItem.itemTag == item.itemTag && !orderItem.isCompleted)
                {
                    orderItem.isCompleted = true;
                    submittedCount++;
                    InventoryManager.Instance.RemoveItem(item);
                    break;
                }
            }
        }

        Debug.Log($"提交了 {submittedCount} 份餐點給顧客");
        OrderDisplayManager.Instance.UpdateOrderDisplayImagesForCustomer(firstCustomer);

        if (order.IsOrderComplete())
        {
            Debug.Log("顧客訂單完成，顧客即將離開");

            // ===== 獎勵計算 =====
            int baseReward = 10;
            int totalExp = 0;
            int totalPopularity = 0;
            int totalMoney = 0;

            foreach (var orderItem in order.selectedItems)
            {
                int multiplier = 1;
                switch (orderItem.menuItem.grade)
                {
                    case BaseMinigame.DishGrade.Perfect:
                        multiplier = 2;
                        break;
                    case BaseMinigame.DishGrade.Good:
                        multiplier = 1;
                        break;
                    case BaseMinigame.DishGrade.Mutated:
                        multiplier = 3;
                        break;
                }

                totalExp += baseReward * multiplier;
                totalPopularity += baseReward * multiplier;
                totalMoney += baseReward * multiplier;
            }

            SessionRewardTracker.Instance.AddRewards(totalExp, totalPopularity, totalMoney);
            SessionRewardTracker.Instance.AddCustomer(); // 增加客流量

            StartCoroutine(DelayCustomerLeave(firstCustomer));
        }
    }

    IEnumerator DelayCustomerLeave(Customer customer)
    {
        yield return new WaitForSeconds(0.5f);
        customer.ReceiveOrder();
        queueManager.LeaveQueue(customer);
    }
}
