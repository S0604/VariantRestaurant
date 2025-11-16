using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeDefinition", menuName = "Game/Upgrade Definition")]
public class UpgradeDefinition : ScriptableObject
{
    public UpgradeType type;
    public string upgradeId = "supply_pickup_amount";

    [Header("单砞﹚")]
    public int maxLevel = 10;
    public float baseValue = 1f;     // 单0/1膀娄
    public float perLevelAdd = 0f;   // – +x
    public float perLevelMul = 0f;   // – ⊙x0 ボぃノ

    [Header("Display")]
    public string displayName = "Supply Pickup Amount";
    [TextArea] public string description = "矗ど–Ω干倒干计秖";
    public string valueUnit = "";  // ㄒ " pts""x"" sec"...

    [Header("Cost")]
    public int baseCost = 100;
    public int costPerLevelAdd = 100;
    public float costPerLevelMul = 1.0f; // >1 计Θ=1 絬┦=0 度猭

    public float Evaluate(int level)
    {
        level = Mathf.Clamp(level, 0, maxLevel);
        float add = baseValue + perLevelAdd * level;
        float mul = (perLevelMul > 0f) ? Mathf.Pow(perLevelMul, level) : 1f;
        return add * mul;
    }
}
