using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class ModeToggleManager : MonoBehaviour
{
    public static ModeToggleManager Instance;

    [Header("營業設定")]
    public float businessDuration = 180f;
    public float closingBufferTime = 10f;

    [Header("時間與 UI")]
    public TimeSystem timeSystem;

    [Header("顾客队列设置")]
    public int maxQueueLength = 4; // 最大允许排队人数
    private List<Customer> customerQueue = new List<Customer>();

    [Header("需要在營業模式啟用的元件")]
    public GameObject[] businessModeUIs;
    public MonoBehaviour[] businessModeScripts;

    [Header("需要在歇業模式啟用的元件")]
    public GameObject[] closedModeUIs;
    public MonoBehaviour[] closedModeScripts;

    [Header("轉場設定")]
    public Image transitionImage;
    public Transform transitionImage1;
    public float transitionDuration = 1.5f;

    [Header("結算 UI")]
    public GameObject resultUI;
    public Button resultConfirmButton;
    public GameResultUI gameResultUI;  // 這是新加的，Inspector 請連結

    [Header("音樂管理")]
    public AudioSource businessMusicSource;
    public AudioSource closedMusicSource;

    private float remainingTime;
    private bool isBusinessMode = false;
    private bool isClosingPhase = false;
    private bool hasTriggered15FLogic = false;
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
        EnterClosedMode();
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

        // 當只剩15 frame (假設每秒60幀)
        if (!hasTriggered15FLogic && remainingTime <= (20f / 60f))
        {
            hasTriggered15FLogic = true;
            CustomerQueueManager.Instance?.ForceRemoveCustomersAt15F();
        }

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            CustomerQueueManager.Instance?.ForceRemoveAllCustomers();
        }
    }

    public HashSet<Customer> GetAliveCustomers()
    {
        return new HashSet<Customer>(aliveCustomers);
    }

    public void ToggleMode()
    {
        if (isBusinessMode)
            return; // 避免強制中途切換

        StartCoroutine(PlayTransition(EnterBusinessMode));
    }

    private void EnterBusinessMode()
    {
        isBusinessMode = true;
        isClosingPhase = false;
        remainingTime = businessDuration;

        timeSystem.ResetTimeVisual();

        SetActiveGroup(businessModeUIs, businessModeScripts, true);
        SetActiveGroup(closedModeUIs, closedModeScripts, false);
        if (businessMusicSource != null) businessMusicSource.Play();
        if (closedMusicSource != null) closedMusicSource.Stop();

        StartCoroutine(TriggerRandomEventAfterDelay(2f));//測試用2F 正式為20F

        Debug.Log("✅ 進入營業模式");
    }

    private IEnumerator TriggerRandomEventAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 目前為測試用途，100% 觸發
        if (RandomEventManager.Instance != null)
        {
            RandomEventManager.Instance.StartEvent();
        }
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

        if (closedMusicSource != null) closedMusicSource.Play();
        if (businessMusicSource != null) businessMusicSource.Stop();

        Debug.Log("🛑 進入歇業模式");
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
        Debug.Log("🔔 營業即將結束，開始關店準備");

        // 等待所有顧客離場
        while (aliveCustomers.Count > 0)
        {
            yield return new WaitForSeconds(1f);
        }

        Debug.Log("✅ 顧客離場，播放轉場後顯示結算畫面");

        // 播放只填滿放大的轉場動畫，完成後顯示結算UI並更新顯示內容
        yield return PlayTransitionFillOnly(() =>
        {
            resultUI.SetActive(true); // 顯示結算UI
            if (gameResultUI != null)
            {
                gameResultUI.Show();   // 呼叫 Show() 更新結算數值
            }
        });

        // 按鈕事件：按下後播放還原轉場動畫，關閉結算UI並切回歇業模式
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
    private IEnumerator PlayTransition(System.Action onMidpoint, System.Action onComplete = null)
    {
        float t = 0f;
        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            float progress = t / transitionDuration;
            transitionImage.fillAmount = Mathf.Lerp(0f, 1f, progress);
            transitionImage1.localScale = new Vector3(1f, Mathf.Lerp(1f, 1.4f, progress), 1f);
            yield return null;
        }

        onMidpoint?.Invoke(); // 模式切換點

        t = 0f;
        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            float progress = t / transitionDuration;
            transitionImage.fillAmount = Mathf.Lerp(1f, 0f, progress);
            transitionImage1.localScale = new Vector3(1f, Mathf.Lerp(1.4f, 1f, progress), 1f);
            yield return null;
        }

        onComplete?.Invoke(); // 完成
    }
    private IEnumerator PlayTransitionFillOnly(System.Action onFilled)
    {
        float t = 0f;
        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            float progress = t / transitionDuration;
            transitionImage.fillAmount = Mathf.Lerp(0f, 1f, progress);
            transitionImage1.localScale = new Vector3(1f, Mathf.Lerp(1f, 1.4f, progress), 1f);
            yield return null;
        }
        onFilled?.Invoke();
    }

    private IEnumerator PlayTransitionResetOnly(System.Action onComplete = null)
    {
        float t = 0f;
        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            float progress = t / transitionDuration;
            transitionImage.fillAmount = Mathf.Lerp(1f, 0f, progress);
            transitionImage1.localScale = new Vector3(1f, Mathf.Lerp(1.4f, 1f, progress), 1f);
            yield return null;
        }
        onComplete?.Invoke();
    }


}