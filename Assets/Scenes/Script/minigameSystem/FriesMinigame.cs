using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections;
using System.Collections.Generic;

public class FriesMinigame : BaseMinigame
{
    [Header("Fries 圖示設定")]
    public Sprite upIcon, downIcon, leftIcon, rightIcon;
    public Sprite upCorrectIcon, downCorrectIcon, leftCorrectIcon, rightCorrectIcon;
    public Sprite upWrongIcon, downWrongIcon, leftWrongIcon, rightWrongIcon;

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
            stageCanvasGroup.alpha = 0f; // 預設透明

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

        foreach (KeyCode key in sequence)
        {
            GameObject iconObj = Instantiate(sequenceIconPrefab, sequenceContainer);
            Image img = iconObj.GetComponent<Image>();
            img.sprite = GetFriesKeySprite(key);
            sequenceIcons.Add(img);

            Animator anim = iconObj.GetComponent<Animator>();
            if (anim != null) anim.SetTrigger("Idle");
        }
    }

    IEnumerator UpdateStageAsync(int stepIndex)
    {
        string folderPath = $"{stageVideoBasePath}/Layer{stepIndex}";
        VideoClip selectedClip = LoadRandomVideoClip(folderPath);

        if (selectedClip != null && videoPlayer != null)
        {
            videoPlayer.Stop();
            videoPlayer.clip = null;
            yield return null;

            videoPlayer.clip = selectedClip;
            videoPlayer.Prepare();

            while (!videoPlayer.isPrepared)
                yield return null;

            videoPlayer.Play();

            // 顯示 stage 畫面
            if (!hasStartedStage && stageCanvasGroup != null)
            {
                hasStartedStage = true;
                stageCanvasGroup.alpha = 1f;
            }
        }
        else
        {
            Debug.LogWarning($"No video found or VideoPlayer is null in {folderPath}");
        }
    }

    VideoClip LoadRandomVideoClip(string folderPath)
    {
        VideoClip[] clips = Resources.LoadAll<VideoClip>(folderPath);
        if (clips != null && clips.Length > 0)
            return clips[Random.Range(0, clips.Length)];
        return null;
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
}
