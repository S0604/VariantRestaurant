using UnityEngine;
using System.Linq;
using DG.Tweening;

public class CustomerPatience : MonoBehaviour
{
    public float totalPatienceTime = 9f;
    public GameObject patienceUIPrefab;
    public Transform uiAnchor;

    private float timePerHeart;
    private int currentHeartIndex = 0;

    private GameObject uiInstance;
    private Transform[] redHearts;

    private bool isRunning = false;
    private bool isInitialized = false;

    public void StartPatience()
    {
        if (isRunning) return;

        if (!isInitialized)
        {
            InitializeUI();
        }

        if (redHearts == null || redHearts.Length == 0)
        {
            Debug.LogWarning("未找到紅心物件，耐心系統無法啟動！");
            return;
        }

        currentHeartIndex = 0;
        isRunning = true;
        timePerHeart = totalPatienceTime / redHearts.Length;

        foreach (var heart in redHearts)
        {
            heart.localScale = Vector3.one;
        }

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

    private void StartNextHeart()
    {
        if (!isRunning || currentHeartIndex >= redHearts.Length) return;

        var heart = redHearts[currentHeartIndex];
        if (heart != null)
        {
            heart.localScale = Vector3.one;

            heart.DOScale(Vector3.zero, timePerHeart)
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
}