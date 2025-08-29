using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BurgerMinigame : BaseMinigame
{
    [Header("WASD 圖示")]
    public Sprite upIcon, downIcon, leftIcon, rightIcon;
    public Sprite upCorrectIcon, downCorrectIcon, leftCorrectIcon, rightCorrectIcon;
    public Sprite upWrongIcon, downWrongIcon, leftWrongIcon, rightWrongIcon;

    [Header("堆疊與動畫")]
    public float wrongIconResetDelay = 0.5f;
    public Transform stackContainer;
    public GameObject stackItemPrefab;
    public float stackItemSpacing = 40f;

    [Header("背景圖組")]
    public Sprite normalBackground;
    public Sprite mutationBackground;
    public Sprite reversalBackground;
    public Sprite extensionBackground;

    [Header("素材庫路徑（Resources）")]
    [Tooltip("一般狀態下的根資料夾，例如：BurgerAssets")]
    public string normalAssetFolder = "BurgerAssets";
    [Tooltip("事件期間共用的根資料夾，例如：BurgerAssets_Event")]
    public string eventAssetFolder = "BurgerAssets_Event";
    [Tooltip("每層子資料夾命名；Layer{0} 會得到 Layer0/Layer1…")]
    public string layerFolderFormat = "Layer{0}";

    private readonly List<KeyCode> sequence = new();
    private readonly List<Image> sequenceIcons = new();
    private readonly List<KeyCode> playerInput = new();

    protected override string GetMinigameName() => "Burger";

    public override void StartMinigame(System.Action<bool, int> callback)
    {
        base.StartMinigame(callback);

        // 背景與圖示使用「本局快照」決定（不受中途變動影響）
        ApplyBackgroundByEventSnapshot();
        ApplyWASDIconsByEventSnapshot();

        sequence.Clear();
        playerInput.Clear();

        foreach (Transform child in stackContainer)
            Destroy(child.gameObject);

        // 指令長度：Extension 才 7，其他 5（依快照）
        int commandLength = (IsEventActiveThisRun() && EventEffectThisRun() == EventEffectType.Extension) ? 7 : 5;
        for (int i = 0; i < commandLength; i++)
            sequence.Add(wasdKeys[Random.Range(0, wasdKeys.Length)]);

        ShowSequenceIcons();

        if (MinigameManager.Instance?.player != null)
            MinigameManager.Instance.player.isCooking = true;
    }

    // ---------- 依快照套背景 / 圖示 ----------
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
        var iconSetSO = IconSetThisRun();
        if (iconSetSO == null) return;

        upIcon = iconSetSO.up; downIcon = iconSetSO.down;
        leftIcon = iconSetSO.left; rightIcon = iconSetSO.right;

        upCorrectIcon = iconSetSO.upCorrect; downCorrectIcon = iconSetSO.downCorrect;
        leftCorrectIcon = iconSetSO.leftCorrect; rightCorrectIcon = iconSetSO.rightCorrect;

        upWrongIcon = iconSetSO.upWrong; downWrongIcon = iconSetSO.downWrong;
        leftWrongIcon = iconSetSO.leftWrong; rightWrongIcon = iconSetSO.rightWrong;
    }
    // --------------------------------------

    void Update()
    {
        if (!isPlaying) return;
        UpdateTimer();

        foreach (KeyCode key in wasdKeys)
            if (Input.GetKeyDown(key)) HandleInput(key);
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

            if (playerInput.Count == step)
            {
                AddStackItem(key, step);
                playerInput.Add(key);

                // Mutation：每次正確後重抽剩餘步驟並刷新圖示（依快照）
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
        var img = sequenceIcons[index];
        img.sprite = correct ? GetCorrectSprite(key) : GetWrongSprite(key);

        if (!correct)
        {
            Sprite original = GetBurgerKeySprite(sequence[index]);
            StartCoroutine(ResetIconAfterDelay(img, original, wrongIconResetDelay));
        }
    }

    IEnumerator ResetIconAfterDelay(Image img, Sprite originalSprite, float delay)
    {
        Sprite wrongSprite = img.sprite;
        yield return new WaitForSeconds(delay);
        if (img != null && img.sprite == wrongSprite)
            img.sprite = originalSprite;
    }

    void AnimateIcon(int index, string trigger)
    {
        if (index < 0 || index >= sequenceIcons.Count) return;
        var anim = sequenceIcons[index].GetComponent<Animator>();
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

        var layout = sequenceContainer.GetComponent<HorizontalLayoutGroup>();
        if (layout != null)
        {
            layout.spacing = spacing;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        }

        for (int i = 0; i < sequence.Count; i++)
        {
            KeyCode key = sequence[i];
            GameObject iconObj = Instantiate(sequenceIconPrefab, sequenceContainer);

            var rt = iconObj.GetComponent<RectTransform>();
            if (rt != null) rt.sizeDelta = new Vector2(iconSize, iconSize);

            var img = iconObj.GetComponent<Image>();
            if (img != null) img.sprite = GetBurgerKeySprite(key);
            sequenceIcons.Add(img);

            var anim = iconObj.GetComponent<Animator>();
            if (anim != null) anim.SetTrigger("Idle");
        }
    }

    // ★ 這裡只分「一般素材庫 / 事件素材庫」兩種
    void AddStackItem(KeyCode key, int stepIndex)
    {
        string baseFolder = IsEventActiveThisRun() ? eventAssetFolder : normalAssetFolder;
        string layerPath = string.IsNullOrEmpty(layerFolderFormat) ? $"Layer{stepIndex}"
                                                                    : string.Format(layerFolderFormat, stepIndex);
        string folderPath = $"{baseFolder}/{layerPath}";

        Sprite[] sprites = Resources.LoadAll<Sprite>(folderPath);

        // 事件庫缺圖時回退一般庫，避免空集合
        if ((sprites == null || sprites.Length == 0) && baseFolder != normalAssetFolder)
            sprites = Resources.LoadAll<Sprite>($"{normalAssetFolder}/{layerPath}");

        if (sprites == null || sprites.Length == 0) return;

        Sprite selected = sprites[Random.Range(0, sprites.Length)];
        GameObject item = Instantiate(stackItemPrefab, stackContainer);
        var img = item.GetComponent<Image>();
        if (img != null) img.sprite = selected;

        var rt = item.GetComponent<RectTransform>();
        if (rt != null) rt.anchoredPosition = new Vector2(0, stepIndex * stackItemSpacing);
    }

    Sprite GetBurgerKeySprite(KeyCode key)
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

    // ----- Mutation（依快照）-----
    void MutateRemainingSequence(int fromIndex)
    {
        if (fromIndex < 0) fromIndex = 0;
        for (int i = fromIndex; i < sequence.Count; i++)
            sequence[i] = wasdKeys[Random.Range(0, wasdKeys.Length)];
    }

    void RefreshRemainingIcons(int fromIndex)
    {
        if (fromIndex < 0) fromIndex = 0;
        for (int i = fromIndex; i < sequenceIcons.Count; i++)
        {
            var img = sequenceIcons[i];
            if (img != null) img.sprite = GetBurgerKeySprite(sequence[i]);

            var anim = img != null ? img.GetComponent<Animator>() : null;
            if (anim != null) anim.SetTrigger("Idle");
        }
    }
    // ----------------------------
}
