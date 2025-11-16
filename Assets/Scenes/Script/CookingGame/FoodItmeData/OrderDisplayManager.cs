using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class OrderDisplayManager : MonoBehaviour
{
    public static OrderDisplayManager Instance { get; private set; }

    public Transform orderPanelContainer;
    public GameObject oneSlotOrderPrefab;
    public GameObject twoSlotOrderPrefab;

    [Header("數量燈圖示（0~3）")]
    public Sprite[] quantityLightSprites;

    private int MaxLights => quantityLightSprites.Length - 1;
    private Dictionary<Customer, GameObject> activeDisplays = new Dictionary<Customer, GameObject>();

    private bool hasPlayedDialogue9 = false; // ✅ 用來確保只觸發一次

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    void Update()
    {
        UpdateOrderDisplays();
    }

    void UpdateOrderDisplays()
    {
        var queue = CustomerQueueManager.Instance.GetCurrentQueue();
        var firstFour = queue.Take(4).ToList();

        // 🔸 Step 1：移除不在前四名的顧客 UI
        var toRemove = activeDisplays.Keys.Except(firstFour).ToList();
        foreach (var customer in toRemove)
        {
            RemoveCustomerDisplay(customer);
        }

        // 🔸 Step 2：按順序依序顯示訂單
        for (int i = 0; i < firstFour.Count; i++)
        {
            var customer = firstFour[i];
            var order = customer.GetComponent<CustomerOrder>();
            if (order == null || !order.IsOrderReady) continue;

            // 如果訂單完成就播放縮小動畫
            if (order.IsOrderComplete())
            {
                RemoveCustomerDisplay(customer, delay: 0.5f);
                continue;
            }

            // ✅ 關鍵修改：如果前方顧客還沒顯示訂單，就暫不顯示
            if (i > 0)
            {
                var prevCustomer = firstFour[i - 1];
                if (!activeDisplays.ContainsKey(prevCustomer))
                {
                    // 前面顧客訂單還沒顯示，先跳過這一輪
                    continue;
                }
            }

            // ✅ 新增：第一次生成訂單時播放對話 9
            if (!hasPlayedDialogue9 && i == 0)
            {
                if (TutorialDialogueController.Instance != null)
                {
                    TutorialDialogueController.Instance.PlayChapter("9");
                    hasPlayedDialogue9 = true;
                }
            }

            // 建立或更新訂單 UI
            if (!activeDisplays.ContainsKey(customer))
            {
                bool sameTag = order.selectedItems.TrueForAll(item => item.menuItem.itemTag == order.selectedItems[0].menuItem.itemTag);
                GameObject prefab = sameTag ? oneSlotOrderPrefab : twoSlotOrderPrefab;

                GameObject display = Instantiate(prefab, orderPanelContainer);
                RectTransform rt = display.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.localScale = Vector3.zero;
                    rt.DOScale(Vector3.one, 0.3f)
                      .SetEase(Ease.OutBack)
                      .SetLink(rt.gameObject, LinkBehaviour.KillOnDestroy);
                }

                activeDisplays.Add(customer, display);
            }

            // 更新訂單內容
            UpdateOrderDisplayImages(activeDisplays[customer], order);

            // 耐心控制：只有第一位開始倒數
            var patience = customer.GetComponent<CustomerPatience>();
            if (i == 0)
                patience?.StartPatience();
            else
                patience?.StopPatience();
        }

        UpdateVisualEffects(firstFour);
    }

    private void RemoveCustomerDisplay(Customer customer, float delay = 0f)
    {
        if (activeDisplays.TryGetValue(customer, out var displayObj))
        {
            if (displayObj != null)
            {
                RectTransform rt = displayObj.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.DOScale(Vector3.zero, 0.3f)
                      .SetEase(Ease.InBack)
                      .SetDelay(delay)
                      .OnComplete(() => Destroy(displayObj))
                      .SetLink(rt.gameObject, LinkBehaviour.KillOnDestroy);
                }
                else
                {
                    Destroy(displayObj, delay > 0 ? delay : 0f);
                }
            }
            activeDisplays.Remove(customer);
            customer.GetComponent<CustomerPatience>()?.StopPatience();
        }
    }

    void UpdateOrderDisplayImages(GameObject displayObj, CustomerOrder order)
    {
        var groupedByTag = order.selectedItems
            .GroupBy(i => i.menuItem.itemTag)
            .Select(g => new
            {
                tag = g.Key,
                sprite = g.First().menuItem.itemImage,
                completedCount = g.Count(item => item.isCompleted),
                totalCount = g.Count()
            })
            .ToList();

        Transform[] slots =
        {
            displayObj.transform.Find("Panel/圖樣"),
            displayObj.transform.Find("Panel/圖樣(1)")
        };

        if (order == null || order.selectedItems == null || order.selectedItems.Count == 0) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (i >= groupedByTag.Count) break;

            var slot = slots[i];
            var group = groupedByTag[i];

            if (slot != null)
            {
                var img = slot.GetComponent<Image>();
                if (img != null)
                    img.sprite = group.sprite;

                var lightImage = slot.Find("數量燈")?.GetComponent<Image>();
                if (lightImage != null)
                {
                    int count = Mathf.Clamp(group.totalCount - group.completedCount, 0, MaxLights);
                    if (quantityLightSprites != null && count < quantityLightSprites.Length)
                        lightImage.sprite = quantityLightSprites[count];
                }

                var checkmark = slot.Find("Checkmark")?.gameObject;
                if (checkmark != null)
                {
                    bool shouldShow = group.completedCount >= group.totalCount;
                    if (shouldShow)
                    {
                        checkmark.SetActive(true);
                        checkmark.transform.localScale = Vector3.zero;
                        checkmark.transform.DOScale(Vector3.one, 0.3f)
                            .SetEase(Ease.OutBack)
                            .SetLink(checkmark, LinkBehaviour.KillOnDestroy);
                    }
                    else
                        checkmark.SetActive(false);
                }
            }
        }
    }

    void UpdateVisualEffects(List<Customer> firstFour)
    {
        for (int i = 0; i < firstFour.Count; i++)
        {
            var customer = firstFour[i];
            if (activeDisplays.ContainsKey(customer))
            {
                GameObject display = activeDisplays[customer];
                Image[] images = display.GetComponentsInChildren<Image>();
                Color targetColor = (i == 0) ? Color.white : new Color(0.5f, 0.5f, 0.5f);
                foreach (var img in images)
                    img.color = targetColor;
            }
        }
    }

    public void UpdateOrderDisplayImagesForCustomer(Customer customer)
    {
        if (activeDisplays.TryGetValue(customer, out var displayObj))
        {
            var order = customer.GetComponent<CustomerOrder>();
            if (order != null)
                UpdateOrderDisplayImages(displayObj, order);
        }
    }
}
