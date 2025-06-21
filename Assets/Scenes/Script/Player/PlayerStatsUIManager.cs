using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUIManager : MonoBehaviour
{
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI popularityText;
    public TextMeshProUGUI customerFlowText;

    public Slider experienceSlider;  // ✅ 經驗條

    private LevelUIManager levelUIManager;

    void Awake()
    {
        levelUIManager = FindObjectOfType<LevelUIManager>();
        if (levelUIManager == null)
        {
            Debug.LogError("找不到 LevelUIManager！");
        }
    }

    void OnEnable()
    {
        if (PlayerData.Instance != null)
            PlayerData.Instance.OnStatsChanged += UpdateUI;
    }

    void OnDisable()
    {
        if (PlayerData.Instance != null)
            PlayerData.Instance.OnStatsChanged -= UpdateUI;
    }

    void Start()
    {
        UpdateUI(); // 初始化顯示
    }

    void UpdateUI()
    {
        if (PlayerData.Instance == null || levelUIManager == null)
            return;

        var stats = PlayerData.Instance.stats;

        moneyText.text = $"{stats.money}";
        popularityText.text = $"{stats.popularity}";
        customerFlowText.text = $"{stats.customerFlow}";

        int requiredExp = levelUIManager.GetRequiredExpForLevel(stats.level);
        experienceSlider.maxValue = requiredExp;
        experienceSlider.value = stats.experience;
    }
}