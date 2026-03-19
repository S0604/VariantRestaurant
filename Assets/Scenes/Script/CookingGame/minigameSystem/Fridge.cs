using UnityEngine;
using UnityEngine.UI;

public class Fridge : MonoBehaviour
{
    // 冰箱提供的補給物品（ScriptableObject 或可實例化資源）
    public MenuItem supplyBoxItem;

    // UI icon 生成位置
    public Transform iconSpawnPoint;

    // UI icon prefab
    public GameObject iconPrefab;

    [Header("Highlight")]
    // 負責高亮顯示的控制器（外框材質切換）
    [SerializeField] private StationHighlighter_SwapOutlineMat highlighter;

    [Header("SFX (Success)")]
    // 音效來源
    [SerializeField] private AudioSource sfxSource;
    // 成功補給音效
    [SerializeField] private AudioClip successClip;
    // 音量
    [SerializeField, Range(0f, 1f)] private float successVolume = 1f;

    // 玩家是否在觸發範圍內
    private bool playerInRange = false;

    // 當前高亮狀態（避免重複設定）
    private bool highlightState = false;

    private void Awake()
    {
        // 若未手動指定 highlighter，從子物件尋找
        if (!highlighter)
            highlighter = GetComponentInChildren<StationHighlighter_SwapOutlineMat>(true);
    }

    private void Update()
    {
        // 玩家不在範圍內則不處理互動
        if (!playerInRange)
            return;

        // 根據背包狀態更新高亮
        RefreshHighlight();

        // 按下 E 進行互動
        if (Input.GetKeyDown(KeyCode.E))
        {
            // 嘗試補給物品
            bool success = TrySupplyBox();

            // 成功才播放音效
            if (success) PlaySuccessSfx();

            // 互動後立即刷新高亮狀態
            RefreshHighlight();
        }
    }

    // 播放補給成功音效
    private void PlaySuccessSfx()
    {
        if (sfxSource == null || successClip == null) return;
        sfxSource.PlayOneShot(successClip, successVolume);
    }

    // 更新高亮狀態
    private void RefreshHighlight()
    {
        bool shouldHighlight = false;

        var inv = InventoryManager.Instance;

        // 玩家在範圍內且背包為空才高亮
        if (playerInRange && inv != null)
        {
            shouldHighlight = inv.GetItemCount() == 0;
        }

        // 避免重複設定高亮狀態
        if (highlighter != null && highlightState != shouldHighlight)
        {
            highlightState = shouldHighlight;
            highlighter.SetHighlight(shouldHighlight);
        }
    }

    // 嘗試從冰箱取得補給物
    private bool TrySupplyBox()
    {
        var inv = InventoryManager.Instance;
        if (inv == null) return false;

        // 若背包不為空，禁止補給
        if (inv.GetItemCount() > 0)
        {
            Debug.Log("背包必須為空才能取得補給！");
            TutorialDialogueController.Instance?.PlayChapter("14_2");

            return false;
        }

        // 未設定補給物
        if (supplyBoxItem == null)
        {
            Debug.LogWarning("[Fridge] supplyBoxItem 未設定。");
            return false;
        }

        // 實例化補給物
        MenuItem itemInstance = Instantiate(supplyBoxItem);

        // 清空背包並加入新物品
        inv.ClearInventory();
        inv.AddItem(itemInstance);

        Debug.Log("已取得補給物，背包已更新");

        // 生成 UI icon
        SpawnSupplyIcon(itemInstance);

        return true;
    }

    // 在 UI 上生成補給物圖示
    private void SpawnSupplyIcon(MenuItem item)
    {
        if (iconPrefab == null || iconSpawnPoint == null || item == null)
        {
            Debug.LogWarning("Fridge icon 設定不完整");
            return;
        }

        // 在指定父物件下生成 icon
        GameObject iconObj = Instantiate(iconPrefab, iconSpawnPoint);

        // 設定 icon 圖片
        Image img = iconObj.GetComponent<Image>();
        if (img != null)
        {
            img.sprite = item.itemImage;
            img.color = Color.white;
        }
    }

    // 玩家進入觸發範圍
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        // 進入範圍時立即刷新高亮
        RefreshHighlight();
    }

    // 玩家離開觸發範圍
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        // 關閉高亮
        if (highlighter != null && highlightState)
        {
            highlightState = false;
            highlighter.SetHighlight(false);
        }
    }
}