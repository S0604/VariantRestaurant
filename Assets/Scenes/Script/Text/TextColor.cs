using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TextColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TMP_Text buttonText;                // 按钮内的文本
    public Color hoverColor = Color.black;     // 滑鼠懸停時的顏色，可在 Inspector 自訂

    private Color originalColor;               // 原始顏色

    private void Start()
    {
        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TMP_Text>();
        }
        originalColor = buttonText.color;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buttonText != null)
        {
            buttonText.color = hoverColor;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonText != null)
        {
            buttonText.color = originalColor;
        }
    }
}
