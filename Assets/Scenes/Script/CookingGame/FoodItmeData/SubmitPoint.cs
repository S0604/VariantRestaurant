using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SubmitPoint : MonoBehaviour
{
    public float interactRange = 3f;
    public CustomerQueueManager queueManager;
    public Transform playerTransform;

    [Header("Reward Settings")]
    public int baseReward = 10;

    [Header("Grade Multiplier")]
    public int goodMultiplier = 1;
    public int perfectMultiplier = 2;
    public int mutatedMultiplier = 3;
    public int badMultiplier = 1;
    public int failMultiplier = 0;

    [Header("Profit Multiplier (Upgrade)")]
    public bool applyProfitToMoney = true;
    public bool applyProfitToExp = true;
    public float profitFallback = 1f;
    public bool debugLog = false;

    private float cachedProfitMult = 1f;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        RefreshReferences();

        UpgradeManager.OnAnyLevelChanged += HandleAnyUpgradeChanged;
        RefreshProfitMultiplierCache();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UpgradeManager.OnAnyLevelChanged -= HandleAnyUpgradeChanged;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshReferences();
        RefreshProfitMultiplierCache();
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
            if (debugLog) Debug.Log("目前沒有顧客等待訂單");
            return;
        }

        Customer firstCustomer = queue[0];
        var order = firstCustomer.GetComponent<CustomerOrder>();
        if (order == null || !order.IsOrderReady)
        {
            if (debugLog) Debug.Log("第一位顧客尚未準備好訂單");
            return;
        }

        if (order.IsOrderComplete())
        {
            if (debugLog) Debug.Log("訂單已完成，顧客即將離開");
            return;
        }

        var inventoryItems = InventoryManager.Instance.GetItems();
        var possibleItems = new List<MenuItem>();

        foreach (var item in inventoryItems)
        {
            if (item == null) continue;

            bool needed = order.selectedItems.Any(orderItem =>
                orderItem != null &&
                orderItem.menuItem != null &&
                orderItem.menuItem.itemTag == item.itemTag &&
                !orderItem.isCompleted);

            if (needed) possibleItems.Add(item);
        }

        if (possibleItems.Count == 0)
        {
            if (debugLog) Debug.Log("物品欄中沒有顧客需要的餐點，無法提交");
            return;
        }

        int submittedCount = 0;

        foreach (var item in possibleItems.ToList())
        {
            if (order.SubmitItem(item))
            {
                submittedCount++;
                InventoryManager.Instance.RemoveItem(item);
            }
        }

        if (debugLog) Debug.Log($"提交了 {submittedCount} 份餐點給顧客");
        OrderDisplayManager.Instance?.UpdateOrderDisplayImagesForCustomer(firstCustomer);

        if (order.IsOrderComplete())
        {
            int totalExp = 0;
            int totalPopularity = 0;
            int totalMoney = 0;

            foreach (var orderItem in order.selectedItems)
            {
                if (orderItem == null) continue;

                int mult = GetMultiplierByGrade(orderItem.deliveredGrade);
                int reward = baseReward * mult;

                totalExp += reward;
                totalPopularity += reward;
                totalMoney += reward;
            }

            float profitMult = cachedProfitMult;

            if (applyProfitToExp)
                totalExp = Mathf.RoundToInt(totalExp * profitMult);

            if (applyProfitToMoney)
                totalMoney = Mathf.RoundToInt(totalMoney * profitMult);

            if (debugLog)
            {
                Debug.Log($"[SubmitReward] profitMult={profitMult}, applyExp={applyProfitToExp}, applyMoney={applyProfitToMoney}, exp={totalExp}, pop={totalPopularity}, money={totalMoney}");
            }

            SessionRewardTracker.Instance?.AddRewards(totalExp, totalPopularity, totalMoney);
            SessionRewardTracker.Instance?.AddCustomer();

            StartCoroutine(DelayCustomerLeave(firstCustomer));
        }
    }

    int GetMultiplierByGrade(BaseMinigame.DishGrade grade)
    {
        switch (grade)
        {
            case BaseMinigame.DishGrade.Perfect: return Mathf.Max(0, perfectMultiplier);
            case BaseMinigame.DishGrade.Good: return Mathf.Max(0, goodMultiplier);
            case BaseMinigame.DishGrade.Bad: return Mathf.Max(0, badMultiplier);
            case BaseMinigame.DishGrade.Mutated: return Mathf.Max(0, mutatedMultiplier);
            default: return Mathf.Max(0, failMultiplier);
        }
    }

    void HandleAnyUpgradeChanged(UpgradeType type, int newLevel)
    {
        if (type != UpgradeType.ProfitMultiplier) return;
        RefreshProfitMultiplierCache();
    }

    void RefreshProfitMultiplierCache()
    {
        float m = profitFallback;

        var upg = UpgradeManager.Instance;
        if (upg != null)
        {
            float v = upg.GetValue(UpgradeType.ProfitMultiplier);
            if (v > 0f) m = v;
        }

        if (m <= 0f) m = 1f;
        cachedProfitMult = m;

        if (debugLog) Debug.Log($"[SubmitPoint] ProfitMultiplier cache updated: {cachedProfitMult}");
    }

    IEnumerator DelayCustomerLeave(Customer customer)
    {
        yield return new WaitForSeconds(0.5f);
        if (customer == null) yield break;

        customer.ReceiveOrder();
        queueManager.LeaveQueue(customer);
    }
}
