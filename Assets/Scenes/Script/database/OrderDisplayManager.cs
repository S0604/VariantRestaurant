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

        // 移除不在前四名的顧客UI，先播放縮小動畫再銷毀
        var toRemove = activeDisplays.Keys.Except(firstFour).ToList();
        foreach (var customer in toRemove)
        {
            if (activeDisplays[customer] != null)
            {
                GameObject displayObj = activeDisplays[customer];
                RectTransform rt = displayObj.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.DOScale(Vector3.zero, 0.3f)
                      .SetEase(Ease.InBack)
                      .OnComplete(() => Destroy(displayObj))
                      .SetLink(rt.gameObject, LinkBehaviour.KillOnDestroy);
                }
                else
                {
                    Destroy(displayObj);
                }
            }
            activeDisplays.Remove(customer);

            if (customer != null && !customer.Equals(null))
            {
                customer.GetComponent<CustomerPatience>()?.StopPatience();
            }
        }

        for (int i = 0; i < firstFour.Count; i++)
        {
            var customer = firstFour[i];
            var order = customer.GetComponent<CustomerOrder>();
            if (order == null || !order.IsOrderReady) continue;

            // 訂單完成時播放縮小動畫再銷毀UI
            if (order.IsOrderComplete())
            {
                if (activeDisplays.ContainsKey(customer))
                {
                    GameObject displayObj = activeDisplays[customer];
                    if (displayObj != null)
                    {
                        UpdateOrderDisplayImages(displayObj, order);

                        RectTransform rt = displayObj.GetComponent<RectTransform>();
                        if (rt != null)
                        {
                            rt.DOScale(Vector3.zero, 0.3f)
                              .SetEase(Ease.InBack)
                              .SetDelay(0.5f) // 延遲0.5秒
                              .OnComplete(() => Destroy(displayObj))
                              .SetLink(rt.gameObject, LinkBehaviour.KillOnDestroy);
                        }
                        else
                        {
                            Destroy(displayObj, 0.5f); // 延遲銷毀
                        }
                    }
                    activeDisplays.Remove(customer);
                    customer.GetComponent<CustomerPatience>()?.StopPatience();
                }
                continue;
            }

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

            UpdateOrderDisplayImages(activeDisplays[customer], order);

            var patience = customer.GetComponent<CustomerPatience>();
            if (i == 0)
                patience?.StartPatience();
            else
                patience?.StopPatience();
        }

        UpdateVisualEffects(firstFour);
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
                    lightImage.sprite = quantityLightSprites[count];
                }

                var checkmark = slot.Find("Checkmark")?.gameObject;
                if (checkmark != null)
                {
                    checkmark.SetActive(group.completedCount >= group.totalCount);
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
                {
                    img.color = targetColor;
                }
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
