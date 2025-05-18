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

    private Dictionary<Customer, GameObject> activeDisplays = new Dictionary<Customer, GameObject>();

    void Update()
    {
        UpdateOrderDisplays();
    }

    void UpdateOrderDisplays()
    {
        var queue = CustomerQueueManager.Instance.GetCurrentQueue();
        var firstFour = queue.Take(4).ToList();

        // 移除不在前四名的 UI
        var toRemove = activeDisplays.Keys.Except(firstFour).ToList();
        foreach (var customer in toRemove)
        {
            if (activeDisplays[customer] != null)
                Destroy(activeDisplays[customer]);
            activeDisplays.Remove(customer);

            // 停止耐心顯示
            customer.GetComponent<CustomerPatience>()?.StopPatience();
        }

        // 建立 UI 並更新
        for (int i = 0; i < firstFour.Count; i++)
        {
            var customer = firstFour[i];
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

            // ✅ 耐心控制（只有第一位顧客才開啟）
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
        Transform slot0 = displayObj.transform.Find("Panel/圖樣");
        Transform slot1 = displayObj.transform.Find("Panel/圖樣(1)");

        if (slot0 != null && items.Count > 0)
        {
            Image img0 = slot0.GetComponent<Image>();
            if (img0 != null) img0.sprite = items[0].itemImage;
        }

        if (slot1 != null && items.Count > 1)
        {
            Image img1 = slot1.GetComponent<Image>();
            if (img1 != null) img1.sprite = items[1].itemImage;
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
