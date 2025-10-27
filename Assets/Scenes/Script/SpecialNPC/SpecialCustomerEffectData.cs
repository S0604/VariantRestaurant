using UnityEngine;

[CreateAssetMenu(menuName = "SpecialCustomer/EffectData")]
public class SpecialCustomerEffectData : ScriptableObject
{
    public enum EffectType
    {
        ModifyPatienceRate,
        ModifySpawnInterval,
        ModifyCookTime 
    }

    public EffectType effectType;
    public float effectValue;
}
