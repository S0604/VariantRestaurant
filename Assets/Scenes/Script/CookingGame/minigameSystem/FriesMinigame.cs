using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class FriesMinigame : BaseMinigame
{
    [Header("Fries 圖示設定")]
    public Sprite upIcon, downIcon, leftIcon, rightIcon;
    public Sprite upCorrectIcon, downCorrectIcon, leftCorrectIcon, rightCorrectIcon;
    public Sprite upWrongIcon, downWrongIcon, leftWrongIcon, rightWrongIcon;

    [Header("Fries 背景圖組")]
    public Sprite normalBackground;
    public Sprite mutationBackground;
    public Sprite reversalBackground;
    public Sprite extensionBackground;

    [Header("影片素材庫路徑")]
    public string normalVideoBasePath = "FriesAssets";
    public string eventVideoBasePath = "FriesAssets_Event";


    [Header("Fries 動畫設定")]
    public Transform stageContainer;
    public float wrongIconResetDelay = 0.5f;

    [Header("Fries Stage 播放設定")]
    public GameObject stagePrefab;
    public string stageVideoBasePath = "FriesAssets";

    private List<KeyCode> sequence = new List<KeyCode>();
    private List<Image> sequenceIcons = new List<Image>();
    private List<KeyCode> playerInput = new List<KeyCode>();

    private GameObject currentStage;
    private VideoPlayer videoPlayer;
    private CanvasGroup stageCanvasGroup;
    private bool hasStartedStage = false;

    private Dictionary<string, List<VideoClip>> videoSets = new Dictionary<string, List<VideoClip>>();
    private List<VideoClip> currentSetClips = new List<VideoClip>();

    protected override string GetMinigameName() => "Fries";

    public override void StartMinigame(System.Action<bool, int> callback)
    {
        base.StartMinigame(callback);

        ApplyBackgroundByEventSnapshot();
        ApplyWASDIconsByEventSnapshot();

        sequence.Clear();
        playerInput.Clear();

        foreach (Transform child in stageContainer)
            Destroy(child.gameObject);

        currentStage = Instantiate(stagePrefab, stageContainer);
        currentStage.transform.localPosition = Vector3.zero;
        currentStage.transform.localRotation = Quaternion.identity;
        currentStage.transform.localScale = Vector3.one;

        videoPlayer = currentStage.GetComponentInChildren<VideoPlayer>();
        stageCanvasGroup = currentStage.GetComponent<CanvasGroup>();

        if (videoPlayer == null)
            Debug.LogError("VideoPlayer component not found in stagePrefab!");
        if (stageCanvasGroup != null)
            stageCanvasGroup.alpha = 0f;

        LoadAllVideoSets();
        SelectRandomVideoSet();

        // 這局的指令長度使用快照，不受中途變化影響
        int commandLength = (IsEventActiveThisRun() && EventEffectThisRun() == EventEffectType.Extension) ? 7 : 5;

        for (int i = 0; i < commandLength; i++)
            sequence.Add(wasdKeys[Random.Range(0, wasdKeys.Length)]);

        ShowSequenceIcons();
        player.isCooking = true;
        hasStartedStage = false;
    }

    void ApplyBackgroundByEventSnapshot()
    {
        if (IsEventActiveThisRun())
        {
            switch (EventEffectThisRun())
            {
                case EventEffectType.Mutation: backgroundImage.sprite = mutationBackground; return;
                case EventEffectType.Reversal: backgroundImage.sprite = reversalBackground; return;
                case EventEffectType.Extension: backgroundImage.sprite = extensionBackground; return;
            }
        }
        backgroundImage.sprite = normalBackground;
    }

    void ApplyWASDIconsByEventSnapshot()
    {
        var iconSet = IconSetThisRun();
        if (iconSet == null) return;

        upIcon = iconSet.up;
        downIcon = iconSet.down;
        leftIcon = iconSet.left;
        rightIcon = iconSet.right;

        upCorrectIcon = iconSet.upCorrect;
        downCorrectIcon = iconSet.downCorrect;
        leftCorrectIcon = iconSet.leftCorrect;
        rightCorrectIcon = iconSet.rightCorrect;

        upWrongIcon = iconSet.upWrong;
        downWrongIcon = iconSet.downWrong;
        leftWrongIcon = iconSet.leftWrong;
        rightWrongIcon = iconSet.rightWrong;
    }

    void Update()
    {
        if (!isPlaying) return;

        UpdateTimer();

        foreach (KeyCode key in wasdKeys)
        {
            if (Input.GetKeyDown(key))
                HandleInput(key);
        }
    }

    void HandleInput(KeyCode key)
    {
        if (playerInput.Count >= sequence.Count) return;

        int step = playerInput.Count;

        if (key == sequence[step])
        {
            ChangeIconSprite(step, key, true);
            AnimateIcon(step, "Correct");
            PlayCorrectSFX();

            playerInput.Add(key);
            StartCoroutine(UpdateStageAsync(step));

            // Mutation：正確輸入後重抽「剩餘步驟」+ 刷新其圖示（使用快照）
            if (IsEventActiveThisRun() && EventEffectThisRun() == EventEffectType.Mutation)
            {
                MutateRemainingSequence(step + 1);
                RefreshRemainingIcons(step + 1);
            }

            if (playerInput.Count == sequence.Count)
            {
                float remainPercent = timer / timeLimit;
                int rank = remainPercent > 0.6f ? 3 : (remainPercent > 0.3f ? 2 : 1);
                StartCoroutine(PlaySuccessAnimation(rank));
            }
        }
        else
        {
            AnimateIcon(step, "Wrong");
            PlayWrongSFX();
            ChangeIconSprite(step, sequence[step], false);
            timer -= 0.5f;
        }
    }

    void ChangeIconSprite(int index, KeyCode key, bool correct)
    {
        if (index < 0 || index >= sequenceIcons.Count) return;
        Image img = sequenceIcons[index];

        Sprite newSprite = correct ? GetCorrectSprite(key) : GetWrongSprite(key);
        img.sprite = newSprite;

        if (!correct)
        {
            Sprite original = GetFriesKeySprite(sequence[index]);
            StartCoroutine(ResetIconAfterDelay(img, original, wrongIconResetDelay));
        }
    }

    IEnumerator ResetIconAfterDelay(Image img, Sprite originalSprite, float delay)
    {
        Sprite wrongSprite = img.sprite;
        yield return new WaitForSeconds(delay);
        if (img.sprite == wrongSprite)
            img.sprite = originalSprite;
    }

    void AnimateIcon(int index, string trigger)
    {
        if (index < 0 || index >= sequenceIcons.Count) return;
        Animator anim = sequenceIcons[index].GetComponent<Animator>();
        if (anim != null) anim.SetTrigger(trigger);
    }

    void ShowSequenceIcons()
    {
        foreach (Transform child in sequenceContainer)
            Destroy(child.gameObject);
        sequenceIcons.Clear();

        bool isExtended = (sequence.Count > 5);
        float spacing = isExtended ? 2f : 7f;
        float baseSize = 80f;
        float iconSize = isExtended ? baseSize * 0.8f : baseSize;

        HorizontalLayoutGroup layout = sequenceContainer.GetComponent<HorizontalLayoutGroup>();
        if (layout != null)
        {
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

        foreach (KeyCode key in sequence)
        {
            GameObject iconObj = Instantiate(sequenceIconPrefab, sequenceContainer);

            RectTransform rt = iconObj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(iconSize, iconSize);

            Image img = iconObj.GetComponent<Image>();
            img.sprite = GetFriesKeySprite(key);
            sequenceIcons.Add(img);

            Animator anim = iconObj.GetComponent<Animator>();
            if (anim != null) anim.SetTrigger("Idle");
        }
    }

    IEnumerator UpdateStageAsync(int stepIndex)
    {
        if (stepIndex >= currentSetClips.Count)
        {
            Debug.LogWarning("Step index exceeds available video clips.");
            yield break;
        }

        VideoClip clip = currentSetClips[stepIndex];
        if (clip != null && videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.clip = null;
            yield return null;

            videoPlayer.clip = clip;
            videoPlayer.Prepare();

            while (!videoPlayer.isPrepared)
                yield return null;

            videoPlayer.Play();

            if (!hasStartedStage && stageCanvasGroup != null)
            {
                hasStartedStage = true;
                stageCanvasGroup.alpha = 1f;
            }
        }
        else
        {
            Debug.LogWarning($"Clip not found for step {stepIndex}");
        }
    }

    void LoadAllVideoSets()
    {
        videoSets.Clear();

        // 根據快照選擇資料夾
        string basePath = IsEventActiveThisRun() ? eventVideoBasePath : normalVideoBasePath;
        VideoClip[] allClips = Resources.LoadAll<VideoClip>(basePath);

        // 如果事件資料夾空，回退一般資料夾
        if ((allClips == null || allClips.Length == 0) && basePath != normalVideoBasePath)
            allClips = Resources.LoadAll<VideoClip>(normalVideoBasePath);

        foreach (var clip in allClips)
        {
            string name = clip.name;
            int splitIndex = name.IndexOf("_Step");
            if (splitIndex > 0)
            {
                string setName = name.Substring(0, splitIndex);
                if (!videoSets.ContainsKey(setName))
                    videoSets[setName] = new List<VideoClip>();
                videoSets[setName].Add(clip);
            }
        }

        foreach (var set in videoSets.Values)
            set.Sort((a, b) => a.name.CompareTo(b.name));
    }

    void SelectRandomVideoSet()
    {
        if (videoSets.Count == 0)
        {
            Debug.LogWarning("No video sets found!");
            currentSetClips = new List<VideoClip>();
            return;
        }

        var setKeys = videoSets.Keys.ToList();
        string selectedKey = setKeys[Random.Range(0, setKeys.Count)];
        currentSetClips = videoSets[selectedKey];
    }

    Sprite GetFriesKeySprite(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.W: return upIcon;
            case KeyCode.S: return downIcon;
            case KeyCode.A: return leftIcon;
            case KeyCode.D: return rightIcon;
        }
        return null;
    }

    Sprite GetCorrectSprite(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.W: return upCorrectIcon;
            case KeyCode.S: return downCorrectIcon;
            case KeyCode.A: return leftCorrectIcon;
            case KeyCode.D: return rightCorrectIcon;
        }
        return null;
    }

    Sprite GetWrongSprite(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.W: return upWrongIcon;
            case KeyCode.S: return downWrongIcon;
            case KeyCode.A: return leftWrongIcon;
            case KeyCode.D: return rightWrongIcon;
        }
        return null;
    }

    // ===== Mutation：重抽剩餘指令 + 刷新圖示（使用本局快照） =====
    void MutateRemainingSequence(int fromIndex)
    {
        if (fromIndex < 0) fromIndex = 0;
        for (int i = fromIndex; i < sequence.Count; i++)
        {
            sequence[i] = wasdKeys[Random.Range(0, wasdKeys.Length)];
        }
    }

    void RefreshRemainingIcons(int fromIndex)
    {
        if (fromIndex < 0) fromIndex = 0;
        for (int i = fromIndex; i < sequenceIcons.Count; i++)
        {
            var img = sequenceIcons[i];
            img.sprite = GetFriesKeySprite(sequence[i]);

            var anim = img.GetComponent<Animator>();
            if (anim != null) anim.SetTrigger("Idle");
        }
    }
    // ===== end Mutation =====
}
