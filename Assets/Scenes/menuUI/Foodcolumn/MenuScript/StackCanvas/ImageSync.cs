using UnityEngine;
using UnityEngine.UI;

public class ImageSync : MonoBehaviour
{
    public Image BbImageUI;  // 目标 Image
    public Image BottomIngredientImage;  // 需要监测的 Image

    private Sprite lastSprite;  // 记录上一次的 sprite，避免重复更新

    void Update()
    {
        // 检查 BottomIngredientImage 的 sprite 是否变化
        if (BottomIngredientImage.sprite != lastSprite)
        {
            UpdateBbImage(BottomIngredientImage.sprite);
            lastSprite = BottomIngredientImage.sprite;  // 更新记录的 sprite
        }
    }

    // 更新 BbImageUI 的显示
    private void UpdateBbImage(Sprite newSprite)
    {
        if (BbImageUI == null)
        {
            Debug.LogError("❌ BbImageUI 未赋值！");
            return;
        }

        if (newSprite != null)
        {
            BbImageUI.sprite = newSprite;  // 更新图片
            BbImageUI.enabled = true;  // 显示 Image 组件
        }
        else
        {
            BbImageUI.enabled = false;  // 隐藏 Image 组件
        }
    }
}
