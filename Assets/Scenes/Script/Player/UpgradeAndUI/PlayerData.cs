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
    public int stars = 0;
}

[Serializable]
public class PlayerDataSaveData
{
    public int money;
    public int popularity;
    public int customerFlow;
    public int experience;
    public int level;
    public int stars;
}

public class PlayerData : MonoBehaviour, ISaveable
{
    public static PlayerData Instance { get; private set; }

    [Header("Data")]
    public PlayerStats stats = new PlayerStats();
    public LevelTableSO levelTable;

    [Header("Save")]
    [SerializeField] private string uniqueID = "PlayerData";

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
    }

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

        while (stats.level < maxLv)
        {
            int need = levelTable.GetRequiredExp(stats.level);
            if (need <= 0 || stats.experience < need) break;

            stats.experience -= need;
            stats.level++;
            OnLevelUp?.Invoke(stats.level);
            Debug.Log($"升級！目前等級：{stats.level}");
        }

        if (stats.level >= maxLv)
        {
            int need = levelTable.GetRequiredExp(stats.level);
            stats.experience = Mathf.Clamp(stats.experience, 0, need);
        }
    }

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

    public int GetStars() => stats.stars;

    public void AddStars(int amount)
    {
        if (amount == 0) return;
        stats.stars = Mathf.Max(0, stats.stars + amount);
        OnStarsChanged?.Invoke(stats.stars);
        NotifyStatsChanged();
    }

    public void SetStars(int value)
    {
        value = Mathf.Max(0, value);
        if (stats.stars == value) return;

        stats.stars = value;
        OnStarsChanged?.Invoke(stats.stars);
        NotifyStatsChanged();
    }

    private void NotifyStatsChanged()
    {
        OnStatsChanged?.Invoke();
    }

    public string GetUniqueID()
    {
        return uniqueID;
    }

    public string CaptureAsJson()
    {
        PlayerDataSaveData data = new PlayerDataSaveData
        {
            money = stats.money,
            popularity = stats.popularity,
            customerFlow = stats.customerFlow,
            experience = stats.experience,
            level = stats.level,
            stars = stats.stars
        };

        return JsonUtility.ToJson(data);
    }

    public void RestoreFromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
            return;

        PlayerDataSaveData data = JsonUtility.FromJson<PlayerDataSaveData>(json);
        if (data == null)
            return;

        stats.money = data.money;
        stats.popularity = data.popularity;
        stats.customerFlow = data.customerFlow;
        stats.experience = data.experience;
        stats.level = Mathf.Max(1, data.level);
        stats.stars = Mathf.Max(0, data.stars);

        if (levelTable != null)
        {
            int maxLv = levelTable.MaxLevel;
            stats.level = Mathf.Clamp(stats.level, 1, maxLv);

            int need = levelTable.GetRequiredExp(stats.level);
            if (need > 0)
            {
                stats.experience = Mathf.Clamp(stats.experience, 0, need);
            }
            else
            {
                stats.experience = Mathf.Max(0, stats.experience);
            }
        }

        OnStarsChanged?.Invoke(stats.stars);
        NotifyStatsChanged();

        Debug.Log("[Save] PlayerData 已還原");
    }
}