using System;
using UnityEngine;

[System.Serializable]
public class PlayerStats
{
    public int money = 0;
    public int popularity = 0;
    public int customerFlow = 0;
    public int experience = 0;
    public int level = 1;
}

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; }

    public PlayerStats stats = new PlayerStats();
    public event Action OnStatsChanged;

    private LevelUIManager levelUIManager;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ✅ 改由 LevelUIManager 注入，避免初始化順序問題
    public void InjectLevelUIManager(LevelUIManager uiManager)
    {
        levelUIManager = uiManager;
        Debug.Log("PlayerData 收到 LevelUIManager 注入");
    }

    public void AddExperience(int amount)
    {
        stats.experience += amount;
        CheckLevelUp();
        NotifyStatsChanged();
    }

    private void CheckLevelUp()
    {
        if (levelUIManager == null)
        {
            Debug.LogWarning("尚未注入 LevelUIManager，無法執行升級計算");
            return;
        }

        while (true)
        {
            int requiredExp = levelUIManager.GetRequiredExpForLevel(stats.level);
            int maxLevel = levelUIManager.levels.Count - 1;

            if (stats.experience < requiredExp || stats.level >= maxLevel)
                break;

            stats.experience -= requiredExp;
            stats.level++;
            Debug.Log($"升級！目前等級：{stats.level}");
        }
    }

    private void NotifyStatsChanged()
    {
        OnStatsChanged?.Invoke();
    }

    public void AddMoney(int amount)
    {
        stats.money += amount;
        NotifyStatsChanged();
    }

    public void AddPopularity(int amount)
    {
        stats.popularity += amount;
        NotifyStatsChanged();
    }

    public void AddCustomerFlow(int amount)
    {
        stats.customerFlow += amount;
        NotifyStatsChanged();
    }
}
