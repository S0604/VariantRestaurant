using System.Collections.Generic;
using UnityEngine;
using System;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;
    public event Action<UpgradeType, int> OnLevelChanged;

    [Header("升級清單（在 Inspector 指定）")]
    public List<UpgradeDefinition> upgradeDefs = new();

    // 相容欄位（保留舊系統可用）
    [Header("相容欄位")]
    public int supplyAmount = 3;                 // 取得補給數量（舊系統用）

    private readonly Dictionary<string, int> _levels = new();

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        // 載入每個升級的等級
        foreach (var def in upgradeDefs)
        {
            if (!def) continue;
            int lv = PlayerPrefs.GetInt($"UPG_{def.upgradeId}", 0);
            _levels[def.upgradeId] = Mathf.Clamp(lv, 0, def.maxLevel);
        }

        // 統一重算一次所有生效數值
        ReapplyRuntimeEffects();
    }

    // === 舊 API：保留相容 ===
    public void UpgradeCookingStation(CookingStation station, int amount)
    {
        if (!station) return;
        station.UpgradeEnergy(amount);
        Debug.Log($"工作站能量上限增加：+{amount}");
    }

    public void UpgradeSupplyAmount(int amount)
    {
        var def = GetDef(UpgradeType.SupplyPickupAmount);
        if (!def)
        {
            supplyAmount += amount;
            Debug.Log($"補給量增加，目前為：{supplyAmount}（無定義）");
            return;
        }

        int lv = GetLevel(def.upgradeId) + amount;
        SetLevel(def.upgradeId, lv);
    }

    // === 新查值 API（推薦使用）===
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

    // === 重置 API（做法1，無退款）===
    public void ResetUpgrade(UpgradeType type)
    {
        var def = GetDef(type);
        if (!def) return;
        SetLevel(def.upgradeId, 0);
    }

    public void ResetAllUpgrades()
    {
        foreach (var def in upgradeDefs)
        {
            if (!def) continue;
            _levels[def.upgradeId] = 0;
            PlayerPrefs.SetInt($"UPG_{def.upgradeId}", 0);
            OnLevelChanged?.Invoke(def.type, 0);
        }
        ReapplyRuntimeEffects();
    }

    [ContextMenu("Reset All Upgrades")]
    private void _Context_ResetAll() => ResetAllUpgrades();

    // --- 私有工具 ---
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

        // 統一重算所有生效數值
        ReapplyRuntimeEffects();

        // 廣播該型別變更（列 UI 會自動刷新）
        OnLevelChanged?.Invoke(def.type, lv);
    }

    /// <summary>
    /// 將所有升級的等級→實際生效數值，於此集中計算。
    /// 目前僅示範相容欄位 supplyAmount；未來你可以在這裡加入其它效果（能量上限、經濟倍率等）。
    /// </summary>
    private void ReapplyRuntimeEffects()
    {
        // 預設值先回填
        supplyAmount = 3;

        // 把每個升級的 Evaluate 套回系統
        foreach (var def in upgradeDefs)
        {
            if (!def) continue;
            int lv = GetLevel(def.upgradeId);
            if (lv <= 0) continue;

            float v = def.Evaluate(lv);

            switch (def.type)
            {
                case UpgradeType.SupplyPickupAmount:
                    supplyAmount = Mathf.Max(1, Mathf.RoundToInt(v));
                    break;

                // 未來你可以在這裡加入更多型別：
                // case UpgradeType.WorkbenchMaxEnergy:
                //     var maxEnergy = Mathf.RoundToInt(v);
                //     foreach (var s in FindObjectsOfType<CookingStation>())
                //     {
                //         s.maxEnergy = maxEnergy;
                //         s.UpdateEnergyUI();
                //     }
                //     break;

                default:
                    break;
            }
        }
    }
}
