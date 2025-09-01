using System.Collections.Generic;
using UnityEngine;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance;

    [Header("卡片列表 (手牌)")]
    public List<CardHoverEffect> cards = new List<CardHoverEffect>();

    [Header("間距設定")]
    public float baseOffset = 80f; // 基準側移量，越近的卡片越多，越遠越少

    private void Awake()
    {
        Instance = this;
    }

    public void OnCardHover(CardHoverEffect hoveredCard)
    {
        int hoverIndex = cards.IndexOf(hoveredCard);

        for (int i = 0; i < cards.Count; i++)
        {
            CardHoverEffect card = cards[i];
            if (card == hoveredCard) continue;

            int distance = Mathf.Abs(i - hoverIndex); // 與 hover 卡片的距離
            int direction = (i < hoverIndex) ? -1 : 1; // 左邊往左，右邊往右

            float offset = baseOffset / distance; // 越近 offset 越大
            card.SetSideOffset(direction * offset);
        }
    }

    public void OnCardExit(CardHoverEffect exitedCard)
    {
        foreach (var card in cards)
        {
            card.ResetSideOffset();
        }
    }
}
