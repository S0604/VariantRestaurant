using UnityEngine;

[CreateAssetMenu(fileName = "PerfectCookNextSkill", menuName = "Skills/Active/PerfectCookNext")]
public class PerfectCookNextSkill : ActiveSkill
{
    public override void Activate(GameObject player)
    {
        Debug.Log($"🍽 使用主動技：{skillName} → 下次料理評級必為 Perfect");

        if (PerfectCookBuffManager.Instance == null)
        {
            Debug.LogWarning("⚠️ 找不到 PerfectCookBuffManager，請確認場景中有該物件");
            return;
        }

        PerfectCookBuffManager.Instance.ActivatePerfectBuff();
    }
}
