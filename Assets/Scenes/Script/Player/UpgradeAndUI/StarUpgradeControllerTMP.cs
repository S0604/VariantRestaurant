using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StarUpgradeControllerTMP : MonoBehaviour
{
    [Header("Unlock Requirements (Level)")]
    [SerializeField] private int requiredLevel = 5;

    [Header("Popularity Requirement (per Star)")]
    [SerializeField] private int basePopularityReq = 50;
    [SerializeField] private int popularityAddPerStar = 0;
    [SerializeField] private float popularityMulPerStar = 1.0f;

    [Header("Cost (per Star)")]
    [SerializeField] private int baseCost = 500;
    [SerializeField] private int addCostPerStar = 250;
    [SerializeField] private float mulCostPerStar = 1.0f;

    [Header("Limit (Optional)")]
    [SerializeField] private int maxStars = 5; // 0/負數 = 不限制

    [Header("UI References")]
    [SerializeField] private Button upgradeButton;

    [Header("UI Texts (Independent)")]
    [SerializeField] private TMP_Text starsText;            // 可選：你可以留空或讓它顯示別的訊息
    [SerializeField] private TMP_Text levelReqText;
    [SerializeField] private TMP_Text popularityReqText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text buttonLabel;

    [Header("Stars Display (Per-Star Images)")]
    [Tooltip("依序放入每一顆星的 Image：Element 0=第1顆，Element 1=第2顆...")]
    [SerializeField] private Image[] starImages;

    [Header("Requirement Icons (optional)")]
    [SerializeField] private Image levelReqIcon;
    [SerializeField] private Image popularityReqIcon;
    [SerializeField] private Image moneyReqIcon;
    [SerializeField] private Image maxReqIcon;

    [Header("Requirement Icon Sprites")]
    [SerializeField] private Sprite levelNotMetSprite;
    [SerializeField] private Sprite levelMetSprite;

    [SerializeField] private Sprite popularityNotMetSprite;
    [SerializeField] private Sprite popularityMetSprite;

    [SerializeField] private Sprite moneyNotMetSprite;
    [SerializeField] private Sprite moneyMetSprite;

    [SerializeField] private Sprite notMaxSprite;
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

        // Stars display: Per-star images
        UpdateStarsImages(stars);

        // （可選）Stars text：你可以不顯示文字
        if (starsText) starsText.text = "";

        if (levelReqText)
            levelReqText.text = $"等級需求 {pd.stats.level}/{requiredLevel}";

        if (popularityReqText)
            popularityReqText.text = $"人氣需求 {pd.stats.popularity}/{needPopularity}";

        if (costText)
            costText.text = isMax ? "" : $"{needCost}";

        SetReqIcon(levelReqIcon, levelOk, levelMetSprite, levelNotMetSprite);
        SetReqIcon(popularityReqIcon, popularityOk, popularityMetSprite, popularityNotMetSprite);

        bool moneyState = isMax ? true : moneyOk;
        SetReqIcon(moneyReqIcon, moneyState, moneyMetSprite, moneyNotMetSprite);

        if (maxReqIcon != null)
        {
            if (isMaxSprite != null || notMaxSprite != null)
            {
                maxReqIcon.sprite = isMax ? isMaxSprite : notMaxSprite;
                maxReqIcon.enabled = maxReqIcon.sprite != null;
            }
        }

        if (upgradeButton)
            upgradeButton.interactable = !isMax && levelOk && popularityOk && moneyOk;

        if (buttonLabel)
        {
            if (isMax) buttonLabel.text = "已達最高等級";
            else if (!levelOk) buttonLabel.text = "等級不足";
            else if (!popularityOk) buttonLabel.text = "人氣不足";
            else if (!moneyOk) buttonLabel.text = "餘額不足";
            else buttonLabel.text = "升級";
        }
    }

    private void UpdateStarsImages(int stars)
    {
        if (starImages == null || starImages.Length == 0) return;

        // stars=0 -> 全關；stars=1 -> 顯示[0]；stars=2 -> 顯示[0..1]
        for (int i = 0; i < starImages.Length; i++)
        {
            if (starImages[i] == null) continue;
            starImages[i].enabled = (stars > 0 && i < stars);
        }
    }

    private void SetReqIcon(Image icon, bool met, Sprite metSprite, Sprite notMetSprite)
    {
        if (icon == null) return;

        Sprite s = met ? metSprite : notMetSprite;
        if (s != null)
        {
            icon.sprite = s;
            icon.enabled = true;
        }
    }

    private void TryUpgradeStar()
    {
        var pd = PlayerData.Instance;
        if (pd == null) return;

        int stars = pd.stats.stars;
        if (maxStars > 0 && stars >= maxStars) return;

        int targetStar = stars + 1;

        if (pd.stats.level < requiredLevel) return;

        int needPopularity = CalcPopularityReq(targetStar);
        if (pd.stats.popularity < needPopularity) return;

        int cost = CalcCost(targetStar);
        if (!pd.SpendMoney(cost)) return;

        pd.AddStars(1);
        RefreshUI();
    }

    private int CalcPopularityReq(int targetStar)
    {
        int step = Mathf.Max(0, targetStar - 1);
        float linear = basePopularityReq + popularityAddPerStar * step;
        float mul = (popularityMulPerStar <= 0f) ? 1f : Mathf.Pow(popularityMulPerStar, step);
        return Mathf.Max(0, Mathf.CeilToInt(linear * mul));
    }

    private int CalcCost(int targetStar)
    {
        int step = Mathf.Max(0, targetStar - 1);
        float linear = baseCost + addCostPerStar * step;
        float mul = (mulCostPerStar <= 0f) ? 1f : Mathf.Pow(mulCostPerStar, step);
        return Mathf.Max(0, Mathf.CeilToInt(linear * mul));
    }
}