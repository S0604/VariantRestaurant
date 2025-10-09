using UnityEngine;

[CreateAssetMenu(fileName = "New Menu Item", menuName = "Menu/Menu Item")]
public class MenuItem : ScriptableObject
{
    // 最終評等（會被變異事件改成 Mutated）
    public BaseMinigame.DishGrade grade;

    [Header("基本資料")]
    public string itemName;
    public string itemTag;

    [Header("顯示圖組（依 DishGrade 索引：Fail=0, Bad=1, Good=2, Perfect=3, Mutated=4）")]
    public Sprite[] gradeSprites;

    // ★ 向下相容：舊程式仍讀取 itemImage，就讓它存在並自動同步
    public Sprite itemImage;

    public Sprite GetSpriteByGrade(BaseMinigame.DishGrade g)
    {
        int index = (int)g;
        return (gradeSprites != null && index >= 0 && index < gradeSprites.Length)
            ? gradeSprites[index]
            : null;
    }

    // ★ 在 Inspector 改東西時自動同步 itemImage
    private void OnValidate()
    {
        itemImage = GetSpriteByGrade(grade);
    }

    // ★ 供外部在改變 grade 後手動同步（如出菜時）
    public void SyncImageToGrade()
    {
        itemImage = GetSpriteByGrade(grade);
    }
}
