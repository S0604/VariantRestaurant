using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FreeModeToggleManager : MonoBehaviour
{
    public static FreeModeToggleManager Instance;

    [Header("模式 UI 與腳本組")]
    public GameObject[] businessModeUIs;
    public MonoBehaviour[] businessModeScripts;

    public GameObject[] closedModeUIs;
    public MonoBehaviour[] closedModeScripts;

    [Header("轉場設定")]
    public Image transitionImage;
    public Transform transitionImageTransform;
    public float transitionDuration = 1.5f;

    [Header("音樂管理")]
    public AudioSource businessMusicSource;
    public AudioSource closedMusicSource;

    private bool isBusinessMode = false;
    public bool IsBusinessMode => isBusinessMode; // ✅ 讓外部能判斷模式狀態

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        EnterClosedMode();
    }

    /// <summary>
    /// 切換營業 / 歇業 模式
    /// </summary>
    public void ToggleMode()
    {
        if (isBusinessMode)
            StartCoroutine(PlayTransition(EnterClosedMode));
        else
            StartCoroutine(PlayTransition(EnterBusinessMode));
    }

    /// <summary>
    /// 進入營業模式
    /// </summary>
    private void EnterBusinessMode()
    {
        isBusinessMode = true;

        SetActiveGroup(businessModeUIs, businessModeScripts, true);
        SetActiveGroup(closedModeUIs, closedModeScripts, false);

        if (businessMusicSource != null) businessMusicSource.Play();
        if (closedMusicSource != null) closedMusicSource.Stop();

        Debug.Log("✅ 進入自由營業模式");

        // 🔹 轉場結束後再延遲 1.6 秒播 Chapter3
        Invoke(nameof(PlayChapter3), 1f);
    }

    private void PlayChapter3()
    {
        if (TutorialDialogueController.Instance != null)
            TutorialDialogueController.Instance.PlayChapter("3");
    }
    /// <summary>
    /// 進入歇業模式
    /// </summary>
    private void EnterClosedMode()
    {
        isBusinessMode = false;

        SetActiveGroup(businessModeUIs, businessModeScripts, false);
        SetActiveGroup(closedModeUIs, closedModeScripts, true);

        if (closedMusicSource != null) closedMusicSource.Play();
        if (businessMusicSource != null) businessMusicSource.Stop();

        // 清空庫存（如果有 InventoryManager）
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.ClearInventory();

        Debug.Log("🛑 進入歇業模式");
    }

    private void SetActiveGroup(GameObject[] uiGroup, MonoBehaviour[] scriptGroup, bool isActive)
    {
        foreach (var go in uiGroup)
            if (go != null) go.SetActive(isActive);

        foreach (var script in scriptGroup)
            if (script != null) script.enabled = isActive;
    }

    /// <summary>
    /// 模式轉場動畫
    /// </summary>
    private IEnumerator PlayTransition(System.Action onSwitch)
    {
        float t = 0f;

        // Fill 動畫
        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            float progress = t / transitionDuration;
            transitionImage.fillAmount = Mathf.Lerp(0f, 1f, progress);
            transitionImageTransform.localScale = new Vector3(1f, Mathf.Lerp(1f, 1.4f, progress), 1f);
            yield return null;
        }

        onSwitch?.Invoke();

        // 還原動畫
        t = 0f;
        while (t < transitionDuration)
        {
            t += Time.deltaTime;
            float progress = t / transitionDuration;
            transitionImage.fillAmount = Mathf.Lerp(1f, 0f, progress);
            transitionImageTransform.localScale = new Vector3(1f, Mathf.Lerp(1.4f, 1f, progress), 1f);
            yield return null;
        }
    }
}
