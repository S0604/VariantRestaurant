using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStatsUIManager : MonoBehaviour
{
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI popularityText;
    public TextMeshProUGUI customerFlowText;
    public Slider experienceSlider;

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
    void Start() => UpdateUI();

    void UpdateUI()
    {
        var pd = PlayerData.Instance;
        if (pd == null || pd.levelTable == null) return;

        var s = pd.stats;
        if (moneyText) moneyText.text = s.money.ToString();
        if (popularityText) popularityText.text = s.popularity.ToString();
        if (customerFlowText) customerFlowText.text = s.customerFlow.ToString();

        int need = pd.levelTable.GetRequiredExp(s.level);
        if (experienceSlider)
        {
            experienceSlider.maxValue = Mathf.Max(1, need);
            experienceSlider.value = Mathf.Clamp(s.experience, 0, need);
        }
    }
}
