using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public enum EventEffectType { Mutation, Reversal, Extension }



public class RandomEventManager : MonoBehaviour
{
    //public class WASDIconSet  舊版
    //{
        //public Sprite up, down, left, right;
        //public Sprite upCorrect, downCorrect, leftCorrect, rightCorrect;
        //public Sprite upWrong, downWrong, leftWrong, rightWrong;
    //}

    public WASDIconSetSO defaultIcons;
    public WASDIconSetSO mutationIcons;
    public WASDIconSetSO reversalIcons;
    public WASDIconSetSO extensionIcons;

    public WASDIconSetSO GetCurrentIcons()
    {
        if (!IsEventActive) return defaultIcons;

        return CurrentEffect switch
        {
            EventEffectType.Mutation => mutationIcons,
            EventEffectType.Reversal => reversalIcons,
            EventEffectType.Extension => extensionIcons,
            _ => defaultIcons
        };
    }


    public static RandomEventManager Instance;

    [Header("事件 UI")]
    public GameObject eventUIPanel;
    public TMP_Text eventNameText;
    public Slider eventTimerSlider;
    public Image eventWarningIcon;

    [Header("事件設定")]
    public float eventDuration = 30f;

    private float timer = 0f;
    private bool isActive = false;
    private EventEffectType currentEffect;

    public bool IsEventActive => isActive;
    public EventEffectType CurrentEffect => currentEffect;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartEvent()
    {
        if (isActive) return;

        // 先設狀態與效果，確保之後呼叫者馬上能拿到正確資訊
        currentEffect = (EventEffectType)UnityEngine.Random.Range(0, 3);//事件選擇範圍
        timer = eventDuration;
        isActive = true;

        // 再處理 UI 與 Coroutine
        if (eventUIPanel != null) eventUIPanel.SetActive(true);
        if (eventWarningIcon != null) eventWarningIcon.enabled = true;

        if (eventNameText != null)
            eventNameText.text = $"隨機事件：{GetEventName(currentEffect)}";

        StartCoroutine(EventCountdown());
    }

    private IEnumerator EventCountdown()
    {
        while (timer > 0f)
        {
            timer -= Time.deltaTime;

            if (eventTimerSlider != null)
                eventTimerSlider.value = timer / eventDuration;

            yield return null;
        }

        EndEvent();
    }

    private void EndEvent()
    {
        isActive = false;
        if (eventUIPanel != null) eventUIPanel.SetActive(false);
        if (eventWarningIcon != null) eventWarningIcon.enabled = false;
    }

    private string GetEventName(EventEffectType effect)
    {
        switch (effect)
        {
            case EventEffectType.Mutation: return "變形模式";
            case EventEffectType.Reversal: return "方向反轉";
            case EventEffectType.Extension: return "按鍵增生";
        }
        return "未知事件";
    }
}
