using UnityEngine;
using UnityEngine.EventSystems;

public class CardHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("卡片技能資料")]
    public string activeSkillName;
    public string activeSkillDesc;
    public string passiveSkillName;
    public string passiveSkillDesc;

    [Header("Hover 設定")]
    public float hoverPosY = 200f;       // 上移 Y
    public float scaleUpFactor = 1.2f;   // 放大
    public float transitionSpeed = 10f;  // 平滑

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Vector3 targetScale;

    private float sideOffset = 0f; // 側移量

    void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        originalScale = transform.localScale;

        targetPosition = originalPosition;
        targetRotation = originalRotation;
        targetScale = originalScale;

        // 加到 CardManager
        if (!CardManager.Instance.cards.Contains(this))
            CardManager.Instance.cards.Add(this);
    }

    void Update()
    {
        Vector3 finalPos = targetPosition + new Vector3(sideOffset, 0f, 0f);

        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPos, Time.deltaTime * transitionSpeed);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * transitionSpeed);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * transitionSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // 顯示技能 UI
        CardDescriptionUI.Instance.ShowDescription(
            activeSkillName, activeSkillDesc,
            passiveSkillName, passiveSkillDesc
        );
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // 隱藏技能 UI
        CardDescriptionUI.Instance.HideDescription();
    }

    // 給 CardManager 呼叫
    public void SetSideOffset(float offset)
    {
        sideOffset = offset;
    }

    public void ResetSideOffset()
    {
        sideOffset = 0f;
    }
}
