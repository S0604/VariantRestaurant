using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

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
    private static bool isSpawning = false;
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
<<<<<<< Updated upstream
<<<<<<< Updated upstream
        
        Debug.Log($"�Ұʤp�C���ɨƥ󪬺A�GActive={RandomEventManager.Instance?.IsEventActive}, Effect={RandomEventManager.Instance?.CurrentEffect}");

        if (currentMinigame != null)
=======
=======
>>>>>>> Stashed changes
        if (currentMinigame != null || isSpawning) return;

        if (InventoryManager.Instance != null && InventoryManager.Instance.GetItemCount() >= 2)
        { Debug.LogWarning("背包已滿，無法開始小遊戲"); return; }

        if (!hasSpawnedGame5Dialogue)
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes
        {
            hasSpawnedGame5Dialogue = true;
            isSpawning = true;
            StartCoroutine(SpawnThenDialogue(type, onComplete));
            return;
        }

<<<<<<< Updated upstream
<<<<<<< Updated upstream
        if (BaseMinigame.HasMaxDishRecords())
        {
            Debug.LogWarning("�A�w�g���ⶵ�Ʋz�����A�Х��M����A�~��I");
            return;
        }
=======
=======
>>>>>>> Stashed changes
        isSpawning = true;
        ProceedToStartMinigame(type, onComplete);
    }

    /* 生成 → 立即對話 → 解凍後才跑回呼 */
    private IEnumerator SpawnThenDialogue(string type, System.Action<bool, int> onComplete)
    {
        yield return null; // 等一帧，確保外部 isSpawning 生效
<<<<<<< Updated upstream
>>>>>>> Stashed changes
=======
>>>>>>> Stashed changes

        var entry = minigames.Find(m => m.minigameType == type);
        if (entry == null) { isSpawning = false; yield break; }

        GameObject prefab = Resources.Load<GameObject>(entry.resourcePath);
        if (prefab == null) { isSpawning = false; yield break; }

        /* 1️⃣ 瞬間生成到場景 */
        GameObject instance = Instantiate(prefab, minigameContainer);
        currentMinigame = instance.GetComponent<BaseMinigame>();
        if (currentMinigame == null)
        { Destroy(instance); isSpawning = false; yield break; }

        /* 2️⃣ 生成後「下一行」立即凍結+對話 */
        Time.timeScale = 0f;
        if (TutorialDialogueController.Instance != null)
            yield return TutorialDialogueController.Instance.PlaySingleChapter("5");
        Time.timeScale = 1f;

        /* 3️⃣ 註冊結束回呼 */
        currentMinigame.StartMinigame((success, rank) =>
        {
            onComplete?.Invoke(success, rank);
            StartCoroutine(DestroyAfterDelay(instance, 0.5f));
            currentMinigame = null;
        });

        isSpawning = false;
    }

    /* 非第一次的生成 */
    private void ProceedToStartMinigame(string type, System.Action<bool, int> onComplete)
    {
        var entry = minigames.Find(m => m.minigameType == type);
        if (entry == null) { isSpawning = false; return; }

        GameObject prefab = Resources.Load<GameObject>(entry.resourcePath);
        if (prefab == null) { isSpawning = false; return; }

        GameObject instance = Instantiate(prefab, minigameContainer);
        currentMinigame = instance.GetComponent<BaseMinigame>();
        if (currentMinigame == null)
        { Destroy(instance); isSpawning = false; return; }

        currentMinigame.StartMinigame((success, rank) =>
        {
            onComplete?.Invoke(success, rank);
<<<<<<< Updated upstream
<<<<<<< Updated upstream
            StartCoroutine(DestroyAfterDelay(instanceObj, 0.5f)); // �ץ��G�q�o�̰��� Coroutine
=======
            StartCoroutine(DestroyAfterDelay(instance, 0.5f));
>>>>>>> Stashed changes
=======
            StartCoroutine(DestroyAfterDelay(instance, 0.5f));
>>>>>>> Stashed changes
            currentMinigame = null;
        });

        isSpawning = false;
    }

    private IEnumerator DestroyAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(obj);
    }
}