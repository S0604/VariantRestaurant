using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerStats
{
    public int money = 0;
    public int popularity = 0;
    public int customerFlow = 0;
    public int experience = 0;
    public int level = 1;

    // ⭐ 星級
    public int stars = 0;
}

public class PlayerData : MonoBehaviour
{
    public static PlayerData Instance { get; private set; }

    [Header("Data")]
    public PlayerStats stats = new PlayerStats();

    [Header("Level Table (Current)")]
    public LevelTableSO levelTable;

    [Header("Level Tables By Stars")]
    public List<LevelTableSO> levelTablesByStars;

    // 🔔 事件
    public event Action OnStatsChanged;
    public event Action<int> OnLevelUp;
    public event Action<int> OnStarsChanged;

    void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        ApplyStarLevelTable(); // ⭐ 初始化只切表，不重置
    }

    // =========================
    // ⭐ 星級 → 切換成長曲線（不重置）
    // =========================
    void ApplyStarLevelTable()
    {
        if (levelTablesByStars == null || levelTablesByStars.Count == 0)
            return;

        int index = Mathf.Clamp(stats.stars, 0, levelTablesByStars.Count - 1);

        levelTable = levelTablesByStars[index];

        Debug.Log($"[PlayerData] 切換LevelTable → Stars: {stats.stars} | index: {index}");
    }

    // ⭐ 重置等級進度（只在星級變動時呼叫）
    void ResetLevelProgress()
    {
        stats.level = 1;
        stats.experience = 0;
    }

    // =========================
    // EXP / 等級
    // =========================
    public void AddExperience(int amount)
    {
        if (amount <= 0) return;

        stats.experience += amount;
        CheckLevelUp();
        NotifyStatsChanged();
    }

    void CheckLevelUp()
    {
        if (!levelTable) return;

        int maxLv = levelTable.MaxLevel;

        // 🔁 連續升級
        while (stats.level < maxLv)
        {
            int need = levelTable.GetRequiredExp(stats.level);
            if (need <= 0 || stats.experience < need) break;

            stats.experience -= need;
            stats.level++;

            OnLevelUp?.Invoke(stats.level);
            Debug.Log($"升級！目前等級：{stats.level}");
        }

        // 🧱 滿級處理
        if (stats.level >= maxLv)
        {
            int need = levelTable.GetRequiredExp(stats.level);
            stats.experience = Mathf.Clamp(stats.experience, 0, need);
        }
    }

    // =========================
    // 💰 金錢 / 屬性
    // =========================
    public bool CanAfford(int cost) => cost <= stats.money;

    public bool SpendMoney(int cost)
    {
        if (!CanAfford(cost)) return false;

        stats.money -= cost;
        NotifyStatsChanged();
        return true;
    }

    public void AddMoney(int amount)
    {
        stats.money += Mathf.Max(0, amount);
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

    // =========================
    // ⭐ Stars（只有這裡會重置等級）
    // =========================
    public int GetStars() => stats.stars;

    public void AddStars(int amount)
    {
        if (amount == 0) return;

        int oldStars = stats.stars;

        stats.stars = Mathf.Max(0, stats.stars + amount);

        if (stats.stars != oldStars)
        {
            ApplyStarLevelTable();
            ResetLevelProgress(); // ⭐ 只有星級變動才重置
        }

        OnStarsChanged?.Invoke(stats.stars);
        NotifyStatsChanged();
    }

    public void SetStars(int value)
    {
        value = Mathf.Max(0, value);

        if (stats.stars == value) return;

        stats.stars = value;

        ApplyStarLevelTable();
        ResetLevelProgress(); // ⭐ 只有星級變動才重置

        OnStarsChanged?.Invoke(stats.stars);
        NotifyStatsChanged();
    }

    // =========================
    // 🔔 事件通知
    // =========================
    void NotifyStatsChanged()
    {
        OnStatsChanged?.Invoke();
    }
}