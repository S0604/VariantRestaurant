using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardClickEffectUI : MonoBehaviour, IPointerClickHandler
{
    private RectTransform rectTransform;

    [Header("動畫設定")]
    public Vector2 centerAnchoredPos = new Vector2(0, 0);
    public float zoomScale = 1.5f;
    public float moveDuration = 0.3f;
    public float stayDuration = 1.5f;

    [Header("等待狀態設定")]
    public float waitingHeight = 100f;

    [Header("冷卻設定")]
    public float cooldownTime = 5f;            // 每張卡的冷卻時間
    public Image cooldownMask;                 // UI 遮罩 (Image type = Filled, Fill Method = Vertical)

    private Vector2 originalPos;
    private Vector3 originalScale;
    private Quaternion originalRotation;

    private bool isCoolingDown = false;
    public bool IsLocked { get; private set; } = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        originalPos = rectTransform.anchoredPosition;
        originalScale = rectTransform.localScale;
        originalRotation = rectTransform.localRotation;

        if (cooldownMask != null)
        {
            cooldownMask.fillAmount = 0f;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 如果已經被鎖住或在冷卻 → 直接忽略
        if (IsLocked || isCoolingDown) return;

        //  一旦點擊就立刻鎖住，避免重複進 queue
        IsLocked = true;

        CardClickManager.Instance.EnqueueCard(this);
    }

    // 進場 + 停留（這部分會阻塞下一張）
    public IEnumerator PlayEnterAndStay()
    {
        IsLocked = true;

        // 進場動畫
        yield return StartCoroutine(MoveToUIPosition(
            centerAnchoredPos,
            Vector3.one * zoomScale,
            Quaternion.identity,
            moveDuration));

        // 👉 觸發主動技
        var card = GetComponent<CardHoverEffect>();
        if (card != null && card.activeSkill != null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            card.activeSkill.Activate(player);
        }

        // 🗨️ 播放教學對話（這裡改成你想播的章節名）
        TutorialDialogueController tutorial = FindObjectOfType<TutorialDialogueController>();
        if (tutorial != null)
        {
            //
            TutorialDialogueController.Instance.PlayChapter("12");
            Debug.Log("🎬 已呼叫播放 12 對話");
        }
        else
        {
            Debug.LogWarning("❌ 找不到 TutorialDialogueController，無法播放對話。");
        }

        // 停留一段時間
        yield return new WaitForSeconds(stayDuration);
    }

    // 離場（獨立進行，不阻塞下一張）
    public IEnumerator PlayExit()
    {
        // 移動回原位
        yield return StartCoroutine(MoveToUIPosition(
            originalPos,
            originalScale,
            originalRotation,
            moveDuration));

        // 強制重置 hover
        var hoverEffect = GetComponent<CardHoverEffect>();
        if (hoverEffect != null)
            hoverEffect.ForceExit();

        //  確保完全回位後 → 進入冷卻
        rectTransform.anchoredPosition = originalPos;
        rectTransform.localScale = originalScale;
        rectTransform.localRotation = originalRotation;

        // 啟動冷卻
        StartCoroutine(StartCooldown());
    }

    private IEnumerator StartCooldown()
    {
        isCoolingDown = true;
        IsLocked = false; // 解鎖，但仍然因為冷卻無法被點擊

        if (cooldownMask != null)
        {
            cooldownMask.fillAmount = 1f;
        }

        float elapsed = 0f;
        while (elapsed < cooldownTime)
        {
            elapsed += Time.deltaTime;
            float ratio = Mathf.Clamp01(elapsed / cooldownTime);

            if (cooldownMask != null)
                cooldownMask.fillAmount = 1f - ratio;

            yield return null;
        }

        if (cooldownMask != null)
            cooldownMask.fillAmount = 0f;

        isCoolingDown = false; // 完成冷卻，允許再次使用
    }

    public void SetWaitingState(bool active)
    {
        if (active)
        {
            // Z=0、Y 抬高
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.anchoredPosition = new Vector2(originalPos.x, originalPos.y + waitingHeight);
        }
        else
        {
            // 還原
            rectTransform.anchoredPosition = originalPos;
            rectTransform.localRotation = originalRotation;
        }
    }

    private IEnumerator MoveToUIPosition(Vector2 targetPos, Vector3 targetScale, Quaternion targetRot, float duration)
    {
        Vector2 startPos = rectTransform.anchoredPosition;
        Vector3 startScale = rectTransform.localScale;
        Quaternion startRot = rectTransform.localRotation;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            rectTransform.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            rectTransform.localRotation = Quaternion.Lerp(startRot, targetRot, t);

            yield return null;
        }

        rectTransform.anchoredPosition = targetPos;
        rectTransform.localScale = targetScale;
        rectTransform.localRotation = targetRot;
    }
}
