using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManagerCard : MonoBehaviour
{
    public static GameManagerCard Instance;

    // Префабы и спрайты
    public Card cardPrefab;
    public Sprite cardBack;
    public Sprite[] fullDeck;
    public Sprite jokerSprite;

    // Коллекции карт
    private List<Card> cards = new List<Card>();
    private List<int> cardIDs = new List<int>();
    public Card firstCard, secondCard; // Текущие выбранные карты

    // UI элементы
    public Transform cardHolder;
    public GameObject UIElements;
    public TextMeshProUGUI finalText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI livesText;

    // Группы интерфейса
    public CanvasGroup startUIGroup;
    public CanvasGroup finalUIGroup;

    // Игровые переменные
    private int pairsMatched = 0; // Количество собранных пар
    private const int totalPairs = 4; // Всего пар для победы
    private float timer; // Таймер игры
    private bool isGameActive = false; // Активна ли игра
    public float maxTime = 60f; // Максимальное время игры
    public int lives = 3; // Количество жизней


    private void Awake()
    {
        // Реализация синглтона
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // Метод начала
    private void Start()
    {
        // Инициализация игровых переменных
        timer = maxTime;
        lives = 3;
        isGameActive = false;
        pairsMatched = 0;
        firstCard = secondCard = null;

        // Настройка видимости UI элементов
        SetActiveSafe(cardHolder?.gameObject, false);
        SetActiveSafe(UIElements, false);
        SetActiveSafe(finalUIGroup?.gameObject, false);

        // Показ стартового экрана с анимацией
        if (startUIGroup != null)
        {
            startUIGroup.alpha = 0;
            startUIGroup.gameObject.SetActive(true);
            StartCoroutine(FadeIn(startUIGroup, 1.5f));
        }
    }

    // Обработка клика по Джокеру
    public void JokerClicked(Card jokerCard)
    {
        if (!isGameActive) return;

        Debug.Log($"[GM] Джокер найден! Отнимаем жизнь");
        
        lives--; // Уменьшаем количество жизней
        UpdateLivesText();
        
        // Сбрасываем текущие карты
        firstCard = null;
        secondCard = null;
        
        if (lives <= 0) 
        {
            Lose(); // Проигрыш если жизни закончились
        }
        else 
        {
            // Перемешиваем существующие карты после нахождения Джокера
            StartCoroutine(ReshuffleAfterJoker());
        }
    }

    // Корутина для перемешивания карт после Джокера
    private IEnumerator ReshuffleAfterJoker()
    {
        yield return new WaitForSeconds(0.3f);
        ReshuffleExistingCards();
    }

    // Метод перемешивания существующих карт
    private void ReshuffleExistingCards()
    {
        // Сбрасываем счетчик пар
        pairsMatched = 0;
        
        // Сохраняем текущие ID карт
        List<int> currentIDs = new List<int>();
        foreach (var card in cards)
        {
            currentIDs.Add(card.cardID);
            card.ResetCard(); // Сбрасываем состояние карты
        }
        
        // Перемешиваем ID
        for (int i = 0; i < currentIDs.Count; i++)
        {
            int randomIndex = Random.Range(i, currentIDs.Count);
            int temp = currentIDs[i];
            currentIDs[i] = currentIDs[randomIndex];
            currentIDs[randomIndex] = temp;
        }
        
        // Присваиваем новые ID картам
        for (int i = 0; i < cards.Count; i++)
        {
            cards[i].cardID = currentIDs[i];
            cards[i].ResetCard();
        }
        
        // Сбрасываем выбранные карты
        firstCard = null;
        secondCard = null;
        
        Debug.Log($"[GM] Карты перемешаны (после джокера). Счетчик пар сброшен: {pairsMatched}");
    }

    // Запуск игры со стартового экрана
    public void StartGame()
    {
        StartCoroutine(HideAndStart(startUIGroup));
    }

    // Рестарт игры с финального экрана
    public void RestartGame()
    {
        StartCoroutine(HideAndStart(finalUIGroup));
    }

    // Корутина скрытия UI и начала игры
    private IEnumerator HideAndStart(CanvasGroup group)
    {
        yield return StartCoroutine(FadeOut(group, 0.8f));
        BeginGameplay();
    }

    // Начало игрового процесса
    private void BeginGameplay()
    {
        // Активируем игровые UI элементы
        SetActiveSafe(cardHolder?.gameObject, true);
        SetActiveSafe(UIElements, true);

        // Сброс игрового состояния
        pairsMatched = 0;
        lives = 3;
        timer = maxTime;
        firstCard = secondCard = null;
        isGameActive = true;
        
        // Обновление UI
        UpdateLivesText();
        UpdateTimerText();

        // Создание и расстановка карт
        ClearCards();
        CreateCards();
        ShuffleAndPlace();
    }

    // Создание набора карт для игры
    private void CreateCards()
    {
        cardIDs.Clear();

        // Создаем список доступных карт (52 стандартные)
        List<int> available = new List<int>();
        for (int i = 0; i < 52; i++)
            available.Add(i);

        // Выбираем 4 случайные карты для создания пар
        List<int> selected = new List<int>();
        while (selected.Count < 4)
        {
            int idx = Random.Range(0, available.Count);
            selected.Add(available[idx]);
            available.RemoveAt(idx);
        }

        // Создаем пары карт
        foreach (int id in selected)
        {
            cardIDs.Add(id);
            cardIDs.Add(id);
        }

        // Добавляем Джокера
        cardIDs.Add(999);

        // Перемешиваем все ID карт
        for (int i = 0; i < cardIDs.Count; i++)
        {
            int j = Random.Range(i, cardIDs.Count);
            int temp = cardIDs[i];
            cardIDs[i] = cardIDs[j];
            cardIDs[j] = temp;
        }
    }

    // Создание и расстановка карт на поле
    private void ShuffleAndPlace()
    {
        cards.Clear();
        for (int i = 0; i < cardIDs.Count; i++)
        {
            // Создаем новую карту
            Card newCard = Instantiate(cardPrefab, cardHolder);
            newCard.cardID = cardIDs[i];
            newCard.gameManager = this;
            newCard.ShowBack(); // Показываем рубашку
            
            // Расчет позиции карты в сетке 3x3
            int row = i / 3;
            int col = i % 3;
            RectTransform rect = newCard.GetComponent<RectTransform>();
            rect.anchoredPosition = new Vector2(
                col * 150 - 150, // Позиция по X
                -row * 200 + 100 // Позиция по Y
            );
            
            cards.Add(newCard);
        }
    }

    // Обработка клика по карте
    public void CardClicked(Card card)
    {
        if (!isGameActive || firstCard == card || secondCard == card) return;

        if (firstCard == null) 
        {
            firstCard = card; // Первая выбранная карта
        }
        else if (secondCard == null)
        {
            secondCard = card; // Вторая выбранная карта
            Invoke(nameof(CheckMatch), 0.2f); // Проверяем совпадение с задержкой
        }
    }

    // Проверка возможности переворота карты
    public bool CanFlipCard() => isGameActive && (firstCard == null || secondCard == null);

    // Проверка совпадения двух выбранных карт
    private void CheckMatch()
    {
        if (firstCard.cardID == secondCard.cardID)
        {
            pairsMatched++; // Увеличиваем счетчик совпадений
            if (pairsMatched == totalPairs) Win(); // Проверяем победу
            firstCard = secondCard = null; // Сбрасываем выбранные карты
        }
        else
        {
            StartCoroutine(FlipBack()); // Карты не совпали - переворачиваем обратно
        }
    }

    // Корутина переворота несовпавших карт
    private IEnumerator FlipBack()
    {
        yield return new WaitForSeconds(0.5f);
        firstCard.HideCard();
        secondCard.HideCard();
        firstCard = secondCard = null;
    }

    // Полные пересоздание и перемешивание карт
    private void Reshuffle()
    {
        ClearCards();
        CreateCards();
        ShuffleAndPlace();
    }

    // Обработка победы
    private void Win()
    {
        isGameActive = false;
        FinalPanel("Ты обманула судьбу... на этот раз.");
    }

    // Обработка проигрыша
    private void Lose()
    {
        isGameActive = false;
        FinalPanel("Яд подействовал... Ты проиграла.");
    }

    // Показ финальной панели
    private void FinalPanel(string msg)
    {
        if (finalText != null) finalText.text = msg;
        if (finalUIGroup != null)
        {
            // Убедимся, что панель интерактивна перед показом
            finalUIGroup.blocksRaycasts = true;
            finalUIGroup.interactable = true;
            StartCoroutine(FadeIn(finalUIGroup, 1.2f));
        }
    }

    private void Update()
    {
        if (isGameActive && timer > 0)
        {
            // Обновление таймера игры
            timer -= Time.deltaTime;
            UpdateTimerText();
            if (timer <= 0) Lose(); // Время вышло - проигрыш
        }
    }

    // Обновление текста таймера
    private void UpdateTimerText()
    {
        if (timerText != null)
            timerText.text = "Время: " + Mathf.CeilToInt(timer) + "с";
    }

    // Обновление текста жизней
    private void UpdateLivesText()
    {
        if (livesText != null)
            livesText.text = "Жизни: " + lives;
    }

    // Безопасное включение/выключение GameObject
    private void SetActiveSafe(GameObject obj, bool state)
    {
        if (obj != null) obj.SetActive(state);
    }

    // Очистка всех карт
    private void ClearCards()
    {
        foreach (var card in cards)
            if (card != null) Destroy(card.gameObject);
        cards.Clear();
        cardIDs.Clear();
    }

    // Корутина плавного появления UI
    private IEnumerator FadeIn(CanvasGroup group, float duration)
    {
        if (group == null) yield break;
        group.gameObject.SetActive(true);
        
        // Включаем интерактивность
        group.blocksRaycasts = true;
        group.interactable = true;
        
        group.alpha = 0;
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            group.alpha = t / duration;
            yield return null;
        }
        group.alpha = 1;
    }

    // Корутина плавного исчезновения UI
    private IEnumerator FadeOut(CanvasGroup group, float duration)
    {
        if (group == null) yield break;
        group.alpha = 1;
        float t = 0;
        while (t < duration)
        {
            t += Time.deltaTime;
            group.alpha = 1 - (t / duration);
            yield return null;
        }
        group.alpha = 0;
        
        group.gameObject.SetActive(false);
    }
}