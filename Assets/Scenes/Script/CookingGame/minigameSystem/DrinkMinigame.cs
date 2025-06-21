using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DrinkMinigame : BaseMinigame
{
    [Header("Drink 圖示設定")]
    public Sprite upIcon, downIcon, leftIcon, rightIcon;
    public Sprite upCorrectIcon, downCorrectIcon, leftCorrectIcon, rightCorrectIcon;
    public Sprite upWrongIcon, downWrongIcon, leftWrongIcon, rightWrongIcon;

    [Header("Drink 動畫設定")]
    public Transform stageContainer;
    public float wrongIconResetDelay = 0.5f;

    [Header("Drink Stage 播放設定")]
    public GameObject stagePrefab;
    public string stageVideoBasePath = "DrinkAssets";

    private List<KeyCode> sequence = new List<KeyCode>();
    private List<Image> sequenceIcons = new List<Image>();
    private List<KeyCode> playerInput = new List<KeyCode>();

    private GameObject currentStage;
    private VideoPlayer videoPlayer;
    private CanvasGroup stageCanvasGroup;
    private bool hasStartedStage = false;

    private Dictionary<string, List<VideoClip>> videoSets = new Dictionary<string, List<VideoClip>>();
    private List<VideoClip> currentSetClips = new List<VideoClip>();

    protected override string GetMinigameName()
    {
        return "Drink";
    }

    public override void StartMinigame(System.Action<bool, int> callback)
    {
        base.StartMinigame(callback);

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

        for (int i = 0; i < 5; i++)
            sequence.Add(wasdKeys[Random.Range(0, wasdKeys.Length)]);

        ShowSequenceIcons();
        player.isCooking = true;
        hasStartedStage = false;
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
            Sprite original = GetDrinkKeySprite(sequence[index]);
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

        foreach (KeyCode key in sequence)
        {
            GameObject iconObj = Instantiate(sequenceIconPrefab, sequenceContainer);
            Image img = iconObj.GetComponent<Image>();
            img.sprite = GetDrinkKeySprite(key);
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
        VideoClip[] allClips = Resources.LoadAll<VideoClip>(stageVideoBasePath);

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

        // 排序 clips 依照 Step0~4 順序
        foreach (var set in videoSets.Values)
        {
            set.Sort((a, b) => a.name.CompareTo(b.name));
        }
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

    Sprite GetDrinkKeySprite(KeyCode key)
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
}
