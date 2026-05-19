using System.Collections.Generic;
using UnityEngine;

public class CardDeck : MonoBehaviour
{
    [Header("Все карты")]
    public List<CardData> allCards = new();

    private List<CardData> _drawPile = new();
    private List<CardData> _discardPile = new();

    public void Initialize()
    {
        _drawPile = new List<CardData>(allCards);
        _discardPile.Clear();
        Shuffle(_drawPile);
    }

    public List<CardData> Draw(int count)
    {
        var drawn = new List<CardData>();

        for (int i = 0; i < count; i++)
        {
            if (_drawPile.Count == 0)
                ReshuffleDiscardIntoDrawPile();

            if (_drawPile.Count == 0)
                break;

            drawn.Add(_drawPile[0]);
            _drawPile.RemoveAt(0);
        }

        return drawn;
    }

    public void AddToDiscard(CardData card)
    {
        if (card == null)
            return;

        _discardPile.Add(card);
    }

    public void AddManyToDiscard(IEnumerable<CardData> cards)
    {
        if (cards == null)
            return;

        foreach (var card in cards)
            AddToDiscard(card);
    }

    private void ReshuffleDiscardIntoDrawPile()
    {
        if (_discardPile.Count == 0)
            return;

        Debug.Log("[CardDeck] Сброс перемешан обратно в колоду");

        _drawPile = new List<CardData>(_discardPile);
        _discardPile.Clear();

        Shuffle(_drawPile);
    }

    private void Shuffle(List<CardData> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    public int DrawPileCount => _drawPile.Count;
    public int DiscardPileCount => _discardPile.Count;
}
