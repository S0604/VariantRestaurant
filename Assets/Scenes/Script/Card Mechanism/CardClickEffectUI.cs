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
    public float cooldownTime = 5f;
    public Image cooldownMask;

    private Vector2 originalPos;
    private Vector3 originalScale;
    private Quaternion originalRotation;

    private bool isCoolingDown = false;
    public bool IsLocked { get; private set; } = false;

    // ✅ 新增：控制對話只播放一次
    private static bool hasPlayedChapter12 = false;

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
        if (IsLocked || isCoolingDown) return;

        IsLocked = true;
        CardClickManager.Instance.EnqueueCard(this);
    }

    public IEnumerator PlayEnterAndStay()
    {
        IsLocked = true;

        yield return StartCoroutine(MoveToUIPosition(
            centerAnchoredPos,
            Vector3.one * zoomScale,
            Quaternion.identity,
            moveDuration));

        var card = GetComponent<CardHoverEffect>();
        if (card != null && card.activeSkill != null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            card.activeSkill.Activate(player);
        }

        // ✅ 只播放一次
        if (!hasPlayedChapter12)
        {
            TutorialDialogueController.Instance.PlayChapter("12");
            hasPlayedChapter12 = true;
            Debug.Log("🎬 已播放章節 12（僅一次）");
        }

        yield return new WaitForSeconds(stayDuration);
    }

    public IEnumerator PlayExit()
    {
        yield return StartCoroutine(MoveToUIPosition(
            originalPos,
            originalScale,
            originalRotation,
            moveDuration));

        var hoverEffect = GetComponent<CardHoverEffect>();
        if (hoverEffect != null)
            hoverEffect.ForceExit();

        rectTransform.anchoredPosition = originalPos;
        rectTransform.localScale = originalScale;
        rectTransform.localRotation = originalRotation;

        StartCoroutine(StartCooldown());
    }

    private IEnumerator StartCooldown()
    {
        isCoolingDown = true;
        IsLocked = false;

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

        isCoolingDown = false;
    }

    public void SetWaitingState(bool active)
    {
        if (active)
        {
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.anchoredPosition = new Vector2(originalPos.x, originalPos.y + waitingHeight);
        }
        else
        {
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