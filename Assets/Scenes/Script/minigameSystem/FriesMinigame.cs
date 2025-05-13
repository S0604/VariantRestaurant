using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class FriesMinigame : BaseMinigame
{
    [Header("Fries 圖示設定")]

    public Sprite upIcon, downIcon, leftIcon, rightIcon;
    public Sprite upCorrectIcon, downCorrectIcon, leftCorrectIcon, rightCorrectIcon;
    public Sprite upWrongIcon, downWrongIcon, leftWrongIcon, rightWrongIcon;

    public GameObject[] stagePrefabs;
    public Transform stageContainer;
    public float wrongIconResetDelay = 0.5f;

    private List<KeyCode> sequence = new List<KeyCode>();
    private List<Image> sequenceIcons = new List<Image>();
    private List<KeyCode> playerInput = new List<KeyCode>();
    private GameObject currentStage;

    public override void StartMinigame(System.Action<bool, int> callback)
    {
        base.StartMinigame(callback);

        sequence.Clear();
        playerInput.Clear();

        foreach (Transform child in stageContainer)
            Destroy(child.gameObject);

        for (int i = 0; i < 5; i++)
            sequence.Add(wasdKeys[Random.Range(0, wasdKeys.Length)]);

        ShowSequenceIcons();
        player.isCooking = true;
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
            UpdateStage(step);

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
            ChangeIconSprite(step, key, false);
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

    void UpdateStage(int stepIndex)
    {
        if (currentStage != null)
            Destroy(currentStage);

        if (stepIndex < stagePrefabs.Length)
            currentStage = Instantiate(stagePrefabs[stepIndex], stageContainer);
    }


    // 取得對應按鍵的預設圖示（非事件用）
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

    // 若要擴充正確與錯誤圖示用的函式，可參考以下寫法：
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
