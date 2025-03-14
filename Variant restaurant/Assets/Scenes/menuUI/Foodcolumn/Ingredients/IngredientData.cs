using UnityEngine;
using TMPro;

[CreateAssetMenu(fileName = "NewIngredient", menuName = "Ingredient/Create New Ingredient")]
public class IngredientData : ScriptableObject
{
    public string ingredientName;   // 食材名称
    public Sprite ingredientImage;  // 食材图片
    public Sprite nameIcon;         // 名称图标
    public bool isBun; // 是否为汉堡面包
    public Sprite bunTopImage; // 面包上层（如果是面包）
    public Sprite bunBottomImage; // 面包下层（如果是面包）
}
