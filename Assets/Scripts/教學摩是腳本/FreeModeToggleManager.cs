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
    public float transitionDuration =10f;

    [Header("UI 控制")]
    [SerializeField] private Button businessButton;

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

    [Header("變異事件設定")]
    [SerializeField] private float randomEventDelay = 20f;
    [SerializeField, Range(0f, 1f)] private float randomEventChance = 0.5f;

    private bool hasPlayedChapter3 = false;
    private bool hasPlayedChapter13 = false;

    private Coroutine randomEventCoroutine;
    private bool hasTriggeredRandomEvent = false;

    private bool isBusinessMode = false;
    public bool IsBusinessMode => isBusinessMode;

    private bool isClosingPhase = false;
    public bool IsClosingPhase => isClosingPhase;

    private float remainingTime;
    public float RemainingBusinessTime => remainingTime;

    private bool isTransitioning = false;

    // 登記現場顧客
    private HashSet<Customer> aliveCustomers = new HashSet<Customer>();

    private int leavingCustomerCount = 0;

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
        if (isTransitioning) return;

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

        Debug.Log("Enter Free Business Mode");

        // 轉場後延遲播放 Chapter3
        StartCoroutine(PlayChapterAfterDelay("3", 1.6f));

        // Random Event: 每次進入營業模式只排程一次
        hasTriggeredRandomEvent = false;
        if (randomEventCoroutine != null)
        {
            StopCoroutine(randomEventCoroutine);
            randomEventCoroutine = null;
        }
        randomEventCoroutine = StartCoroutine(TriggerRandomEventAfterDelay(randomEventDelay));
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

        // 切回歇業時停止事件排程，避免歇業後還觸發
        if (randomEventCoroutine != null)
        {
            StopCoroutine(randomEventCoroutine);
            randomEventCoroutine = null;
        }
        hasTriggeredRandomEvent = false;

        Debug.Log("Enter Closed Mode");
    }

    #endregion

    #region Random Event

    private IEnumerator TriggerRandomEventAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 確保還在營業模式，且只觸發一次
        if (!isBusinessMode || hasTriggeredRandomEvent) yield break;

        hasTriggeredRandomEvent = true;

        if (RandomEventManager.Instance != null && UnityEngine.Random.value < randomEventChance)
        {
            RandomEventManager.Instance.StartEvent();
        }
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

        Debug.Log("Business ending soon. Start closing phase.");

        // 設定離場數量
        leavingCustomerCount = aliveCustomers.Count;

        // 讓所有顧客開始離場
        foreach (var c in new List<Customer>(aliveCustomers))
        {
            if (c != null)
                c.LeaveImmediately();
        }

        // 等待全部離開
        while (leavingCustomerCount > 0)
        {
            yield return null;
        }

        Debug.Log("All customers left. Show result UI.");

        // 轉場 + 結算
        yield return PlayTransitionFillOnly(() =>
        {
            resultUI.SetActive(true);
            gameResultUI?.Show();
        });

        resultConfirmButton.onClick.RemoveAllListeners();
        resultConfirmButton.onClick.AddListener(() =>
        {
            // ✅ 1. 先切換內容（UI / 模式）
            resultUI.SetActive(false);
            EnterClosedMode();

            // ✅ 2. 再播放遮罩打開動畫
            StartCoroutine(PlayTransitionResetOnly());
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
        isTransitioning = true;

        // ❌ 禁用按鈕
        if (businessButton != null)
            businessButton.interactable = false;

        float t = 0f;

        // 遮罩蓋住
        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            float progress = t / transitionDuration;

            if (transitionImage != null)
                transitionImage.fillAmount = Mathf.Lerp(0f, 1f, progress);

            yield return null;
        }

        if (transitionImage != null)
            transitionImage.fillAmount = 1f;

        // 切換內容
        onSwitch?.Invoke();

        // 解除遮罩
        t = 0f;
        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            float progress = t / transitionDuration;

            if (transitionImage != null)
                transitionImage.fillAmount = Mathf.Lerp(1f, 0f, progress);

            yield return null;
        }

        if (transitionImage != null)
            transitionImage.fillAmount = 0f;

        // ✅ 恢復按鈕
        if (businessButton != null)
            businessButton.interactable = true;

        isTransitioning = false;
    }
    private IEnumerator PlayTransitionFillOnly(Action onFilled)
    {
        float t = 0f;

        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            float progress = t / transitionDuration;

            if (transitionImage != null)
                transitionImage.fillAmount = Mathf.Lerp(0f, 1f, progress);

            yield return null;
        }

        if (transitionImage != null)
            transitionImage.fillAmount = 1f;

        onFilled?.Invoke();
    }
    private IEnumerator PlayTransitionResetOnly(Action onComplete = null)
    {
        float t = 0f;

        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            float progress = t / transitionDuration;

            if (transitionImage != null)
                transitionImage.fillAmount = Mathf.Lerp(1f, 0f, progress);

            yield return null;
        }

        if (transitionImage != null)
            transitionImage.fillAmount = 0f;

        onComplete?.Invoke();
    }
    #endregion

    #region 教學對話

    public IEnumerator PlayChapterAfterDelay(string chapterID, float delay)
    {
        // 防止同一場重複播放
        if (chapterID == "3" && hasPlayedChapter3) yield break;
        if (chapterID == "13" && hasPlayedChapter13) yield break;

        yield return new WaitForSeconds(delay);

        if (TutorialDialogueController.Instance != null)
            yield return TutorialDialogueController.Instance.PlaySingleChapter(chapterID);

        // 標記已播放
        if (chapterID == "3") hasPlayedChapter3 = true;
        if (chapterID == "13") hasPlayedChapter13 = true;

        // Chapter 13 結束後開放時間
        if (chapterID == "13")
        {
            AllowTimeFlow = true;
            Debug.Log("Chapter 13 finished. Free business timer starts.");
        }
    }
    #endregion

    #region 顧客離場回報

    public void OnCustomerDespawn(Customer customer)
    {
        if (aliveCustomers.Contains(customer))
            aliveCustomers.Remove(customer);

        leavingCustomerCount--;

        if (leavingCustomerCount < 0)
            leavingCustomerCount = 0;
    }
    #endregion

}
