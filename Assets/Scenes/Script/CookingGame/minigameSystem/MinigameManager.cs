using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MinigameManager : MonoBehaviour
{
    public static MinigameManager Instance { get; private set; }

    public List<MinigameEntry> minigames = new List<MinigameEntry>();
    public Transform minigameContainer;

    [Header("���a����")]
    public Player player;

    private BaseMinigame currentMinigame;
    public bool IsPlaying => currentMinigame != null;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    [System.Serializable]
    public class MinigameEntry
    {
        public string minigameType;
        public string resourcePath;
    }

    public void StartMinigame(string type, System.Action<bool, int> onComplete)
    {
        
        Debug.Log($"�Ұʤp�C���ɨƥ󪬺A�GActive={RandomEventManager.Instance?.IsEventActive}, Effect={RandomEventManager.Instance?.CurrentEffect}");

        if (currentMinigame != null)
        {
            Debug.LogWarning("�w���p�C���b�i�椤�I");
            return;
        }

        if (BaseMinigame.HasMaxDishRecords())
        {
            Debug.LogWarning("�A�w�g���ⶵ�Ʋz�����A�Х��M����A�~��I");
            return;
        }

        var entry = minigames.Find(m => m.minigameType == type);
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

        if (currentMinigame == null)
        {
            Debug.LogError("���J���p�C�� prefab �ʤ� BaseMinigame �ե�I");
            Destroy(instanceObj);
            return;
        }

        currentMinigame.StartMinigame((success, rank) =>
        {
            onComplete?.Invoke(success, rank);
            StartCoroutine(DestroyAfterDelay(instanceObj, 0.5f)); // �ץ��G�q�o�̰��� Coroutine
            currentMinigame = null;
        });
    }

    private IEnumerator DestroyAfterDelay(GameObject obj, float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(obj);
    }
}
