using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public abstract class BaseMinigame : MonoBehaviour
{
    [Header("基本 UI 設定")]
    public GameObject cookingUI;
    public Image timerBar;
    public Image backgroundImage;
    public Sprite defaultBackground;

    protected Player player;

    [Header("時間設定")]
    public float timeLimit = 10f;
    public float endDelay = 1.5f;

    [Header("指令圖示")]
    public Transform sequenceContainer;
    public GameObject sequenceIconPrefab;

    [Header("結束動畫")]
    public Transform endAnimationContainer;
    public GameObject[] endAnimations; // 依 rank 顯示對應動畫
    public Transform failAnimationContainer;
    public GameObject failAnimation;

    [Header("指令鍵")]
    public KeyCode[] wasdKeys = new KeyCode[] { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };

    [Header("音效")]
    public AudioSource audioSource;
    public AudioClip correctSFX;
    public AudioClip wrongSFX;

    public MenuItem baseMenuItem;
    public MenuItem garbageItem;
    protected DishGrade evaluatedGrade;
    protected float timer;
    protected System.Action<bool, int> onCompleteCallback;
    protected bool isPlaying = false;
    private bool hasEnded = false;

    public enum DishGrade
    {
        Perfect = 3,
        Good = 2,
        Bad = 1,
        Fail = 0,
        // ★ 新增：變異事件專屬的判定等級
        Mutated = 4
    }

    public static BaseMinigame CurrentInstance { get; private set; }

    // ===== 事件快照（本局固定使用） =====
    protected bool eventActiveThisRun = false;
    protected EventEffectType eventEffectThisRun = default;
    protected WASDIconSetSO iconSetThisRun = null;

    /// <summary>在本局開始時拍下事件狀態與圖示組</summary>
    protected void CaptureEventContext()
    {
        var mgr = RandomEventManager.Instance;
        if (mgr != null && mgr.IsEventActive)
        {
            eventActiveThisRun = true;
            eventEffectThisRun = mgr.CurrentEffect;
            iconSetThisRun = mgr.GetCurrentIcons(); // 可能為 null
        }
        else
        {
            eventActiveThisRun = false;
            eventEffectThisRun = default;
            iconSetThisRun = (mgr != null) ? mgr.defaultIcons : null; // 可給預設
        }
    }

    // 提供給子類方便取用
    protected bool IsEventActiveThisRun() => eventActiveThisRun;
    protected EventEffectType EventEffectThisRun() => eventEffectThisRun;
    protected WASDIconSetSO IconSetThisRun() => iconSetThisRun;

    public virtual void StartMinigame(System.Action<bool, int> callback)
    {
        // 先拍快照，確保本局狀態固定
        CaptureEventContext();

        timer = timeLimit;
        onCompleteCallback = callback;
        isPlaying = true;
        hasEnded = false;
        CurrentInstance = this;

        player = MinigameManager.Instance?.player;
        if (player != null) player.isCooking = true;

        cookingUI.SetActive(true);
        backgroundImage.sprite = defaultBackground;
    }

    protected void UpdateTimer()
    {
        if (!isPlaying) return;

        timer -= Time.deltaTime;

        if (timer <= 0 && !hasEnded)
        {
            timer = 0;
            StartCoroutine(PlayFailAnimation());
        }

        if (timerBar != null)
            timerBar.fillAmount = timer / timeLimit;
    }

    protected IEnumerator PlaySuccessAnimation(int rank)
    {
        isPlaying = false;
        hasEnded = true;

        GameObject anim = null;
        if (rank > 0 && rank <= endAnimations.Length)
            anim = Instantiate(endAnimations[rank - 1], endAnimationContainer);

        yield return new WaitForSeconds(endDelay);
        if (anim != null) Destroy(anim);

        evaluatedGrade = (DishGrade)rank;
        FinishMinigameInternal(true, rank);
    }

    protected IEnumerator PlayFailAnimation()
    {
        isPlaying = false;
        hasEnded = true;

        GameObject anim = Instantiate(failAnimation, failAnimationContainer);
        yield return new WaitForSeconds(endDelay);
        if (anim != null) Destroy(anim);

        evaluatedGrade = DishGrade.Fail;
        FinishMinigameInternal(false, 0);
    }

    protected virtual void FinishMinigameInternal(bool success, int rank = 0)
    {
        if (player != null) player.isCooking = false;
        GenerateMenuItemByGrade(success ? (DishGrade)rank : DishGrade.Fail);
        cookingUI.SetActive(false);
        onCompleteCallback?.Invoke(success, rank);
    }

    protected void GenerateMenuItemByGrade(DishGrade grade)
    {
        // 先決定「最後等級」
        DishGrade finalGrade = grade;
        if (IsEventActiveThisRun() && grade != DishGrade.Fail)
            finalGrade = DishGrade.Mutated;

        // ★ 播放對應等級的出菜音效（不影響其他流程）
        GameAudio.Instance?.PlayDishComplete(finalGrade);

        // 再依「最後等級」挑模板：Mutated 走正常商品模板
        MenuItem template = (finalGrade == DishGrade.Fail) ? garbageItem : baseMenuItem;
        if (template == null)
        {
            Debug.LogWarning("[BaseMinigame] 產物模板未設置！");
            return;
        }

        MenuItem item = Instantiate(template);
        item.grade = finalGrade;

        // 同步 itemImage（給舊 UI）
        item.SyncImageToGrade();

        InventoryManager.Instance.AddItem(item);
    }



    protected void PlayCorrectSFX()
    {
        if (audioSource != null && correctSFX != null)
            audioSource.PlayOneShot(correctSFX);
    }

    protected void PlayWrongSFX()
    {
        if (audioSource != null && wrongSFX != null)
            audioSource.PlayOneShot(wrongSFX);
    }

    protected abstract string GetMinigameName();
}
