using UnityEngine;

public class SpecialCustomerEffectManager : MonoBehaviour
{
    public static SpecialCustomerEffectManager Instance { get; private set; }

    public float patienceRateModifier = 0f;
    public float spawnRateModifier = 0f;
    public float cookTimeModifier = 0f; 


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void ApplyEffect(SpecialCustomerEffectData effect)
    {
        switch (effect.effectType)
        {
            case SpecialCustomerEffectData.EffectType.ModifyPatienceRate:
                patienceRateModifier += effect.effectValue;
                break;
            case SpecialCustomerEffectData.EffectType.ModifySpawnInterval:
                spawnRateModifier += effect.effectValue;
                break;
            case SpecialCustomerEffectData.EffectType.ModifyCookTime: // ✅ 新增
                cookTimeModifier += effect.effectValue;
                break;
        }
    }

    public void RemoveEffect(SpecialCustomerEffectData effect)
    {
        switch (effect.effectType)
        {
            case SpecialCustomerEffectData.EffectType.ModifyPatienceRate:
                patienceRateModifier -= effect.effectValue;
                break;
            case SpecialCustomerEffectData.EffectType.ModifySpawnInterval:
                spawnRateModifier -= effect.effectValue;
                break;
            case SpecialCustomerEffectData.EffectType.ModifyCookTime: // ✅ 新增
                cookTimeModifier -= effect.effectValue;
                break;
        }
    }

}
