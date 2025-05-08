using UnityEngine;
using System.Collections.Generic;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance { get; private set; }

    [System.Serializable]
    public class MinigameEntry
    {
        public string minigameType;
        public BaseMinigame minigamePrefab;
    }

    public List<MinigameEntry> minigames = new List<MinigameEntry>();
    public Transform minigameContainer;

    private BaseMinigame currentMinigame;

    void Awake()
    {
        // Singleton ��@
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // �i��A�p�G�Ʊ楦�b���������ɫO�d
    }

    public void StartMinigame(string type, System.Action<bool, int> onComplete)
    {
        if (currentMinigame != null)
        {
            Debug.LogWarning("�w���p�C���b�i�椤�I");
            return;
        }

        MinigameEntry entry = minigames.Find(m => m.minigameType == type);
        if (entry == null)
        {
            Debug.LogError($"�䤣������p�C������: {type}");
            return;
        }

        BaseMinigame instance = Instantiate(entry.minigamePrefab, minigameContainer);
        currentMinigame = instance;
        instance.StartMinigame((success, rank) =>
        {
            onComplete?.Invoke(success, rank);
            Destroy(instance.gameObject);
            currentMinigame = null;
        });
    }
}
