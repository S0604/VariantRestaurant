using UnityEngine;

[CreateAssetMenu(fileName = "MaxPatienceBonusSkill", menuName = "Skills/Passive/MaxPatienceBonus")]
public class MaxPatienceBonusSkill : PassiveSkill
{
    [Header("最大上限加成")] public float maxPatienceBonus = 3f;   // +3 點上限

    public override void Activate(GameObject player)
    {
        Debug.Log($"✨ 被動技能觸發：{skillName} → 顧客耐心上限 +{maxPatienceBonus} 點");

        // 告訴全域管理器（可累加）
        PassiveSkillManager.Instance.maxPatienceBonus += maxPatienceBonus;
    }
}