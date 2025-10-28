using UnityEngine;
using DG.Tweening;

public class CustomerPatience : MonoBehaviour
{
    [Header("耐心設定")]
    public float maxPatience = 9f;       // 最大耐心
    public float currentPatience = 9f;        // 當前耐心，隨時間減少

    [Header("UI設定")]
    public GameObject patienceUIPrefab;
    public Transform uiAnchor;
    public float heartScale = 0.06f;

    private GameObject uiInstance;
    private Transform[] hearts;           // 三顆紅心
    private int currentHeartIndex = 0;

    private bool isRunning = false;

    public void StartPatience()
    {
        currentPatience = maxPatience;
        isRunning = true;
        UpdateHearts();
    }

    void Update()
    {
        if (!isRunning) return;

        // 耐心隨時間減少
        currentPatience -= Time.deltaTime;
        currentPatience = Mathf.Max(currentPatience, 0);

        UpdateHearts();

        if (currentPatience <= 0)
        {
            isRunning = false;
            OutOfPatience();
        }
    }

    private void InitializeUI()
    {
        if (uiInstance != null) Destroy(uiInstance);

        if (patienceUIPrefab != null && uiAnchor != null)
        {
            uiInstance = Instantiate(patienceUIPrefab, uiAnchor);
            uiInstance.transform.localScale = Vector3.one * heartScale;

            hearts = uiInstance.GetComponentsInChildren<Transform>();
            hearts = System.Array.FindAll(hearts, t => t.name.Contains("紅心"));
            System.Array.Sort(hearts, (a, b) => a.GetSiblingIndex() - b.GetSiblingIndex());

            // 初始全部紅心都可見
            foreach (var heart in hearts)
            {
                heart.localScale = Vector3.one;
            }
        }
        else
        {
            Debug.LogWarning("CustomerPatience: 未設定 UI Prefab 或 Anchor");
        }
    }

    public void StopPatience()
    {
        isRunning = false;

        if (uiInstance != null)
        {
            uiInstance.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    Destroy(uiInstance);
                    uiInstance = null;
                });
        }
    }

    private void UpdateHearts()
    {
        if (hearts == null || hearts.Length != 3) return;

        float ratio = currentPatience / maxPatience;   // 0 ~ 1
        int heartZone = Mathf.FloorToInt((1 - ratio) * 3f); // 0,1,2

        // 保證索引不超過
        heartZone = Mathf.Clamp(heartZone, 0, 2);

        // 更新每顆心的縮放
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < heartZone)
            {
                // 已經消耗完的心，縮到0
                hearts[i].localScale = Vector3.zero;
            }
            else if (i == heartZone)
            {
                // 當前區間心，隨耐心值比例縮小
                float zoneStart = 1f - (i + 1) / 3f;
                float zoneEnd = 1f - i / 3f;
                float zoneRatio = Mathf.InverseLerp(zoneStart, zoneEnd, ratio);
                hearts[i].localScale = Vector3.one * zoneRatio;
            }
            else
            {
                // 未到的心，保持滿心
                hearts[i].localScale = Vector3.one;
            }
        }
    }

    private void OutOfPatience()
    {
        Debug.Log($"{gameObject.name} 耐心耗盡！");
        if (uiInstance != null)
        {
            uiInstance.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    Destroy(uiInstance);
                    uiInstance = null;
                    GetComponent<Customer>()?.LeaveAndDespawn();
                });
        }
        else
        {
            GetComponent<Customer>()?.LeaveAndDespawn();
        }
    }

    // 可以額外增加耐心
    public void AddExtraPatience(float seconds)
    {
        currentPatience = Mathf.Min(currentPatience + seconds, maxPatience);
        UpdateHearts();
    }
}
