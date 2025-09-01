using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public enum EventEffectType { Mutation, Reversal, Extension }



public class RandomEventManager : MonoBehaviour
{
    //public class WASDIconSet  �ª�
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

    [Header("�ƥ� UI")]
    public GameObject eventUIPanel;
    public TMP_Text eventNameText;
    public Slider eventTimerSlider;
    public Image eventWarningIcon;

    [Header("�ƥ�]�w")]
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

        // ���]���A�P�ĪG�A�T�O����I�s�̰��W�ள�쥿�T��T
        currentEffect = (EventEffectType)UnityEngine.Random.Range(0, 3);//�ƥ��ܽd��
        timer = eventDuration;
        isActive = true;

        // �A�B�z UI �P Coroutine
        if (eventUIPanel != null) eventUIPanel.SetActive(true);
        if (eventWarningIcon != null) eventWarningIcon.enabled = true;

        if (eventNameText != null)
            eventNameText.text = $"�H���ƥ�G{GetEventName(currentEffect)}";

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
            case EventEffectType.Mutation: return "�ܧμҦ�";
            case EventEffectType.Reversal: return "��V����";
            case EventEffectType.Extension: return "����W��";
        }
        return "�����ƥ�";
    }
}
