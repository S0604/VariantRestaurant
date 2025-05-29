using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class LevelData
{
    public int level;
    public int requiredExp;
    public Sprite levelSprite;
}

public class LevelUIManager : MonoBehaviour
{
    [Header("UI References")]
    public Image levelIcon;               // ✅ 等級圖示（Image）
    public Slider experienceSlider;       // ✅ 經驗條（Slider）

    [Header("Level Data Table")]
    public List<LevelData> levels = new List<LevelData>();

    void OnEnable()
    {
        if (PlayerData.Instance != null)
            PlayerData.Instance.OnStatsChanged += UpdateLevelUI;
    }

    void OnDisable()
    {
        if (PlayerData.Instance != null)
            PlayerData.Instance.OnStatsChanged -= UpdateLevelUI;
    }

    void Start()
    {
        UpdateLevelUI();
    }

    public void UpdateLevelUI()
    {
        var stats = PlayerData.Instance.stats;
        var levelInfo = GetLevelData(stats.level);

        if (levelInfo != null)
        {
            if (levelIcon != null)
                levelIcon.sprite = levelInfo.levelSprite;

            if (experienceSlider != null)
            {
                experienceSlider.maxValue = levelInfo.requiredExp;
                experienceSlider.value = stats.experience;
            }
        }
        else
        {
            Debug.LogWarning($"等級 {stats.level} 未找到對應資料！");
        }
    }

    public LevelData GetLevelData(int level)
    {
        return levels.Find(l => l.level == level);
    }

    public int GetRequiredExpForLevel(int level)
    {
        var data = GetLevelData(level);
        return data != null ? data.requiredExp : 0;
    }
}
