using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public abstract class BaseMinigame : MonoBehaviour
{
    [Header("基本 UI 設定")]
    public GameObject cookingUI;
    public Image timerBar;
    public Image backgroundImage;
    public Sprite defaultBackground;

    [Header("玩家控制")]
    public Player player;

    [Header("時間設定")]
    public float timeLimit = 10f;
    public float endDelay = 1.5f;

    [Header("指令事件")]
    public List<RandomEvent> randomEvents;
    protected RandomEvent activeEvent;

    [Header("指令圖示生成")]
    public Transform sequenceContainer;
    public GameObject sequenceIconPrefab;

    [Header("完成動畫設定")]
    public Transform endAnimationContainer;
    public GameObject[] endAnimations; // 成功動畫，index 對應 rank-1
    public Transform failAnimationContainer;
    public GameObject failAnimationPrefab; // 失敗動畫

    [Header("指令鍵")]
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

        // 從事件資料中隨機挑一個（可選）
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

            Debug.Log("成功完成！Rank: {rank}");

            if (rank > 0 && rank <= endAnimations.Length)
            {
                GameObject anim = Instantiate(endAnimations[rank - 1], endAnimationContainer);
                yield return new WaitForSeconds(endDelay);
                Destroy(anim);
            }
        }
        else
        {
            Debug.Log("失敗結束遊戲。");

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
