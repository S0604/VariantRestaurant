using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class FreeModeSaveData
{
    public bool isBusinessMode;
    public bool isClosingPhase;
    public float remainingTime;
    public bool allowTimeFlow;
    public bool hasPlayedChapter3;
    public bool hasPlayedChapter13;
    public bool hasTriggeredRandomEvent;
}

public class FreeModeToggleManager : MonoBehaviour, ISaveable
{
    public static FreeModeToggleManager Instance;

    [Header("Save")]
    [SerializeField] private string uniqueID = "FreeModeToggleManager";

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
    public float transitionDuration = 10f;

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

    private HashSet<Customer> aliveCustomers = new HashSet<Customer>();
    private int leavingCustomerCount = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        EnterClosedMode();
    }

    private void Update()
    {
        if (!isBusinessMode || !allowTimeFlow) return;

        remainingTime -= Time.deltaTime;
        remainingTime = Mathf.Max(0f, remainingTime);

        if (timeSystem != null && businessDuration > 0f)
        {
            timeSystem.UpdateTimeVisual(Mathf.Clamp01(remainingTime / businessDuration));
        }

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

        StartCoroutine(PlayChapterAfterDelay("3", 1.6f));

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
        remainingTime = 0f;

        timeSystem?.ResetTimeVisual();

        SetActiveGroup(businessModeUIs, businessModeScripts, false);
        SetActiveGroup(closedModeUIs, closedModeScripts, true);

        ClearAllInventories();

        businessMusicSource?.Stop();
        closedMusicSource?.Play();

        if (randomEventCoroutine != null)
        {
            StopCoroutine(randomEventCoroutine);
            randomEventCoroutine = null;
        }

        hasTriggeredRandomEvent = false;

        Debug.Log("Enter Closed Mode");
    }

    private IEnumerator TriggerRandomEventAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (!isBusinessMode || hasTriggeredRandomEvent) yield break;

        hasTriggeredRandomEvent = true;

        if (RandomEventManager.Instance != null && UnityEngine.Random.value < randomEventChance)
        {
            RandomEventManager.Instance.StartEvent();
        }
    }

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

    private IEnumerator HandleClosingPhase()
    {
        isClosingPhase = true;

        Debug.Log("Business ending soon. Start closing phase.");

        leavingCustomerCount = aliveCustomers.Count;

        foreach (var c in new List<Customer>(aliveCustomers))
        {
            if (c != null)
                c.LeaveImmediately();
        }

        while (leavingCustomerCount > 0)
        {
            yield return null;
        }

        Debug.Log("All customers left. Show result UI.");

        yield return PlayTransitionFillOnly(() =>
        {
            if (resultUI != null)
                resultUI.SetActive(true);

            gameResultUI?.Show();
        });

        if (resultConfirmButton != null)
        {
            resultConfirmButton.onClick.RemoveAllListeners();
            resultConfirmButton.onClick.AddListener(() =>
            {
                if (resultUI != null)
                    resultUI.SetActive(false);

                EnterClosedMode();
                StartCoroutine(PlayTransitionResetOnly());
            });
        }
    }

    private void ClearAllInventories()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.ClearInventory();
    }

    private void SetActiveGroup(GameObject[] uiGroup, MonoBehaviour[] scriptGroup, bool isActive)
    {
        foreach (var go in uiGroup)
        {
            if (go != null) go.SetActive(isActive);
        }

        foreach (var script in scriptGroup)
        {
            if (script != null) script.enabled = isActive;
        }
    }

    private IEnumerator PlayTransition(Action onSwitch)
    {
        isTransitioning = true;

        if (businessButton != null)
            businessButton.interactable = false;

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

        onSwitch?.Invoke();

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

    public IEnumerator PlayChapterAfterDelay(string chapterID, float delay)
    {
        if (chapterID == "3" && hasPlayedChapter3) yield break;
        if (chapterID == "13" && hasPlayedChapter13) yield break;

        yield return new WaitForSeconds(delay);

        if (TutorialDialogueController.Instance != null)
            yield return TutorialDialogueController.Instance.PlaySingleChapter(chapterID);

        if (chapterID == "3") hasPlayedChapter3 = true;
        if (chapterID == "13") hasPlayedChapter13 = true;

        if (chapterID == "13")
        {
            AllowTimeFlow = true;
            Debug.Log("Chapter 13 finished. Free business timer starts.");
        }
    }

    public void OnCustomerDespawn(Customer customer)
    {
        if (aliveCustomers.Contains(customer))
            aliveCustomers.Remove(customer);

        leavingCustomerCount--;

        if (leavingCustomerCount < 0)
            leavingCustomerCount = 0;
    }

    public string GetUniqueID()
    {
        return uniqueID;
    }

    public string CaptureAsJson()
    {
        FreeModeSaveData data = new FreeModeSaveData
        {
            isBusinessMode = isBusinessMode,
            isClosingPhase = isClosingPhase,
            remainingTime = remainingTime,
            allowTimeFlow = allowTimeFlow,
            hasPlayedChapter3 = hasPlayedChapter3,
            hasPlayedChapter13 = hasPlayedChapter13,
            hasTriggeredRandomEvent = hasTriggeredRandomEvent
        };

        return JsonUtility.ToJson(data);
    }

    public void RestoreFromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
            return;

        FreeModeSaveData data = JsonUtility.FromJson<FreeModeSaveData>(json);
        if (data == null)
            return;

        if (randomEventCoroutine != null)
        {
            StopCoroutine(randomEventCoroutine);
            randomEventCoroutine = null;
        }

        allowTimeFlow = data.allowTimeFlow;
        hasPlayedChapter3 = data.hasPlayedChapter3;
        hasPlayedChapter13 = data.hasPlayedChapter13;
        hasTriggeredRandomEvent = data.hasTriggeredRandomEvent;
        isClosingPhase = data.isClosingPhase;
        remainingTime = Mathf.Clamp(data.remainingTime, 0f, businessDuration);

        ApplyModeStateFromSave(data.isBusinessMode);

        Debug.Log("[Save] FreeModeToggleManager 已還原");
    }

    private void ApplyModeStateFromSave(bool targetBusinessMode)
    {
        isBusinessMode = targetBusinessMode;

        if (isBusinessMode)
        {
            SetActiveGroup(businessModeUIs, businessModeScripts, true);
            SetActiveGroup(closedModeUIs, closedModeScripts, false);

            businessMusicSource?.Play();
            closedMusicSource?.Stop();

            if (timeSystem != null && businessDuration > 0f)
            {
                timeSystem.UpdateTimeVisual(Mathf.Clamp01(remainingTime / businessDuration));
            }

            if (!hasTriggeredRandomEvent)
            {
                randomEventCoroutine = StartCoroutine(TriggerRandomEventAfterDelay(randomEventDelay));
            }
        }
        else
        {
            SetActiveGroup(businessModeUIs, businessModeScripts, false);
            SetActiveGroup(closedModeUIs, closedModeScripts, true);

            businessMusicSource?.Stop();
            closedMusicSource?.Play();

            timeSystem?.ResetTimeVisual();
        }

        if (resultUI != null)
            resultUI.SetActive(false);

        if (businessButton != null)
            businessButton.interactable = !isTransitioning;
    }
}