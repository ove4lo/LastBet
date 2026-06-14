using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Данные игры")]
    public GameState gameState;

    [Header("Порядок игровых сцен")]
    public string[] sceneOrder =
    {
        "Scene1_Cabaret",
        "Scene2_Dressing",
        "Scene3_Bar",
        "Scene4_Casino",
        "Scene5_Backstage",
        "Scene6_Office",
        "Scene7_FinalStake"
    };

    public GameplayState CurrentState { get; private set; } = GameplayState.MainMenu;

    private void Awake()
    {
        Debug.Log($"[GameManager] Awake | name={name} | scene={gameObject.scene.name}", this);

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[GameManager] Duplicate destroyed", this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (gameState == null)
        {
            Debug.LogError("[GameManager] GameState не назначен", this);
            return;
        }

        if (SaveSystem.HasSave())
        {
            SaveSystem.Load(gameState);
            Debug.Log("[GameManager] Сохранение загружено");
        }
        else
        {
            gameState.ResetAll();
        }

        SceneTransition.Instance.FadeToScene("MainMenu");
    }

    private void OnDestroy()
    {
        Debug.LogWarning($"[GameManager] OnDestroy | name={name} | scene={gameObject.scene.name}", this);
    }

    public void StartNewGame()
    {
        if (gameState == null)
            return;

        gameState.ResetAll();
        SaveSystem.Delete();
        LoadSceneByIndex(0);
    }

    public void ContinueGame()
    {
        if (gameState == null)
            return;

        LoadSceneByIndex(gameState.currentSceneIndex);
    }

    public void LoadNextScene()
    {
        if (gameState == null)
            return;

        LoadSceneByIndex(gameState.currentSceneIndex + 1);
    }

    public void LoadSceneByIndex(int index)
    {
        if (gameState == null)
        {
            Debug.LogError("[GameManager] GameState не назначен", this);
            return;
        }

        if (index >= sceneOrder.Length)
        {
            LoadEnding();
            return;
        }

        gameState.currentSceneIndex = index;
        SaveSystem.Save(gameState);
        SetState(GameplayState.Playing);
        SceneTransition.Instance.FadeToScene(sceneOrder[index]);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SetState(GameplayState.MainMenu);
        SceneTransition.Instance.FadeToScene("MainMenu");
    }

    public void LoadMiniGame(string miniGameSceneName, MiniGameType miniGameType)
    {
        if (gameState == null)
        {
            Debug.LogError("[GameManager] GameState не назначен", this);
            return;
        }

        gameState.returnSceneName = sceneOrder[gameState.currentSceneIndex];
        gameState.currentMiniGame = miniGameType;

        SetState(GameplayState.MiniGame);
        SceneTransition.Instance.FadeToScene(miniGameSceneName);
    }

    public void FinishBarMiniGame(bool won)
    {
        if (gameState == null)
        {
            Debug.LogError("[GameManager] GameState не назначен", this);
            return;
        }

        gameState.ApplyBarMiniGameResult(won);
        ReturnFromMiniGame();
    }

    public void FinishMiniGame(bool won)
    {
        if (gameState == null)
        {
            Debug.LogError("[GameManager] GameState не назначен", this);
            return;
        }

        switch (gameState.currentMiniGame)
        {
            case MiniGameType.CardGame:
                gameState.ApplyBarMiniGameResult(won);
                break;

            case MiniGameType.Roulette:
                gameState.AddToken(won ? TokenType.Analysis : TokenType.Obedience);
                break;

            default:
                Debug.LogWarning($"[GameManager] Неизвестная мини-игра: {gameState.currentMiniGame}");
                break;
        }

        ReturnFromMiniGame();
    }

    public void FinishMiniGame(bool won, TokenType token)
    {
        if (gameState == null)
        {
            Debug.LogError("[GameManager] GameState не назначен", this);
            return;
        }

        gameState.AddToken(token);
        ReturnFromMiniGame();
    }
    
    public void FinishJackpotMiniGame(JackpotFinalResult result)
    {
        if (gameState == null)
        {
            Debug.LogError("[GameManager] GameState не назначен", this);
            return;
        }

        gameState.ApplyJackpotResult(result);
        ReturnFromMiniGame();
    }

    public void FinishJokerMiniGame(bool won)
    {
        if (gameState == null)
        {
            Debug.LogError("[GameManager] GameState не назначен", this);
            return;
        }

        gameState.ApplyJokerResult(won);
        Debug.Log($"[GameManager] Джокер завершён. Won={won}");
        ReturnFromMiniGame();
    }

    public void ReturnFromMiniGame()
    {
        SaveSystem.Save(gameState);
        SetState(GameplayState.Playing);

        if (string.IsNullOrEmpty(gameState.returnSceneName))
        {
            Debug.LogWarning("[GameManager] returnSceneName пустой, загружается текущая сюжетная сцена");
            SceneTransition.Instance.FadeToScene(sceneOrder[gameState.currentSceneIndex]);
            return;
        }

        SceneTransition.Instance.FadeToScene(gameState.returnSceneName);
    }

    public void LoadEnding()
    {
        if (gameState == null)
        {
            Debug.LogError("[GameManager] GameState не назначен", this);
            return;
        }

        SetState(GameplayState.Ending);
        SaveSystem.Delete();

        string endingScene = gameState.GetEnding().ToString();
        Debug.Log($"[GameManager] Концовка: {endingScene} (Б:{gameState.revolt} П:{gameState.obedience} А:{gameState.analysis})");
        SceneTransition.Instance.FadeToScene(endingScene);
    }

    public void Pause()
    {
        if (CurrentState != GameplayState.Playing && CurrentState != GameplayState.Dialogue)
            return;

        SetState(GameplayState.Paused);
        Time.timeScale = 0f;
    }

    public void Resume()
    {
        if (CurrentState != GameplayState.Paused)
            return;

        SetState(GameplayState.Playing);
        Time.timeScale = 1f;
    }

    public void OnDialogueStart()
    {
        SetState(GameplayState.Dialogue);
    }

    public void OnDialogueEnd()
    {
        SetState(GameplayState.Playing);
    }

    public bool IsPlaying => CurrentState == GameplayState.Playing;
    public bool IsPaused => CurrentState == GameplayState.Paused;
    public bool IsInDialogue => CurrentState == GameplayState.Dialogue;
    public bool IsInEnding => CurrentState == GameplayState.Ending;

    private void SetState(GameplayState newState)
    {
        CurrentState = newState;
        Debug.Log($"[GameManager] Состояние: {newState}");
    }
}
