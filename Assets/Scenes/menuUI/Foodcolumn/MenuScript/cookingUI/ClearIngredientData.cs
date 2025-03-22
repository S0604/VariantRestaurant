using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClearIngredientData : MonoBehaviour
{
    public ItemSlot[] itemSlots; // CookingUI 里的所有 ItemSlot
    public Image BottomBunImage, TopIngredientImage, MiddleIngredientImage, BottomIngredientImage, TopBunImage; // StackPanel 内的 UI 组件
    public Transform stackPanel; // 用于引用 StackPanel Transform

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

        // 清除 StackPanel 中的 Sauces(Clone) 游戏对象
        ClearSaucesFromStackPanel();

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

    private void ClearSaucesFromStackPanel()
    {
        // 遍历 StackPanel 下的所有子对象
        foreach (Transform child in stackPanel)
        {
            // 如果子对象的名字包含 "Sauces(Clone)"，则销毁该对象
            if (child.gameObject.name.Contains("Sauces(Clone)"))
            {
                Destroy(child.gameObject); // 销毁该子对象
            }
        }
    }
}
