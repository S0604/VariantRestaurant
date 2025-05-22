using UnityEngine;
using System.Collections.Generic;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance { get; private set; }

    [Header("結果圖示管理")]
    public Transform dishDisplayContainer;
    public GameObject dishDisplayPrefab;
    public List<RankSpriteSet> rankSpriteSets = new List<RankSpriteSet>();



    [System.Serializable]
    public class RankSpriteSet
    {
        public string minigameType;
        public Sprite[] rankSprites; // index 0 = 失敗圖，1~3 = 成功圖
    }


    public List<MinigameEntry> minigames = new List<MinigameEntry>();
    public Transform minigameContainer;

    [Header("玩家控制")]
    public Player player; // <--- 新增這一行

    private BaseMinigame currentMinigame;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // 視情況保留
    }

    [System.Serializable]
    public class MinigameEntry
    {
        public string minigameType;      // 例如 "Burger" 或 "Fries"
        public string resourcePath;      // 例如 "Minigames/BurgerMinigame"
    }

    private Sprite[] GetSpritesForType(string type)
    {
        RankSpriteSet set = rankSpriteSets.Find(s => s.minigameType == type);
        return set != null ? set.rankSprites : null;
    }

    public void StartMinigame(string type, System.Action<bool, int> onComplete)
    {
        if (currentMinigame != null)
        {
            Debug.LogWarning("已有小遊戲在進行中！");
            return;
        }

        //限制料理紀錄上限
        if (BaseMinigame.HasMaxDishRecords())
        {
            Debug.LogWarning("你已經有兩項料理紀錄，請先清除後再繼續！");
            return;
        }

        MinigameEntry entry = minigames.Find(m => m.minigameType == type);
        if (entry == null)
        {
            Debug.LogError($"找不到對應小遊戲類型: {type}");
            return;
        }

        GameObject prefab = Resources.Load<GameObject>(entry.resourcePath);
        if (prefab == null)
        {
            Debug.LogError($"無法從 Resources 載入 prefab: {entry.resourcePath}");
            return;
        }

        GameObject instanceObj = Instantiate(prefab, minigameContainer);
        currentMinigame = instanceObj.GetComponent<BaseMinigame>();
        Sprite[] sprites = GetSpritesForType(type);
        currentMinigame.InitializeDisplay(type, dishDisplayContainer, dishDisplayPrefab, sprites);

        if (currentMinigame == null)
        {
            Debug.LogError("載入的小遊戲 prefab 缺少 BaseMinigame 組件！");
            Destroy(instanceObj);
            return;
        }

        currentMinigame.StartMinigame((success, rank) =>
        {
            onComplete?.Invoke(success, rank);
            Destroy(instanceObj);
            currentMinigame = null;
        });
    }

    public void RefreshDishDisplay()
    {
        if (dishDisplayContainer == null || dishDisplayPrefab == null) return;

        foreach (Transform child in dishDisplayContainer)
        {
            Destroy(child.gameObject);
        }

        // 因為已經清空 completedDishes，所以不需要重建任何圖示
        Debug.Log("Manager 收到清除通知，已刷新畫面！");
    }



}

