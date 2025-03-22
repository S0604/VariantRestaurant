using UnityEngine;

[CreateAssetMenu(fileName = "NewIngredient", menuName = "Ingredient/Create New Ingredient")]
public class IngredientData : ScriptableObject
{
    public string ingredientName;   // 食材名称
    public Sprite ingredientImage;  // 食材图片
    public Sprite nameIcon;         // 名称图标

    public bool isBun;              // 是否是汉堡面包
    public Sprite bunTopImage;      // 面包上层
    public Sprite bunBottomImage;   // 面包下层

    public bool isSauce;            //  是否是酱料
    public Sprite sauceBottleImage; // 酱料瓶的图片
    public GameObject saucePrefab;  //  酱料的预制体（带动画）
}
