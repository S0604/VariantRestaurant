using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    [Header("Text UI")]
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI popularityText;
    public TextMeshProUGUI customerFlowText;

    [Header("Level UI")]
    public Image levelIcon;
    public Slider experienceSlider;

    private System.Action<int> levelUpCallback;

    void OnEnable()
    {
        if (PlayerData.Instance == null) return;

        PlayerData.Instance.OnStatsChanged += UpdateUI;

        levelUpCallback = _ => UpdateUI();
        PlayerData.Instance.OnLevelUp += levelUpCallback;

        // 如果你未來要星星UI即時更新
        PlayerData.Instance.OnStarsChanged += _ => UpdateUI();
    }

    void OnDisable()
    {
        if (PlayerData.Instance == null) return;

        PlayerData.Instance.OnStatsChanged -= UpdateUI;

        if (levelUpCallback != null)
            PlayerData.Instance.OnLevelUp -= levelUpCallback;

        PlayerData.Instance.OnStarsChanged -= _ => UpdateUI();
    }

    void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        var pd = PlayerData.Instance;
        if (pd == null || pd.levelTable == null) return;

        var s = pd.stats;
        var table = pd.levelTable;

        if (moneyText) moneyText.text = s.money.ToString();
        if (popularityText) popularityText.text = s.popularity.ToString();
        if (customerFlowText) customerFlowText.text = s.customerFlow.ToString();

        if (levelIcon)
            levelIcon.sprite = table.GetLevelSprite(s.level);

        int need = table.GetRequiredExp(s.level);

        if (experienceSlider)
        {
            experienceSlider.maxValue = Mathf.Max(1, need);
            experienceSlider.value = Mathf.Clamp(s.experience, 0, need);
        }
    }
}