using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TextColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text buttonText; // 按钮内的文本

    private Color originalColor; // 记录原始颜色

    private void Start()
    {
        // 确保按钮有 TextMeshPro 组件
        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TMP_Text>();
        }
        originalColor = buttonText.color; // 记录原来的颜色
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buttonText != null)
        {
            buttonText.color = Color.black; // 鼠标悬停时变黄
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonText != null)
        {
            buttonText.color = originalColor; // 鼠标移开时恢复原来的颜色
        }
    }
}
