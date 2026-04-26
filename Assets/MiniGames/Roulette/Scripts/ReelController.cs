using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

// ======================================================
// ReelController.cs
//
// ЧТО ДЕЛАЕТ:
//   Один барабан слот-машины. Содержит ленту символов которая
//   прокручивается вертикально. RectMask2D на родительском объекте
//   обрезает всё кроме центральной строки — игрок видит один символ.
//
// КАК РАБОТАЕТ:
//   Initialize() — передаём спрайты, создаём Image-объекты программно
//   StartSpin()  — лента едет вниз бесконечно
//   StopSpin()   — плавное торможение, нужный символ встаёт в центр
//   ForceSymbol()— принудительно ставит символ (для джекпота на 6-м)
//
// СТРУКТУРА В ИЕРАРХИИ:
//   ReelWindow (RectMask2D — 128×128px, видна одна ячейка)
//   └── Reel_Left   (этот скрипт, RectTransform)
//       ├── Symbol_0 (Image, 128×128)
//       ├── Symbol_1 (Image, 128×128)
//       ├── Symbol_2 (Image, 128×128)
//       ├── Symbol_3 (Image, 128×128)
//       └── Symbol_4 (Image, 128×128)
//
// КУДА КЛАСТЬ:
//   Assets/MiniGames/Roulette/Scripts/ReelController.cs
//
// ПРИКРЕПЛЯТЬ К:
//   Объекты Reel_Left, Reel_Center, Reel_Right внутри ReelWindow
//
// В INSPECTOR ЗАПОЛНИТЬ:
//   symbolImages[]   — 5 компонентов Image (или оставь пустым —
//                      создадутся программно через Initialize)
//   symbolHeight     — высота одного символа в пикселях (128)
//   spinSpeed        — скорость прокрутки пикс/сек (1200)
//   slowdownDuration — длительность торможения в секундах (0.5)
// ======================================================

public class ReelController : MonoBehaviour
{
    [Header("━━ Символы ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
    [Tooltip("5 компонентов Image дочерних объектов Symbol_0..4.\n" +
             "Если оставить пустым — создадутся автоматически при Initialize().")]
    public Image[] symbolImages;

    [Header("━━ Параметры ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━")]
    [Tooltip("Высота одного символа в пикселях. Должна совпадать с высотой Image.")]
    public float symbolHeight = 128f;

    [Tooltip("Скорость прокрутки в пикселях в секунду")]
    public float spinSpeed = 1200f;

    [Tooltip("Длительность плавного торможения перед остановкой")]
    public float slowdownDuration = 0.5f;

    // Публичное свойство — SlotMachineManager ждёт пока IsSpinning = false
    public bool IsSpinning { get; private set; }

    private RectTransform _rt;
    private Sprite[]      _sprites;
    private int           _count;
    private float         _totalHeight;
    private Coroutine     _spinCoroutine;

    // ══════════════════════════════════════════════════
    // ИНИЦИАЛИЗАЦИЯ
    // ══════════════════════════════════════════════════

    /// <summary>
    /// Инициализировать барабан спрайтами.
    /// Вызывается из SlotMachineManager.Start().
    /// Если symbolImages пустой — создаёт Image объекты программно.
    /// </summary>
    public void Initialize(Sprite[] sprites)
    {
        _rt          = GetComponent<RectTransform>();
        _sprites     = sprites;
        _count       = sprites.Length;
        _totalHeight = symbolHeight * _count;

        if (symbolImages == null || symbolImages.Length == 0)
            CreateSymbolImages();

        // Случайные символы на старте — выглядит живее
        RandomizeSymbols();
    }

    // Создаёт 5 дочерних Image программно
    private void CreateSymbolImages()
    {
        // Чистим старые дочерние объекты
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        symbolImages = new Image[_count];

        for (int i = 0; i < _count; i++)
        {
            var go  = new GameObject($"Symbol_{i}");
            go.transform.SetParent(transform, false);

            var img            = go.AddComponent<Image>();
            img.sprite         = _sprites[i];
            img.preserveAspect = true;

            var rt              = go.GetComponent<RectTransform>();
            rt.sizeDelta        = new Vector2(symbolHeight, symbolHeight);
            // Символ 0 в центре (Y=0), остальные выше (Y=+128, +256...)
            rt.anchoredPosition = new Vector2(0, i * symbolHeight);

            symbolImages[i] = img;
        }
    }

    private void RandomizeSymbols()
    {
        foreach (var img in symbolImages)
            img.sprite = _sprites[Random.Range(0, _count)];
    }

    // ══════════════════════════════════════════════════
    // ВРАЩЕНИЕ
    // ══════════════════════════════════════════════════

    /// <summary>
    /// Начать непрерывное вращение.
    /// Вызывается из SlotMachineManager когда игрок нажал "Крутить".
    /// </summary>
    public void StartSpin()
    {
        if (IsSpinning) return;
        if (_spinCoroutine != null) StopCoroutine(_spinCoroutine);
        _spinCoroutine = StartCoroutine(SpinLoop());
    }

    /// <summary>
    /// Плавно остановить барабан на нужном символе.
    /// symbolIndex — какой символ (0..4) должен оказаться в центре.
    /// Вызывается поочерёдно для каждого барабана с небольшой задержкой.
    /// </summary>
    public void StopSpin(int symbolIndex)
    {
        if (!IsSpinning) return;
        if (_spinCoroutine != null) StopCoroutine(_spinCoroutine);
        _spinCoroutine = StartCoroutine(StopRoutine(symbolIndex));
    }

    /// <summary>
    /// Мгновенно поставить нужный символ без анимации.
    /// Используется для принудительного джекпота на 6-м кручении.
    /// </summary>
    public void ForceSymbol(int symbolIndex)
    {
        if (_spinCoroutine != null)
        {
            StopCoroutine(_spinCoroutine);
            _spinCoroutine = null;
        }

        IsSpinning = false;
        _rt.anchoredPosition = new Vector2(_rt.anchoredPosition.x, -symbolIndex * symbolHeight);
    }

    // Бесконечная прокрутка вниз с зацикливанием
    private IEnumerator SpinLoop()
    {
        IsSpinning = true;

        while (true)
        {
            _rt.anchoredPosition += Vector2.down * spinSpeed * Time.deltaTime;

            // Зацикливаем: когда лента уехала на полную высоту — возвращаем наверх
            if (_rt.anchoredPosition.y <= -_totalHeight)
            {
                var pos = _rt.anchoredPosition;
                pos.y += _totalHeight;
                _rt.anchoredPosition = pos;
            }

            yield return null;
        }
    }

    // Плавная остановка через DOTween
    private IEnumerator StopRoutine(int symbolIndex)
    {
        float targetY    = -symbolIndex * symbolHeight;
        float currentY   = _rt.anchoredPosition.y;

        // Находим ближайшую целевую позицию (с учётом прокрутки нескольких оборотов)
        float normalized = ((currentY % _totalHeight) + _totalHeight) % _totalHeight;
        float delta      = targetY - normalized;
        if (delta > 0) delta -= _totalHeight; // всегда крутим вниз

        // Добавляем один полный оборот для убедительности
        float finalY = currentY + delta - _totalHeight;

        yield return _rt.DOAnchorPosY(finalY, slowdownDuration)
            .SetEase(Ease.OutCubic)
            .WaitForCompletion();

        // Фиксируем точную позицию
        _rt.anchoredPosition = new Vector2(_rt.anchoredPosition.x, targetY);
        IsSpinning = false;
    }

    // ══════════════════════════════════════════════════
    // ЭФФЕКТЫ
    // ══════════════════════════════════════════════════

    /// <summary>
    /// Пульсация золотым при джекпоте.
    /// Вызывается из SlotMachineManager.ShowJackpot().
    /// </summary>
    public void PlayJackpotEffect()
    {
        foreach (var img in symbolImages)
        {
            img.DOColor(new Color(1f, 0.85f, 0.2f), 0.25f)
               .SetLoops(6, LoopType.Yoyo)
               .SetUpdate(true);
        }
    }
}