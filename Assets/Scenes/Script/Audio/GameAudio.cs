using UnityEngine;

/// <summary>
/// 全局音效管理：出菜依等級播放；取得補給播放。
/// 場景放一個即可（DontDestroyOnLoad 可依需要開啟）。
/// </summary>
public class GameAudio : MonoBehaviour
{
    public static GameAudio Instance { get; private set; }

    [Header("出菜音效（依 DishGrade 索引）")]
    [Tooltip("Fail=0, Bad=1, Good=2, Perfect=3, Mutated=4")]
    public AudioClip[] dishGradeClips = new AudioClip[5];

    [Header("取得補給音效")]
    public AudioClip supplyPickupClip;

    [Header("共用音量")]
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Tooltip("若未指定，會在 Awake 建一個")]
    public AudioSource sharedSource;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (sharedSource == null)
        {
            sharedSource = gameObject.AddComponent<AudioSource>();
            sharedSource.playOnAwake = false;
        }
        // 若想跨場景保留，取消註解：
        // DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        // 監聽背包新增物品 → 判斷補給
        InventoryManager.OnItemAdded += HandleItemAdded;
    }

    private void OnDisable()
    {
        InventoryManager.OnItemAdded -= HandleItemAdded;
    }

    void HandleItemAdded(MenuItem item)
    {
        if (item == null) return;

        // 你專案裡補給的判斷：用 tag "SupplyBox"（也可在這裡擴充）
        if (!string.IsNullOrEmpty(item.itemTag) &&
            (item.itemTag == "SupplyBox" || item.itemTag == "Supply" || item.itemTag == "SUPPLY"))
        {
            PlaySupplyPickup();
        }
    }

    public void PlayDishComplete(BaseMinigame.DishGrade grade)
    {
        int idx = Mathf.Clamp((int)grade, 0, dishGradeClips.Length - 1);
        var clip = (dishGradeClips != null && idx < dishGradeClips.Length) ? dishGradeClips[idx] : null;
        if (clip != null) sharedSource.PlayOneShot(clip, sfxVolume);
    }

    public void PlaySupplyPickup()
    {
        if (supplyPickupClip != null) sharedSource.PlayOneShot(supplyPickupClip, sfxVolume);
    }
}
