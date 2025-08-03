using UnityEngine;

[CreateAssetMenu(menuName = "SpecialCustomer/EffectData")]
public class SpecialCustomerEffectData : ScriptableObject
{
    public enum EffectType
    {
        ModifyPatienceRate,
        ModifySpawnInterval,
        ModifyCookTime // ✅ 新增這個項目
    }

    public EffectType effectType;
    public float effectValue;
}
