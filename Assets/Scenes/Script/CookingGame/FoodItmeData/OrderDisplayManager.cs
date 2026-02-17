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

    private int MaxLights => quantityLightSprites != null ? quantityLightSprites.Length - 1 : 0;
    private readonly Dictionary<Customer, GameObject> activeDisplays = new Dictionary<Customer, GameObject>();

    private bool hasPlayedDialogue9 = false;

    [Header("Order Display SFX Clips")]
    [SerializeField] private AudioClip spawnOrderSfx;
    [SerializeField] private AudioClip despawnOrderSfx;

    [Header("Order Display SFX Mixers (AudioSource Output)")]
    [Tooltip("用於 UI 出現音效的 AudioSource。請在 Inspector 把 Output 指到你想要的 AudioMixerGroup。")]
    [SerializeField] private AudioSource spawnSfxSource;

    [Tooltip("用於 UI 消失音效的 AudioSource。請在 Inspector 把 Output 指到你想要的 AudioMixerGroup。")]
    [SerializeField] private AudioSource despawnSfxSource;

    [Header("SFX Volume")]
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        UpdateOrderDisplays();
    }

    private void PlaySpawnSfx()
    {
        if (spawnOrderSfx == null) return;

        if (spawnSfxSource != null)
        {
            spawnSfxSource.PlayOneShot(spawnOrderSfx, sfxVolume);
        }
        else
        {
            // 保底：若沒指定 AudioSource，就用臨時 AudioSource 播（不走 mixer group）
            var cam = Camera.main;
            Vector3 pos = cam != null ? cam.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(spawnOrderSfx, pos, sfxVolume);
        }
    }

    private void PlayDespawnSfx()
    {
        if (despawnOrderSfx == null) return;

        if (despawnSfxSource != null)
        {
            despawnSfxSource.PlayOneShot(despawnOrderSfx, sfxVolume);
        }
        else
        {
            // 保底：若沒指定 AudioSource，就用臨時 AudioSource 播（不走 mixer group）
            var cam = Camera.main;
            Vector3 pos = cam != null ? cam.transform.position : transform.position;
            AudioSource.PlayClipAtPoint(despawnOrderSfx, pos, sfxVolume);
        }
    }

    private void UpdateOrderDisplays()
    {
        var queue = CustomerQueueManager.Instance.GetCurrentQueue();
        var firstFour = queue.Take(4).ToList();

        // Step 1：移除不在前四名的顧客 UI
        var toRemove = activeDisplays.Keys.Except(firstFour).ToList();
        foreach (var customer in toRemove)
        {
            RemoveCustomerDisplay(customer);
        }

        // Step 2：按順序依序顯示訂單
        for (int i = 0; i < firstFour.Count; i++)
        {
            var customer = firstFour[i];
            var order = customer.GetComponent<CustomerOrder>();
            if (order == null || !order.IsOrderReady) continue;

            // 訂單完成就移除
            if (order.IsOrderComplete())
            {
                RemoveCustomerDisplay(customer, delay: 0.5f);
                continue;
            }

            // 如果前方顧客還沒顯示訂單，就暫不顯示
            if (i > 0)
            {
                var prevCustomer = firstFour[i - 1];
                if (!activeDisplays.ContainsKey(prevCustomer))
                    continue;
            }

            // 第一次生成訂單時播放對話 9（只觸發一次）
            if (!hasPlayedDialogue9 && i == 0)
            {
                if (TutorialDialogueController.Instance != null)
                {
                    TutorialDialogueController.Instance.PlayChapter("9");
                    hasPlayedDialogue9 = true;
                }
            }

            // 建立訂單 UI
            if (!activeDisplays.ContainsKey(customer))
            {
                bool sameTag = order.selectedItems.TrueForAll(item => item.menuItem.itemTag == order.selectedItems[0].menuItem.itemTag);
                GameObject prefab = sameTag ? oneSlotOrderPrefab : twoSlotOrderPrefab;

                GameObject display = Instantiate(prefab, orderPanelContainer);

                RectTransform rt = display.GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.localScale = Vector3.zero;

                    // 出現音效：放大動畫真正開始時播放（同步視覺）
                    rt.DOScale(Vector3.one, 0.3f)
                      .SetEase(Ease.OutBack)
                      .OnStart(PlaySpawnSfx)
                      .SetLink(rt.gameObject, LinkBehaviour.KillOnDestroy);
                }
                else
                {
                    // fallback：沒有 RectTransform，直接播放
                    PlaySpawnSfx();
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
        if (!activeDisplays.TryGetValue(customer, out var displayObj))
            return;

        if (displayObj != null)
        {
            RectTransform rt = displayObj.GetComponent<RectTransform>();
            if (rt != null)
            {
                // 消失音效：縮小動畫真正開始時播放（delay 後才播，最貼合視覺）
                rt.DOScale(Vector3.zero, 0.3f)
                  .SetEase(Ease.InBack)
                  .SetDelay(delay)
                  .OnStart(PlayDespawnSfx)
                  .OnComplete(() => Destroy(displayObj))
                  .SetLink(rt.gameObject, LinkBehaviour.KillOnDestroy);
            }
            else
            {
                // fallback：沒有 RectTransform，延遲到實際 Destroy 時再播
                DOVirtual.DelayedCall(delay, PlayDespawnSfx);
                Destroy(displayObj, delay > 0 ? delay : 0f);
            }
        }

        activeDisplays.Remove(customer);
        customer.GetComponent<CustomerPatience>()?.StopPatience();
    }

    private void UpdateOrderDisplayImages(GameObject displayObj, CustomerOrder order)
    {
        if (order == null || order.selectedItems == null || order.selectedItems.Count == 0) return;

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

            if (slot == null) continue;

            var img = slot.GetComponent<Image>();
            if (img != null)
                img.sprite = group.sprite;

            var lightImage = slot.Find("數量燈")?.GetComponent<Image>();
            if (lightImage != null && quantityLightSprites != null && quantityLightSprites.Length > 0)
            {
                int count = Mathf.Clamp(group.totalCount - group.completedCount, 0, MaxLights);
                if (count < quantityLightSprites.Length)
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
                {
                    checkmark.SetActive(false);
                }
            }
        }
    }

    private void UpdateVisualEffects(List<Customer> firstFour)
    {
        for (int i = 0; i < firstFour.Count; i++)
        {
            var customer = firstFour[i];
            if (!activeDisplays.ContainsKey(customer)) continue;

            GameObject display = activeDisplays[customer];
            Image[] images = display.GetComponentsInChildren<Image>();
            Color targetColor = (i == 0) ? Color.white : new Color(0.5f, 0.5f, 0.5f);

            foreach (var img in images)
                img.color = targetColor;
        }
    }

    public void UpdateOrderDisplayImagesForCustomer(Customer customer)
    {
        if (!activeDisplays.TryGetValue(customer, out var displayObj)) return;

        var order = customer.GetComponent<CustomerOrder>();
        if (order != null)
            UpdateOrderDisplayImages(displayObj, order);
    }
}
