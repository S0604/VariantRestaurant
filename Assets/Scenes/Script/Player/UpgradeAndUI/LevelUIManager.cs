using UnityEngine;
using UnityEngine.UI;

public class LevelUIManager : MonoBehaviour
{
    [Header("UI References")]
    public Image levelIcon;
    public Slider experienceSlider;

    void OnEnable()
    {
        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.OnStatsChanged += UpdateLevelUI;
            PlayerData.Instance.OnLevelUp += _ => UpdateLevelUI();
        }
    }
    void OnDisable()
    {
        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.OnStatsChanged -= UpdateLevelUI;
            PlayerData.Instance.OnLevelUp -= _ => UpdateLevelUI();
        }
    }

    void Start() => UpdateLevelUI();

    public void UpdateLevelUI()
    {
        var pd = PlayerData.Instance;
        if (pd == null || pd.levelTable == null) return;

        var stats = pd.stats;
        var table = pd.levelTable;

        // 圖示
        if (levelIcon) levelIcon.sprite = table.GetLevelSprite(stats.level);

        // 經驗條
        int need = table.GetRequiredExp(stats.level);
        if (experienceSlider)
        {
            experienceSlider.maxValue = Mathf.Max(1, need); // 避免分母0
            experienceSlider.value = Mathf.Clamp(stats.experience, 0, need);
        }
    }
}
