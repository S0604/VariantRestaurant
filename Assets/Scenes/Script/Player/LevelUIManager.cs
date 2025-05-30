using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    public int requiredExp;
    public Sprite levelIcon; // 若 UI 需要等級圖示
}

public class LevelUIManager : MonoBehaviour
{
    public static LevelUIManager Instance { get; private set; }

    public List<LevelData> levels = new List<LevelData>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // ✅ 主動注入給 PlayerData（若已初始化）
        if (PlayerData.Instance != null)
        {
            PlayerData.Instance.InjectLevelUIManager(this);
        }
    }

    public int GetRequiredExpForLevel(int level)
    {
        if (level < 0 || level >= levels.Count)
        {
            Debug.LogWarning($"等級 {level} 超出等級資料表範圍！");
            return int.MaxValue; // 防止誤升級
        }
        return levels[level].requiredExp;
    }
}
