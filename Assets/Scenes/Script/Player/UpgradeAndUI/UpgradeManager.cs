using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public class UpgradeLevelEntry
{
    public string upgradeId;
    public int level;
}

[Serializable]
public class UpgradeManagerSaveData
{
    public List<UpgradeLevelEntry> levels = new List<UpgradeLevelEntry>();
}

public class UpgradeManager : MonoBehaviour, ISaveable
{
    public static UpgradeManager Instance;

    public static event Action<UpgradeType, int> OnAnyLevelChanged;
    public event Action<UpgradeType, int> OnLevelChanged;

    [Header("升級清單（在 Inspector 指定）")]
    public List<UpgradeDefinition> upgradeDefs = new();

    [Header("相容欄位（舊系統可用）")]
    public int supplyAmount = 3;

    [Header("Save")]
    [SerializeField] private string uniqueID = "UpgradeManager";

    private readonly Dictionary<string, int> _levels = new();

    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadLevelsFromPrefsAsFallback();
        ReapplyRuntimeEffects();
        BroadcastAllCurrentLevels();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BroadcastAllCurrentLevels();
    }

    private void LoadLevelsFromPrefsAsFallback()
    {
        _levels.Clear();

        foreach (var def in upgradeDefs)
        {
            if (!def) continue;

            int lv = PlayerPrefs.GetInt($"UPG_{def.upgradeId}", 0);
            _levels[def.upgradeId] = Mathf.Clamp(lv, 0, def.maxLevel);
        }
    }

    public float GetValue(UpgradeType type)
    {
        var def = GetDef(type);
        if (!def) return 0f;
        return def.Evaluate(GetLevel(def.upgradeId));
    }

    public int GetLevel(UpgradeType type)
    {
        var def = GetDef(type);
        return def ? GetLevel(def.upgradeId) : 0;
    }

    public void SetLevel(UpgradeType type, int level)
    {
        var def = GetDef(type);
        if (def) SetLevel(def.upgradeId, level);
    }

    public void ResetUpgrade(UpgradeType type)
    {
        var def = GetDef(type);
        if (!def) return;
        SetLevel(def.upgradeId, 0);
    }

    public void ResetUpgrade(string upgradeId)
    {
        if (string.IsNullOrEmpty(upgradeId)) return;

        var def = upgradeDefs.Find(d => d && d.upgradeId == upgradeId);
        if (!def) return;

        SetLevel(def.upgradeId, 0);
    }

    public void ResetAllUpgrades()
    {
        foreach (var def in upgradeDefs)
        {
            if (!def) continue;
            SetLevel(def.upgradeId, 0);
        }
    }

    public void RebuildFromDefinitions()
    {
        Dictionary<string, int> oldLevels = new Dictionary<string, int>(_levels);

        _levels.Clear();

        foreach (var def in upgradeDefs)
        {
            if (!def) continue;

            int lv = 0;
            if (oldLevels.TryGetValue(def.upgradeId, out int savedLv))
            {
                lv = Mathf.Clamp(savedLv, 0, def.maxLevel);
            }
            else
            {
                lv = Mathf.Clamp(PlayerPrefs.GetInt($"UPG_{def.upgradeId}", 0), 0, def.maxLevel);
            }

            _levels[def.upgradeId] = lv;
        }

        SyncLevelsToPrefs();
        ReapplyRuntimeEffects();
        BroadcastAllCurrentLevels();
    }

    private UpgradeDefinition GetDef(UpgradeType type)
    {
        return upgradeDefs.Find(d => d && d.type == type);
    }

    private int GetLevel(string key)
    {
        return _levels.TryGetValue(key, out var lv) ? lv : 0;
    }

    private void SetLevel(string key, int lv)
    {
        if (string.IsNullOrEmpty(key)) return;

        var def = upgradeDefs.Find(d => d && d.upgradeId == key);
        if (!def) return;

        lv = Mathf.Clamp(lv, 0, def.maxLevel);
        _levels[key] = lv;

        PlayerPrefs.SetInt($"UPG_{key}", lv);
        PlayerPrefs.Save();

        ReapplyRuntimeEffects();

        OnLevelChanged?.Invoke(def.type, lv);
        OnAnyLevelChanged?.Invoke(def.type, lv);
    }

    private void ReapplyRuntimeEffects()
    {
        var supplyDef = GetDef(UpgradeType.SupplyPickupAmount);
        if (supplyDef)
        {
            int lv = GetLevel(supplyDef.upgradeId);
            supplyAmount = Mathf.Max(1, Mathf.RoundToInt(supplyDef.Evaluate(lv)));
        }
    }

    private void BroadcastAllCurrentLevels()
    {
        foreach (var def in upgradeDefs)
        {
            if (!def) continue;

            int lv = GetLevel(def.upgradeId);
            OnAnyLevelChanged?.Invoke(def.type, lv);
            OnLevelChanged?.Invoke(def.type, lv);
        }
    }

    private void SyncLevelsToPrefs()
    {
        foreach (var def in upgradeDefs)
        {
            if (!def) continue;

            int lv = GetLevel(def.upgradeId);
            PlayerPrefs.SetInt($"UPG_{def.upgradeId}", lv);
        }

        PlayerPrefs.Save();
    }

    public string GetUniqueID()
    {
        return uniqueID;
    }

    public string CaptureAsJson()
    {
        UpgradeManagerSaveData data = new UpgradeManagerSaveData();

        foreach (var def in upgradeDefs)
        {
            if (!def) continue;

            UpgradeLevelEntry entry = new UpgradeLevelEntry
            {
                upgradeId = def.upgradeId,
                level = GetLevel(def.upgradeId)
            };

            data.levels.Add(entry);
        }

        return JsonUtility.ToJson(data);
    }

    public void RestoreFromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
            return;

        UpgradeManagerSaveData data = JsonUtility.FromJson<UpgradeManagerSaveData>(json);
        if (data == null)
            return;

        _levels.Clear();

        foreach (var def in upgradeDefs)
        {
            if (!def) continue;
            _levels[def.upgradeId] = 0;
        }

        if (data.levels != null)
        {
            foreach (var entry in data.levels)
            {
                if (entry == null || string.IsNullOrEmpty(entry.upgradeId))
                    continue;

                var def = upgradeDefs.Find(d => d && d.upgradeId == entry.upgradeId);
                if (!def)
                    continue;

                _levels[entry.upgradeId] = Mathf.Clamp(entry.level, 0, def.maxLevel);
            }
        }

        SyncLevelsToPrefs();
        ReapplyRuntimeEffects();
        BroadcastAllCurrentLevels();

        Debug.Log("[Save] UpgradeManager 已還原");
    }
}