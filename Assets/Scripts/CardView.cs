using UnityEngine;

/// <summary>
/// 카드별 IsFaceUp 저장
/// </summary>
public class CardView
{

    public GameObject Card { get; private set; }

    public bool IsFaceUp;

    public CardView(GameObject card)
    {
        Card = card;
        IsFaceUp = false;
    }
}
