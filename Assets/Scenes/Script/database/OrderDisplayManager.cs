using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class OrderDisplayManager : MonoBehaviour
{
    public Transform orderPanelContainer;
    public GameObject oneSlotOrderPrefab;
    public GameObject twoSlotOrderPrefab;

    [Header("數量燈圖示（0~3）")]
    public Sprite[] quantityLightSprites;

    private int MaxLights => quantityLightSprites.Length - 1;
    private Dictionary<Customer, GameObject> activeDisplays = new Dictionary<Customer, GameObject>();

    void Update()
    {
        UpdateOrderDisplays();
    }

    void UpdateOrderDisplays()
    {
        // 先移除已經被 Destroy 的顧客（Unity 中會 == null）
        var invalidCustomers = activeDisplays.Keys.Where(c => c == null).ToList();
        foreach (var c in invalidCustomers)
            activeDisplays.Remove(c);

        // 取得排隊中前四位
        var queue = CustomerQueueManager.Instance.GetCurrentQueue();
        var firstFour = queue.Take(4).ToList();

        // 移除已經不在前四位的顧客（但還活著）
        var toRemove = activeDisplays.Keys.Except(firstFour).ToList();
        foreach (var customer in toRemove)
        {
            if (activeDisplays[customer] != null)
                Destroy(activeDisplays[customer]);

            activeDisplays.Remove(customer);

            if (customer != null)
                customer.GetComponent<CustomerPatience>()?.StopPatience();
        }

        // 前四位更新 UI
        for (int i = 0; i < firstFour.Count; i++)
        {
            var customer = firstFour[i];
            if (customer == null) continue;

            var order = customer.GetComponent<CustomerOrder>();
            if (order == null || !order.IsOrderReady) continue;

            if (!activeDisplays.ContainsKey(customer))
            {
                bool sameTag = order.selectedItems.TrueForAll(item => item.itemTag == order.selectedItems[0].itemTag);
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

                UpdateOrderDisplayImages(display, order.selectedItems);
                activeDisplays.Add(customer, display);
            }

            var patience = customer.GetComponent<CustomerPatience>();
            if (i == 0)
                patience?.StartPatience();
            else
                patience?.StopPatience();
        }

        UpdateVisualEffects(firstFour);
    }

    void UpdateOrderDisplayImages(GameObject displayObj, List<MenuItem> items)
    {
        var groupedByTag = items
            .GroupBy(i => i.itemTag)
            .Select(g => new {
                tag = g.Key,
                sprite = g.First().itemImage,
                count = g.Count()
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
                    int count = Mathf.Clamp(group.count, 0, MaxLights);
                    lightImage.sprite = quantityLightSprites[count];
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
}