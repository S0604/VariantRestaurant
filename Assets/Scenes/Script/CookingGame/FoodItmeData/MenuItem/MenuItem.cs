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
    public BaseMinigame.DishGrade grade; 

    public string itemName;
    public Sprite[] gradeSprites;  
    public string itemTag;
    public Sprite itemImage;  

    public Sprite GetSpriteByGrade(ItemGrade grade)
    {
        int index = (int)grade;
        if (index >= 0 && index < gradeSprites.Length)
            return gradeSprites[index];
        return null;

    }
}
