using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StarUpgradeControllerTMP : MonoBehaviour
{
    [Header("Unlock Requirements (Level)")]
    [SerializeField] private int requiredLevel = 5;

    [Header("Popularity Requirement (per Star)")]
    [SerializeField] private int basePopularityReq = 50;        // 第 1 顆星需要的人氣
    [SerializeField] private int popularityAddPerStar = 0;      // 每顆星增加的人氣需求
    [SerializeField] private float popularityMulPerStar = 1.0f; // >1 代表指數成長；=1 不倍增；<=0 視為 1

    [Header("Cost (per Star)")]
    [SerializeField] private int baseCost = 500;          // 第 1 顆星成本
    [SerializeField] private int addCostPerStar = 250;    // 每顆星增加
    [SerializeField] private float mulCostPerStar = 1.0f; // >1 指數成長；=1 不倍增；<=0 視為 1

    [Header("Limit (Optional)")]
    [SerializeField] private int maxStars = 5;            // 0/負數 = 不限制

    [Header("UI References")]
    [SerializeField] private Button upgradeButton;

    [Header("UI Texts (Independent)")]
    [SerializeField] private TMP_Text starsText;            // Stars: x/y
    [SerializeField] private TMP_Text levelReqText;         // Level requirement text (independent)
    [SerializeField] private TMP_Text popularityReqText;    // Popularity requirement text (independent)
    [SerializeField] private TMP_Text costText;             // Cost text
    [SerializeField] private TMP_Text buttonLabel;          // Button label (optional)

    [Header("Requirement Icons (optional)")]
    [Tooltip("等級條件圖示 Image")]
    [SerializeField] private Image levelReqIcon;
    [Tooltip("人氣條件圖示 Image")]
    [SerializeField] private Image popularityReqIcon;
    [Tooltip("金錢條件圖示 Image")]
    [SerializeField] private Image moneyReqIcon;
    [Tooltip("已達最大星等（或可升級）圖示 Image")]
    [SerializeField] private Image maxReqIcon;

    [Header("Requirement Icon Sprites")]
    [Tooltip("未達成等級條件時顯示")]
    [SerializeField] private Sprite levelNotMetSprite;
    [Tooltip("達成等級條件時顯示")]
    [SerializeField] private Sprite levelMetSprite;

    [Tooltip("未達成人氣條件時顯示")]
    [SerializeField] private Sprite popularityNotMetSprite;
    [Tooltip("達成人氣條件時顯示")]
    [SerializeField] private Sprite popularityMetSprite;

    [Tooltip("未達成金錢條件時顯示")]
    [SerializeField] private Sprite moneyNotMetSprite;
    [Tooltip("達成金錢條件時顯示")]
    [SerializeField] private Sprite moneyMetSprite;

    [Tooltip("未達最大星等（仍可升級）時顯示")]
    [SerializeField] private Sprite notMaxSprite;
    [Tooltip("已達最大星等時顯示")]
    [SerializeField] private Sprite isMaxSprite;

    private void OnEnable()
    {
        if (PlayerData.Instance != null)
            PlayerData.Instance.OnStatsChanged += RefreshUI;

        RefreshUI();
    }

    private void OnDisable()
    {
        if (PlayerData.Instance != null)
            PlayerData.Instance.OnStatsChanged -= RefreshUI;
    }

    private void Start()
    {
        if (upgradeButton)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(TryUpgradeStar);
        }

        RefreshUI();
    }

    private void RefreshUI()
    {
        var pd = PlayerData.Instance;
        if (pd == null) return;

        int stars = pd.stats.stars;
        bool isMax = (maxStars > 0 && stars >= maxStars);

        int targetStar = stars + 1;

        int needPopularity = CalcPopularityReq(targetStar);
        int needCost = CalcCost(targetStar);

        bool levelOk = pd.stats.level >= requiredLevel;
        bool popularityOk = pd.stats.popularity >= needPopularity;
        bool moneyOk = pd.CanAfford(needCost);

        // Stars text
        if (starsText)
            starsText.text = (maxStars > 0) ? $"Stars: {stars}/{maxStars}" : $"Stars: {stars}";

        // Level requirement text (independent)
        if (levelReqText)
            levelReqText.text = $"等級需求 {pd.stats.level}/{requiredLevel}";

        // Popularity requirement text (independent)
        if (popularityReqText)
            popularityReqText.text = $"人氣需求 {pd.stats.popularity}/{needPopularity}";

        // Cost text
        if (costText)
            costText.text = isMax ? "" : $"{needCost}";

        // Icons: update per condition
        SetReqIcon(levelReqIcon, levelOk, levelMetSprite, levelNotMetSprite);
        SetReqIcon(popularityReqIcon, popularityOk, popularityMetSprite, popularityNotMetSprite);

        // 金錢條件：如果已達 max，通常不需要再顯示缺錢/達成，但你可選擇仍顯示「達成」
        // 這裡採用：isMax 時也顯示 moneyMetSprite（若有），避免看起來像缺錢卡住
        bool moneyState = isMax ? true : moneyOk;
        SetReqIcon(moneyReqIcon, moneyState, moneyMetSprite, moneyNotMetSprite);

        // Max icon：isMax true/false
        if (maxReqIcon != null)
        {
            if (isMaxSprite == null && notMaxSprite == null)
            {
                // 若沒設定 sprite，不做事
            }
            else
            {
                maxReqIcon.sprite = isMax ? isMaxSprite : notMaxSprite;
                maxReqIcon.enabled = maxReqIcon.sprite != null;
            }
        }

        // Button interactable
        if (upgradeButton)
            upgradeButton.interactable = !isMax && levelOk && popularityOk && moneyOk;

        // Button label (optional)
        if (buttonLabel)
        {
            if (isMax) buttonLabel.text = "已達最高等級";
            else if (!levelOk) buttonLabel.text = "等級不足";
            else if (!popularityOk) buttonLabel.text = "人氣不足";
            else if (!moneyOk) buttonLabel.text = "餘額不足";
            else buttonLabel.text = "升級";
        }
    }

    private void SetReqIcon(Image icon, bool met, Sprite metSprite, Sprite notMetSprite)
    {
        if (icon == null) return;

        Sprite s = met ? metSprite : notMetSprite;

        // 若沒有提供對應 sprite，就不改變現有 sprite（避免被清空）
        if (s != null)
        {
            icon.sprite = s;
            icon.enabled = true;
        }
        else
        {
            // 如果你希望「沒設定 sprite 就隱藏」，把這行打開：
            // icon.enabled = false;
        }
    }

    private void TryUpgradeStar()
    {
        var pd = PlayerData.Instance;
        if (pd == null) return;

        int stars = pd.stats.stars;
        if (maxStars > 0 && stars >= maxStars) return;

        int targetStar = stars + 1;

        // 再檢查一次需求（避免 UI 未刷新）
        if (pd.stats.level < requiredLevel) return;

        int needPopularity = CalcPopularityReq(targetStar);
        if (pd.stats.popularity < needPopularity) return;

        int cost = CalcCost(targetStar);
        if (!pd.SpendMoney(cost)) return;

        pd.AddStars(1);
        RefreshUI();
    }

    // 需求： (base + add*(Star-1)) * mul^(Star-1)
    private int CalcPopularityReq(int targetStar)
    {
        int step = Mathf.Max(0, targetStar - 1);
        float linear = basePopularityReq + popularityAddPerStar * step;
        float mul = (popularityMulPerStar <= 0f) ? 1f : Mathf.Pow(popularityMulPerStar, step);
        return Mathf.Max(0, Mathf.CeilToInt(linear * mul));
    }

    // 成本： (base + add*(Star-1)) * mul^(Star-1)
    private int CalcCost(int targetStar)
    {
        int step = Mathf.Max(0, targetStar - 1);
        float linear = baseCost + addCostPerStar * step;
        float mul = (mulCostPerStar <= 0f) ? 1f : Mathf.Pow(mulCostPerStar, step);
        return Mathf.Max(0, Mathf.CeilToInt(linear * mul));
    }
}
