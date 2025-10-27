using UnityEngine;

[CreateAssetMenu(fileName = "MaxEnergyBonusSkill", menuName = "Skills/Passive/MaxEnergyBonus")]
public class MaxEnergyBonusSkill : PassiveSkill
{
    [Header("烹飪台能量上限加成")]
    public int bonusEnergy = 2;

    public override void Activate(GameObject player)
    {
        Debug.Log($"💪 被動技能觸發：{skillName} → 烹飪台最大能量 +{bonusEnergy}");

        // 將加成記錄進全域被動管理器
        PassiveSkillManager.Instance.maxEnergyBonus += bonusEnergy;

        // 立即套用到所有現有的烹飪台
        var stations = Object.FindObjectsByType<CookingStation>(FindObjectsSortMode.None);
        foreach (var station in stations)
        {
            station.UpgradeEnergy(bonusEnergy);
        }
    }
}
