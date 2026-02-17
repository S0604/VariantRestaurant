using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class IntroSequenceController : MonoBehaviour
{
    [Header("影片列表")]
    public List<VideoClip> clips = new List<VideoClip>();

    [Header("UI")]
    public RawImage videoTarget;            // 用來顯示影片的 RawImage (需配合 RenderTexture)
    public TMP_Text pressAnyKeyText;        // 提示玩家按任意鍵繼續
    public Button skipAllButton;            // 跳過全部影片的按鈕
    public string skipButtonLabelWhenLoading = "Loading..."; // 按下跳過後顯示的文字

    [Header("場景設定")]
    public string nextSceneName = "GameScene";  // 播放完影片後要切換的場景名稱
    public bool showOnlyOncePerRun = false;     // 是否每次遊戲啟動只播放一次
    private static bool hasShownThisRun = false;

    private VideoPlayer vp;
    private AudioSource audioSource;

    private int index = 0;                  // 目前播放到第幾支影片
    private bool waitingForInput = false;   // 是否正在等待玩家按任意鍵
    private bool finishedAll = false;       // 是否已播放完全部影片
    private bool isLoading = false;         // 是否正在載入下一個場景

    void Awake()
    {
        // 如果設定只播放一次，且已經播放過，直接切換場景
        if (showOnlyOncePerRun && hasShownThisRun)
        {
            LoadNextScene();
            return;
        }

        // 初始化 VideoPlayer
        vp = GetComponent<VideoPlayer>();
        if (vp == null) vp = gameObject.AddComponent<VideoPlayer>();

        // 初始化 AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // 設定 VideoPlayer 屬性
        vp.playOnAwake = false;
        vp.renderMode = VideoRenderMode.RenderTexture; // 將影片輸出到 RenderTexture
        vp.audioOutputMode = VideoAudioOutputMode.AudioSource;
        vp.SetTargetAudioSource(0, audioSource);
        vp.isLooping = false;
        vp.loopPointReached += OnClipFinished;        // 影片播放完事件

        // 隱藏按任意鍵提示
        if (pressAnyKeyText) pressAnyKeyText.gameObject.SetActive(false);

        // 設定跳過按鈕事件
        if (skipAllButton)
        {
            skipAllButton.onClick.RemoveAllListeners();
            skipAllButton.onClick.AddListener(SkipAll);
        }
    }

    void Start()
    {
        if (showOnlyOncePerRun) hasShownThisRun = true;

        // 如果影片列表為空，直接切換場景
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

        // 玩家按任意鍵時的行為
        if (waitingForInput && Input.anyKeyDown)
        {
            waitingForInput = false;
            if (pressAnyKeyText) pressAnyKeyText.gameObject.SetActive(false);

            if (finishedAll)
            {
                // 全部播放完，按鍵後切換場景
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
                // 已到最後一支影片
                finishedAll = true;
                ShowPressAnyKeyToStart();
            }
        }
    }

    private void PlayClip(int i)
    {
        if (i < 0 || i >= clips.Count)
        {
            // 如果索引超出範圍，直接標記全部播放完
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
        while (!vp.isPrepared) yield return null;
        vp.Play();
    }

    // 影片播放完回調
    private void OnClipFinished(VideoPlayer _)
    {
        if (index < clips.Count - 1)
        {
            // 還有下一支影片，等待玩家按任意鍵
            waitingForInput = true;
            if (pressAnyKeyText)
            {
                pressAnyKeyText.text = "按下任意按鈕繼續.";
                pressAnyKeyText.gameObject.SetActive(true);
            }
        }
        else
        {
            // 最後一支影片播放完，顯示開始提示
            finishedAll = true;
            ShowPressAnyKeyToStart();
        }
    }

    private void ShowPressAnyKeyToStart()
    {
        waitingForInput = true;
        if (pressAnyKeyText)
        {
            pressAnyKeyText.text = "按下任意按鈕繼續.";
            pressAnyKeyText.gameObject.SetActive(true);
        }
    }

    // 跳過全部影片
    private void SkipAll()
    {
        if (isLoading) return;

        // 更新 UI
        if (skipAllButton)
        {
            skipAllButton.interactable = false;
            var label = skipAllButton.GetComponentInChildren<TMP_Text>();
            if (label) label.text = skipButtonLabelWhenLoading;
        }

        LoadNextScene();
    }

    // 切換場景
    private void LoadNextScene()
    {
        if (isLoading) return;
        isLoading = true;

        if (vp) vp.loopPointReached -= OnClipFinished;

        if (!string.IsNullOrEmpty(nextSceneName))
            SceneManager.LoadScene(nextSceneName);
    }
}
