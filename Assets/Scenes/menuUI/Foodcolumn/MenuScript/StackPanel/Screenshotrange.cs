using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Screenshotrange : MonoBehaviour
{
    public Image icon; // UI 图片
    private ItemSlotHoverHandler hoverHandler;
    private IngredientData storedIngredient;


    // Start is called before the first frame update
    void Start()
    {
        if (icon == null)
        {
            Debug.LogWarning("⚠️ ItemSlot: icon 未分配，请在 Inspector 中设置。", this);
            return;
        }

        icon.enabled = false;
        hoverHandler = GetComponent<ItemSlotHoverHandler>();

    }

    // Update is called once per frame
    public void SetScreenshotrangeItem(IngredientData ingredient)
    {
        storedIngredient = ingredient;
        icon.enabled = ingredient != null;
        icon.sprite = ingredient?.ingredientImage;

        if (ingredient != null)
        {
            icon.sprite = ingredient.ingredientImage;
            icon.enabled = true;
            Debug.Log($"✅ ItemSlot 设定食材: {ingredient.ingredientName}");
        }
        else
        {
            icon.sprite = null;
            icon.enabled = false;
            Debug.Log("❌ ItemSlot 取消食材");
        }

        // 通知 CookingMenuUI 更新 StackPanel
        if (CookingMenuUI.Instance != null)
        {
            CookingMenuUI.Instance.UpdateStackPanel();
        }
        else
        {
            Debug.LogError("❌ CookingMenuUI.Instance 为空，无法更新 StackPanel！");
        }

        hoverHandler?.ForceHideNameIcon();
    }
}
