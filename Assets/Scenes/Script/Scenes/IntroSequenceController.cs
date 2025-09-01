using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;

public class IntroSequenceController : MonoBehaviour
{
    [Header("Clips�]�̶��ǡ^")]
    public List<VideoClip> clips = new List<VideoClip>();

    [Header("UI")]
    public RawImage videoTarget;            // ��ܼv���� RawImage�]�� Texture ����@�� RenderTexture�^
    public TMP_Text pressAnyKeyText;        // ���ܤ�r�G�����q���u�����N���~��v�B�̫�@�q������|��ܡu�����N��}�l�v
    public Button skipAllButton;            // �o�����s = ���L�����ʵe
    public string skipButtonLabelWhenLoading = "Loading..."; // ���L�����

    [Header("����/����")]
    public string nextSceneName = "GameScene";  // ���������θ��L��n�i�J������
    public bool showOnlyOncePerRun = false;     // �����Ұʥu��ܤ@���]�Ҧp���Ц^�D��椣�A�X�{�^
    private static bool hasShownThisRun = false;

    private VideoPlayer vp;
    private AudioSource audioSource;

    private int index = 0;
    private bool waitingForInput = false;   // �u����e�@�q�u�����v��~�|�}��
    private bool finishedAll = false;       // �Ҧ��q��������
    private bool isLoading = false;

    void Awake()
    {
        // �@������ܡG�Y�����Ұʤw�g�ݹL�A��������D����
        if (showOnlyOncePerRun && hasShownThisRun)
        {
            LoadNextScene();
            return;
        }

        vp = GetComponent<VideoPlayer>();
        if (vp == null) vp = gameObject.AddComponent<VideoPlayer>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

        // VideoPlayer �򥻳]�w
        vp.playOnAwake = false;
        vp.renderMode = VideoRenderMode.RenderTexture; // �f�t RawImage+RenderTexture
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

        // �S�v�� �� �����i�D����
        if (clips == null || clips.Count == 0)
        {
            LoadNextScene();
            return;
        }

        // �۰ʼ��Ĥ@�q
        PlayClip(0);
    }

    void Update()
    {
        if (isLoading) return;

        // �u���b�u�W�@�q�����v�~�������N��
        if (waitingForInput && Input.anyKeyDown)
        {
            waitingForInput = false;
            if (pressAnyKeyText) pressAnyKeyText.gameObject.SetActive(false);

            if (finishedAll)
            {
                // �̫�@�q�w�����A�A�����N��~������
                LoadNextScene();
                return;
            }

            // ���U�@�q
            index++;
            if (index < clips.Count)
            {
                PlayClip(index);
            }
            else
            {
                // �z�פW���|��o�̡]�] loopPointReached �w�B�z finishedAll�^
                finishedAll = true;
                ShowPressAnyKeyToStart();
            }
        }
    }

    private void PlayClip(int i)
    {
        if (i < 0 || i >= clips.Count)
        {
            // ���b�G�Y���޿��~�N��������
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
        // �@�q���� �� �٦��U�@�q�H��ܡ������N���~�򡨡F�S���U�@�q�H��ܡ������N��}�l�]�i�J�D�����^��
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

        // �I�� UI ���s���L����
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
