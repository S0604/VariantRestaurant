using UnityEngine;
using UnityEngine.EventSystems;

public class CardHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

    [Header("技能")]
    public ActiveSkill activeSkill;
    public PassiveSkill passiveSkill;


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
    private CardClickEffectUI clickEffect; // 🔑 點擊動畫控制

    void Start()
    {
        originalPosition = transform.localPosition;
        originalRotation = transform.localRotation;
        originalScale = transform.localScale;

        targetPosition = originalPosition;
        targetRotation = originalRotation;
        targetScale = originalScale;

        clickEffect = GetComponent<CardClickEffectUI>();

        // 加到 CardManager
        if (!CardManager.Instance.cards.Contains(this))
            CardManager.Instance.cards.Add(this);

        // ✅ 只要掛載被動技能就立即啟動
        if (passiveSkill != null)
        {
            passiveSkill.Activate(gameObject);
        }
    }

    void Update()
    {
        // 🔒 如果正在被點擊動畫控制，就不要更新 hover 動畫
        if (clickEffect != null && clickEffect.IsLocked) return;

        Vector3 finalPos = targetPosition + new Vector3(sideOffset, 0f, 0f);

        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPos, Time.deltaTime * transitionSpeed);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * transitionSpeed);
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * transitionSpeed);
    }

    public void EquipCard(GameObject player)
    {
        if (passiveSkill != null)
        {
            passiveSkill.Activate(player);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (clickEffect != null && clickEffect.IsLocked) return;

        Debug.Log("Pointer Enter: " + gameObject.name);
        targetPosition = new Vector3(originalPosition.x, hoverPosY, originalPosition.z);
        targetRotation = Quaternion.Euler(0f, 0f, 0f);
        targetScale = originalScale * scaleUpFactor;

        CardManager.Instance.OnCardHover(this);

        // ✅ 直接從技能物件取資料
        string activeName = activeSkill != null ? activeSkill.skillName : "";
        string activeDesc = activeSkill != null ? activeSkill.description : "";
        string passiveName = passiveSkill != null ? passiveSkill.skillName : "";
        string passiveDesc = passiveSkill != null ? passiveSkill.description : "";

        CardDescriptionUI.Instance.ShowDescription(activeName, activeDesc, passiveName, passiveDesc);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (clickEffect != null && clickEffect.IsLocked) return; // 🔒 忽略點擊狀態

        Debug.Log("Pointer Exit: " + gameObject.name);
        targetPosition = originalPosition;
        targetRotation = originalRotation;
        targetScale = originalScale;

        CardManager.Instance.OnCardExit(this);

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
    public void ForceExit()
    {
        targetPosition = originalPosition;
        targetRotation = originalRotation;
        targetScale = originalScale;

        CardManager.Instance.OnCardExit(this);
        CardDescriptionUI.Instance.HideDescription();

        Debug.Log("Force Exit: " + gameObject.name);
    }

}
