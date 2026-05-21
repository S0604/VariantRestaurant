using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TextColor : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler,
    IPointerDownHandler,
    IPointerUpHandler
{
    public TMP_Text buttonText;

    [Header("顏色設定")]
    public Color hoverColor = Color.black;
    public Color pressedColor = Color.gray;

    private Color originalColor;
    private bool isHovering = false;

    private void Awake()
    {
        if (buttonText == null)
        {
            buttonText = GetComponentInChildren<TMP_Text>();
        }
    }

    private void OnEnable()
    {
        if (buttonText != null)
        {
            // 重新記錄目前正常顏色
            originalColor = buttonText.color;

            // 防止 alpha 被記錄成 0
            if (originalColor.a <= 0f)
            {
                originalColor.a = 1f;
            }

            buttonText.color = originalColor;
        }

        isHovering = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
        UpdateTextColor();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        UpdateTextColor();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (buttonText != null)
        {
            buttonText.color = KeepAlpha(pressedColor);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        UpdateTextColor();
    }

    private void OnDisable()
    {
        if (buttonText != null)
        {
            buttonText.color = originalColor;
        }

        isHovering = false;
    }

    private void UpdateTextColor()
    {
        if (buttonText == null) return;

        if (isHovering)
        {
            buttonText.color = KeepAlpha(hoverColor);
        }
        else
        {
            buttonText.color = originalColor;
        }
    }

    // 保留原本 alpha，避免透明
    private Color KeepAlpha(Color targetColor)
    {
        targetColor.a = originalColor.a > 0 ? originalColor.a : 1f;
        return targetColor;
    }
}