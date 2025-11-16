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

    /* ===== 首次播放標記 ===== */
    private static bool hasPlayedHONGXIAOOnce = false;
    private static bool hasPlayedDrink003Once = false;
    /* ======================== */

    /* ===== 新增：追蹤隊伍狀態 ===== */
    private int lastQueueCount = -1;
    /* ============================ */

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

        // ✅ 新增：偵測隊伍變化（有→無）
        CheckQueueStatusForDialogue();

        float distance = Vector3.Distance(transform.position, playerTransform.position);
        if (distance <= interactRange && Input.GetKeyDown(KeyCode.E))
        {
            TrySubmitOrder();
        }
    }

    /* ===== 新增方法：當隊伍變成空時播放對話13 ===== */
    private void CheckQueueStatusForDialogue()
    {
        var queue = queueManager.GetCurrentQueue();
        int currentCount = queue.Count;

        // 如果從「有顧客」變成「沒有顧客」
        if (lastQueueCount > 0 && currentCount == 0)
        {
            Debug.Log("隊伍變為空，觸發教學對話13");

            if (TutorialDialogueController.Instance != null)
            {
                // 開始 Coroutine 播放 Chapter13 並解除時間禁止
                StartCoroutine(PlayChapter13AndEnableTime());
            }
        }

        lastQueueCount = currentCount;
    }

    /// <summary>
    /// 播放 Chapter13，對話結束後解除 FreeModeToggleManager 的時間禁止
    /// </summary>
    private IEnumerator PlayChapter13AndEnableTime()
    {
        if (TutorialDialogueController.Instance == null)
            yield break;

        // 假設你有 Coroutine 版的 PlayChapter
        yield return TutorialDialogueController.Instance.PlaySingleChapter("13");

        // 對話結束，解除 FreeModeToggleManager 的時間禁止
        if (FreeModeToggleManager.Instance != null)
        {
            FreeModeToggleManager.Instance.AllowTimeFlow = true;
            Debug.Log("對話13結束，解除 FreeModeToggleManager 時間禁止");
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

        /* 1. 百太郎 Variant → 只播 11 + 解鎖（只一次）*/
        if (!hasPlayedHONGXIAOOnce &&
            currentFirst.name.Contains("百太郎 Variant"))
        {
            hasPlayedHONGXIAOOnce = true;
            if (TutorialDialogueController.Instance != null)
                TutorialDialogueController.Instance.PlayChapter("11");
            if (TutorialProgressManager.Instance != null)
                TutorialProgressManager.Instance.CompleteEvent("Unlock HONGXIAO Card");
            return;   // 優先處理，後面不再檢查
        }

        /* 2. Drink (003) 檢查（只播放一次）*/
        if (!hasPlayedDrink003Once && currentFirst != lastFirstCustomer)
        {
            lastFirstCustomer = currentFirst;
            StartCoroutine(CheckDrink003AfterReady(currentFirst));
        }
    }

    private IEnumerator CheckDrink003AfterReady(Customer customer)
    {
        var order = customer.GetComponent<CustomerOrder>();
        if (order == null) yield break;

        yield return new WaitUntil(() => order.IsOrderReady);

        if (hasPlayedDrink003Once) yield break;

        bool hasDrink003 = order.selectedItems.Any(item =>
            item.menuItem.itemName == "Drink" &&
            item.menuItem.itemTag == "003");

        if (hasDrink003)
        {
            hasPlayedDrink003Once = true;
            Debug.Log("第一位顧客包含 Drink (003)，延遲 1 秒觸發對話 10");
            yield return new WaitForSeconds(1f);
            if (TutorialDialogueController.Instance != null)
                TutorialDialogueController.Instance.PlayChapter("10");
            if (TutorialProgressManager.Instance != null)
                TutorialProgressManager.Instance.CompleteEvent("Unlock drinks");
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

            int baseReward = 10;
            int totalExp = 0;
            int totalPopularity = 0;
            int totalMoney = 0;

            foreach (var orderItem in order.selectedItems)
            {
                int multiplier = 1;
                switch (orderItem.menuItem.grade)
                {
                    case BaseMinigame.DishGrade.Perfect: multiplier = 2; break;
                    case BaseMinigame.DishGrade.Good: multiplier = 1; break;
                    case BaseMinigame.DishGrade.Mutated: multiplier = 3; break;
                }

                totalExp += baseReward * multiplier;
                totalPopularity += baseReward * multiplier;
                totalMoney += baseReward * multiplier;
            }

            SessionRewardTracker.Instance.AddRewards(totalExp, totalPopularity, totalMoney);
            SessionRewardTracker.Instance.AddCustomer();

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
