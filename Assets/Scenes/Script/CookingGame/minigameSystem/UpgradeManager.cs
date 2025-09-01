using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("�ɵ��ɯ�")]
    public int supplyAmount = 1;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void UpgradeCookingStation(CookingStation station, int amount)
    {
        station.UpgradeEnergy(amount);
        Debug.Log($"�u�@����q�W���W�[�G+{amount}");
    }

    public void UpgradeSupplyAmount(int amount)
    {
        supplyAmount += amount;
        Debug.Log($"�ɵ��q�W�[�A�ثe���G{supplyAmount}");
    }
}
