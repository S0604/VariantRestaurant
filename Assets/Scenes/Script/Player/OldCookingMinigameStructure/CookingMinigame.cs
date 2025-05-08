using UnityEngine;
using System.Collections;
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
    public float endDelay = 1.5f;

    [Header("普通圖示")]
    public Sprite upIcon;
    public Sprite downIcon;
    public Sprite leftIcon;
    public Sprite rightIcon;

    [Header("正確圖示")]
    public Sprite upCorrectIcon;
    public Sprite downCorrectIcon;
    public Sprite leftCorrectIcon;
    public Sprite rightCorrectIcon;

    [Header("錯誤圖示")]
    public Sprite upWrongIcon;
    public Sprite downWrongIcon;
    public Sprite leftWrongIcon;
    public Sprite rightWrongIcon;
    [Header("錯誤圖示恢復設定")]
    public float wrongIconResetDelay = 0.5f;

    [Header("隨機事件圖組")]
    public List<RandomEvent> randomEvents;

    [Header("指令序列圖示設定")]
    public Transform sequenceContainer;
    public GameObject sequenceIconPrefab;

    [Header("堆疊動畫設定")]
    public Transform stackContainer;
    public GameObject stackItemPrefab;
    public float stackItemSpacing = 40f;

    [Header("結束動畫設定")]
    public Transform endAnimationContainer;
    public GameObject[] endAnimations; // 對應 rank 1~3 的 prefab

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

        public Sprite upCorrectIcon;
        public Sprite downCorrectIcon;
        public Sprite leftCorrectIcon;
        public Sprite rightCorrectIcon;

        public Sprite upWrongIcon;
        public Sprite downWrongIcon;
        public Sprite leftWrongIcon;
        public Sprite rightWrongIcon;

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

        foreach (Transform child in stackContainer)
        {
            Destroy(child.gameObject);
        }

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

        int step = playerInput.Count;

        if (key == sequence[step])
        {
            // ✅ 不論如何都更新圖示與動畫
            ChangeIconSprite(step, key, true); // 切換為正確圖示
            AnimateIcon(step, "Correct");      // 播放正確動畫

            // ✅ 若是首次正確輸入才堆疊和紀錄
            if (playerInput.Count == step)
            {
                AddStackItem(key, step);
                playerInput.Add(key);

                if (playerInput.Count == sequence.Count)
                {
                    StartCoroutine(PlayEndAnimation(true));
                }
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

        Sprite newSprite = null;
        if (activeEvent != null)
        {
            newSprite = correct ? GetEventCorrectSprite(key) : GetEventWrongSprite(key);
        }
        else
        {
            newSprite = correct ? GetDefaultCorrectSprite(key) : GetDefaultWrongSprite(key);
        }

        img.sprite = newSprite;

        // 錯誤輸入時啟動恢復原圖示
        if (!correct)
        {
            Sprite originalSprite = GetKeySprite(sequence[index]); // 取得該步驟原本的圖示
            StartCoroutine(ResetIconAfterDelay(img, originalSprite, wrongIconResetDelay));
        }
    }

    IEnumerator ResetIconAfterDelay(Image img, Sprite originalSprite, float delay)
    {
        Sprite wrongSprite = img.sprite; // ⚠️ 記錄錯誤時的 sprite

        yield return new WaitForSeconds(delay);

        // ✅ 若圖示尚未被改變（代表使用者沒有正確輸入），才還原
        if (img.sprite == wrongSprite)
        {
            img.sprite = originalSprite;
        }
    }


    Sprite GetDefaultCorrectSprite(KeyCode key)
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

    Sprite GetDefaultWrongSprite(KeyCode key)
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

    Sprite GetEventCorrectSprite(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.W: return activeEvent.upCorrectIcon;
            case KeyCode.S: return activeEvent.downCorrectIcon;
            case KeyCode.A: return activeEvent.leftCorrectIcon;
            case KeyCode.D: return activeEvent.rightCorrectIcon;
        }
        return null;
    }

    Sprite GetEventWrongSprite(KeyCode key)
    {
        switch (key)
        {
            case KeyCode.W: return activeEvent.upWrongIcon;
            case KeyCode.S: return activeEvent.downWrongIcon;
            case KeyCode.A: return activeEvent.leftWrongIcon;
            case KeyCode.D: return activeEvent.rightWrongIcon;
        }
        return null;
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
                anim.SetTrigger("Idle");
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

        GameObject item = Instantiate(stackItemPrefab, stackContainer);
        Image img = item.GetComponent<Image>();
        img.sprite = selected;

        RectTransform rt = item.GetComponent<RectTransform>();
        rt.anchoredPosition = new Vector2(0, stepIndex * stackItemSpacing);
    }

    IEnumerator PlayEndAnimation(bool success)
    {
        isPlaying = true;

        // 鎖定玩家移動
        player.isCooking = true;

        int rank = 0;
        if (success)
        {
            float remainPercent = timer / timeLimit;
            if (remainPercent > 0.6f) rank = 3;
            else if (remainPercent > 0.3f) rank = 2;
            else rank = 1;
        }

        // 播放結束動畫（如果成功）
        GameObject anim = null;
        if (success && rank > 0 && rank <= endAnimations.Length)
        {
            anim = Instantiate(endAnimations[rank - 1], endAnimationContainer);
        }

        // 等待 endDelay 時間
        yield return new WaitForSeconds(endDelay);

        // 清理動畫
        if (anim != null)
        {
            Destroy(anim);
        }

        // 關閉 UI、小遊戲結束
        cookingUI.SetActive(false);
        player.isCooking = false; // 解鎖玩家移動
        onCompleteCallback?.Invoke(success, rank);
    }


    void FinishMinigame(bool success)
    {
        StartCoroutine(PlayEndAnimation(success));
    }
}
