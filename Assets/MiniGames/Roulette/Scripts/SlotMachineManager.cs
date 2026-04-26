using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

// ======================================================
// SlotMachineManager.cs
//
// ЧТО ДЕЛАЕТ:
//   Управляет мини-игрой "Старый автомат" в сцене Roulette.
//   Реализует механику ключа: кручения, выбор Сломать/Продолжить,
//   принудительный джекпот на 6-м кручении, токены.
//
// ЛОГИКА ТОКЕНОВ:
//   Джекпот на кручениях 1-3 (до выбора) → +1 Revolt
//   Выбрал "Сломать"                      → +1 Revolt
//   Выбрал "Крутить ещё" (после 3-го)    → +1 Obedience
//     (джекпот на 4, 5, 6 — без доп. токена, только ключ)
//
// ПРИНУДИТЕЛЬНЫЙ ДЖЕКПОТ:
//   На 6-м кручении все три барабана принудительно
//   ставятся на один символ (ForceSymbol) — выглядит как обычный джекпот.
//
// ПОСЛЕ ПОЛУЧЕНИЯ КЛЮЧА:
//   Реплика Эвелин + плейсхолдер звука → GameManager.LoadNextScene()
//
// КУДА КЛАСТЬ:
//   Assets/MiniGames/Roulette/Scripts/SlotMachineManager.cs
//
// ПРИКРЕПЛЯТЬ К:
//   Пустой объект "SlotMachineManager" в сцене Roulette
//   + добавить Audio Source на тот же объект
//
// В INSPECTOR ЗАПОЛНИТЬ:
//   reels[]          — три ReelController (Reel_Left, Center, Right)
//   symbols[]        — 5 спрайтов символов
//   spinButton       — кнопка "Крутить"
//   breakButton      — кнопка "Сломать" (скрыта по умолчанию)
//   continueButton   — кнопка "Крутить ещё" (скрыта по умолчанию)
//   startPanel       — CanvasGroup стартовой панели
//   resultPanel      — CanvasGroup панели результата (ключ получен)
//   choicePanel      — CanvasGroup панели выбора Сломать/Продолжить
//   resultText       — TextMeshPro реплики Эвелин при получении ключа
//   startText        — TextMeshPro текста стартовой панели
//   spinSound        — звук вращения
//   jackpotSound     — звук джекпота
//   breakSound       — звук слома автомата
//   doorSound        — звук открывающейся двери
// ======================================================

public class SlotMachineManager : MonoBehaviour
{
    [Header("━━ Барабаны ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
    [Tooltip("Три компонента ReelController: Reel_Left, Reel_Center, Reel_Right")]
    public ReelController[] reels = new ReelController[3];

    [Header("━━ Символы (5 штук) ━━━━━━━━━━━━━━━━━━━━━━━")]
    [Tooltip("[0]=Туз  [1]=Череп  [2]=Бокал  [3]=Маска  [4]=Роза\n" +
             "Пока нет арта — создай цветные Square спрайты:\n" +
             "ПКМ → Create → 2D → Sprites → Square")]
    public Sprite[] symbols = new Sprite[5];

    [Header("━━ Кнопки ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
    [Tooltip("Кнопка 'Крутить' — основная")]
    public Button spinButton;

    [Tooltip("Кнопка 'Сломать автомат' — появляется после 3-го кручения.\nВыключена по умолчанию.")]
    public Button breakButton;

    [Tooltip("Кнопка 'Крутить ещё' — появляется вместе с 'Сломать'.\nВыключена по умолчанию.")]
    public Button continueButton;

    [Header("━━ UI панели ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
    [Tooltip("CanvasGroup стартовой панели (реплика Эвелин + кнопка Играть)")]
    public CanvasGroup startPanel;

    [Tooltip("CanvasGroup панели результата (ключ получен, реплика, переход)")]
    public CanvasGroup resultPanel;

    [Tooltip("CanvasGroup панели выбора Сломать/Продолжить.\nПоявляется после 3-го кручения.")]
    public CanvasGroup choicePanel;

    [Header("━━ Тексты ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
    [Tooltip("TextMeshPro финальной реплики Эвелин (ключ получен)")]
    public TextMeshProUGUI resultText;

    [Tooltip("TextMeshPro текста на стартовой панели")]
    public TextMeshProUGUI startText;

    [Header("━━ Звуки ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
    [Tooltip("Звук вращения барабанов")]
    public AudioClip spinSound;

    [Tooltip("Звук джекпота — три одинаковых")]
    public AudioClip jackpotSound;

    [Tooltip("Звук слома автомата")]
    public AudioClip breakSound;

    [Tooltip("Звук открывающейся двери — плейсхолдер")]
    public AudioClip doorSound;

    [Header("━━ Настройки ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
    [Tooltip("Задержка между остановками барабанов в секундах")]
    public float reelStopDelay = 0.4f;

    [Tooltip("Задержка перед показом результата после остановки")]
    public float resultDelay   = 0.8f;

    // ── Внутреннее состояние ──────────────────────────

    private AudioSource _audio;
    private bool  _isSpinning       = false;
    private int   _spinCount        = 0;     // сколько раз прокрутили
    private bool  _chosenToContinue = false; // выбрал ли "Крутить ещё"
    private int[] _result           = new int[3]; // индексы выпавших символов

    // Нарративные тексты
    const string TXT_START     = "Старый автомат. Говорят, он сломан.\nГоворят многое.";
    const string TXT_WIN_EARLY = "Три одинаковых.\nЧто-то щёлкнуло внутри. Ключ?\n\n*звук открывающейся двери*";
    const string TXT_WIN_BREAK = "Рычаг сломался.\nИз щели выпал маленький ключ.\n\n*звук открывающейся двери*";
    const string TXT_WIN_LATE  = "Наконец-то. Ключ упал на пол.\nАвтомат мигнул последний раз.\n\n*звук открывающейся двери*";

    // ══════════════════════════════════════════════════
    // ИНИЦИАЛИЗАЦИЯ
    // ══════════════════════════════════════════════════

    void Awake()
    {
        _audio = GetComponent<AudioSource>();
        if (_audio == null) _audio = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        foreach (var reel in reels)
            reel.Initialize(symbols);

        if (startText != null) startText.text = TXT_START;

        // Стартовое состояние UI
        ShowPanel(startPanel, instant: true);
        HidePanel(resultPanel, instant: true);
        HidePanel(choicePanel, instant: true);

        if (spinButton    != null) spinButton.gameObject.SetActive(false);
        if (breakButton   != null) breakButton.gameObject.SetActive(false);
        if (continueButton!= null) continueButton.gameObject.SetActive(false);
    }

    // ══════════════════════════════════════════════════
    // КНОПКИ — подключать через Button.OnClick в Inspector
    // ══════════════════════════════════════════════════

    /// <summary>
    /// Кнопка "Играть" на стартовой панели.
    /// Скрывает стартовый экран и показывает кнопку "Крутить".
    /// Подключить: Button_Start → OnClick → OnStartClicked()
    /// </summary>
    public void OnStartClicked()
    {
        HidePanel(startPanel, instant: false, onDone: () =>
        {
            if (spinButton != null) spinButton.gameObject.SetActive(true);
        });
    }

    /// <summary>
    /// Кнопка "Крутить" — основная игровая.
    /// Подключить: SpinButton → OnClick → OnSpinClicked()
    /// </summary>
    public void OnSpinClicked()
    {
        if (_isSpinning) return;
        StartCoroutine(SpinRoutine());
    }

    /// <summary>
    /// Кнопка "Сломать автомат" — появляется после 3-го кручения.
    /// Даёт +1 Revolt, выдаёт ключ.
    /// Подключить: BreakButton → OnClick → OnBreakClicked()
    /// </summary>
    public void OnBreakClicked()
    {
        HidePanel(choicePanel, instant: false);
        if (spinButton != null) spinButton.gameObject.SetActive(false);

        PlaySound(breakSound);
        GameManager.Instance.gameState.AddToken(TokenType.Revolt);
        Debug.Log("[SlotMachine] Сломала → +1 Revolt");

        ShowKeyResult(TXT_WIN_BREAK);
    }

    /// <summary>
    /// Кнопка "Крутить ещё" — появляется после 3-го кручения.
    /// Даёт +1 Obedience, продолжает игру.
    /// Подключить: ContinueButton → OnClick → OnContinueClicked()
    /// </summary>
    public void OnContinueClicked()
    {
        HidePanel(choicePanel, instant: false);

        // Токен послушания — один раз при первом выборе продолжить
        if (!_chosenToContinue)
        {
            _chosenToContinue = true;
            GameManager.Instance.gameState.AddToken(TokenType.Obedience);
            Debug.Log("[SlotMachine] Выбрала продолжить → +1 Obedience");
        }
    }

    /// <summary>
    /// Кнопка "Продолжить" на финальной панели (ключ получен).
    /// Переход в следующую сцену.
    /// Подключить: Button_Continue → OnClick → OnResultContinueClicked()
    /// </summary>
    public void OnResultContinueClicked()
    {
        GameManager.Instance.LoadNextScene();
    }

    // ══════════════════════════════════════════════════
    // ЛОГИКА ВРАЩЕНИЯ
    // ══════════════════════════════════════════════════

    private IEnumerator SpinRoutine()
    {
        _isSpinning = true;
        if (spinButton != null) spinButton.interactable = false;

        _spinCount++;
        Debug.Log($"[SlotMachine] Кручение #{_spinCount}");

        bool isForcedJackpot = (_spinCount >= 6);

        // Определяем результат
        if (isForcedJackpot)
        {
            // На 6-м кручении — принудительный джекпот
            int jackpotSymbol = Random.Range(0, symbols.Length);
            _result[0] = _result[1] = _result[2] = jackpotSymbol;
            Debug.Log($"[SlotMachine] Принудительный джекпот на 6-м кручении: символ {jackpotSymbol}");
        }
        else
        {
            // Обычные случайные символы
            for (int i = 0; i < 3; i++)
                _result[i] = Random.Range(0, symbols.Length);
        }

        PlaySound(spinSound);

        // Запускаем все барабаны одновременно
        foreach (var reel in reels)
            reel.StartSpin();

        if (isForcedJackpot)
        {
            // На 6-м — даём покрутиться секунду, затем принудительно ставим
            yield return new WaitForSeconds(1.5f);
            for (int i = 0; i < reels.Length; i++)
            {
                reels[i].ForceSymbol(_result[i]);
                yield return new WaitForSeconds(0.3f);
            }
        }
        else
        {
            // Обычная остановка поочерёдно
            for (int i = 0; i < reels.Length; i++)
            {
                yield return new WaitForSeconds(reelStopDelay);
                reels[i].StopSpin(_result[i]);
            }

            // Ждём пока последний барабан остановился
            yield return new WaitUntil(() => !reels[reels.Length - 1].IsSpinning);
        }

        yield return new WaitForSeconds(resultDelay);

        _isSpinning = false;

        // Проверяем результат
        bool isJackpot = _result[0] == _result[1] && _result[1] == _result[2];

        if (isJackpot)
        {
            OnJackpot();
        }
        else
        {
            // Не джекпот — показываем выбор если 3+ кручений
            if (_spinCount >= 3)
                ShowChoicePanel();
            else
                if (spinButton != null) spinButton.interactable = true;
        }
    }

    // Джекпот — выдаём ключ
    private void OnJackpot()
    {
        PlaySound(jackpotSound);

        foreach (var reel in reels)
            reel.PlayJackpotEffect();

        // Токен — только если это был ранний джекпот (до выбора "продолжить")
        if (!_chosenToContinue)
        {
            GameManager.Instance.gameState.AddToken(TokenType.Revolt);
            Debug.Log("[SlotMachine] Джекпот (ранний) → +1 Revolt");
        }
        else
        {
            Debug.Log("[SlotMachine] Джекпот (после продолжения) → токенов нет");
        }

        string msg = _spinCount >= 6 ? TXT_WIN_LATE : TXT_WIN_EARLY;
        ShowKeyResult(msg);
    }

    // Показать выбор Сломать/Продолжить
    private void ShowChoicePanel()
    {
        if (spinButton != null) spinButton.gameObject.SetActive(false);
        ShowPanel(choicePanel, instant: false);
    }

    // Показать финальный экран с репликой Эвелин
    private void ShowKeyResult(string text)
    {
        PlaySound(doorSound);

        if (resultText != null) resultText.text = text;
        ShowPanel(resultPanel, instant: false);
    }

    // ══════════════════════════════════════════════════
    // УТИЛИТЫ
    // ══════════════════════════════════════════════════

    private void PlaySound(AudioClip clip)
    {
        if (_audio != null && clip != null)
            _audio.PlayOneShot(clip);
    }

    private void ShowPanel(CanvasGroup g, bool instant, System.Action onDone = null)
    {
        if (g == null) return;
        g.gameObject.SetActive(true);
        if (instant)
        {
            g.alpha = 1f; g.interactable = true; g.blocksRaycasts = true;
            onDone?.Invoke();
            return;
        }
        g.alpha = 0f; g.interactable = false; g.blocksRaycasts = false;
        g.DOFade(1f, 0.4f).SetUpdate(true).OnComplete(() =>
        {
            g.interactable = true; g.blocksRaycasts = true;
            onDone?.Invoke();
        });
    }

    private void HidePanel(CanvasGroup g, bool instant, System.Action onDone = null)
    {
        if (g == null) return;
        if (instant)
        {
            g.alpha = 0f; g.interactable = false; g.blocksRaycasts = false;
            g.gameObject.SetActive(false); onDone?.Invoke();
            return;
        }
        g.interactable = false; g.blocksRaycasts = false;
        g.DOFade(0f, 0.35f).SetUpdate(true).OnComplete(() =>
        {
            g.gameObject.SetActive(false); onDone?.Invoke();
        });
    }
}