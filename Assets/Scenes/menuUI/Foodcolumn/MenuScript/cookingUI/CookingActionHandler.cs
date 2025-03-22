using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class NewRecipeUIManager : MonoBehaviour
{
    public GameObject newRecipePanel;
    public Image recipeImage;
    public TMP_Text recipeNameText;

    public void ShowNewRecipe(string imagePath)
    {
        if (!File.Exists(imagePath))
        {
            Debug.LogError("图片不存在: " + imagePath);
            return;
        }

        byte[] imageData = File.ReadAllBytes(imagePath);
        Texture2D texture = new Texture2D(2, 2);
        if (texture.LoadImage(imageData))
        {
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
            recipeImage.sprite = sprite;
            recipeImage.preserveAspect = true;
            newRecipePanel.SetActive(true);
        }
    }

    public void HideNewRecipe()
    {
        newRecipePanel.SetActive(false);
    }

    public void SetRecipeName(string name)
    {
        recipeNameText.text = name;
    }
}
