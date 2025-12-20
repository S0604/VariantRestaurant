using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 統一管理「完成料理」與「拾取補給」兩種瞬間音效。
/// 場景內掛一顆 GameObject 即可，會自動建立 AudioSource。
/// </summary>
public class GameAudio : MonoBehaviour
{
    public static GameAudio Instance { get; private set; }

    [Header("完成料理音效 (對應 DishGrade)")]
    [Tooltip("Fail=0, Normal=1, Good=2, Perfect=3, Mutated=4")]
    public AudioClip[] dishGradeClips = new AudioClip[5];

    [Header("拾取補給音效")]
    public AudioClip supplyPickupClip;

    [Header("音量")]
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("若空著會自動建立")]
    public AudioSource sharedSource;

    /* ---------- 生命週期 ---------- */
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 需要跨場景就開啟
        // DontDestroyOnLoad(gameObject);

        if (sharedSource == null)
        {
            sharedSource = gameObject.AddComponent<AudioSource>();
            sharedSource.playOnAwake = false;
        }
    }

    private void OnEnable()
    {
        // 監聽背包新增事件（補給箱判定）
        InventoryManager.OnInventoryChanged += HandleInventoryChanged;
    }

    private void OnDisable()
    {
        InventoryManager.OnInventoryChanged -= HandleInventoryChanged;
    }

    /* ========== 公開呼叫 ========== */
    /// <summary>
    /// 小遊戲結束時呼叫：依最終等級播放對應音效。
    /// </summary>
    public void PlayDishComplete(BaseMinigame.DishGrade grade)
    {
        int idx = Mathf.Clamp((int)grade, 0, dishGradeClips.Length - 1);
        AudioClip clip = dishGradeClips[idx];
        if (clip != null) sharedSource.PlayOneShot(clip, sfxVolume);
    }

    /// <summary>
    /// 拾取補給箱時呼叫（也可由外部手動觸發）。
    /// </summary>
    public void PlaySupplyPickup()
    {
        if (supplyPickupClip != null) sharedSource.PlayOneShot(supplyPickupClip, sfxVolume);
    }

    /* ========== 內部監聽 ========== */
    private void HandleInventoryChanged(List<MenuItem> currentItems)
    {
        if (currentItems.Count == 0) return;

        MenuItem newest = currentItems[currentItems.Count - 1];
        if (newest == null) return;

        // 簡單判定：tag 含 Supply 就播
        if (!string.IsNullOrEmpty(newest.itemTag) &&
            newest.itemTag.ToUpper().Contains("SUPPLY"))
        {
            PlaySupplyPickup();
        }
    }
}