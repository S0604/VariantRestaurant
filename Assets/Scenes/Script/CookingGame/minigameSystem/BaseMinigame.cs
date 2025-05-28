using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public abstract class BaseMinigame : MonoBehaviour
{
    [Header("基本 UI 設定")]
    public GameObject cookingUI;
    public Image timerBar;
    public Image backgroundImage;
    public Sprite defaultBackground;

    protected Player player;

    [Header("時間設定")]
    public float timeLimit = 10f;
    public float endDelay = 1.5f;

    [Header("指令事件")]
    public List<RandomEvent> randomEvents;
    protected RandomEvent activeEvent;

    [Header("指令圖示生成")]
    public Transform sequenceContainer;
    public GameObject sequenceIconPrefab;

    [Header("完成動畫")]
    public Transform endAnimationContainer;
    public GameObject[] endAnimations;

    [Header("失敗動畫")]
    public Transform failAnimationContainer;
    public GameObject failAnimation;

    [Header("指令鍵")]
    public KeyCode[] wasdKeys = new KeyCode[] { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };

    public static BaseMinigame CurrentInstance { get; private set; }

    public MenuItem baseMenuItem;   // 此小遊戲對應的料理種類（漢堡、薯條、飲料）
    public MenuItem garbageItem;    // 共用垃圾 MenuItem

    protected DishGrade evaluatedGrade;

    protected float timer;
    protected System.Action<bool, int> onCompleteCallback;
    protected bool isPlaying = false;
    private bool hasEnded = false;

    [Header("料理顯示")]
    public Transform dishDisplayContainer;
    public GameObject dishDisplayPrefab;

    public enum DishGrade
    {
        Perfect = 3,
        Good = 2,
        Bad = 1,
        Fail = 0
    }

    public virtual void StartMinigame(System.Action<bool, int> callback)
    {
        timer = timeLimit;
        onCompleteCallback = callback;
        CurrentInstance = this;

        if (MinigameManager.Instance != null && MinigameManager.Instance.player != null)
            player = MinigameManager.Instance.player;

        if (player != null)
            player.isCooking = true;

        cookingUI.SetActive(true);
        backgroundImage.sprite = defaultBackground;

        if (randomEvents != null && randomEvents.Count > 0)
            activeEvent = randomEvents[Random.Range(0, randomEvents.Count)];
        else
            activeEvent = null;

        isPlaying = true;
        hasEnded = false;
    }

    protected void UpdateTimer()
    {
        if (!isPlaying) return;

        timer -= Time.deltaTime;
        if (timer < 0)
        {
            timer = 0;
            if (!hasEnded) StartCoroutine(PlayFailAnimation());
        }

        if (timerBar != null)
            timerBar.fillAmount = timer / timeLimit;
    }

    protected IEnumerator PlaySuccessAnimation(int rank)
    {
        isPlaying = false;
        hasEnded = true;

        GameObject anim = null;
        if (rank > 0 && rank <= endAnimations.Length)
            anim = Instantiate(endAnimations[rank - 1], endAnimationContainer);

        yield return new WaitForSeconds(endDelay);

        if (anim != null) Destroy(anim);

        evaluatedGrade = (DishGrade)rank;

        FinishMinigame(true, rank);
    }

    protected IEnumerator PlayFailAnimation()
    {
        isPlaying = false;
        hasEnded = true;

        GameObject anim = null;
        if (failAnimation != null && failAnimationContainer != null)
            anim = Instantiate(failAnimation, failAnimationContainer);

        yield return new WaitForSeconds(endDelay);

        if (anim != null) Destroy(anim);

        evaluatedGrade = DishGrade.Fail;

        FinishMinigame(false, 0);
    }

    protected void FinishMinigame(bool success, int rank = 0)
    {
        cookingUI.SetActive(false);

        if (player != null)
            player.isCooking = false;

        onCompleteCallback?.Invoke(success, rank);
        GenerateMenuItemByGrade(evaluatedGrade);
    }

    protected void GenerateMenuItemByGrade(DishGrade grade)
    {
        Debug.Log($"[Minigame] grade: {grade}, baseMenuItem: {baseMenuItem}, garbageItem: {garbageItem}, InventoryManager: {InventoryManager.Instance}");

        MenuItem itemToAdd = null;

        if (grade == DishGrade.Fail || grade == DishGrade.Bad)
        {
            itemToAdd = garbageItem;
        }
        else
        {
            itemToAdd = Instantiate(baseMenuItem);
            itemToAdd.grade = grade;
        }

        if (itemToAdd != null)
        {
            InventoryManager.Instance.AddItem(itemToAdd);
            UpdateDishDisplay();
        }
    }

    protected void UpdateDishDisplay()
    {
        if (dishDisplayContainer == null || dishDisplayPrefab == null) return;

        foreach (Transform child in dishDisplayContainer)
            Destroy(child.gameObject);

        List<MenuItem> allItems = InventoryManager.Instance.GetAllItems();
        int count = Mathf.Min(2, allItems.Count);

        for (int i = allItems.Count - count; i < allItems.Count; i++)
        {
            MenuItem item = allItems[i];
            GameObject dishObj = Instantiate(dishDisplayPrefab, dishDisplayContainer);
            Image img = dishObj.GetComponent<Image>();
            img.sprite = item.GetSpriteByGrade((ItemGrade)item.grade);
        }
    }

    public static bool HasMaxDishRecords()
    {
        return InventoryManager.Instance.GetItemCount() >= 2;
    }

    protected abstract string GetMinigameName();

    protected Sprite GetKeySprite(KeyCode key)
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
        return null;
    }
}
