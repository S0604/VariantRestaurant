using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FreeModeToggleManager : MonoBehaviour
{
    public static FreeModeToggleManager Instance;

    [Header("營業設定")]
    public float businessDuration = 180f;
    public float closingBufferTime = 10f;

    [Header("時間與 UI")]
    public TimeSystem timeSystem;

    [Header("模式 UI 與腳本組")]
    public GameObject[] businessModeUIs;
    public MonoBehaviour[] businessModeScripts;
    public GameObject[] closedModeUIs;
    public MonoBehaviour[] closedModeScripts;

    [Header("轉場設定")]
    public Image transitionImage;
    public Transform transitionImageTransform;
    public float transitionDuration = 1.5f;

    [Header("結算 UI")]
    public GameObject resultUI;
    public Button resultConfirmButton;
    public GameResultUI gameResultUI;

    [Header("音樂管理")]
    public AudioSource businessMusicSource;
    public AudioSource closedMusicSource;

    [Header("控制時間流逝")]
    [Tooltip("在教學對話結束前禁止時間流逝")]
    [SerializeField] private bool allowTimeFlow = false;
    public bool AllowTimeFlow { get => allowTimeFlow; set => allowTimeFlow = value; }

    private bool isBusinessMode = false;
    public bool IsBusinessMode => isBusinessMode;

    private bool isClosingPhase = false;
    public bool IsClosingPhase => isClosingPhase;

    private float remainingTime;
    public float RemainingBusinessTime => remainingTime;

    // 登記現場顧客
    private HashSet<Customer> aliveCustomers = new HashSet<Customer>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        EnterClosedMode();
    }

    private void Update()
    {
        if (!isBusinessMode || !allowTimeFlow) return;

        remainingTime -= Time.deltaTime;
        timeSystem?.UpdateTimeVisual(Mathf.Clamp01(remainingTime / businessDuration));

        if (!isClosingPhase && remainingTime <= closingBufferTime)
        {
            StartCoroutine(HandleClosingPhase());
        }

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            ForceRemoveAllCustomers();
        }
    }

    #region 營業模式切換

    public void ToggleMode()
    {
        if (isBusinessMode) return;
        StartCoroutine(PlayTransition(EnterBusinessMode));
    }

    private void EnterBusinessMode()
    {
        isBusinessMode = true;
        isClosingPhase = false;
        remainingTime = businessDuration;

        timeSystem?.ResetTimeVisual();

        SetActiveGroup(businessModeUIs, businessModeScripts, true);
        SetActiveGroup(closedModeUIs, closedModeScripts, false);

        businessMusicSource?.Play();
        closedMusicSource?.Stop();

        Debug.Log("✅ 進入自由營業模式");

        // 轉場後延遲播放 Chapter3
        StartCoroutine(PlayChapterAfterDelay("3", 1.6f));
    }

    private void EnterClosedMode()
    {
        isBusinessMode = false;
        isClosingPhase = false;

        timeSystem?.ResetTimeVisual();

        SetActiveGroup(businessModeUIs, businessModeScripts, false);
        SetActiveGroup(closedModeUIs, closedModeScripts, true);

        ClearAllInventories();

        businessMusicSource?.Stop();
        closedMusicSource?.Play();

        Debug.Log("🛑 進入歇業模式");
    }

    #endregion

    #region 顧客管理

    public void RegisterCustomer(Customer customer)
    {
        aliveCustomers.Add(customer);
    }

    public void UnregisterCustomer(Customer customer)
    {
        aliveCustomers.Remove(customer);
    }

    private void ForceRemoveAllCustomers()
    {
        foreach (var c in new List<Customer>(aliveCustomers))
        {
            if (c != null) c.ForceLeaveAndDespawn();
        }
    }

    #endregion

    #region 關店流程

    private IEnumerator HandleClosingPhase()
    {
        isClosingPhase = true;
        Debug.Log("🔔 營業即將結束，開始關店準備");

        // 強制顧客離開隊伍
        ForceRemoveAllCustomers();

        // 等待顧客完全消失
        while (aliveCustomers.Count > 0)
        {
            yield return null;
        }

        Debug.Log("✅ 顧客離場完畢，播放結算轉場");

        // 播放轉場動畫並顯示結算 UI
        yield return PlayTransitionFillOnly(() =>
        {
            resultUI.SetActive(true);
            gameResultUI?.Show();
        });

        // 按鈕事件：按下後還原轉場，關閉結算 UI，回歇業模式
        resultConfirmButton.onClick.RemoveAllListeners();
        resultConfirmButton.onClick.AddListener(() =>
        {
            StartCoroutine(PlayTransitionResetOnly(() =>
            {
                resultUI.SetActive(false);
                EnterClosedMode();
            }));
        });
    }

    #endregion

    #region 背包清空

    private void ClearAllInventories()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.ClearInventory();
    }

    #endregion

    #region 轉場動畫

    private void SetActiveGroup(GameObject[] uiGroup, MonoBehaviour[] scriptGroup, bool isActive)
    {
        foreach (var go in uiGroup) if (go != null) go.SetActive(isActive);
        foreach (var script in scriptGroup) if (script != null) script.enabled = isActive;
    }

    private IEnumerator PlayTransition(Action onSwitch)
    {
        float t = 0f;

        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            float progress = t / transitionDuration;
            if (transitionImage != null) transitionImage.fillAmount = Mathf.Lerp(0f, 1f, progress);
            if (transitionImageTransform != null)
                transitionImageTransform.localScale = new Vector3(1f, Mathf.Lerp(1f, 1.4f, progress), 1f);
            yield return null;
        }

        onSwitch?.Invoke();

        t = 0f;
        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            float progress = t / transitionDuration;
            if (transitionImage != null) transitionImage.fillAmount = Mathf.Lerp(1f, 0f, progress);
            if (transitionImageTransform != null)
                transitionImageTransform.localScale = new Vector3(1f, Mathf.Lerp(1.4f, 1f, progress), 1f);
            yield return null;
        }
    }

    private IEnumerator PlayTransitionFillOnly(Action onFilled)
    {
        float t = 0f;
        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            float progress = t / transitionDuration;
            if (transitionImage != null) transitionImage.fillAmount = Mathf.Lerp(0f, 1f, progress);
            if (transitionImageTransform != null)
                transitionImageTransform.localScale = new Vector3(1f, Mathf.Lerp(1f, 1.4f, progress), 1f);
            yield return null;
        }
        onFilled?.Invoke();
    }

    private IEnumerator PlayTransitionResetOnly(Action onComplete = null)
    {
        float t = 0f;
        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            float progress = t / transitionDuration;
            if (transitionImage != null) transitionImage.fillAmount = Mathf.Lerp(1f, 0f, progress);
            if (transitionImageTransform != null)
                transitionImageTransform.localScale = new Vector3(1f, Mathf.Lerp(1.4f, 1f, progress), 1f);
            yield return null;
        }
        onComplete?.Invoke();
    }

    #endregion

    #region 教學對話

    private IEnumerator PlayChapterAfterDelay(string chapterID, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (TutorialDialogueController.Instance != null)
            yield return TutorialDialogueController.Instance.PlaySingleChapter(chapterID);

        // 對話13結束，自由營業開始
        if (chapterID == "13")
        {
            AllowTimeFlow = true;
            Debug.Log("對話13結束，自由營業開始計時");
        }
    }

    #endregion
}
