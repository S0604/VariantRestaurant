using UnityEngine;
using System.Collections.Generic;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance { get; private set; }

    [System.Serializable]
    public class MinigameEntry
    {
        public string minigameType;
        public BaseMinigame minigamePrefab;
    }

    public List<MinigameEntry> minigames = new List<MinigameEntry>();
    public Transform minigameContainer;

    private BaseMinigame currentMinigame;

    void Awake()
    {
        // Singleton 實作
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 可選，如果希望它在場景切換時保留
    }

    public void StartMinigame(string type, System.Action<bool, int> onComplete)
    {
        if (currentMinigame != null)
        {
            Debug.LogWarning("已有小遊戲在進行中！");
            return;
        }

        MinigameEntry entry = minigames.Find(m => m.minigameType == type);
        if (entry == null)
        {
            Debug.LogError($"找不到對應小遊戲類型: {type}");
            return;
        }

        BaseMinigame instance = Instantiate(entry.minigamePrefab, minigameContainer);
        currentMinigame = instance;
        instance.StartMinigame((success, rank) =>
        {
            onComplete?.Invoke(success, rank);
            Destroy(instance.gameObject);
            currentMinigame = null;
        });
    }
}
