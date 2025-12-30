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
        if (lockMode == LockMode.ButtonInteractable && targetButton == null)
            targetButton = GetComponent<Button>();

        if (startLocked)
            Lock(true);
    }

    private void OnEnable()
    {
        if (!unlocked)
            SafeSubscribe();
    }

    private void OnDisable()
    {
        if (TutorialProgressManager.Instance == null) return;

        var ev = TutorialProgressManager.Instance.GetEvent(requiredEventID);
        if (ev != null)
            ev.onEventCompleted.RemoveListener(Unlock);
    }

    /* ---------- 核心 ---------- */

    private void SafeSubscribe()
    {
        if (string.IsNullOrEmpty(requiredEventID))
        {
            Debug.LogError("[FeatureLockerPro] requiredEventID 為空", this);
            return;
        }

        var manager = TutorialProgressManager.Instance;
        if (manager == null)
        {
            Debug.LogWarning("[FeatureLockerPro] 找不到 TutorialProgressManager", this);
            return;
        }

        var ev = manager.GetEvent(requiredEventID);

        if (ev.isCompleted)
        {
            Unlock();
            return;
        }

        ev.onEventCompleted.AddListener(Unlock);
    }

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
                if (targetButton)
                    targetButton.interactable = !isLock;
                break;

            case LockMode.MonoBehaviourToggle:
                if (targetScript)
                    targetScript.enabled = !isLock;
                break;
        }
    }
}
