using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class NewRecipeUI : MonoBehaviour
{
    public Image recipeImage; // 显示新料理的 UI Image 组件

    public void SetRecipeImage(string imagePath)
    {
        if (!File.Exists(imagePath))
        {
            Debug.LogError($"图片文件未找到: {imagePath}");
            return;
        }

        byte[] imageData = File.ReadAllBytes(imagePath);
        Texture2D texture = new Texture2D(2, 2);
        if (texture.LoadImage(imageData))
        {
            Sprite newSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            recipeImage.sprite = newSprite;
            recipeImage.preserveAspect = true;
            Debug.Log("成功加载新料理图片: " + imagePath);
        }
        else
        {
            Debug.LogError("加载图片失败: " + imagePath);
        }
    }
}
