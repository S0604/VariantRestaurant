using UnityEngine;

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

    public float Evaluate(int level)
    {
        level = Mathf.Clamp(level, 0, maxLevel);
        float add = baseValue + perLevelAdd * level;
        float mul = (perLevelMul > 0f) ? Mathf.Pow(perLevelMul, level) : 1f;
        return add * mul;
    }
}
