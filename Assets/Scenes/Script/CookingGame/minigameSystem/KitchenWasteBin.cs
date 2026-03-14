using UnityEngine;

public class KitchenWasteBin : MonoBehaviour
{
    [Header("補給箱 UI 清除設定")]
    public Transform iconSpawnPoint;

    [Header("Highlight")]
    [SerializeField] private StationHighlighter_SwapOutlineMat highlighter;

    [Header("SFX (Success)")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip successClip;
    [SerializeField, Range(0f, 1f)] private float successVolume = 1f;

    private bool isPlayerNearby = false;

    /* 🔒 只播一次 6_3 */
    private static bool hasClearedOnce = false;

    private void Awake()
    {
        if (!highlighter)
            highlighter = GetComponentInChildren<StationHighlighter_SwapOutlineMat>(true);
    }

    private void PlaySuccessSfx()
    {
        if (sfxSource == null || successClip == null) return;
        sfxSource.PlayOneShot(successClip, successVolume);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (highlighter) highlighter.SetHighlight(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (highlighter) highlighter.SetHighlight(false);
        }
    }

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            bool didClear = false;

            if (InventoryManager.Instance != null)
            {
                // 只有背包真的有東西時，才視為成功清空（你也可以改成永遠成功）
                if (InventoryManager.Instance.GetItemCount() > 0)
                {
                    InventoryManager.Instance.ClearInventory();
                    didClear = true;
                }
            }

            bool didClearUI = ClearSupplyUI();
            didClear = didClear || didClearUI;

            if (didClear)
            {
                PlaySuccessSfx();
                Debug.Log("廚餘桶已清空玩家背包與補給 UI！");
            }
            else
            {
                Debug.Log("廚餘桶：背包與補給 UI 都是空的，無需清空。");
            }
        }
    }

    // 回傳是否真的有刪除到 UI 物件
    bool ClearSupplyUI()
    {
        if (iconSpawnPoint == null) return false;

        bool removedAny = false;
        foreach (Transform t in iconSpawnPoint)
        {
            if (t == null) continue;
            removedAny = true;
            Destroy(t.gameObject);
        }
        return removedAny;
    }
}