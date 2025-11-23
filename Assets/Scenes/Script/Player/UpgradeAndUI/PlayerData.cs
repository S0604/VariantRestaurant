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

    [Header("Data")]
    public PlayerStats stats = new PlayerStats();
    public LevelTableSO levelTable; // ← 在 Inspector 指定

    public event Action OnStatsChanged;
    public event Action<int> OnLevelUp; // 給音效/演出/UI

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // 經驗與等級 --------------------
    public void AddExperience(int amount)
    {
        if (amount <= 0) return;
        stats.experience += amount;
        CheckLevelUp();
        NotifyStatsChanged();
    }

    private void CheckLevelUp()
    {
        if (!levelTable) return;

        int maxLv = levelTable.MaxLevel;
        // 連跳等
        while (stats.level < maxLv)
        {
            int need = levelTable.GetRequiredExp(stats.level);
            if (need <= 0 || stats.experience < need) break;
            stats.experience -= need;
            stats.level++;
            OnLevelUp?.Invoke(stats.level);
            Debug.Log($"升級！目前等級：{stats.level}");
        }

        // 滿級時經驗不要再往上疊，夾在需求上限（顯示用）
        if (stats.level >= maxLv)
        {
            int need = levelTable.GetRequiredExp(stats.level);
            stats.experience = Mathf.Clamp(stats.experience, 0, need);
        }
    }

    // 金錢與屬性 --------------------
    public bool CanAfford(int cost) => cost <= stats.money;
    public bool SpendMoney(int cost)
    {
        if (!CanAfford(cost)) return false;
        stats.money -= cost;
        NotifyStatsChanged();
        return true;
    }
    public void AddMoney(int amount) { stats.money += Mathf.Max(0, amount); NotifyStatsChanged(); }
    public void AddPopularity(int amount) { stats.popularity += amount; NotifyStatsChanged(); }
    public void AddCustomerFlow(int amount) { stats.customerFlow += amount; NotifyStatsChanged(); }

    // 事件 --------------------------
    private void NotifyStatsChanged() => OnStatsChanged?.Invoke();
}
