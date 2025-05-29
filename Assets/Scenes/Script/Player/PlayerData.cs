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

    void Start()
    {
        levelUIManager = FindObjectOfType<LevelUIManager>();
        if (levelUIManager == null)
        {
            Debug.LogError("找不到 LevelUIManager！");
        }
    }

    public void AddExperience(int amount)
    {
        stats.experience += amount;
        CheckLevelUp();
        NotifyStatsChanged();
    }

    private void CheckLevelUp()
    {
        while (true)
        {
            if (levelUIManager == null) break;

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
