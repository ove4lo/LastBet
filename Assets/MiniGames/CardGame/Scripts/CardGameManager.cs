using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class CardGameManager : MonoBehaviour
{
    [Header("Настройки игры")]
    public int winScoreThreshold = 35;
    public int totalRounds = 8;
    public int cardsToDeal = 6;

    [Header("Генерация клиентов")]
    public Sprite[] customerPortraits;

    [TextArea(2, 4)]
    public string[] requestTexts =
    {
        "Люблю горечь. Сделай выразительный напиток.",
        "Хочу цитрусовый вкус. Без лишней грязи.",
        "Налей что-нибудь зелёное и крепкое.",
        "Смешай три разных вкуса.",
        "Хочу чистый вкус без лишнего.",
        "Только без тухлятины.",
        "Мне нужен рискованный напиток.",
        "Сделай что-нибудь странное, но годное."
    };

    [Header("UI: Клиент")]
    public CustomerView customerView;

    [Header("UI: Слоты ряда")]
    public CardSlot[] rowSlots = new CardSlot[3];

    [Header("UI: Рука игрока")]
    public Transform handPanel;
    public HandFanLayout handFanLayout;
    public GameObject cardViewPrefab;

    [Header("UI: Счёт и раунд")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI roundText;

    [Header("UI: Панель результата")]
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;
    public Button continueButton;

    [Header("DEBUG")]
    public TextMeshProUGUI deckDebugText;

    private CardDeck _deck;
    private CustomerData _currentCustomer;
    private int _totalScore;
    private int _currentRound;

    private readonly System.Collections.Generic.List<CardView> _handViews = new();

    private void Start()
    {
        _deck = GetComponent<CardDeck>();

        if (_deck == null)
        {
            Debug.LogError("[CardGameManager] CardDeck не найден", this);
            return;
        }

        if (cardViewPrefab == null)
        {
            Debug.LogError("[CardGameManager] CardViewPrefab не назначен", this);
            return;
        }

        if (handPanel == null)
        {
            Debug.LogError("[CardGameManager] HandPanel не назначен", this);
            return;
        }

        _deck.Initialize();

        if (resultPanel != null)
            resultPanel.SetActive(false);

        _totalScore = 0;
        _currentRound = 0;

        StartRound();
    }

    private void StartRound()
    {
        _currentRound++;

        if (roundText != null)
            roundText.text = $"Клиент {_currentRound} / {totalRounds}";

        foreach (var slot in rowSlots)
        {
            if (slot != null)
                slot.Clear();
        }

        ClearHand();

        _currentCustomer = GenerateRandomCustomer();

        if (customerView != null)
        {
            customerView.Show(
                _currentCustomer.portraitSprite,
                _currentCustomer.requestText
            );
        }

        var drawnCards = _deck.Draw(cardsToDeal);

        if (drawnCards.Count == 0)
        {
            EndGame(false, "Карты закончились");
            return;
        }

        foreach (var data in drawnCards)
            SpawnCardInHand(data);

        RefreshHandLayout();
        UpdateScoreUI();
    }

    private CustomerData GenerateRandomCustomer()
    {
        var customer = ScriptableObject.CreateInstance<CustomerData>();

        customer.customerName = "Гость";

        if (customerPortraits != null && customerPortraits.Length > 0)
            customer.portraitSprite = customerPortraits[Random.Range(0, customerPortraits.Length)];

        customer.requiredType = RandomRecipeType();
        customer.preferredType = RandomRecipeType();
        customer.bonusForPreferred = 1;

        customer.ruleType = RandomRule();

        customer.requestText = BuildRequestText(customer);

        return customer;
    }

    private CocktailType RandomRecipeType()
    {
        int value = Random.Range(0, 3);

        return value switch
        {
            0 => CocktailType.Bitter,
            1 => CocktailType.Lemonchello,
            _ => CocktailType.Absinthe
        };
    }

    private CustomerRuleType RandomRule()
    {
        int value = Random.Range(0, 5);

        return value switch
        {
            0 => CustomerRuleType.None,
            1 => CustomerRuleType.NoDamagedCards,
            2 => CustomerRuleType.WantsRainbow,
            3 => CustomerRuleType.WantsTriplet,
            _ => CustomerRuleType.NoAdjacencyBonus
        };
    }

    private string BuildRequestText(CustomerData customer)
    {
        if (requestTexts != null && requestTexts.Length > 0)
        {
            string randomText = requestTexts[Random.Range(0, requestTexts.Length)];

            if (!string.IsNullOrWhiteSpace(randomText))
                return randomText;
        }

        string required = TypeName(customer.requiredType);

        return customer.ruleType switch
        {
            CustomerRuleType.NoDamagedCards => $"Хочу {required}. Только без тухлятины.",
            CustomerRuleType.WantsRainbow => $"Хочу {required}. Смешай три разных вкуса.",
            CustomerRuleType.WantsTriplet => $"Хочу {required}. Сделай чистый вкус.",
            CustomerRuleType.NoAdjacencyBonus => $"Хочу {required}. Без хитрых сочетаний.",
            _ => $"Хочу {required}. Любимый вкус: {TypeName(customer.preferredType)}."
        };
    }

    private void SpawnCardInHand(CardData data)
    {
        var go = Instantiate(cardViewPrefab, handPanel);
        var cv = go.GetComponent<CardView>();

        if (cv == null)
        {
            Debug.LogError("[CardGameManager] В CardViewPrefab нет CardView", go);
            Destroy(go);
            return;
        }

        cv.Init(data, this);
        _handViews.Add(cv);

        go.transform.localScale = Vector3.one;
    }

    private void RefreshHandLayout()
    {
        if (handFanLayout != null)
            handFanLayout.Refresh();
    }

    private void ClearHand()
    {
        foreach (var cv in _handViews)
        {
            if (cv != null)
                Destroy(cv.gameObject);
        }

        _handViews.Clear();
    }

    public void OnCardSelectedFromHand(CardView cardView)
    {
        if (cardView == null)
            return;

        bool placed = false;

        foreach (var slot in rowSlots)
        {
            if (slot == null)
                continue;

            if (!slot.HasCard)
            {
                slot.PlaceCard(cardView);
                _handViews.Remove(cardView);
                placed = true;
                break;
            }
        }

        if (!placed)
            return;

        RefreshHandLayout();

        if (RowFull())
            EvaluateRound();
    }

    private bool RowFull()
    {
        foreach (var slot in rowSlots)
        {
            if (slot == null || !slot.HasCard)
                return false;
        }

        return true;
    }

    private void EvaluateRound()
    {
        var cards = new CardData[3];

        for (int i = 0; i < 3; i++)
            cards[i] = rowSlots[i].PlacedCard;

        RoundScoreResult result = CardGameScoring.CalculateRoundScore(cards, _currentCustomer);

        _deck.AddManyToDiscard(cards);

        if (result.IsFatal)
        {
            EndGame(false, result.Reason);
            return;
        }

        if (!result.IsFailed)
            _totalScore += result.Score;

        UpdateScoreUI();

        DOVirtual.DelayedCall(0.8f, () =>
        {
            if (_currentRound >= totalRounds)
                EndGame(_totalScore >= winScoreThreshold);
            else
                StartRound();
        });
    }

    private void EndGame(bool won, string reason = "")
    {
        if (resultPanel != null)
            resultPanel.SetActive(true);

        if (resultText != null)
        {
            if (won)
                resultText.text = $"Клиент доволен!\nИтого: {_totalScore} очков";
            else if (string.IsNullOrEmpty(reason))
                resultText.text = $"Недостаточно качества...\nИтого: {_totalScore} / {winScoreThreshold}";
            else
                resultText.text = $"Поражение!\n{reason}";
        }

        if (resultPanel != null)
        {
            resultPanel.transform.localScale = Vector3.zero;
            resultPanel.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack);
        }

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(() =>
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.FinishMiniGame(won);
                else
                    Debug.LogWarning("[CardGameManager] GameManager.Instance не найден");
            });
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = $"Очки: {_totalScore} / {winScoreThreshold}";

        if (deckDebugText != null && _deck != null)
        {
            deckDebugText.text =
                $"Колода: {_deck.DrawPileCount}\n" +
                $"Сброс: {_deck.DiscardPileCount}";
        }
    }

    private string TypeName(CocktailType type)
    {
        return type switch
        {
            CocktailType.Bitter => "Биттер",
            CocktailType.Lemonchello => "Лимончелло",
            CocktailType.Absinthe => "Абсент",
            CocktailType.Damaged => "Испорченная",
            _ => "любой вкус"
        };
    }
}
