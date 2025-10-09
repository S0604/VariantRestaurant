using UnityEngine;

[CreateAssetMenu(fileName = "MaxPatienceBonusSkill", menuName = "Skills/Passive/MaxPatienceBonus")]
public class MaxPatienceBonusSkill : PassiveSkill
{
    public float bonusSeconds = 2f;

    public override void Activate(GameObject player)
    {
        Debug.Log($"✨ 被動技能觸發：{skillName} → 顧客耐心上限 +{bonusSeconds} 秒");

        // 告訴 CustomerManager / Spawner 之類的地方
        PassiveSkillManager.Instance.maxPatienceBonus += bonusSeconds;
    }
}
