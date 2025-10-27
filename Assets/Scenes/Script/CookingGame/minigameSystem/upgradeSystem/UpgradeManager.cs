using System.Collections.Generic;
using UnityEngine;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager Instance;

    [Header("升級清單（在 Inspector 指定）")]
    public List<UpgradeDefinition> upgradeDefs = new();

    // 相容欄位（保留舊系統可用）
    [Header("相容欄位")]
    public int supplyAmount = 3;

    private readonly Dictionary<string, int> _levels = new();

    private void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        foreach (var def in upgradeDefs)
        {
            if (!def) continue;
            int lv = PlayerPrefs.GetInt($"UPG_{def.upgradeId}", 0);
            _levels[def.upgradeId] = Mathf.Clamp(lv, 0, def.maxLevel);
        }

        var supplyDef = GetDef(UpgradeType.SupplyPickupAmount);
        if (supplyDef)
            supplyAmount = Mathf.Max(1, Mathf.RoundToInt(supplyDef.Evaluate(GetLevel(supplyDef.upgradeId))));
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

        supplyAmount = Mathf.Max(1, Mathf.RoundToInt(def.Evaluate(GetLevel(def.upgradeId))));
        Debug.Log($"補給量更新：{supplyAmount}（Lv.{GetLevel(def.upgradeId)}）");
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

        if (def.type == UpgradeType.SupplyPickupAmount)
            supplyAmount = Mathf.Max(1, Mathf.RoundToInt(def.Evaluate(lv)));
    }
}
