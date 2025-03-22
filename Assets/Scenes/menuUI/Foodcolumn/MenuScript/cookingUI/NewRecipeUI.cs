using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class NewRecipeUI : MonoBehaviour
{
    public Image recipeImage;
    public TMP_Text recipeNameText;

    public void SetRecipeImage(string path)
    {
        if (!File.Exists(path)) return;

        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(File.ReadAllBytes(path));
        recipeImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
    }

    public void SetRecipeName(string name)
    {
        recipeNameText.text = name;
    }
}