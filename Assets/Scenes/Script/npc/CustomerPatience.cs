using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.UI;

public class CustomerPatience : MonoBehaviour
{
    [Header("最大上限")] public float maxPatience = 9f;
    [Header("當前初始值")] public float currentPatience = 9f;
    [Header("每秒遞減量")] public float decreasePerSec = 1f;

    [Header("UI 設定")]
    public GameObject patienceUIPrefab;
    public Transform uiAnchor;

    /* 狀態 */
    private bool isRunning = false;
    private Coroutine decreaseCoroutine;

    /* UI */
    private GameObject uiInstance;
    private Transform[] redHearts;      // 3 顆紅心

    /* ---------- 生命週期 ---------- */
    void Start()
    {
        // 確保不超過最大上限，也不低於 0
        currentPatience = Mathf.Clamp(currentPatience, 0f, maxPatience);
    }

    /* ---------- 對外介面 ---------- */
    /** 開始掉血（受 Time.timeScale 影響） */
    public void StartPatience()
    {
        if (isRunning) return;
        isRunning = true;
        SpawnUI();
        decreaseCoroutine = StartCoroutine(DecreaseOverTime());
    }

    /** 暫停掉血（對話期用） */
    public void StopPatience()
    {
        isRunning = false;
        if (decreaseCoroutine != null)
            StopCoroutine(decreaseCoroutine);
        CloseUI();
    }

    /** 立即增減耐心（不超上限） */
    public void AddPatience(float amount)
    {
        currentPatience = Mathf.Clamp(currentPatience + amount, 0f, maxPatience);
        UpdateHearts();
    }

    /** 立即扣血（不超下限） */
    public void ReducePatience(float amount)
    {
        currentPatience = Mathf.Clamp(currentPatience - amount, 0f, maxPatience);
        if (currentPatience <= 0f) HandleOutOfPatience();
        else UpdateHearts();
    }

    /* ---------- 內部實作 ---------- */
    private IEnumerator DecreaseOverTime()
    {
        while (currentPatience > 0f)
        {
            yield return new WaitForSeconds(1f);   // 受 Time.timeScale 影響
            currentPatience -= decreasePerSec;
            UpdateHearts();
        }
        HandleOutOfPatience();
    }

    private void UpdateHearts()
    {
        // 🔒 空值保護
        if (redHearts == null || redHearts.Length == 0) return;
        for (int i = 0; i < 3; i++)
            if (redHearts[i] == null) return;

        float oneThird = maxPatience / 3f;
        for (int i = 0; i < 3; i++)
        {
            float lower = i * oneThird;
            float upper = (i + 1) * oneThird;

            float scale = 0f;
            if (currentPatience >= upper) scale = 1f;
            else if (currentPatience > lower) scale = (currentPatience - lower) / oneThird;

            redHearts[i].localScale = Vector3.one * scale;
        }
    }
    private void HandleOutOfPatience()
    {
        isRunning = false;
        Debug.Log($"{gameObject.name} 耐心耗盡！");
        CloseUI();
        GetComponent<Customer>()?.LeaveAndDespawn();
    }

    private void SpawnUI()
    {
        if (patienceUIPrefab == null || uiAnchor == null) return;
        uiInstance = Instantiate(patienceUIPrefab, uiAnchor);
        redHearts = uiInstance.GetComponentsInChildren<Transform>()
                              .Where(t => t.name.Contains("紅心"))
                              .OrderBy(t => t.GetSiblingIndex())
                              .Take(3)
                              .ToArray();
        UpdateHearts();
    }

    private void CloseUI()
    {
        if (uiInstance == null) return;
        Destroy(uiInstance);
        uiInstance = null;
    }
}