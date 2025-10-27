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

    [Header("使用次數設定")]
    public int maxUses = 3;                    // 使用次數上限
    private int remainingUses;                 // 當前剩餘可用次數
    public Text useCountText;                  // 顯示剩餘次數（可選）

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

        remainingUses = maxUses;

        if (cooldownMask != null)
            cooldownMask.fillAmount = 0f;

        UpdateUseCountUI();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsLocked || isCoolingDown) return;

        // 若仍有可用次數
        if (remainingUses > 0)
        {
            IsLocked = true;
            CardClickManager.Instance.EnqueueCard(this);
        }
    }

    public IEnumerator PlayEnterAndStay()
    {
        IsLocked = true;

        // 進場動畫
        yield return StartCoroutine(MoveToUIPosition(
            centerAnchoredPos,
            Vector3.one * zoomScale,
            Quaternion.identity,
            moveDuration));

        // 觸發主動技
        var card = GetComponent<CardHoverEffect>();
        if (card != null && card.activeSkill != null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            card.activeSkill.Activate(player);
        }

        // 使用次數 -1
        remainingUses = Mathf.Max(remainingUses - 1, 0);
        UpdateUseCountUI();

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

        IsLocked = false;

        // ✅ 所有次數用完後才觸發冷卻
        if (remainingUses <= 0 && !isCoolingDown)
            StartCoroutine(StartCooldown());
    }

    private IEnumerator StartCooldown()
    {
        isCoolingDown = true;
        IsLocked = false;

        if (cooldownMask != null)
            cooldownMask.fillAmount = 1f;

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

        // ✅ 冷卻結束後重置次數
        remainingUses = maxUses;
        UpdateUseCountUI();

        isCoolingDown = false;
    }

    private void UpdateUseCountUI()
    {
        if (useCountText != null)
            useCountText.text = remainingUses.ToString();
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
