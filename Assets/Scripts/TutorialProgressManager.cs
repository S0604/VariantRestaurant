using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TutorialEvent
{
    public string eventID;
    public bool isCompleted = false;
    public UnityEngine.Events.UnityEvent onEventCompleted;
}

public class TutorialProgressManager : MonoBehaviour
{
    private static TutorialProgressManager _instance;
    public static TutorialProgressManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<TutorialProgressManager>();
                if (_instance == null)
                {
                   // Debug.LogError("場景中沒有 TutorialProgressManager！");
                }
            }
            return _instance;
        }
    }

    [SerializeField] private List<TutorialEvent> tutorialEvents = new List<TutorialEvent>();
    private Dictionary<string, TutorialEvent> eventDict = new Dictionary<string, TutorialEvent>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject); // 保證場景裡只有一個
            return;
        }
        _instance = this;
        InitializeDictionary();
    }

    private void InitializeDictionary()
    {
        eventDict.Clear();
        foreach (var ev in tutorialEvents)
        {
            if (!string.IsNullOrEmpty(ev.eventID) && !eventDict.ContainsKey(ev.eventID))
                eventDict.Add(ev.eventID, ev);
        }
    }

    public void CompleteEvent(string eventID)
    {
        Debug.Log($"嘗試完成事件: {eventID}");
        if (eventDict.TryGetValue(eventID, out TutorialEvent ev))
        {
            Debug.Log($"找到事件: {ev.eventID}, 已完成: {ev.isCompleted}");
            if (!ev.isCompleted)
            {
                ev.isCompleted = true;
                ev.onEventCompleted?.Invoke();
                Debug.Log($"[Tutorial] 事件完成: {eventID}");
            }
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
}
