using UnityEngine;

public abstract class CardSkill : ScriptableObject
{
    public string skillName;
    [TextArea] public string description;

    // 執行技能時會呼叫這個
    public abstract void Activate(GameObject user);
}
