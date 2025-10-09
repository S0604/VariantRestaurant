using UnityEngine;

//[CreateAssetMenu(menuName = "CardSkills/PassiveSkill")]
public class PassiveSkill : CardSkill
{
    public override void Activate(GameObject user)
    {
        Debug.Log($"啟動被動技：{skillName}");
        // 被動技一般是裝備就套用，例如加血量上限、加攻擊力...
    }
}