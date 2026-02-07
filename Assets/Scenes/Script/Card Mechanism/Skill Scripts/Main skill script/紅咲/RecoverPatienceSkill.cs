using UnityEngine;

[CreateAssetMenu(fileName = "RecoverPatienceSkill", menuName = "Skills/Active/RecoverPatience")]
public class RecoverPatienceSkill : ActiveSkill
{
    [Header("回復量")] public float recoverAmount = 6f;   // 當前耐心 +6

    public override void Activate(GameObject player)
    {
        Debug.Log($"⚡ 使用主動技：{skillName} → 回復顧客耐心 +{recoverAmount}");

        // 找出場景中所有 CustomerPatience
        var customers = FindObjectsOfType<CustomerPatience>();
        foreach (var cp in customers)
        {
            if (cp != null)
            {
                cp.AddPatience(recoverAmount);   // 直接加血，已內建 Clamp
            }
        }
    }
}