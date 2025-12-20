using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance { get; private set; }

    [Header("小遊戲登記表")]
    public List<MinigameEntry> minigames = new List<MinigameEntry>();
    public Transform minigameContainer;

    [Header("玩家引用")]
    public Player player;

    private BaseMinigame currentMinigame;
    public bool IsPlaying => currentMinigame != null;

    /* ========== 鎖 & 對話標記 ========== */
    private static bool isSpawning = false;   // 正在生成 or 對話中
    private static bool hasSpawnedGame5Dialogue = false;
    /* ==================================== */

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [System.Serializable]
    public class MinigameEntry
    {
        public string minigameType;
        public string resourcePath;
    }

    /* --------------- 唯一入口 --------------- */
    public void StartMinigame(string type, System.Action<bool, int> onComplete)
    {
        if (currentMinigame != null || isSpawning) return;

        if (InventoryManager.Instance != null && InventoryManager.Instance.GetItemCount() >= 2)
        {
            Debug.LogWarning("背包已滿（≥2），無法開始小遊戲");
            return;
        }

        if (!hasSpawnedGame5Dialogue)
        {
            hasSpawnedGame5Dialogue = true;
            isSpawning = true;
            StartCoroutine(SpawnThenDialogue(type, onComplete));
            return;
        }

        isSpawning = true;
        ProceedToStartMinigame(type, onComplete);
    }

    /* =====================================================
       第一次特殊流程： 生成 → 對話（凍結遊戲但不凍結對話）→ 開始小遊戲
       ===================================================== */
    private IEnumerator SpawnThenDialogue(string type, System.Action<bool, int> onComplete)
    {
        yield return null; // 確保 isSpawning 生效

        var entry = minigames.Find(m => m.minigameType == type);
        if (entry == null) { isSpawning = false; yield break; }

        GameObject prefab = Resources.Load<GameObject>(entry.resourcePath);
        if (prefab == null) { isSpawning = false; yield break; }

        /* 1️⃣ 生成 */
        GameObject instance = Instantiate(prefab, minigameContainer);
        currentMinigame = instance.GetComponent<BaseMinigame>();
        if (currentMinigame == null)
        {
            Destroy(instance);
            isSpawning = false;
            yield break;
        }

        /* 2️⃣ 凍結世界，但 UI+對話不凍結 */
        Time.timeScale = 0f;

        /* 🔥 改為 realtime，不會因為 TimeScale=0 而卡死 */
        if (TutorialDialogueController.Instance != null)
            yield return TutorialDialogueController.Instance.PlaySingleChapter("5");

        /* 3️⃣ 恢復世界 */
        Time.timeScale = 1f;

        /* 4️⃣ 啟動小遊戲（正常時間流動） */
        currentMinigame.StartMinigame((success, rank) =>
        {
            onComplete?.Invoke(success, rank);
            StartCoroutine(DestroyAfterDelay(instance, 0.5f));
            currentMinigame = null;
        });

        isSpawning = false;
    }


    /* =====================================================
       非第一次的小遊戲啟動（不需要對話）
       ===================================================== */
    private void ProceedToStartMinigame(string type, System.Action<bool, int> onComplete)
    {
        var entry = minigames.Find(m => m.minigameType == type);
        if (entry == null) { isSpawning = false; return; }

        GameObject prefab = Resources.Load<GameObject>(entry.resourcePath);
        if (prefab == null) { isSpawning = false; return; }

        GameObject instance = Instantiate(prefab, minigameContainer);
        currentMinigame = instance.GetComponent<BaseMinigame>();
        if (currentMinigame == null)
        {
            Destroy(instance);
            isSpawning = false;
            return;
        }

        currentMinigame.StartMinigame((success, rank) =>
        {
            onComplete?.Invoke(success, rank);
            StartCoroutine(DestroyAfterDelay(instance, 0.5f));
            currentMinigame = null;
        });

        isSpawning = false;
    }

    /* =====================================================
       延遲銷毀（改用 unscaled）
       ===================================================== */
    private IEnumerator DestroyAfterDelay(GameObject obj, float delay)
    {
        float t = 0;
        while (t < delay)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        if (obj != null)
            Destroy(obj);
    }
}
