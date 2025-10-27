using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HoverAnimationTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("References")]
    public Animator hintAnimator; // 指向提示 Image 的 Animator

    [Header("Animator Parameters")]
    public string hoverTriggerName = "OnHover";   // 滑鼠移入時觸發
    public string exitTriggerName = "OnExit";     // 滑鼠離開時觸發

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hintAnimator != null)
            hintAnimator.SetTrigger(hoverTriggerName);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hintAnimator != null)
            hintAnimator.SetTrigger(exitTriggerName);
    }
}
