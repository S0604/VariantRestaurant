using UnityEngine;

[CreateAssetMenu(fileName = "RecoverPatienceSkill", menuName = "Skills/Active/RecoverPatience")]
public class RecoverPatienceSkill : ActiveSkill
{
    public float extraSeconds = 5f; // 要回復的秒數

    public override void Activate(GameObject player)
    {
        Debug.Log($"⚡ 使用主動技：{skillName} → 回復顧客耐心 +{extraSeconds}s");

        // 找出場景中所有 CustomerPatience
        var customers = FindObjectsOfType<CustomerPatience>();
        foreach (var cp in customers)
        {
            if (cp != null)
            {
                cp.AddExtraPatience(extraSeconds);
            }
        }
    }
}
