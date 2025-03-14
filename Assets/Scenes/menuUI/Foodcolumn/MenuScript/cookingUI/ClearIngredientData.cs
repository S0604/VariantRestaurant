using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClearIngredientData : MonoBehaviour
{
    public ItemSlot[] itemSlots; // CookingUI 里的所有 ItemSlot
    public Image BottomBunImage, TopIngredientImage, MiddleIngredientImage, BottomIngredientImage, TopBunImage; // StackPanel 内的 UI 组件

    public void OpenUI()
    {
        gameObject.SetActive(true);

        foreach (var slot in itemSlots)
        {
            slot.ClearSlot(); // 确保 ItemSlot 重新打开时是空的
        }

        ClearStackPanelImages();
    }


    public void CloseUI()
    {
        // 清除所有 ItemSlot 的 IngredientData
        foreach (var slot in itemSlots)
        {
            slot.ClearSlot();
        }

        // 清空 StackPanel 中的 Image 组件
        ClearStackPanelImages();

        gameObject.SetActive(false);

        // 确保所有 ItemSlot 的 Image 组件在 UI 打开时显示
        foreach (var slot in itemSlots)
        {
            var slotImage = slot.GetComponent<Image>();
            if (slotImage != null)
            {
                slotImage.enabled = true;  // 确保 ItemSlot 的 Image 组件显示
            }
        }
    }

    private void ClearStackPanelImages()
    {
        BottomBunImage.sprite = null;
        TopIngredientImage.sprite = null;
        MiddleIngredientImage.sprite = null;
        BottomIngredientImage.sprite = null;
        TopBunImage.sprite = null;

        // 隐藏 Image 组件
        BottomBunImage.enabled = false;
        TopIngredientImage.enabled = false;
        MiddleIngredientImage.enabled = false;
        BottomIngredientImage.enabled = false;
        TopBunImage.enabled = false;
    }
}