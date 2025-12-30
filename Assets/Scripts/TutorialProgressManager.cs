using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

[System.Serializable]
public class TutorialEvent
{
    public string eventID;
    public bool isCompleted = false;
    public UnityEvent onEventCompleted = new UnityEvent(); // ⭐ 關鍵修正
}

public class TutorialProgressManager : MonoBehaviour
{
    private static TutorialProgressManager _instance;
    public static TutorialProgressManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = FindObjectOfType<TutorialProgressManager>();
            return _instance;
        }
    }

    [SerializeField] private List<TutorialEvent> tutorialEvents = new List<TutorialEvent>();
    private Dictionary<string, TutorialEvent> eventDict = new Dictionary<string, TutorialEvent>();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
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
            if (ev == null || string.IsNullOrEmpty(ev.eventID))
                continue;

            if (!eventDict.ContainsKey(ev.eventID))
                eventDict.Add(ev.eventID, ev);
        }
    }

    /* ---------- 對外 API ---------- */

    /// <summary>
    /// 完成一個教學事件
    /// </summary>
    public void CompleteEvent(string eventID)
    {
        if (string.IsNullOrEmpty(eventID)) return;

        var ev = GetOrCreateEvent(eventID);

        if (ev.isCompleted) return;

        ev.isCompleted = true;
        ev.onEventCompleted.Invoke();

        Debug.Log($"[Tutorial] 事件完成: {eventID}");
    }

    /// <summary>
    /// 詢問事件是否已完成
    /// </summary>
    public bool IsEventCompleted(string eventID)
    {
        return GetOrCreateEvent(eventID).isCompleted;
    }

    /// <summary>
    /// 取得事件（一定不為 null）
    /// </summary>
    public TutorialEvent GetEvent(string eventID)
    {
        return GetOrCreateEvent(eventID);
    }

    /* ---------- 核心 ---------- */

    private TutorialEvent GetOrCreateEvent(string eventID)
    {
        if (eventDict.TryGetValue(eventID, out var ev))
            return ev;

        ev = new TutorialEvent { eventID = eventID };
        tutorialEvents.Add(ev);
        eventDict.Add(eventID, ev);

        Debug.LogWarning($"[Tutorial] 自動建立事件: {eventID}");
        return ev;
    }
}
