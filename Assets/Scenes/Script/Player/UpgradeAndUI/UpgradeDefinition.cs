using UnityEngine;

public enum UpgradeCategory
{
    Supply,          // 補給相關
    WorkStation,       // 工作站/能量
    Economy,         // 經濟/被動收益
    Other
}

[CreateAssetMenu(fileName = "UpgradeDefinition", menuName = "Game/Upgrade Definition")]
public class UpgradeDefinition : ScriptableObject
{
    public UpgradeType type;
    public string upgradeId = "supply_pickup_amount";

    [Header("等級設定")]
    public int maxLevel = 10;
    public float baseValue = 1f;     // 等級0/1基礎值
    public float perLevelAdd = 0f;   // 每級 +x
    public float perLevelMul = 0f;   // 每級 ×x（0 表示不用）

    [Header("Display")]
    public string displayName = "";
    [TextArea] public string description = "";
    public string valueUnit = "";  // 例如 " pts"、"x"、" sec"...

    // UpgradeDefinition.cs 內加一行欄位（靠近 displayName/description）
    public UpgradeCategory category = UpgradeCategory.Other;

    [Header("Cost")]
    public int baseCost = 100;
    public int costPerLevelAdd = 100;
    public float costPerLevelMul = 1.0f; // >1 代表指數成長，=1 線性，=0 僅加法

    public float Evaluate(int level)
    {
        level = Mathf.Clamp(level, 0, maxLevel);
        float add = baseValue + perLevelAdd * level;
        float mul = (perLevelMul > 0f) ? Mathf.Pow(perLevelMul, level) : 1f;
        return add * mul;
    }
}
