using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class IntroSequenceController : MonoBehaviour
{
    [Header("Clips（依順序）")]
    public List<VideoClip> clips = new List<VideoClip>();

    [Header("UI")]
    public RawImage videoTarget;            // 顯示影片的 RawImage（其 Texture 指到一個 RenderTexture）
    public TMP_Text pressAnyKeyText;        // 提示文字：中間段落「按任意鍵繼續」、最後一段播完後會顯示「按任意鍵開始」
    public Button skipAllButton;            // 這顆按鈕 = 跳過全部動畫
    public string skipButtonLabelWhenLoading = "Loading..."; // 跳過中顯示

    [Header("場景/控制")]
    public string nextSceneName = "GameScene";  // 全部結束或跳過後要進入的場景
    public bool showOnlyOncePerRun = false;     // 本次啟動只顯示一次（例如反覆回主選單不再出現）
    private static bool hasShownThisRun = false;

    private VideoPlayer vp;
    private AudioSource audioSource;

    private int index = 0;
    private bool waitingForInput = false;   // 只有當前一段「播完」後才會開放
    private bool finishedAll = false;       // 所有段落都播完
    private bool isLoading = false;

    void Awake()
    {
        // 一次性顯示：若本次啟動已經看過，直接切到主場景
        if (showOnlyOncePerRun && hasShownThisRun)
        {
            LoadNextScene();
            return;
        }

        vp = GetComponent<VideoPlayer>();
        if (vp == null) vp = gameObject.AddComponent<VideoPlayer>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // VideoPlayer 基本設定
        vp.playOnAwake = false;
        vp.renderMode = VideoRenderMode.RenderTexture; // 搭配 RawImage+RenderTexture
        vp.audioOutputMode = VideoAudioOutputMode.AudioSource;
        vp.SetTargetAudioSource(0, audioSource);
        vp.isLooping = false;
        vp.loopPointReached += OnClipFinished;

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

        // 沒影片 → 直接進主場景
        if (clips == null || clips.Count == 0)
        {
            LoadNextScene();
            return;
        }

        // 自動播第一段
        PlayClip(0);
    }

    void Update()
    {
        if (isLoading) return;

        // 只有在「上一段播完」才接受任意鍵
        if (waitingForInput && Input.anyKeyDown)
        {
            waitingForInput = false;
            if (pressAnyKeyText) pressAnyKeyText.gameObject.SetActive(false);

            if (finishedAll)
            {
                // 最後一段已播完，再按任意鍵才切場景
                LoadNextScene();
                return;
            }

            // 播下一段
            index++;
            if (index < clips.Count)
            {
                PlayClip(index);
            }
            else
            {
                // 理論上不會到這裡（因 loopPointReached 已處理 finishedAll）
                finishedAll = true;
                ShowPressAnyKeyToStart();
            }
        }
    }

    private void PlayClip(int i)
    {
        if (i < 0 || i >= clips.Count)
        {
            // 防呆：若索引錯誤就直接收場
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

    private void OnClipFinished(VideoPlayer _)
    {
        // 一段播完 → 還有下一段？顯示“按任意鍵繼續”；沒有下一段？顯示“按任意鍵開始（進入主場景）”
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

        // 點擊 UI 按鈕跳過全部
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
            SceneManager.LoadScene(nextSceneName);
    }
}
