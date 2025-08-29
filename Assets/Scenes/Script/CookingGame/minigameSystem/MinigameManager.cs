using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance { get; private set; }

    public List<MinigameEntry> minigames = new List<MinigameEntry>();
    public Transform minigameContainer;

    [Header("玩家控制")]
    public Player player;

    private BaseMinigame currentMinigame;
    public bool IsPlaying => currentMinigame != null;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [System.Serializable]
    public class MinigameEntry
    {
        public string minigameType;
        public string resourcePath;
    }

    public void StartMinigame(string type, System.Action<bool, int> onComplete)
    {
        
        Debug.Log($"啟動小遊戲時事件狀態：Active={RandomEventManager.Instance?.IsEventActive}, Effect={RandomEventManager.Instance?.CurrentEffect}");

        if (currentMinigame != null)
        {
            Debug.LogWarning("已有小遊戲在進行中！");
            return;
        }

        if (BaseMinigame.HasMaxDishRecords())
        {
            Debug.LogWarning("你已經有兩項料理紀錄，請先清除後再繼續！");
            return;
        }

        var entry = minigames.Find(m => m.minigameType == type);
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
            StartCoroutine(DestroyAfterDelay(instanceObj, 0.5f)); // 修正：從這裡執行 Coroutine
            currentMinigame = null;
        });
    }

    private IEnumerator DestroyAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(obj);
    }
}
