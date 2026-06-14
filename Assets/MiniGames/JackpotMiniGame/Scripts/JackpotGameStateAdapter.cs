using UnityEngine;

public sealed class JackpotGameStateAdapter : MonoBehaviour
{
    [Header("Интеграция")]
    [SerializeField] private bool finishThroughGameManager = true;
    [SerializeField] private bool showDebugLogs = true;

    public void ApplyResult(JackpotFinalResult result)
    {
        if (result == null)
            return;

        if (GameManager.Instance == null || GameManager.Instance.gameState == null)
        {
            Debug.LogWarning("[JackpotGameStateAdapter] GameManager или GameState не найден.");
            return;
        }

        GameManager.Instance.gameState.ApplyJackpotResult(result);

        if (showDebugLogs)
        {
            Debug.Log(
                $"[JackpotGameStateAdapter] Saved | Outcome={result.Outcome}, " +
                $"Jackpot={result.IsJackpot}, Joker={result.JokerCardObtained}, " +
                $"Spins={result.SpinCount}"
            );
        }
    }

    public void FinishMiniGame(JackpotFinalResult result)
    {
        if (result == null)
            return;

        if (GameManager.Instance == null)
        {
            Debug.LogWarning("[JackpotGameStateAdapter] GameManager.Instance не найден.");
            return;
        }

        if (finishThroughGameManager)
        {
            GameManager.Instance.FinishJackpotMiniGame(result);
            return;
        }

        ApplyResult(result);
    }
}
