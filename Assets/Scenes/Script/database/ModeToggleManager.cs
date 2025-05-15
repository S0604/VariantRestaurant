using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ModeToggleManager : MonoBehaviour
{
    [Header("UI 切換")]
    public GameObject businessUI;
    public GameObject nonBusinessUI;

    [Header("營業模式腳本")]
    public MonoBehaviour[] businessScripts;

    [Header("非營業模式腳本")]
    public MonoBehaviour[] nonBusinessScripts;

    [Header("連結時間系統")]
    public TimeSystem timeSystem;

    private bool isBusinessMode = false;
    private bool isClosing = false;

    public void ToggleMode()
    {
        isBusinessMode = !isBusinessMode;
        if (isBusinessMode)
            UpdateMode(); // 只在啟動營業時立即切換
        else
            StartCoroutine(HandleBusinessClosing());
    }

    public void SetBusinessMode(bool active)
    {
        isBusinessMode = active;
        if (active)
            UpdateMode(); // ✅ 啟動營業時立即切換
        // ❌ 關閉營業流程將由 HandleBusinessClosing 控制
    }

    private void UpdateMode()
    {
        if (businessUI != null) businessUI.SetActive(isBusinessMode);
        if (nonBusinessUI != null) nonBusinessUI.SetActive(!isBusinessMode);

        foreach (var script in businessScripts)
            if (script != null) script.enabled = isBusinessMode;

        foreach (var script in nonBusinessScripts)
            if (script != null) script.enabled = !isBusinessMode;

        if (isBusinessMode)
        {
            isClosing = false;
            timeSystem?.StartCooldown();
        }
        else
        {
            timeSystem?.StopCooldown();
        }
    }

    public void StartClosingProcessFromTimeSystem()
    {
        if (!isBusinessMode || isClosing) return;
        StartCoroutine(HandleBusinessClosing());
    }

    private IEnumerator HandleBusinessClosing()
    {
        isClosing = true;
        Debug.Log("【ModeToggleManager】開始關店流程");

        // 關閉顧客生成腳本
        foreach (var script in businessScripts)
        {
            if (script != null && (script.GetType().Name.Contains("CustomerSpawner") || script.GetType().Name.Contains("CustomerGenerator")))
            {
                script.enabled = false;
                Debug.Log($"【ModeToggleManager】已關閉 {script.GetType().Name}");
            }
        }

        // 顧客離場
        var queueManager = CustomerQueueManager.Instance;
        if (queueManager != null)
        {
            var customers = new List<Customer>(queueManager.GetCurrentQueue());
            foreach (var c in customers)
                c.LeaveAndDespawn();

            while (queueManager.GetCurrentQueue().Count > 0)
                yield return null;
        }

        Debug.Log("【ModeToggleManager】所有顧客已離開，切換為非營業模式");

        isBusinessMode = false;
        UpdateMode(); // ✅ 延遲模式切換，直到顧客全走
        isClosing = false;
    }

    private void Start()
    {
        UpdateMode(); // 初始化
    }
}
