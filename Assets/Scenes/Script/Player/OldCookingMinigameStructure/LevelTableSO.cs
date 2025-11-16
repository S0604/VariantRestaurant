using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelData
{
    public int level;
    public int requiredExp;
    public Sprite levelSprite;
}

[CreateAssetMenu(fileName = "LevelTable", menuName = "Game/Level Table")]
public class LevelTableSO : ScriptableObject
{
    public List<LevelData> levels = new List<LevelData>();

    public LevelData GetLevelData(int level) => levels.Find(l => l.level == level);
    public int GetRequiredExp(int level) => GetLevelData(level)?.requiredExp ?? 0;
    public Sprite GetLevelSprite(int level) => GetLevelData(level)?.levelSprite;
    public int MaxLevel => levels.Count > 0 ? levels[^1].level : 1; // 最後一筆的 level
}

