using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public abstract class BaseMinigame : MonoBehaviour
{
    [Header("�� UI �]�w")]
    public GameObject cookingUI;
    public Image timerBar;
    public Image backgroundImage;
    public Sprite defaultBackground;

    protected Player player;

    [Header("�ɶ��]�w")]
    public float timeLimit = 10f;
    public float endDelay = 1.5f;

    [Header("���O�ƥ�")]
    public List<RandomEvent> randomEvents;
    protected RandomEvent activeEvent;

    [Header("���O�ϥܥͦ�")]
    public Transform sequenceContainer;
    public GameObject sequenceIconPrefab;

    [Header("�����ʵe")]
    public Transform endAnimationContainer;
    public GameObject[] endAnimations;

    [Header("���Ѱʵe")]
    public Transform failAnimationContainer;
    public GameObject failAnimation;

    [Header("���O��")]
    public KeyCode[] wasdKeys = new KeyCode[] { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };

    [Header("���������Ϥ�")]
    [HideInInspector] public Transform dishDisplayContainer;
    [HideInInspector] public GameObject dishDisplayPrefab;
    [HideInInspector] public Sprite[] rankSprites;
    [HideInInspector] public string minigameType;
    public static BaseMinigame CurrentInstance { get; private set; }

    protected float timer;
    protected System.Action<bool, int> onCompleteCallback;
    protected bool isPlaying = false;
    private bool hasEnded = false;

    protected static List<DishRecord> completedDishes = new List<DishRecord>();

    public static List<DishRecord> GetCompletedDishes()
    {
        return new List<DishRecord>(completedDishes); // �Ǧ^�ƻs���H���~�����
    }

    public struct DishRecord
    {
        public string dishName;
        public int rank;
        public Sprite sprite; // �s�W�G��ڭn��ܪ��Ϥ�
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

        RecordDishResult(GetMinigameName(), rank);
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



        FinishMinigame(false, 0);
    }

    protected void FinishMinigame(bool success, int rank = 0)
    {
        cookingUI.SetActive(false);

        if (player != null)
            player.isCooking = false;

        RecordDishResult(GetMinigameName(), 0);

        onCompleteCallback?.Invoke(success, rank);

    }

    protected void RecordDishResult(string dishName, int rank)
    {
        if (rank < 0 || rank >= rankSprites.Length) return;

        if (completedDishes.Count >= 2)
            completedDishes.RemoveAt(0);

        completedDishes.Add(new DishRecord
        {
            dishName = dishName,
            rank = rank,
            sprite = rankSprites[rank] // �� �s�U�ӡA���Ӥ��ݭn�a minigameType �P�_
        });

        UpdateDishDisplay();
    }


    protected void UpdateDishDisplay()
    {
        if (dishDisplayContainer == null || dishDisplayPrefab == null) return;

        foreach (Transform child in dishDisplayContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (DishRecord record in completedDishes)
        {
            GameObject dishObj = Instantiate(dishDisplayPrefab, dishDisplayContainer);
            Image img = dishObj.GetComponent<Image>();

            img.sprite = record.sprite; // �� �ϥΰO���U�Ӫ��Ϥ�
        }
    }



    public void InitializeDisplay(string type, Transform container, GameObject prefab, Sprite[] sprites)
    {
        minigameType = type;
        dishDisplayContainer = container;
        dishDisplayPrefab = prefab;
        rankSprites = sprites;
        CurrentInstance = this;
    }


    protected void ShowDisplayDish(int rank)
    {
        if (dishDisplayContainer == null || dishDisplayPrefab == null || rankSprites == null) return;

        while (dishDisplayContainer.childCount >= 2)
            Destroy(dishDisplayContainer.GetChild(0).gameObject);

        GameObject dishObj = Instantiate(dishDisplayPrefab, dishDisplayContainer);
        Image img = dishObj.GetComponent<Image>();

        int spriteIndex = Mathf.Clamp(rank, 0, rankSprites.Length - 1);
        img.sprite = rankSprites[spriteIndex];
    }

    public static bool HasMaxDishRecords()
    {
        return completedDishes.Count >= 2;
    }

    public static void ClearAllDishRecords()
    {
        completedDishes.Clear();

        Debug.Log($"�M�������ACurrentInstance: {(CurrentInstance == null ? "NULL" : CurrentInstance.name)}");

    }



    public static void ClearCompletedDishes(Transform dishDisplayContainer)
    {
        completedDishes.Clear();

        if (dishDisplayContainer != null)
        {
            foreach (Transform child in dishDisplayContainer)
            {
                GameObject.Destroy(child.gameObject);
            }
        }
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