using UnityEngine;
using System.Linq;
using DG.Tweening;

public class CustomerPatience : MonoBehaviour
{
    public float totalPatienceTime = 9f;
    public GameObject patienceUIPrefab;
    public Transform uiAnchor;

    [Header("單顆紅心耐心時間限制")]
    public float minPatienceDurationPerHeart = 0.3f;
    public float maxPatienceDurationPerHeart = 10f;

    private float timePerHeart;
    private int currentHeartIndex = 0;

    private GameObject uiInstance;
    private Transform[] redHearts;

    private bool isRunning = false;
    private bool isInitialized = false;

    public void StartPatience()
    {
        if (isRunning) return;

        if (!isInitialized) InitializeUI();

        if (redHearts == null || redHearts.Length == 0)
        {
            Debug.LogWarning("未找到紅心物件，耐心系統無法啟動！");
            return;
        }

        currentHeartIndex = 0;
        isRunning = true;

        // ✅ 基礎耐心 + 被動技能加成
        float baseTime = totalPatienceTime + PassiveSkillManager.Instance.maxPatienceBonus;

        timePerHeart = baseTime / redHearts.Length;

        foreach (var heart in redHearts)
            heart.localScale = Vector3.one;

        StartNextHeart();
    }

    public void ForcePatience(float forcedSeconds)
    {
        if (isRunning) return;

        InitUIIfNeeded();

        if (redHearts == null || redHearts.Length == 0)
        {
            Debug.LogWarning("未找到紅心物件，耐心系統無法啟動！");
            return;
        }

        currentHeartIndex = 0;
        isRunning = true;
        timePerHeart = forcedSeconds / redHearts.Length;

        foreach (var heart in redHearts)
            heart.localScale = Vector3.one;

        StartNextHeart();
    }

    private void InitUIIfNeeded()
    {
        if (isInitialized) return;

        if (patienceUIPrefab != null && uiAnchor != null)
        {
            uiInstance = Instantiate(patienceUIPrefab, uiAnchor);
            uiInstance.transform.localScale = Vector3.zero;
            uiInstance.transform.DOScale(Vector3.one * 0.06f, 0.3f).SetEase(Ease.OutBack);

            redHearts = uiInstance.GetComponentsInChildren<Transform>()
                .Where(t => t.name.Contains("紅心"))
                .OrderBy(t => t.GetSiblingIndex())
                .ToArray();

            isInitialized = true;
        }
        else
        {
            Debug.LogWarning("CustomerPatience 初始化失敗：未設定 prefab 或 anchor");
        }
    }

    private void InitializeUI()
    {
        InitUIIfNeeded(); // 讓兩者統一呼叫邏輯
    }

    private void StartNextHeart()
    {
        if (!isRunning || currentHeartIndex >= redHearts.Length) return;

        var heart = redHearts[currentHeartIndex];
        if (heart != null)
        {
            heart.localScale = Vector3.one;

            // 🔹 防呆，避免 SpecialCustomerEffectManager 為 null
            float modifier = 1f;
            if (SpecialCustomerEffectManager.Instance != null)
                modifier += SpecialCustomerEffectManager.Instance.patienceRateModifier;

            float rawTime = timePerHeart * Mathf.Max(modifier, 0.1f);
            float adjustedTime = Mathf.Clamp(rawTime, minPatienceDurationPerHeart, maxPatienceDurationPerHeart);

            Debug.Log($"[Patience] heart {currentHeartIndex + 1}/{redHearts.Length}, adjustedTime: {adjustedTime}");

   
            heart.DOKill();

            heart.DOScale(Vector3.zero, adjustedTime)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    currentHeartIndex++;

                    if (currentHeartIndex < redHearts.Length)
                    {
                        StartNextHeart();
                    }
                    else
                    {
                        isRunning = false;
                        HandleOutOfPatience();
                    }
                });
        }
    }

    public void StopPatience()
    {
        isRunning = false;

        if (uiInstance != null)
        {
            Destroy(uiInstance);
            uiInstance = null;
        }
    }

    private void HandleOutOfPatience()
    {
        Debug.Log($"{gameObject.name} 耐心耗盡！");

        if (uiInstance != null)
        {
            uiInstance.transform.DOScale(Vector3.zero, 0.3f)
                .SetEase(Ease.InBack)
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
    public void AddExtraPatience(float seconds)
    {
        if (!isInitialized || redHearts == null || redHearts.Length == 0) return;

        // 增加總耐心時間
        totalPatienceTime += seconds;

        // 重新計算每顆心的時間
        timePerHeart = totalPatienceTime / redHearts.Length;

        Debug.Log($"{gameObject.name} 耐心增加 {seconds} 秒，新總耐心 = {totalPatienceTime}");

        // 讓正在縮小的心重新計算時間
        if (currentHeartIndex < redHearts.Length)
        {
            // 先殺掉正在執行的 DOTween 動畫
            redHearts[currentHeartIndex].DOKill();

            // 重新啟動當前這顆心
            StartNextHeart();
        }
    }


}
