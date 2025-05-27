using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModeToggleManager : MonoBehaviour
{
    public static ModeToggleManager Instance;

    [Header("營業設定")]
    public float businessDuration = 180f;
    public float closingBufferTime = 10f;

    [Header("時間與 UI")]
    public TimeSystem timeSystem;

    [Header("需要在營業模式啟用的元件")]
    public GameObject[] businessModeUIs;
    public MonoBehaviour[] businessModeScripts;

    [Header("需要在歇業模式啟用的元件")]
    public GameObject[] closedModeUIs;
    public MonoBehaviour[] closedModeScripts;

    private float remainingTime;
    private bool isBusinessMode = false;
    private bool isClosingPhase = false;
    private HashSet<Customer> aliveCustomers = new HashSet<Customer>();

    public float RemainingBusinessTime => remainingTime;
    public bool IsClosingPhase => isClosingPhase;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        EnterClosedMode(); // 初始為歇業
    }

    private void Update()
    {
        if (!isBusinessMode) return;

        remainingTime -= Time.deltaTime;
        timeSystem.UpdateTimeVisual(Mathf.Clamp01(remainingTime / businessDuration));

        if (!isClosingPhase && remainingTime <= closingBufferTime)
        {
            StartCoroutine(HandleClosingPhase());
        }

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
        }
    }

    public void ToggleMode()
    {
        if (isBusinessMode)
            return; // 避免強制中途切換

        EnterBusinessMode();
    }

    private void EnterBusinessMode()
    {
        isBusinessMode = true;
        isClosingPhase = false;
        remainingTime = businessDuration;

        timeSystem.ResetTimeVisual();

        SetActiveGroup(businessModeUIs, businessModeScripts, true);
        SetActiveGroup(closedModeUIs, closedModeScripts, false);

        Debug.Log("進入營業模式");
    }

    private void EnterClosedMode()
    {
        isBusinessMode = false;
        isClosingPhase = false;

        timeSystem.ResetTimeVisual();

        SetActiveGroup(businessModeUIs, businessModeScripts, false);
        SetActiveGroup(closedModeUIs, closedModeScripts, true);

        // 新增：清空所有 Inventory
        ClearAllInventories();

        Debug.Log("進入歇業模式");
    }

    private void ClearAllInventories()
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.ClearInventory(); // 使用新版方法
        }
    }


    private IEnumerator HandleClosingPhase()
    {
        isClosingPhase = true;
        Debug.Log("營業即將結束，開始關店準備");

        while (aliveCustomers.Count > 0)
        {
            Debug.Log($"等待顧客離場中，剩餘：{aliveCustomers.Count}");
            yield return new WaitForSeconds(1f);
        }

        Debug.Log("所有顧客已離場，切換至歇業模式");
        EnterClosedMode();
    }

    public void RegisterCustomer(Customer customer)
    {
        aliveCustomers.Add(customer);
    }

    public void UnregisterCustomer(Customer customer)
    {
        aliveCustomers.Remove(customer);
    }

    private void SetActiveGroup(GameObject[] uiGroup, MonoBehaviour[] scriptGroup, bool isActive)
    {
        foreach (GameObject go in uiGroup)
        {
            if (go != null) go.SetActive(isActive);
        }

        foreach (MonoBehaviour script in scriptGroup)
        {
            if (script != null) script.enabled = isActive;
        }
    }


}
