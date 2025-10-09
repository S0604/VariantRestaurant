using UnityEngine;

//[CreateAssetMenu(menuName = "CardSkills/ActiveSkill")]
public class ActiveSkill : CardSkill
{
    public override void Activate(GameObject user)
    {
        Debug.Log($"使用主動技：{skillName}");
        // 這裡放實際效果，例如發射火球、回血、給 buff...
    }
}
