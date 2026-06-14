using UnityEngine;

[CreateAssetMenu(fileName = "GameState", menuName = "Game/GameState")]
public class GameState : ScriptableObject
{
    // =========================================================
    // ЖЕТОНЫ
    // =========================================================

    [Header("Жетоны (скрытые от игрока)")]
    [Tooltip("Бунт — за сопротивление и дерзкие выборы")]
    public int revolt;

    [Tooltip("Послушание — за подчинение и согласие")]
    public int obedience;

    [Tooltip("Анализ — за наблюдение, внимательность и понимание ситуации")]
    public int analysis;


    // =========================================================
    // ПРОГРЕСС
    // =========================================================

    [Header("Прогресс прохождения")]
    [Tooltip("Индекс текущей сцены в массиве sceneOrder[] в GameManager")]
    public int currentSceneIndex;

    [Tooltip("Имя сцены для возврата после мини-игры")]
    public string returnSceneName;

    [Tooltip("Тип запущенной мини-игры")]
    public MiniGameType currentMiniGame;


    // =========================================================
    // СЦЕНА С КОКТЕЙЛЕМ В ГРИМЁРКЕ
    // =========================================================

    [Header("Коктейль в гримёрке")]
    [Tooltip("Выпила ли Эвелин коктейль хоть раз")]
    public bool cocktailDrunk;

    [Tooltip("Сколько раз Эвелин выпила коктейль")]
    public int cocktailCount;

    [Tooltip("Осмотрела ли Эвелин коктейль")]
    public bool cocktailInspected;


    // =========================================================
    // МИНИ-ИГРА КОКТЕЙЛИ У ЛЕО
    // =========================================================

    [Header("Коктейли у Лео")]
    [Tooltip("Мини-игра коктейлей завершена")]
    public bool barMiniGameCompleted;

    [Tooltip("Мини-игра коктейлей выиграна")]
    public bool barMiniGameWon;

    [Tooltip("План полуночи известен")]
    public bool midnightPlanKnown;

    [Tooltip("Ключ от кабинета Виктора получен")]
    public bool officeKeyObtained;

    [Tooltip("Искажение восприятия усилено")]
    public bool distortionIncreased;


    // =========================================================
    // ДЖЕКПОТ / СТАРЫЙ АВТОМАТ
    // =========================================================

    [Header("Джекпот / Старый автомат")]
    [Tooltip("Мини-игра Джекпот завершена")]
    public bool jackpotCompleted;

    [Tooltip("Игрок отказался играть")]
    public bool jackpotRefused;

    [Tooltip("Итог мини-игры по новой модели")]
    public JackpotOutcome jackpotOutcome;

    [Tooltip("Основной скрытый стиль результата")]
    public JackpotBehaviourToken jackpotToken;

    [Tooltip("Левый символ итоговой комбинации")]
    public JackpotSymbolType jackpotLeftSymbol;

    [Tooltip("Центральный символ итоговой комбинации")]
    public JackpotSymbolType jackpotCenterSymbol;

    [Tooltip("Правый символ итоговой комбинации")]
    public JackpotSymbolType jackpotRightSymbol;

    [Tooltip("Сколько Бунта начислил джекпот")]
    public int jackpotRevoltDelta;

    [Tooltip("Сколько Послушания начислил джекпот")]
    public int jackpotObedienceDelta;

    [Tooltip("Сколько Анализа начислил джекпот")]
    public int jackpotAnalysisDelta;

    [Tooltip("Количество сделанных прокруток")]
    public int jackpotSpinCount;

    [Tooltip("Выпал ли джекпот")]
    public bool jackpotIsJackpot;

    [Tooltip("Получена ли карта Джокера из автомата")]
    public bool jokerCardObtained;

    [Tooltip("Использована ли карта Джокера позже")]
    public bool jokerCardUsed;


    // =========================================================
    // МЕТОДЫ — ЖЕТОНЫ
    // =========================================================

    public void AddToken(TokenType type, int amount = 1)
    {
        if (amount <= 0)
            return;

        switch (type)
        {
            case TokenType.Revolt:
                revolt += amount;
                break;

            case TokenType.Obedience:
                obedience += amount;
                break;

            case TokenType.Analysis:
                analysis += amount;
                break;
        }

        Debug.Log($"[Жетон] +{amount} {type} | Итого → Бунт:{revolt} Послушание:{obedience} Анализ:{analysis}");
    }

    public EndingType GetEnding()
    {
        if (revolt > obedience && revolt > analysis)
            return EndingType.Freedom;

        if (obedience > revolt && obedience > analysis)
            return EndingType.Submission;

        return EndingType.Death;
    }


    // =========================================================
    // МЕТОДЫ — КОКТЕЙЛЬ В ГРИМЁРКЕ
    // =========================================================

    public void DrinkCocktail()
    {
        cocktailDrunk = true;
        cocktailCount++;

        AddToken(TokenType.Obedience);

        if (cocktailCount > 1)
            AddToken(TokenType.Analysis);

        Debug.Log($"[Коктейль] Выпит раз: {cocktailCount}");
    }

    public void RefuseCocktail()
    {
        AddToken(TokenType.Revolt);
        Debug.Log("[Коктейль] Отказ → +1 Бунт");
    }

    public void InspectCocktail()
    {
        cocktailInspected = true;
        AddToken(TokenType.Analysis);
        Debug.Log("[Коктейль] Осмотрен → +1 Анализ");
    }


    // =========================================================
    // МЕТОДЫ — МИНИ-ИГРА КОКТЕЙЛИ У ЛЕО
    // =========================================================

    public void ApplyBarMiniGameResult(bool won)
    {
        barMiniGameCompleted = true;
        barMiniGameWon = won;
        officeKeyObtained = true;

        if (won)
        {
            midnightPlanKnown = true;
            AddToken(TokenType.Analysis);
            Debug.Log("[Коктейли] Победа → Анализ +1, ключ получен, план полуночи известен");
        }
        else
        {
            distortionIncreased = true;
            AddToken(TokenType.Obedience);
            Debug.Log("[Коктейли] Поражение → Послушание +1, ключ получен, искажение усилено");
        }
    }


    // =========================================================
    // МЕТОДЫ — ДЖЕКПОТ / СТАРЫЙ АВТОМАТ
    // =========================================================

    public void ApplyJackpotResult(JackpotFinalResult result)
    {
        if (result == null)
        {
            Debug.LogWarning("[Джекпот] Пустой результат не сохранён");
            return;
        }

        jackpotCompleted = true;
        jackpotRefused = result.Refused;

        jackpotOutcome = result.Outcome;
        jackpotToken = result.Token;

        jackpotLeftSymbol = result.LeftSymbol;
        jackpotCenterSymbol = result.CenterSymbol;
        jackpotRightSymbol = result.RightSymbol;

        jackpotRevoltDelta = result.RevoltDelta;
        jackpotObedienceDelta = result.ObedienceDelta;
        jackpotAnalysisDelta = result.AnalysisDelta;

        jackpotSpinCount = result.SpinCount;
        jackpotIsJackpot = result.IsJackpot;

        if (result.JokerCardObtained)
            jokerCardObtained = true;

        AddToken(TokenType.Revolt, result.RevoltDelta);
        AddToken(TokenType.Obedience, result.ObedienceDelta);
        AddToken(TokenType.Analysis, result.AnalysisDelta);

        Debug.Log(
            $"[Джекпот] {result.Title} | " +
            $"Jackpot={result.IsJackpot}, JokerCard={result.JokerCardObtained}, " +
            $"Spins={result.SpinCount}, R={result.RevoltDelta}, " +
            $"O={result.ObedienceDelta}, A={result.AnalysisDelta}"
        );
    }


    // =========================================================
    // СБРОС
    // =========================================================

    public void ResetAll()
    {
        ResetTokens();
        ResetProgress();
        ResetDressingCocktail();
        ResetBarMiniGame();
        ResetJackpot();

        Debug.Log("[GameState] Сброс выполнен");
    }

    private void ResetTokens()
    {
        revolt = 0;
        obedience = 0;
        analysis = 0;
    }

    private void ResetProgress()
    {
        currentSceneIndex = 0;
        returnSceneName = "";

        // Раскомментируй только если в MiniGameType есть None.
        // currentMiniGame = MiniGameType.None;
    }

    private void ResetDressingCocktail()
    {
        cocktailDrunk = false;
        cocktailCount = 0;
        cocktailInspected = false;
    }

    private void ResetBarMiniGame()
    {
        barMiniGameCompleted = false;
        barMiniGameWon = false;
        midnightPlanKnown = false;
        officeKeyObtained = false;
        distortionIncreased = false;
    }

    private void ResetJackpot()
    {
        jackpotCompleted = false;
        jackpotRefused = false;

        jackpotOutcome = JackpotOutcome.Refused;
        jackpotToken = JackpotBehaviourToken.None;

        jackpotLeftSymbol = JackpotSymbolType.Blank;
        jackpotCenterSymbol = JackpotSymbolType.Blank;
        jackpotRightSymbol = JackpotSymbolType.Blank;

        jackpotRevoltDelta = 0;
        jackpotObedienceDelta = 0;
        jackpotAnalysisDelta = 0;

        jackpotSpinCount = 0;
        jackpotIsJackpot = false;

        jokerCardObtained = false;
        jokerCardUsed = false;
    }
}