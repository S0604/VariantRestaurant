using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TextColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public TMP_Text buttonText;
    public Color hoverColor = Color.black;

    private Color originalColor;
    private bool isHovering = false;

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
        EventSystem.current.SetSelectedGameObject(null); // 避免卡在 selected
        UpdateTextColor(); // 再次根據 hover 狀態更新文字顏色
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
        if (buttonText != null)
        {
            buttonText.color = isHovering ? hoverColor : originalColor;
        }
    }
}
