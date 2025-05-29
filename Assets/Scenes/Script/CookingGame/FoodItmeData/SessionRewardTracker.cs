using UnityEngine;

public class SessionRewardTracker : MonoBehaviour
{
    public static SessionRewardTracker Instance { get; private set; }

    public int sessionExp;
    public int sessionPopularity;
    public int sessionMoney;
    public int sessionCustomerFlow;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AddRewards(int exp, int pop, int money)
    { sessionExp += exp; sessionPopularity += pop; sessionMoney += money; }

    public void AddCustomer() => sessionCustomerFlow++;

    public void ApplyToPlayerData()
    {
        PlayerData pd = PlayerData.Instance;
        pd.AddExperience(sessionExp);
        pd.AddPopularity(sessionPopularity);
        pd.AddMoney(sessionMoney);
        pd.AddCustomerFlow(sessionCustomerFlow);
    }
    public void ResetSession()
    {
        sessionExp = sessionPopularity = sessionMoney = sessionCustomerFlow = 0;
    }
}
