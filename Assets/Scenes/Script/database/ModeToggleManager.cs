using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class ModeToggleManager : MonoBehaviour
{
    [Header("UI 切換")]
    public GameObject businessUI;
    public GameObject nonBusinessUI;

    [Header("營業模式腳本")]
    public MonoBehaviour[] businessScripts;

    [Header("非營業模式腳本")]
    public MonoBehaviour[] nonBusinessScripts;

    [Header("營業模式設定")]
    public float businessDuration = 60f; // 營業時間（秒）

    private bool isBusinessMode = false;
    private float businessTimer = 0f;
    private bool isTimerRunning = false;
    private bool isClosing = false;

    private void Start()
    {
        UpdateMode(); // 初始化狀態
    }

    private void Update()
    {
        if (isTimerRunning && !isClosing)
        {
            businessTimer -= Time.deltaTime;

            if (businessTimer <= 0f)
            {
                StartCoroutine(HandleBusinessClosing());
            }
        }
    }

    public void ToggleMode()
    {
        isBusinessMode = !isBusinessMode;
        UpdateMode();
    }

    public void SetBusinessMode(bool active)
    {
        EventSystem.current.SetSelectedGameObject(null); // 清除選中
        isBusinessMode = active;
        UpdateMode();
    }

    private void UpdateMode()
    {
        // UI 切換
        if (businessUI != null) businessUI.SetActive(isBusinessMode);
        if (nonBusinessUI != null) nonBusinessUI.SetActive(!isBusinessMode);

        // 啟用腳本
        foreach (var script in businessScripts)
            if (script != null) script.enabled = isBusinessMode;

        foreach (var script in nonBusinessScripts)
            if (script != null) script.enabled = !isBusinessMode;

        // 計時器控制
        if (isBusinessMode)
        {
            businessTimer = businessDuration;
            isTimerRunning = true;
            isClosing = false;
        }
        else
        {
            isTimerRunning = false;
            isClosing = false;
        }
    }

    private IEnumerator HandleBusinessClosing()
    {
        Debug.Log("🔔 營業時間結束，開始關店流程...");
        isClosing = true;
        isTimerRunning = false;

        // 1. 停止產生新顧客
        var spawner = FindObjectOfType<CustomerSpawner>();
        if (spawner != null) spawner.enabled = false;

        // 2. 通知所有非隊伍顧客直接離開（假設他們不是在 CustomerQueueManager 中）
        var allCustomers = FindObjectsOfType<Customer>();
        foreach (var c in allCustomers)
        {
            if (!CustomerQueueManager.Instance.GetCurrentQueue().Contains(c))
            {
                c.LeaveAndDespawn();
            }
        }

        // 3. 通知排隊顧客依序離開
        var queue = CustomerQueueManager.Instance.GetCurrentQueue();
        foreach (var c in queue)
        {
            c.LeaveAndDespawn();
            yield return new WaitForSeconds(0.5f); // 間隔讓離開更自然
        }

        // 等待所有顧客銷毀
        while (CustomerManager.Instance != null && CustomerManager.Instance.CurrentCustomerCount > 0)
        {
            yield return null;
        }

        Debug.Log("✅ 所有顧客已離場，切換為非營業模式");
        SetBusinessMode(false);
    }
}
