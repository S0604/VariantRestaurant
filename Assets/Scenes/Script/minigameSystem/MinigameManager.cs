using UnityEngine;
using System.Collections.Generic;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance { get; private set; }

    [Header("���G�ϥܺ޲z")]
    public Transform dishDisplayContainer;
    public GameObject dishDisplayPrefab;
    public List<RankSpriteSet> rankSpriteSets = new List<RankSpriteSet>();



    [System.Serializable]
    public class RankSpriteSet
    {
        public string minigameType;
        public Sprite[] rankSprites; // index 0 = ���ѹϡA1~3 = ���\��
    }


    public List<MinigameEntry> minigames = new List<MinigameEntry>();
    public Transform minigameContainer;

    [Header("���a����")]
    public Player player; // <--- �s�W�o�@��

    private BaseMinigame currentMinigame;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // �����p�O�d
    }

    [System.Serializable]
    public class MinigameEntry
    {
        public string minigameType;      // �Ҧp "Burger" �� "Fries"
        public string resourcePath;      // �Ҧp "Minigames/BurgerMinigame"
    }

    private Sprite[] GetSpritesForType(string type)
    {
        RankSpriteSet set = rankSpriteSets.Find(s => s.minigameType == type);
        return set != null ? set.rankSprites : null;
    }

    public void StartMinigame(string type, System.Action<bool, int> onComplete)
    {
        if (currentMinigame != null)
        {
            Debug.LogWarning("�w���p�C���b�i�椤�I");
            return;
        }

        //����Ʋz�����W��
        if (BaseMinigame.HasMaxDishRecords())
        {
            Debug.LogWarning("�A�w�g���ⶵ�Ʋz�����A�Х��M����A�~��I");
            return;
        }

        MinigameEntry entry = minigames.Find(m => m.minigameType == type);
        if (entry == null)
        {
            Debug.LogError($"�䤣������p�C������: {type}");
            return;
        }

        GameObject prefab = Resources.Load<GameObject>(entry.resourcePath);
        if (prefab == null)
        {
            Debug.LogError($"�L�k�q Resources ���J prefab: {entry.resourcePath}");
            return;
        }

        GameObject instanceObj = Instantiate(prefab, minigameContainer);
        currentMinigame = instanceObj.GetComponent<BaseMinigame>();
        Sprite[] sprites = GetSpritesForType(type);
        currentMinigame.InitializeDisplay(type, dishDisplayContainer, dishDisplayPrefab, sprites);

        if (currentMinigame == null)
        {
            Debug.LogError("���J���p�C�� prefab �ʤ� BaseMinigame �ե�I");
            Destroy(instanceObj);
            return;
        }

        currentMinigame.StartMinigame((success, rank) =>
        {
            onComplete?.Invoke(success, rank);
            Destroy(instanceObj);
            currentMinigame = null;
        });
    }

    public void RefreshDishDisplay()
    {
        if (dishDisplayContainer == null || dishDisplayPrefab == null) return;

        foreach (Transform child in dishDisplayContainer)
        {
            Destroy(child.gameObject);
        }

        // �]���w�g�M�� completedDishes�A�ҥH���ݭn���إ���ϥ�
        Debug.Log("Manager ����M���q���A�w��s�e���I");
    }



}

