using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    public static event Action<UpgradeType, int> OnAnyLevelChanged;
    public event Action<UpgradeType, int> OnLevelChanged;

    [Header("升級清單（在 Inspector 指定）")]
    public List<UpgradeDefinition> upgradeDefs = new();

    [Header("相容欄位（舊系統可用）")]
    public int supplyAmount = 3;

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

        LoadLevelsFromPrefs();
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

    private void LoadLevelsFromPrefs()
    {
        _levels.Clear();

        foreach (var def in upgradeDefs)
        {
            if (!def) continue;

            int lv = PlayerPrefs.GetInt($"UPG_{def.upgradeId}", 0);
            _levels[def.upgradeId] = Mathf.Clamp(lv, 0, def.maxLevel);
        }
    }

    // =========================
    //  Public API
    // =========================

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

    /// <summary>
    /// 重置單一升級（不做退款，只把等級回到 0）
    /// </summary>
    public void ResetUpgrade(UpgradeType type)
    {
        var def = GetDef(type);
        if (!def) return;
        SetLevel(def.upgradeId, 0);
    }

    /// <summary>
    /// 以 upgradeId 重置（給特定 UI 或 debug 用）
    /// </summary>
    public void ResetUpgrade(string upgradeId)
    {
        if (string.IsNullOrEmpty(upgradeId)) return;

        var def = upgradeDefs.Find(d => d && d.upgradeId == upgradeId);
        if (!def) return;

        SetLevel(def.upgradeId, 0);
    }

    /// <summary>
    /// 重置所有升級（不做退款）
    /// </summary>
    public void ResetAllUpgrades()
    {
        foreach (var def in upgradeDefs)
        {
            if (!def) continue;
            SetLevel(def.upgradeId, 0);
        }
    }

    /// <summary>
    /// 若你在 Inspector 改了 upgradeDefs 或 upgradeId，需要重新載入一次
    /// </summary>
    public void RebuildFromDefinitions()
    {
        LoadLevelsFromPrefs();
        ReapplyRuntimeEffects();
        BroadcastAllCurrentLevels();
    }

    // =========================
    //  Internal
    // =========================

    private UpgradeDefinition GetDef(UpgradeType type) =>
        upgradeDefs.Find(d => d && d.type == type);

    private int GetLevel(string key) =>
        _levels.TryGetValue(key, out var lv) ? lv : 0;

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
        // 舊系統相容：supplyAmount 仍維持可用
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
        }
    }
}
