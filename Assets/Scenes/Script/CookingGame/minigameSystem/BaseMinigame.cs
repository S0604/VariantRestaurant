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

    [Header("���O�ϥ�")]
    public Transform sequenceContainer;
    public GameObject sequenceIconPrefab;

    [Header("�����ʵe")]
    public Transform endAnimationContainer;
    public GameObject[] endAnimations; // �� rank ��ܹ����ʵe
    public Transform failAnimationContainer;
    public GameObject failAnimation;

    [Header("���O��")]
    public KeyCode[] wasdKeys = new KeyCode[] { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };

    [Header("����")]
    public AudioSource audioSource;
    public AudioClip correctSFX;
    public AudioClip wrongSFX;

    public MenuItem baseMenuItem;
    public MenuItem garbageItem;
    protected DishGrade evaluatedGrade;
    protected float timer;
    protected System.Action<bool, int> onCompleteCallback;
    protected bool isPlaying = false;
    private bool hasEnded = false;

    public enum DishGrade { Perfect = 3, Good = 2, Bad = 1, Fail = 0 }

    public static BaseMinigame CurrentInstance { get; private set; }

    // ===== �ƥ�ַӡ]�����T�w�ϥΡ^ =====
    protected bool eventActiveThisRun = false;
    protected EventEffectType eventEffectThisRun = default;
    protected WASDIconSetSO iconSetThisRun = null;

    /// <summary>�b�����}�l�ɩ�U�ƥ󪬺A�P�ϥܲ�</summary>
    protected void CaptureEventContext()
    {
        var mgr = RandomEventManager.Instance;
        if (mgr != null && mgr.IsEventActive)
        {
            eventActiveThisRun = true;
            eventEffectThisRun = mgr.CurrentEffect;
            iconSetThisRun = mgr.GetCurrentIcons(); // �i�ର null
        }
        else
        {
            eventActiveThisRun = false;
            eventEffectThisRun = default;
            iconSetThisRun = (mgr != null) ? mgr.defaultIcons : null; // �i���w�]
        }
    }

    // ���ѵ��l����K����
    protected bool IsEventActiveThisRun() => eventActiveThisRun;
    protected EventEffectType EventEffectThisRun() => eventEffectThisRun;
    protected WASDIconSetSO IconSetThisRun() => iconSetThisRun;

    public virtual void StartMinigame(System.Action<bool, int> callback)
    {
        // ����ַӡA�T�O�������A�T�w
        CaptureEventContext();

        timer = timeLimit;
        onCompleteCallback = callback;
        isPlaying = true;
        hasEnded = false;
        CurrentInstance = this;

        player = MinigameManager.Instance?.player;
        if (player != null) player.isCooking = true;

        cookingUI.SetActive(true);
        backgroundImage.sprite = defaultBackground;
    }

    protected void UpdateTimer()
    {
        if (!isPlaying) return;

        timer -= Time.deltaTime;

        if (timer <= 0 && !hasEnded)
        {
            timer = 0;
            StartCoroutine(PlayFailAnimation());
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

        evaluatedGrade = (DishGrade)rank;
        FinishMinigameInternal(true, rank);
    }

    protected IEnumerator PlayFailAnimation()
    {
        isPlaying = false;
        hasEnded = true;

        GameObject anim = Instantiate(failAnimation, failAnimationContainer);
        yield return new WaitForSeconds(endDelay);
        if (anim != null) Destroy(anim);

        evaluatedGrade = DishGrade.Fail;
        FinishMinigameInternal(false, 0);
    }

    protected virtual void FinishMinigameInternal(bool success, int rank = 0)
    {
        if (player != null) player.isCooking = false;
        GenerateMenuItemByGrade(success ? (DishGrade)rank : DishGrade.Fail);
        cookingUI.SetActive(false);
        onCompleteCallback?.Invoke(success, rank);
    }

    protected void GenerateMenuItemByGrade(DishGrade grade)
    {
        MenuItem item = (grade == DishGrade.Fail || grade == DishGrade.Bad) ? garbageItem : Instantiate(baseMenuItem);
        item.grade = grade;
        InventoryManager.Instance.AddItem(item);
    }

    public static bool HasMaxDishRecords()
    {
        return InventoryManager.Instance.GetItemCount() >= 2;
    }

    protected void PlayCorrectSFX()
    {
        if (audioSource != null && correctSFX != null)
            audioSource.PlayOneShot(correctSFX);
    }

    protected void PlayWrongSFX()
    {
        if (audioSource != null && wrongSFX != null)
            audioSource.PlayOneShot(wrongSFX);
    }

    protected abstract string GetMinigameName();
}
