using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class CardSlot : MonoBehaviour
{
    public Image slotFrame;
    public Vector2 placedCardSize = new Vector2(200f, 270f);

    public CardData PlacedCard { get; private set; }
    public bool HasCard => PlacedCard != null;

    private CardView _currentView;

    public void PlaceCard(CardView cardView)
    {
        if (cardView == null)
            return;

        _currentView = cardView;
        PlacedCard = cardView.Data;

        cardView.SetInteractable(false);
        cardView.KillTweens();

        RectTransform rt = cardView.transform as RectTransform;
        RectTransform slotRt = transform as RectTransform;

        if (rt == null || slotRt == null)
            return;

        rt.SetParent(transform, false);
        rt.anchorMin = new Vector2(0, 0);
        rt.anchorMax = new Vector2(1, 1);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one;
        rt.sizeDelta = Vector2.zero; 
    }

    public void Clear()
    {
        if (_currentView != null)
        {
            _currentView.KillTweens();
            Destroy(_currentView.gameObject);
        }

        _currentView = null;
        PlacedCard = null;
    }
}
