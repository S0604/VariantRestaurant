using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TextColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IPointerDownHandler, IPointerUpHandler
{
    public TMP_Text buttonText;
    public Color hoverColor = Color.black;
    public Color pressedColor = Color.gray;

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
        EventSystem.current.SetSelectedGameObject(null);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        buttonText.color = pressedColor; // 按住時變色
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        UpdateTextColor(); // 放開時恢復
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
