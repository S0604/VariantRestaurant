using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class CookingMinigame : MonoBehaviour
{
    [Header("基本設定")]
    public GameObject cookingUI;
    public Image timerBar;
    public Image backgroundImage;
    public Sprite defaultBackground;
    public Player player;
    public float timeLimit = 5f;

    [Header("普通圖示")]
    public Sprite upIcon;
    public Sprite downIcon;
    public Sprite leftIcon;
    public Sprite rightIcon;

    [Header("隨機事件圖組")]
    public List<RandomEvent> randomEvents;

    [Header("指令序列圖示設定")]
    public Transform sequenceContainer;
    public GameObject sequenceIconPrefab;

    [Header("堆疊動畫設定")]
    public Transform stackContainer;        // 堆疊父物件
    public GameObject stackItemPrefab;      // 堆疊用的 prefab
    [Header("堆疊間距設定")]
    public float stackItemSpacing = 40f;
    [Header("結束動畫設定")]
    public float endDelay = 1.5f;
    public GameObject[] endAnimations; // index 0 = rank1, index 1 = rank2, index 2 = rank3
    public Transform endAnimationContainer;    // 動畫生成位置

    private List<KeyCode> sequence = new List<KeyCode>();
    private List<Image> sequenceIcons = new List<Image>();
    private List<KeyCode> playerInput = new List<KeyCode>();
    private float timer;
    private bool isPlaying = false;
    private System.Action<bool, int> onCompleteCallback;
    private KeyCode[] wasdKeys = new KeyCode[] { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };
    private RandomEvent activeEvent;

    [System.Serializable]
    public class RandomEvent
    {
        public string eventName;
        public Sprite upIcon;
        public Sprite downIcon;
        public Sprite leftIcon;
        public Sprite rightIcon;
        public Sprite background;
    }

    public void StartMinigame(System.Action<bool, int> callback)
    {
        isPlaying = true;
        timer = timeLimit;
        playerInput.Clear();
        sequence.Clear();
        onCompleteCallback = callback;
        cookingUI.SetActive(true);

        // 清除堆疊物件
        foreach (Transform child in stackContainer)
        {
            Destroy(child.gameObject);
        }

        // 隨機事件決定
        if (Random.value < 0.4f && randomEvents.Count > 0)
        {
            activeEvent = randomEvents[Random.Range(0, randomEvents.Count)];
            backgroundImage.sprite = activeEvent.background;
        }
        else
        {
            activeEvent = null;
            backgroundImage.sprite = defaultBackground;
        }

        // 隨機產生指令序列
        for (int i = 0; i < 5; i++)
        {
            sequence.Add(wasdKeys[Random.Range(0, wasdKeys.Length)]);
        }

        ShowSequenceIcons();

        player.isCooking = true;
    }

    void Update()
    {
        if (!isPlaying) return;

        timer -= Time.deltaTime;
        timerBar.fillAmount = timer / timeLimit;

        if (timer <= 0)
        {
            FinishMinigame(false);
            return;
        }

        foreach (KeyCode key in wasdKeys)
        {
            if (Input.GetKeyDown(key))
            {
                HandleInput(key);
            }
        }
    }

    void HandleInput(KeyCode key)
    {
        if (playerInput.Count >= sequence.Count) return;

        if (key == sequence[playerInput.Count])
        {
            int step = playerInput.Count;

            AnimateIcon(step, "Correct");
            AddStackItem(key, step); // 額外堆疊動畫
            playerInput.Add(key);

            if (playerInput.Count == sequence.Count)
            {
                FinishMinigame(true);
            }
        }
        else
        {
            AnimateIcon(playerInput.Count, "Wrong");
            timer -= 0.5f;
            if (timer < 0) timer = 0;
        }
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
        else
        {
            switch (key)
            {
                case KeyCode.W: return upIcon;
                case KeyCode.S: return downIcon;
                case KeyCode.A: return leftIcon;
                case KeyCode.D: return rightIcon;
            }
        }
        return null;
    }

    void AnimateIcon(int index, string trigger)
    {
        if (index >= 0 && index < sequenceIcons.Count)
        {
            Animator anim = sequenceIcons[index].GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger(trigger);
            }
        }
    }

    void ShowSequenceIcons()
    {
        foreach (Transform child in sequenceContainer)
        {
            Destroy(child.gameObject);
        }

        sequenceIcons.Clear();

        foreach (KeyCode key in sequence)
        {
            GameObject iconObj = Instantiate(sequenceIconPrefab, sequenceContainer);
            Image img = iconObj.GetComponent<Image>();
            img.sprite = GetKeySprite(key);
            sequenceIcons.Add(img);

            Animator anim = iconObj.GetComponent<Animator>();
            if (anim != null)
            {
                anim.SetTrigger("Idle"); // 初始 idle 狀態顯示圖示
            }
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
        GameObject item = Instantiate(stackItemPrefab);

        Image img = item.GetComponent<Image>();
        img.sprite = selected;

        RectTransform rt = item.GetComponent<RectTransform>();
        rt.SetParent(stackContainer, false);

        // 計算 Y 座標：第一張在底部，後面每張依照設定距離向上堆疊
        float yOffset = stackItemSpacing * stackContainer.childCount;

        rt.pivot = new Vector2(0.5f, 0); // 底部對齊
        rt.anchorMin = new Vector2(0.5f, 0);
        rt.anchorMax = new Vector2(0.5f, 0);
        rt.anchoredPosition = new Vector2(0, yOffset);
    }



    void FinishMinigame(bool success)
    {
        isPlaying = false;

        int rank = 0;
        if (success)
        {
            float remainPercent = timer / timeLimit;
            if (remainPercent > 0.6f) rank = 3;
            else if (remainPercent > 0.3f) rank = 2;
            else rank = 1;
        }

        StartCoroutine(PlayEndSequence(rank, success));
    }

    System.Collections.IEnumerator PlayEndSequence(int rank, bool success)
    {
        GameObject animInstance = PlayEndAnimation(rank);

        yield return new WaitForSeconds(endDelay);

        if (animInstance != null)
        {
            Destroy(animInstance);
        }

        cookingUI.SetActive(false);
        player.isCooking = false;

        onCompleteCallback?.Invoke(success, rank);
    }



    GameObject PlayEndAnimation(int rank)
    {
        int index = Mathf.Clamp(rank - 1, 0, endAnimations.Length - 1);
        if (endAnimations.Length == 0 || endAnimations[index] == null)
        {
            Debug.LogWarning("No end animation prefab assigned for rank: " + rank);
            return null;
        }

        Transform parent = endAnimationContainer != null ? endAnimationContainer : cookingUI.transform;
        return Instantiate(endAnimations[index], parent);
    }


}
