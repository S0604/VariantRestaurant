using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("補給升級")]
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
        Debug.Log($"工作站能量上限增加：+{amount}");
    }

    public void UpgradeSupplyAmount(int amount)
    {
        supplyAmount += amount;
        Debug.Log($"補給量增加，目前為：{supplyAmount}");
    }
}
