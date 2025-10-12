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

    [Header("手動覆寫")]
    [Tooltip("勾選時會依 gradeSprites 自動帶入；取消勾選可手動指定下方圖片")]
    public bool lockImageToGrade = true;
    [Tooltip("取消勾選上方開關後，這張圖會直接用作 itemImage")]
    public Sprite manualImageOverride;

    // ★ 向下相容：舊程式仍讀取 itemImage，就讓它存在並自動同步/或覆寫
    public Sprite itemImage;

    public Sprite GetSpriteByGrade(BaseMinigame.DishGrade g)
    {
        int index = (int)g;
        return (gradeSprites != null && index >= 0 && index < gradeSprites.Length)
            ? gradeSprites[index]
            : null;
    }

    // Inspector 改東西時自動同步（但可被手動覆寫）
    private void OnValidate()
    {
        if (lockImageToGrade)
        {
            itemImage = GetSpriteByGrade(grade);
        }
        else
        {
            itemImage = manualImageOverride != null ? manualImageOverride : itemImage;
        }
    }

    // 外部在改變 grade 後手動同步（如出菜時）
    public void SyncImageToGrade()
    {
        if (lockImageToGrade)
            itemImage = GetSpriteByGrade(grade);
        else
            itemImage = (manualImageOverride != null) ? manualImageOverride : itemImage;
    }
}
