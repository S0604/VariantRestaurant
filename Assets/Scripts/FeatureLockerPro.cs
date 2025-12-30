using UnityEngine;
using UnityEngine.UI;

public enum LockMode
{
    SetActive,
    ButtonInteractable,
    MonoBehaviourToggle
}

public class FeatureLockerPro : MonoBehaviour
{
    [SerializeField] private string requiredEventID;
    [SerializeField] private LockMode lockMode = LockMode.SetActive;
    [SerializeField] private bool startLocked = true;

    [Header("可选拖入")]
    [SerializeField] private Button targetButton;
    [SerializeField] private MonoBehaviour targetScript;

    private bool unlocked = false;

    /* ---------- 生命周期 ---------- */
    private void Awake()
    {
        // 自动找组件
        if (lockMode == LockMode.ButtonInteractable && targetButton == null)
            targetButton = GetComponent<Button>();
        if (lockMode == LockMode.MonoBehaviourToggle && targetScript == null)
            targetScript = GetComponent<MonoBehaviour>();

        // 不直接锁，等下一帧确保单例已建立
        if (startLocked)
            Invoke(nameof(FirstLock), 0f);
    }

    private void FirstLock() => Lock(true);

    private void OnEnable()
    {
        // 再延后一帧注册事件，避免单例还没初始化
        if (!unlocked)
            Invoke(nameof(SafeSubscribe), 0f);
    }

    private void SafeSubscribe()
    {
        if (TutorialProgressManager.Instance != null)
            TutorialProgressManager.Instance.GetEvent(requiredEventID).onEventCompleted.AddListener(Unlock);
        else
            Debug.LogWarning("[FeatureLockerPro] 找不到 TutorialProgressManager，请确认场景已放置。", this);
    }

    private void OnDisable()
    {
        // 播放結束時 Manager 可能已經被銷毀，直接返回
        if (TutorialProgressManager.Instance == null) return;

        var ev = TutorialProgressManager.Instance.GetEvent(requiredEventID);
        if (ev != null)
            ev.onEventCompleted.RemoveListener(Unlock);
    }
    /* ---------- 核心 ---------- */
    private void Unlock()
    {
        if (unlocked) return;
        unlocked = true;
        Lock(false);
        enabled = false;
    }

    private void Lock(bool isLock)
    {
        switch (lockMode)
        {
            case LockMode.SetActive:
                gameObject.SetActive(!isLock);
                break;
            case LockMode.ButtonInteractable:
                if (targetButton) targetButton.interactable = !isLock;
                break;
            case LockMode.MonoBehaviourToggle:
                if (targetScript) targetScript.enabled = !isLock;
                break;
        }
    }
}