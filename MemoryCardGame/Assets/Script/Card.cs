using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IPointerClickHandler
{
    public int cardID; // ID карты                          
    public GameManagerCard gameManager; // Ссылка на игровой менеджер        
    public Image cardImage; // Изображение карты                     

    private bool isFlipped = false; // Состояние карты: перевернута или нет        

    private void Start()
    {
        isFlipped = false;
        ShowBack(); // Показываем рубашку карты при старте
        
        // Автоматически находим менеджер игры если не установлен
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManagerCard>();
    }

    // Обработчик клика мыши/тапа по карте
    public void OnPointerClick(PointerEventData eventData)
    {
        OnCardClicked();
    }

    // Основной метод обработки клика по карте
    public void OnCardClicked()
    {
        if (isFlipped) return; // Если карта уже перевернута - выходим

        if (gameManager == null) return;

        // Проверяем можно ли переворачивать карты в текущий момент
        if (!gameManager.CanFlipCard()) return;

        isFlipped = true;
        ShowFace(); // Показываем лицевую сторону
        
        // Джокер обрабатывается сразу
        if (cardID == 999)
        {
            gameManager.JokerClicked(this); // 999 - специальный ID для карты Джокер
            return;
        }
        
        // Сообщаем менеджеру о клике на обычную карту
        gameManager.CardClicked(this);
    }

    // Показать рубашку карты
    public void ShowBack()
    {
        if (cardImage != null && GameManagerCard.Instance != null)
        {
            cardImage.sprite = GameManagerCard.Instance.cardBack;
        }
    }

    // Показать лицевую сторону карты
    public void ShowFace()
    {
        if (cardImage == null || GameManagerCard.Instance == null) return;

        // Отображаем специальный спрайт для Джокера
        if (cardID == 999)
        {
            cardImage.sprite = GameManagerCard.Instance.jokerSprite;
        }
        else
        {
            // Берем спрайт из колоды по ID карты
            cardImage.sprite = GameManagerCard.Instance.fullDeck[cardID];
        }
    }

    // Скрыть карту (перевернуть обратно)
    public void HideCard()
    {
        isFlipped = false;
        ShowBack();
    }
    
    // Сбросить состояние карты
    public void ResetCard()
    {
        isFlipped = false;
        ShowBack();
    }
}