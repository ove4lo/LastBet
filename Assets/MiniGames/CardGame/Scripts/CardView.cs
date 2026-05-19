using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(Image))]
public class CardView : MonoBehaviour,
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler
{
    public CardData Data { get; private set; }

    private CardGameManager _manager;

    private Image _image;

    private bool _interactable = true;

    public void Init(CardData data, CardGameManager manager)
    {
        Data = data;

        _manager = manager;

        _image = GetComponent<Image>();

        if (_image != null && data != null)
        {
            _image.sprite = data.cardSprite;

            _image.color = Color.white;

            _image.preserveAspect = true;
        }
    }

    public void SetInteractable(bool value)
    {
        _interactable = value;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!_interactable)
            return;

        transform.DOKill();

        transform.DOPunchScale(
            Vector3.one * 0.08f,
            0.15f,
            5,
            0.5f
        );

        if (_manager != null)
        {
            _manager.OnCardSelectedFromHand(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_interactable)
            return;

        transform.DOKill();

        transform.DOScale(
            1.05f,
            0.12f
        );
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_interactable)
            return;

        transform.DOKill();

        transform.DOScale(
            1f,
            0.12f
        );
    }
}