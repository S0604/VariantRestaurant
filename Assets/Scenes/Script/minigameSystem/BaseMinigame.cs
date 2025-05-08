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

    [Header("���a����")]
    public Player player;

    [Header("�ɶ��]�w")]
    public float timeLimit = 10f;
    public float endDelay = 1.5f;

    [Header("���O�ƥ�")]
    public List<RandomEvent> randomEvents;
    protected RandomEvent activeEvent;

    [Header("���O�ϥܥͦ�")]
    public Transform sequenceContainer;
    public GameObject sequenceIconPrefab;

    [Header("�����ʵe�]�w")]
    public Transform endAnimationContainer;
    public GameObject[] endAnimations; // ���\�ʵe�Aindex ���� rank-1
    public Transform failAnimationContainer;
    public GameObject failAnimationPrefab; // ���Ѱʵe

    [Header("���O��")]
    public KeyCode[] wasdKeys = new KeyCode[] { KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D };

    protected float timer;
    protected System.Action<bool, int> onCompleteCallback;
    protected bool isPlaying = false;

    public virtual void StartMinigame(System.Action<bool, int> callback)
    {
        timer = timeLimit;
        onCompleteCallback = callback;
        isPlaying = true;

        cookingUI.SetActive(true);
        backgroundImage.sprite = defaultBackground;
        player.isCooking = true;

        // �q�ƥ��Ƥ��H���D�@�ӡ]�i��^
        if (randomEvents != null && randomEvents.Count > 0)
            activeEvent = randomEvents[Random.Range(0, randomEvents.Count)];
        else
            activeEvent = null;
    }

    protected void UpdateTimer(System.Action onTimeOut)
    {
        timer -= Time.deltaTime;
        if (timer < 0)
        {
            timer = 0;
            if (isPlaying)
            {
                isPlaying = false;
                onTimeOut?.Invoke();
            }
        }

        if (timerBar != null)
            timerBar.fillAmount = timer / timeLimit;
    }

    protected IEnumerator PlayEndAnimation(bool success)
    {
        isPlaying = false;
        int rank = 0;

        if (success)
        {
            float remainPercent = timer / timeLimit;
            if (remainPercent > 0.6f) rank = 3;
            else if (remainPercent > 0.3f) rank = 2;
            else rank = 1;

            Debug.Log("���\�����IRank: {rank}");

            if (rank > 0 && rank <= endAnimations.Length)
            {
                GameObject anim = Instantiate(endAnimations[rank - 1], endAnimationContainer);
                yield return new WaitForSeconds(endDelay);
                Destroy(anim);
            }
        }
        else
        {
            Debug.Log("���ѵ����C���C");

            if (failAnimationPrefab != null && failAnimationContainer != null)
            {
                GameObject failAnim = Instantiate(failAnimationPrefab, failAnimationContainer);
                yield return new WaitForSeconds(endDelay);
                Destroy(failAnim);
            }
            else
            {
                yield return new WaitForSeconds(endDelay);
            }
        }

        FinishMinigame(success, rank);
    }

    protected void FinishMinigame(bool success, int rank = 0)
    {
        cookingUI.SetActive(false);
        player.isCooking = false;
        onCompleteCallback?.Invoke(success, rank);
    }
}
