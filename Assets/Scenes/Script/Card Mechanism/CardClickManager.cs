using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardClickManager : MonoBehaviour
{
    public static CardClickManager Instance;

    private Queue<CardClickEffectUI> queue = new Queue<CardClickEffectUI>();
    private bool isRunning = false;

    private void Awake()
    {
        Instance = this;
    }

    public void EnqueueCard(CardClickEffectUI card)
    {
        queue.Enqueue(card);

        if (!isRunning)
            StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        isRunning = true;

        while (queue.Count > 0)
        {
            var currentCard = queue.Dequeue();

            // 👉 等待卡片「進場 + 停留」，但不等離場
            yield return StartCoroutine(currentCard.PlayEnterAndStay());

            // 下一張卡若存在 → 先進等待狀態
            if (queue.Count > 0)
            {
                var nextCard = queue.Peek();
                nextCard.SetWaitingState(true);
            }

            // 👉 離場自己跑，不影響下一張
            StartCoroutine(currentCard.PlayExit());
        }

        isRunning = false;
    }
}
