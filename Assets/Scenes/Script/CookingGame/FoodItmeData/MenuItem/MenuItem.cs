using UnityEngine;

public enum ItemGrade
{
    Perfect,
    Good,
    Garbage
}

[CreateAssetMenu(fileName = "New Menu Item", menuName = "Menu/Menu Item")]
public class MenuItem : ScriptableObject
{
    public BaseMinigame.DishGrade grade; //加入料理品質等級

    public string itemName;
    public Sprite[] gradeSprites;  // 依照 enum ItemGrade 順序
    public string itemTag;
    public Sprite itemImage;  // 可選：代表該道菜的主要圖片

    public Sprite GetSpriteByGrade(ItemGrade grade)
    {
        int index = (int)grade;
        if (index >= 0 && index < gradeSprites.Length)
            return gradeSprites[index];
        return null;

    }
}
