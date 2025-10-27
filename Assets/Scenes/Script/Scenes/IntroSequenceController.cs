using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class IntroSequenceController : MonoBehaviour
{
    [Header("影片剪輯設定")]
    public List<VideoClip> clips = new List<VideoClip>();
    // 要依序播放的影片列表

    [Header("UI 介面")]
    public RawImage videoTarget;            // 用來顯示影片的 RawImage（RenderTexture 會被指派到這裡）
    public TMP_Text pressAnyKeyText;        // 「按任意鍵繼續」或「按任意鍵開始」提示文字
    public Button skipAllButton;            // 「跳過全部」按鈕
    public string skipButtonLabelWhenLoading = "Loading..."; // 當跳過後載入時顯示的文字

    [Header("場景切換設定")]
    public string nextSceneName = "GameScene";  // 播放結束後要載入的場景名稱
    public bool showOnlyOncePerRun = false;     // 是否只在這次遊戲運行中顯示一次（防止重播）
    private static bool hasShownThisRun = false; // 記錄是否已顯示過

    private VideoPlayer vp;
    private AudioSource audioSource;

    private int index = 0;             // 目前播放中的影片索引
    private bool waitingForInput = false;   // 是否正在等待玩家按鍵繼續
    private bool finishedAll = false;       // 是否所有影片都播完了
    private bool isLoading = false;         // 是否正在載入下一場景

    void Awake()
    {
        // 若設定成「這次遊戲只播放一次」，而且之前已經播過，就直接跳到主場景
        if (showOnlyOncePerRun && hasShownThisRun)
        {
            LoadNextScene();
            return;
        }

        // 準備 VideoPlayer 與 AudioSource
        vp = GetComponent<VideoPlayer>();
        if (vp == null) vp = gameObject.AddComponent<VideoPlayer>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // 設定 VideoPlayer 參數
        vp.playOnAwake = false;
        vp.renderMode = VideoRenderMode.RenderTexture; // 使用 RenderTexture 輸出
        vp.audioOutputMode = VideoAudioOutputMode.AudioSource;
        vp.SetTargetAudioSource(0, audioSource);
        vp.isLooping = false;
        vp.loopPointReached += OnClipFinished; // 當影片播放完畢時呼叫 OnClipFinished()

        if (pressAnyKeyText) pressAnyKeyText.gameObject.SetActive(false);

        if (skipAllButton)
        {
            skipAllButton.onClick.RemoveAllListeners();
            skipAllButton.onClick.AddListener(SkipAll);
        }
    }

    void Start()
    {
        if (showOnlyOncePerRun) hasShownThisRun = true;

        // 沒有影片的話，直接進場景
        if (clips == null || clips.Count == 0)
        {
            LoadNextScene();
            return;
        }

        // 播放第一支影片
        PlayClip(0);
    }

    void Update()
    {
        if (isLoading) return;

        // 如果正在等待玩家按鍵
        if (waitingForInput && Input.anyKeyDown)
        {
            waitingForInput = false;
            if (pressAnyKeyText) pressAnyKeyText.gameObject.SetActive(false);

            if (finishedAll)
            {
                // 所有影片播完 → 任何鍵開始遊戲
                LoadNextScene();
                return;
            }

            // 播放下一支影片
            index++;
            if (index < clips.Count)
            {
                PlayClip(index);
            }
            else
            {
                // 理論上不會進到這裡（最後一支會由 OnClipFinished 處理）
                finishedAll = true;
                ShowPressAnyKeyToStart();
            }
        }
    }

    private void PlayClip(int i)
    {
        if (i < 0 || i >= clips.Count)
        {
            // 錯誤保護：若超出範圍，就直接結束
            finishedAll = true;
            ShowPressAnyKeyToStart();
            return;
        }

        finishedAll = false;
        waitingForInput = false;
        if (pressAnyKeyText) pressAnyKeyText.gameObject.SetActive(false);

        vp.Stop();
        vp.clip = clips[i];
        vp.Prepare();
        StartCoroutine(PlayWhenPrepared());
    }

    private System.Collections.IEnumerator PlayWhenPrepared()
    {
        while (!vp.isPrepared) yield return null; // 等待影片準備好
        vp.Play(); // 開始播放
    }

    private void OnClipFinished(VideoPlayer _)
    {
        // 當影片播完時：
        // 若還有下一支 → 顯示「Press any key to continue」
        // 若是最後一支 → 顯示「Press any key to start」
        if (index < clips.Count - 1)
        {
            waitingForInput = true;
            if (pressAnyKeyText)
            {
                pressAnyKeyText.text = "Press any key to continue.";
                pressAnyKeyText.gameObject.SetActive(true);
            }
        }
        else
        {
            finishedAll = true;
            ShowPressAnyKeyToStart();
        }
    }

    private void ShowPressAnyKeyToStart()
    {
        waitingForInput = true;
        if (pressAnyKeyText)
        {
            pressAnyKeyText.text = "Press any key to start.";
            pressAnyKeyText.gameObject.SetActive(true);
        }
    }

    private void SkipAll()
    {
        if (isLoading) return;

        // 點擊跳過按鈕時 → 停用按鈕、顯示 Loading... 並直接載入主場景
        if (skipAllButton)
        {
            skipAllButton.interactable = false;
            var label = skipAllButton.GetComponentInChildren<TMP_Text>();
            if (label) label.text = skipButtonLabelWhenLoading;
        }

        LoadNextScene();
    }

    private void LoadNextScene()
    {
        if (isLoading) return;
        isLoading = true;

        if (vp) vp.loopPointReached -= OnClipFinished;

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.sceneLoaded += OnSceneLoaded; // 監聽載入完成
            SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // 銷毀自己（避免殘留）
        Destroy(gameObject);
    }
}
