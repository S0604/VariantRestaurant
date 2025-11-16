using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeRowUI_TMP : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text valueNowText;
    [SerializeField] private TMP_Text valueNextText;
    [SerializeField] private TMP_Text costText;

    [Header("Button")]
    [SerializeField] private Button upgradeButton;

    private UpgradeDefinition def;
    private UpgradeManager upgMgr;
    private UpgradeMenuControllerTMP menu;

    public void Setup(UpgradeDefinition definition, UpgradeManager mgr, UpgradeMenuControllerTMP m)
    {
        def = definition; upgMgr = mgr; menu = m;

        if (titleText) titleText.text = string.IsNullOrEmpty(def.displayName) ? def.upgradeId : def.displayName;
        if (descText) descText.text = def.description;

        if (upgMgr != null) upgMgr.OnLevelChanged += HandleLevelChanged;

        if (upgradeButton)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(OnClickUpgrade);
        }

        RefreshTexts();
        RefreshInteractable();
    }

    void OnDestroy()
    {
        if (upgMgr != null) upgMgr.OnLevelChanged -= HandleLevelChanged;
    }

    void HandleLevelChanged(UpgradeType type, int newLv)
    {
        if (!def || def.type != type) return;
        RefreshTexts();
        RefreshInteractable();
    }

    public void RefreshTexts()
    {
        if (!def || !upgMgr) return;

        int lv = upgMgr.GetLevel(def.type);
        int maxLv = def.maxLevel;

        float nowVal = upgMgr.GetValue(def.type);
        float nextVal = def.Evaluate(Mathf.Min(lv + 1, maxLv));

        if (levelText) levelText.text = $"Lv. {lv}/{maxLv}";
        if (valueNowText) valueNowText.text = $"Now: {FormatValue(nowVal)}";
        if (valueNextText) valueNextText.text = (lv >= maxLv) ? "Next: — (MAX)" : $"Next: {FormatValue(nextVal)}";

        if (costText) costText.text = (lv >= maxLv) ? "Cost: —" : $"Cost: {CalcCost(lv + 1)}";
    }

    public void RefreshInteractable()
    {
        if (!def || !upgMgr || upgradeButton == null) return;

        int lv = upgMgr.GetLevel(def.type);
        if (lv >= def.maxLevel) { upgradeButton.interactable = false; return; }

        int cost = CalcCost(lv + 1);
        var pd = PlayerData.Instance;
        bool can = (pd == null) ? false : pd.CanAfford(cost);

        upgradeButton.interactable = can;
    }

    void OnClickUpgrade()
    {
        if (!def || !upgMgr) return;

        int lv = upgMgr.GetLevel(def.type);
        if (lv >= def.maxLevel) return;

        int cost = CalcCost(lv + 1);
        var pd = PlayerData.Instance;
        if (pd == null || !pd.SpendMoney(cost)) return;

        // 真的升級
        upgMgr.SetLevel(def.type, lv + 1);

        // 顯示立即更新
        RefreshTexts();
        RefreshInteractable();
    }

    int CalcCost(int targetLevel)
    {
        // 成本： (base + add*(Lv-1)) * mul^(Lv-1)
        int steps = Mathf.Max(0, targetLevel - 1);
        float baseC = Mathf.Max(0, def.baseCost);
        float add = Mathf.Max(0, def.perLevelAdd);
        float mul = (def.perLevelMul <= 0f) ? 1f : def.perLevelMul;

        float v = (baseC + add * steps) * Mathf.Pow(mul, steps);
        return Mathf.CeilToInt(v);
    }

    string FormatValue(float v)
    {
        return string.IsNullOrEmpty(def.valueUnit) ? v.ToString("0.##") : $"{v:0.##}{def.valueUnit}";
    }
}
