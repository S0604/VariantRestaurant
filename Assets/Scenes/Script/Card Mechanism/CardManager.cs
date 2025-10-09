using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance;

    [Header("卡片列表 (手牌)")]
    public List<CardHoverEffect> cards = new List<CardHoverEffect>();

    [Header("間距設定")]
    public float baseOffset = 80f;

    private void Awake()
    {
        Instance = this;
    }

    public void OnCardHover(CardHoverEffect hoveredCard)
    {
        int hoverIndex = cards.IndexOf(hoveredCard);

        for (int i = 0; i < cards.Count; i++)
        {
            var card = cards[i];
            var clickEffect = card.GetComponent<CardClickEffectUI>();

            if (card == hoveredCard || (clickEffect != null && clickEffect.IsLocked))
                continue; // 🔒 跳過動畫中的卡片

            int distance = Mathf.Abs(i - hoverIndex);
            int direction = (i < hoverIndex) ? -1 : 1;
            float offset = baseOffset / distance;

            card.SetSideOffset(direction * offset);
        }
    }

    public void OnCardExit(CardHoverEffect exitedCard)
    {
        foreach (var card in cards)
        {
            var clickEffect = card.GetComponent<CardClickEffectUI>();
            if (clickEffect != null && clickEffect.IsLocked) continue; // 🔒 動畫中不重設
            card.ResetSideOffset();
        }
    }
}
