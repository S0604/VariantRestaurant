using UnityEngine;
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream

public enum ItemGrade
{
    Perfect,
    Good,
    Garbage
}
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes

[CreateAssetMenu(fileName = "New Menu Item", menuName = "Menu/Menu Item")]
public class MenuItem : ScriptableObject
{
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
    public BaseMinigame.DishGrade grade; 

    public string itemName;
    public Sprite[] gradeSprites;  
    public string itemTag;
    public Sprite itemImage;  

    public Sprite GetSpriteByGrade(ItemGrade grade)
=======
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
    // 餐點的評級（可能會在烹飪小遊戲後被改變，例如 Mutated 為特殊菜色）
    public BaseMinigame.DishGrade grade;

    [Header("基本資料")]
    public string itemName;   // 餐點名稱（如：漢堡、薯條、飲料）
    public string itemTag;    // 餐點標籤，用於分類或識別用途

    [Header("對應圖片（依照 DishGrade）")]
    [Tooltip("依照不同 DishGrade 顯示的圖片，例如：Fail=0, Bad=1, Good=2, Perfect=3, Mutated=4")]
    public Sprite[] gradeSprites;

    [Header("圖像顯示設定")]
    [Tooltip("若啟用，餐點圖片會依照 grade 自動切換為對應圖片")]
    public bool lockImageToGrade = true;

    [Tooltip("若未啟用自動切換，則可以手動指定圖片覆蓋")]
    public Sprite manualImageOverride;

    [Tooltip("實際顯示用的餐點圖片（由程式自動設定或手動指定）")]
    public Sprite itemImage;

    /// <summary>
    /// 根據等級取得對應圖片。
    /// </summary>
    public Sprite GetSpriteByGrade(BaseMinigame.DishGrade g)
>>>>>>> Stashed changes
    {
        int index = (int)grade;
        if (index >= 0 && index < gradeSprites.Length)
            return gradeSprites[index];
        return null;

<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
<<<<<<< Updated upstream
=======
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
    /// <summary>
    /// 當在 Inspector 編輯此 ScriptableObject 時自動執行，
    /// 用於即時更新圖片顯示（避免手動同步）。
    /// </summary>
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

    /// <summary>
    /// 在遊戲執行中手動同步圖片至當前等級。
    /// （例如烹飪完成後根據評級更新餐點圖片）
    /// </summary>
    public void SyncImageToGrade()
    {
        if (lockImageToGrade)
            itemImage = GetSpriteByGrade(grade);
        else
            itemImage = (manualImageOverride != null) ? manualImageOverride : itemImage;
>>>>>>> Stashed changes
    }
}
