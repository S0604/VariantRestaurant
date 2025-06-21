using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemAutoDestroy : MonoBehaviour
{
    void Awake()
    {
        // 找出場景中所有 EventSystem
        var existingSystems = FindObjectsOfType<EventSystem>();

        // 如果有超過 1 個，就刪掉這個新的 EventSystem
        if (existingSystems.Length > 1)
        {
            Debug.LogWarning("已有 EventSystem 存在，銷毀多餘的 EventSystem。");
            Destroy(gameObject);
        }
    }
}
