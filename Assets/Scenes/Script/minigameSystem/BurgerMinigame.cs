using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BurgerMinigame : BaseMinigame
{
    [Header("���q�ϥ�")]
    public Sprite upIcon, downIcon, leftIcon, rightIcon;

    [Header("���T�ϥ�")]
    public Sprite upCorrectIcon, downCorrectIcon, leftCorrectIcon, rightCorrectIcon;

    [Header("���~�ϥ�")]
    public Sprite upWrongIcon, downWrongIcon, leftWrongIcon, rightWrongIcon;

    [Header("���~�ϥܫ�_�]�w")]
    public float wrongIconResetDelay = 0.5f;

    [Header("���|�ʵe�]�w")]
    public Transform stackContainer;
    public GameObject stackItemPrefab;
    public float stackItemSpacing = 40f;

    private List<KeyCode> sequence = new List<KeyCode>();
    private List<Image> sequenceIcons = new List<Image>();
    private List<KeyCode> playerInput = new List<KeyCode>();
    protected bool isPlaying;


    public override void StartMinigame(System.Action<bool, int> callback)
    {
        base.StartMinigame(callback);
        isPlaying = true;
        sequence.Clear();
        playerInput.Clear();

        foreach (Transform child in stackContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < 5; i++)
            sequence.Add(wasdKeys[Random.Range(0, wasdKeys.Length)]);

        ShowSequenceIcons();
        player.isCooking = true;
    }

    void Update()
    {
        if (!isPlaying) return;

        UpdateTimer(() => StartCoroutine(PlayEndAnimation(false)));

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

            if (playerInput.Count == step)
            {
                AddStackItem(key, step);
                playerInput.Add(key);

                if (playerInput.Count == sequence.Count)
                    FinishMinigame(true);
            }
        }
        else
        {
            AnimateIcon(step, "Wrong");
            ChangeIconSprite(step, key, false);
            timer -= 0.5f;
            if (timer < 0) timer = 0;
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
            Sprite original = GetKeySprite(sequence[index]);
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
            img.sprite = GetKeySprite(key);
            sequenceIcons.Add(img);

            Animator anim = iconObj.GetComponent<Animator>();
            if (anim != null) anim.SetTrigger("Idle");
        }
    }

    void AddStackItem(KeyCode key, int stepIndex)
    {
        string folderPath = $"BurgerAssets/Layer{stepIndex}";
        Sprite[] sprites = Resources.LoadAll<Sprite>(folderPath);

        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogWarning($"No sprites found in: Resources/{folderPath}");
            return;
        }

        Sprite selected = sprites[Random.Range(0, sprites.Length)];
        GameObject item = Instantiate(stackItemPrefab, stackContainer);
        item.GetComponent<Image>().sprite = selected;

        RectTransform rt = item.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, stepIndex * stackItemSpacing);
    }

    Sprite GetKeySprite(KeyCode key)
    {
        if (activeEvent != null)
        {
            switch (key)
            {
                case KeyCode.W: return activeEvent.upIcon;
                case KeyCode.S: return activeEvent.downIcon;
                case KeyCode.A: return activeEvent.leftIcon;
                case KeyCode.D: return activeEvent.rightIcon;
            }
        }

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
        if (activeEvent != null)
        {
            switch (key)
            {
                case KeyCode.W: return activeEvent.upCorrectIcon;
                case KeyCode.S: return activeEvent.downCorrectIcon;
                case KeyCode.A: return activeEvent.leftCorrectIcon;
                case KeyCode.D: return activeEvent.rightCorrectIcon;
            }
        }

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
        if (activeEvent != null)
        {
            switch (key)
            {
                case KeyCode.W: return activeEvent.upWrongIcon;
                case KeyCode.S: return activeEvent.downWrongIcon;
                case KeyCode.A: return activeEvent.leftWrongIcon;
                case KeyCode.D: return activeEvent.rightWrongIcon;
            }
        }

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
