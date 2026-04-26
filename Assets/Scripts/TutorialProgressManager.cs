using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class TutorialEvent
{
    public string eventID;
    public bool isCompleted = false;
    public UnityEngine.Events.UnityEvent onEventCompleted;
}

[Serializable]
public class TutorialEventSaveData
{
    public string eventID;
    public bool isCompleted;
}

[Serializable]
public class TutorialProgressSaveData
{
    public List<TutorialEventSaveData> events = new List<TutorialEventSaveData>();
}

public class TutorialProgressManager : MonoBehaviour, ISaveable
{
    private static TutorialProgressManager _instance;
    public static TutorialProgressManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<TutorialProgressManager>();
            }
            return _instance;
        }
    }

    [SerializeField] private List<TutorialEvent> tutorialEvents = new List<TutorialEvent>();

    [Header("存檔唯一ID")]
    [SerializeField] private string uniqueID = "TutorialProgressManager";

    private Dictionary<string, TutorialEvent> eventDict = new Dictionary<string, TutorialEvent>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeDictionary();
    }

    private void InitializeDictionary()
    {
        eventDict.Clear();

        foreach (var ev in tutorialEvents)
        {
            if (ev == null)
                continue;

            if (string.IsNullOrEmpty(ev.eventID))
                continue;

            if (!eventDict.ContainsKey(ev.eventID))
            {
                eventDict.Add(ev.eventID, ev);
            }
            else
            {
                Debug.LogWarning($"[Tutorial] 發現重複 eventID: {ev.eventID}");
            }
        }
    }

    public void CompleteEvent(string eventID)
    {
        if (string.IsNullOrEmpty(eventID))
        {
            Debug.LogWarning("[Tutorial] eventID 為空");
            return;
        }

        if (eventDict.TryGetValue(eventID, out TutorialEvent ev))
        {
            if (ev.isCompleted)
            {
                Debug.Log($"[Tutorial] 已完成，略過重複觸發: {eventID}");
                return;
            }

            ev.isCompleted = true;
            ev.onEventCompleted?.Invoke();
            Debug.Log($"[Tutorial] 事件完成: {eventID}");
        }
        else
        {
            Debug.LogWarning($"[Tutorial] 找不到事件ID: {eventID}");
        }
    }

    public bool IsEventCompleted(string eventID)
    {
        return eventDict.TryGetValue(eventID, out TutorialEvent ev) && ev.isCompleted;
    }

    public TutorialEvent GetEvent(string eventID)
    {
        eventDict.TryGetValue(eventID, out TutorialEvent ev);
        return ev;
    }

    public void ResetAllTutorialProgress()
    {
        foreach (var ev in tutorialEvents)
        {
            if (ev != null)
            {
                ev.isCompleted = false;
            }
        }

        Debug.Log("[Tutorial] 所有教學進度已重置");
    }

    public string GetUniqueID()
    {
        return uniqueID;
    }

    public string CaptureAsJson()
    {
        TutorialProgressSaveData saveData = new TutorialProgressSaveData();

        foreach (var ev in tutorialEvents)
        {
            if (ev == null || string.IsNullOrEmpty(ev.eventID))
                continue;

            TutorialEventSaveData eventData = new TutorialEventSaveData
            {
                eventID = ev.eventID,
                isCompleted = ev.isCompleted
            };

            saveData.events.Add(eventData);
        }

        return JsonUtility.ToJson(saveData);
    }

    public void RestoreFromJson(string json)
    {
        if (string.IsNullOrEmpty(json))
            return;

        TutorialProgressSaveData saveData = JsonUtility.FromJson<TutorialProgressSaveData>(json);

        if (saveData == null || saveData.events == null)
            return;

        InitializeDictionary();

        foreach (var loadedEvent in saveData.events)
        {
            if (loadedEvent == null || string.IsNullOrEmpty(loadedEvent.eventID))
                continue;

            if (eventDict.TryGetValue(loadedEvent.eventID, out TutorialEvent localEvent))
            {
                localEvent.isCompleted = loadedEvent.isCompleted;
            }
        }

        Debug.Log("[Tutorial] 教學進度已還原，已完成事件不會重複觸發");
    }
}