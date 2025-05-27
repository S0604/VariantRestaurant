using UnityEngine;
using System.Collections.Generic;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance { get; private set; }


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

}

