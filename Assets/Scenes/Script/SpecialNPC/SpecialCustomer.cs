using UnityEngine;

public class SpecialCustomer : MonoBehaviour
{
    public SpecialCustomerEffectData[] effects;
    private bool isEffectApplied = false;

    void OnTriggerEnter(Collider other)
    {
        // 檢查是否碰到指定的觸發器（可透過 tag 或 name 判斷）
        if (other.CompareTag("SpecialEffectTrigger"))
        {
            TryApplyEffect();
        }
    }

    void OnDestroy()
    {
        RemoveEffect();
    }

    public void TryApplyEffect()
    {
        if (isEffectApplied) return;

        foreach (var effect in effects)
        {
            SpecialCustomerEffectManager.Instance.ApplyEffect(effect);
        }

        isEffectApplied = true;
    }

    public void RemoveEffect()
    {
        foreach (var effect in effects)
        {
            SpecialCustomerEffectManager.Instance.RemoveEffect(effect);
        }

        isEffectApplied = false;
    }
}
