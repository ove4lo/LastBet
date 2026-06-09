using UnityEngine;

[CreateAssetMenu(fileName = "GameState", menuName = "Game/GameState")]
public class GameState : ScriptableObject
{
    [Header("Жетоны (скрытые от игрока)")]
    [Tooltip("Бунт — сопротивление контролю")]
    public int revolt;

    [Tooltip("Послушание — принятие контроля")]
    public int obedience;

    [Tooltip("Анализ — внимательность и понимание манипуляции")]
    public int analysis;

    [Header("Прогресс прохождения")]
    public int currentSceneIndex;
    public string returnSceneName;
    public MiniGameType currentMiniGame;

    [Header("Гримёрка / коктейль")]
    public bool cocktailDrunk;
    public bool cocktailInspected;
    public int cocktailCount;

    [Header("Бар Лео / мини-игра Коктейли")]
    public bool barMiniGameWon;
    public bool officeKeyObtained;
    public bool midnightPlanKnown;

    [Header("Джекпот — скрытые последствия")]
    public bool jackpotCompleted;
    public string jackpotOutcome;
    public string jackpotRiskLevel;
    public int jackpotRiskScore;
    public int jackpotDebt;
    public int jackpotReward;
    public int jackpotSpinCount;
    public bool jackpotStoppedByPlayer;
    public bool jackpotSawHairpin;
    public bool jackpotSawDebt;
    public string leoRelationAfterJackpot;

    [Header("Кабинет Виктора / Джокер")]
    public bool jokerWon;
    public bool truthAvailable;

    [Header("Финальная мини-игра")]
    public bool finalBetWon;

    public void AddToken(TokenType type, int amount = 1)
    {
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

    public void DrinkCocktail()
    {
        cocktailDrunk = true;
        cocktailCount++;
        AddToken(TokenType.Obedience);
        Debug.Log($"[Коктейль] Выпит раз: {cocktailCount} → +1 Послушание");
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

    public void ApplyBarMiniGameResult(bool won)
    {
        barMiniGameWon = won;
        officeKeyObtained = true;

        if (won)
        {
            midnightPlanKnown = true;
            AddToken(TokenType.Analysis);
            Debug.Log("[Бар Лео] Победа → +1 Анализ, ключ получен, план полуночи известен");
        }
        else
        {
            midnightPlanKnown = false;
            AddToken(TokenType.Obedience);
            Debug.Log("[Бар Лео] Провал → +1 Послушание, ключ всё равно получен");
        }
    }

    public void ApplyJackpotResult(
        string outcome,
        string token,
        string leoRelationState,
        string riskLevel,
        int spinCount,
        int reward,
        int debt,
        int riskScore,
        bool stoppedByPlayer,
        bool sawHairpin,
        bool sawDebt)
    {
        jackpotCompleted = true;
        jackpotOutcome = outcome;
        leoRelationAfterJackpot = leoRelationState;
        jackpotRiskLevel = riskLevel;
        jackpotSpinCount = spinCount;
        jackpotReward = reward;
        jackpotDebt = debt;
        jackpotRiskScore = riskScore;
        jackpotStoppedByPlayer = stoppedByPlayer;
        jackpotSawHairpin = sawHairpin;
        jackpotSawDebt = sawDebt;

        AddToken(ParseToken(token));

        Debug.Log(
            $"[Джекпот] Итог={outcome} | Токен={token} | " +
            $"Лео далее={leoRelationState} | Риск={riskLevel}/{riskScore} | " +
            $"Долг={debt} | Прокруты={spinCount} | Заколка={sawHairpin}"
        );
    }

    public void ApplyJokerResult(bool won)
    {
        jokerWon = won;

        if (won)
        {
            truthAvailable = true;
            AddToken(TokenType.Analysis);
            Debug.Log("[Джокер] Победа → +1 Анализ, правда доступна");
        }
        else
        {
            Debug.Log("[Джокер] Провал → без жетона, искажение усиливается визуально");
        }
    }

    public void ApplyFinalBetResult(bool won)
    {
        finalBetWon = won;

        if (won)
        {
            AddToken(TokenType.Analysis);
            Debug.Log("[Последняя ставка] Победа → +1 Анализ");
        }
        else
        {
            AddToken(TokenType.Obedience);
            Debug.Log("[Последняя ставка] Провал → +1 Послушание");
        }
    }

    private TokenType ParseToken(string token)
    {
        switch (token)
        {
            case "Revolt":
                return TokenType.Revolt;

            case "Obedience":
                return TokenType.Obedience;

            case "Analysis":
                return TokenType.Analysis;

            default:
                Debug.LogWarning($"[GameState] Неизвестный токен '{token}', будет использован Analysis.");
                return TokenType.Analysis;
        }
    }

    private void ResetJackpot()
    {
        jackpotCompleted = false;
        jackpotOutcome = "";
        jackpotRiskLevel = "";
        jackpotRiskScore = 0;
        jackpotDebt = 0;
        jackpotReward = 0;
        jackpotSpinCount = 0;
        jackpotStoppedByPlayer = false;
        jackpotSawHairpin = false;
        jackpotSawDebt = false;
        leoRelationAfterJackpot = "";
    }

    public void ResetAll()
    {
        revolt = 0;
        obedience = 0;
        analysis = 0;

        currentSceneIndex = 0;
        returnSceneName = "";
        currentMiniGame = MiniGameType.CardGame;

        cocktailDrunk = false;
        cocktailInspected = false;
        cocktailCount = 0;

        barMiniGameWon = false;
        officeKeyObtained = false;
        midnightPlanKnown = false;

        jokerWon = false;
        truthAvailable = false;
        finalBetWon = false;

        ResetJackpot();
        Debug.Log("[GameState] Сброс выполнен");
    }
}
