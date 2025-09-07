using UnityEngine;

[CreateAssetMenu(fileName = "New Menu Item", menuName = "Menu/Menu Item")]
public class MenuItem : ScriptableObject
{
    // �̲׵����]�|�Q�ܲ��ƥ�令 Mutated�^
    public BaseMinigame.DishGrade grade;

    [Header("�򥻸��")]
    public string itemName;
    public string itemTag;

    [Header("��ܹϲա]�� DishGrade ���ޡGFail=0, Bad=1, Good=2, Perfect=3, Mutated=4�^")]
    public Sprite[] gradeSprites;

    // �� �V�U�ۮe�G�µ{����Ū�� itemImage�A�N�����s�b�æ۰ʦP�B
    public Sprite itemImage;

    public Sprite GetSpriteByGrade(BaseMinigame.DishGrade g)
    {
        int index = (int)g;
        return (gradeSprites != null && index >= 0 && index < gradeSprites.Length)
            ? gradeSprites[index]
            : null;
    }

    // �� �b Inspector ��F��ɦ۰ʦP�B itemImage
    private void OnValidate()
    {
        itemImage = GetSpriteByGrade(grade);
    }

    // �� �ѥ~���b���� grade ���ʦP�B�]�p�X��ɡ^
    public void SyncImageToGrade()
    {
        itemImage = GetSpriteByGrade(grade);
    }
}
