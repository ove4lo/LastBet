using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

namespace SentryToolkit
{
    /// <summary>
    /// Менеджер туториала. Показывает шаги обучения с подсветкой кнопок и tooltip-подсказками.
    ///
    /// Исправления по сравнению с оригинальной версией:
    /// — Позиционирование tooltip происходит через корутину, после того как текст
    ///   отрендерен и размер блока актуален. В оригинале tooltipSize бралась
    ///   до рендера — блок позиционировался по старому размеру и съезжал.
    /// — Добавлено событие OnTutorialCompleted для внешней подписки.
    ///   LastBetMiniGameManager подписывается на него, чтобы запустить таймер
    ///   только после завершения туториала, а не параллельно с ним.
    /// </summary>
    public class UITutorialManager : MonoBehaviour
    {
        [Header("Базовые ссылки")]
        [Tooltip("Полноэкранная затемняющая панель.")]
        public GameObject overlayPanel;

        [Tooltip("Prefab подсказки. Внутри: Background, Text, Arrow, NextButton, SkipButton.")]
        public GameObject tooltipPrefab;

        public List<TutorialSequence> TutorialSequences = new List<TutorialSequence>();

        [Header("Расположение tooltip")]
        [SerializeField] private float tooltipOffset = 28f;
        [SerializeField] private Vector2 screenPadding = new Vector2(28f, 28f);

        [Header("Отладка")]
        public SequenceID testSequenceID;
        public ButtonID testButtonID;
        public int currentStepIndex = 0;

        /// <summary>
        /// Вызывается когда игрок прошёл все шаги или нажал «Пропустить».
        /// LastBetMiniGameManager подписывается сюда, чтобы запустить раунд.
        /// </summary>
        public event Action OnTutorialCompleted;

        public static Action<ButtonID> OnButtonFocus = delegate { };

        private bool _isTutorialActive;
        private TutorialSequence _currentSequence;
        private GameObject _currentTooltipObject;
        private TooltipUI _currentTooltip;
        private readonly Dictionary<ButtonID, UIButton> _buttonDictionary = new Dictionary<ButtonID, UIButton>();

        private void Awake()
        {
            CacheAllButtons();
            HideAll();
        }

        private void OnEnable()
        {
            CacheAllButtons();
        }

        private void CacheAllButtons()
        {
            _buttonDictionary.Clear();

            UIButton[] allButtons = FindObjectsByType<UIButton>(FindObjectsInactive.Include);
            foreach (UIButton button in allButtons)
            {
                if (button == null || button.buttonID == ButtonID.None)
                    continue;

                if (!_buttonDictionary.ContainsKey(button.buttonID))
                    _buttonDictionary.Add(button.buttonID, button);
            }
        }

        [Button]
        public void DebugTutorialSequence() => StartTutorial(testSequenceID);

        [Button]
        public void DebugFocusOnButton() => FocusOnButton(testButtonID, "Это тестовая подсказка.");

        public void StartTutorial(SequenceID sequenceID)
        {
            CacheAllButtons();

            TutorialSequence sequence = TutorialSequences.Find(seq => seq.sequenceName == sequenceID);
            if (sequence == null || sequence.steps == null || sequence.steps.Count == 0)
            {
                Debug.LogWarning($"[Tutorial] Последовательность не найдена или пуста: {sequenceID}");
                // Туториал не запустился — сообщаем что завершили, чтобы раунд не завис.
                OnTutorialCompleted?.Invoke();
                return;
            }

            _isTutorialActive = true;
            _currentSequence = sequence;
            currentStepIndex = 0;

            ShowStep();
        }

        private void ShowStep()
        {
            if (!_isTutorialActive || _currentSequence == null)
                return;

            if (currentStepIndex >= _currentSequence.steps.Count)
            {
                EndTutorial();
                return;
            }

            UITutorialStep step = _currentSequence.steps[currentStepIndex];

            if (overlayPanel != null)
            {
                overlayPanel.SetActive(true);
                overlayPanel.transform.SetAsLastSibling();
            }

            OnButtonFocus?.Invoke(step.focusButtonID);
            ShowTooltip(step.focusButtonID, step.message);
        }

        public void NextStep()
        {
            if (!_isTutorialActive || _currentSequence == null)
                return;

            UITutorialStep step = _currentSequence.steps[currentStepIndex];
            step.OnStepCompleted?.Invoke();

            currentStepIndex++;
            ShowStep();
        }

        public void EndTutorial()
        {
            _isTutorialActive = false;
            _currentSequence = null;
            currentStepIndex = 0;

            HideAll();
            OnButtonFocus?.Invoke(ButtonID.None);

            Debug.Log("[Tutorial] Завершён.");

            // Оповещаем подписчиков — в частности LastBetMiniGameManager,
            // чтобы он запустил таймер раунда.
            OnTutorialCompleted?.Invoke();
        }

        private void HideAll()
        {
            if (overlayPanel != null)
                overlayPanel.SetActive(false);

            if (_currentTooltipObject != null)
                _currentTooltipObject.SetActive(false);
        }

        public void FocusOnButton(ButtonID buttonID, string tooltipText = "", Action onFocusedButtonClicked = null)
        {
            if (overlayPanel != null)
            {
                overlayPanel.SetActive(true);
                overlayPanel.transform.SetAsLastSibling();
            }

            OnButtonFocus?.Invoke(buttonID);

            if (!string.IsNullOrWhiteSpace(tooltipText))
                ShowTooltip(buttonID, tooltipText);
        }

        private void EnsureTooltip()
        {
            if (_currentTooltipObject != null)
                return;

            if (tooltipPrefab == null || overlayPanel == null)
            {
                Debug.LogWarning("[Tutorial] Tooltip prefab или overlay panel не назначены.");
                return;
            }

            Transform parent = overlayPanel.transform.parent != null
                ? overlayPanel.transform.parent
                : overlayPanel.transform;

            _currentTooltipObject = Instantiate(tooltipPrefab, parent);
            _currentTooltipObject.name = tooltipPrefab.name + "_Runtime";
            _currentTooltipObject.transform.SetAsLastSibling();

            _currentTooltip = _currentTooltipObject.GetComponent<TooltipUI>();
            if (_currentTooltip == null)
            {
                Debug.LogError("[Tutorial] На корне prefab должен быть компонент TooltipUI.");
                return;
            }

            _currentTooltip.BindButtons(NextStep, EndTutorial);
        }

        private void ShowTooltip(ButtonID buttonID, string tooltipText)
        {
            EnsureTooltip();

            if (_currentTooltipObject == null || _currentTooltip == null)
                return;

            _currentTooltipObject.SetActive(true);
            _currentTooltipObject.transform.SetAsLastSibling();

            // Сначала устанавливаем текст — это меняет размер блока.
            // Позиционирование запускаем через корутину на следующий кадр,
            // когда Layout уже пересчитал реальные размеры tooltip.
            _currentTooltip.SetText(tooltipText);
            _currentTooltip.SetControlsVisible(_isTutorialActive);

            StartCoroutine(PositionTooltipNextFrame(buttonID));
        }

        /// <summary>
        /// Ждёт один кадр после установки текста, затем позиционирует tooltip.
        /// Это исправляет проблему когда размер блока брался до рендера текста
        /// и tooltip съезжал в сторону.
        /// </summary>
        private IEnumerator PositionTooltipNextFrame(ButtonID buttonID)
        {
            // Ждём конца кадра — Canvas пересчитает размеры после SetText.
            yield return new WaitForEndOfFrame();

            if (_currentTooltipObject == null || _currentTooltip == null)
                yield break;

            if (buttonID == ButtonID.None)
            {
                PositionAtCenter();
                yield break;
            }

            UIButton targetButton = FindButton(buttonID);
            if (targetButton == null)
            {
                Debug.LogError($"[Tutorial] Кнопка с ID {buttonID} не найдена. Tooltip — по центру.");
                PositionAtCenter();
                yield break;
            }

            RectTransform targetRect = targetButton.GetComponent<RectTransform>();
            PositionNearTarget(targetRect);
        }

        private UIButton FindButton(ButtonID id)
        {
            if (id == ButtonID.None)
                return null;

            if (_buttonDictionary.TryGetValue(id, out UIButton cached) && cached != null)
                return cached;

            UIButton[] allButtons = FindObjectsByType<UIButton>(FindObjectsInactive.Include);
            foreach (UIButton button in allButtons)
            {
                if (button != null && button.buttonID == id)
                {
                    _buttonDictionary[id] = button;
                    return button;
                }
            }

            return null;
        }

        private void PositionAtCenter()
        {
            RectTransform tooltipRect = GetTooltipRect();
            if (tooltipRect == null)
                return;

            tooltipRect.anchorMin = new Vector2(0.5f, 0.5f);
            tooltipRect.anchorMax = new Vector2(0.5f, 0.5f);
            tooltipRect.pivot = new Vector2(0.5f, 0.5f);
            tooltipRect.anchoredPosition = Vector2.zero;

            if (_currentTooltip != null)
                _currentTooltip.SetArrowVisible(false);
        }

        private void PositionNearTarget(RectTransform targetRect)
        {
            RectTransform tooltipRect = GetTooltipRect();
            RectTransform overlayRect = overlayPanel != null
                ? overlayPanel.GetComponent<RectTransform>()
                : null;

            if (tooltipRect == null || overlayRect == null || targetRect == null)
                return;

            // ForceUpdateCanvases вызывается здесь уже после WaitForEndOfFrame —
            // текст отрендерен, размеры актуальны.
            Canvas.ForceUpdateCanvases();

            Vector3[] targetWorldCorners = new Vector3[4];
            targetRect.GetWorldCorners(targetWorldCorners);

            Vector2 targetMin    = WorldToOverlayLocal(overlayRect, targetWorldCorners[0]);
            Vector2 targetMax    = WorldToOverlayLocal(overlayRect, targetWorldCorners[2]);
            Vector2 targetCenter = (targetMin + targetMax) * 0.5f;
            Vector2 targetSize   = targetMax - targetMin;

            // Берём rect.size после рендера — теперь он актуален.
            Vector2 tooltipSize = tooltipRect.rect.size;
            if (tooltipSize.x <= 1f || tooltipSize.y <= 1f)
                tooltipSize = tooltipRect.sizeDelta;

            Rect overlayBounds = overlayRect.rect;

            float rightSpace  = overlayBounds.xMax - (targetCenter.x + targetSize.x * 0.5f);
            float leftSpace   = (targetCenter.x - targetSize.x * 0.5f) - overlayBounds.xMin;
            float topSpace    = overlayBounds.yMax - (targetCenter.y + targetSize.y * 0.5f);
            float bottomSpace = (targetCenter.y - targetSize.y * 0.5f) - overlayBounds.yMin;

            Placement placement;

            if (rightSpace  >= tooltipSize.x + tooltipOffset) placement = Placement.Right;
            else if (leftSpace  >= tooltipSize.x + tooltipOffset) placement = Placement.Left;
            else if (bottomSpace >= tooltipSize.y + tooltipOffset) placement = Placement.Bottom;
            else if (topSpace   >= tooltipSize.y + tooltipOffset) placement = Placement.Top;
            else placement = PickLargestSpace(rightSpace, leftSpace, topSpace, bottomSpace);

            Vector2 desired = targetCenter;
            Vector2 pivot   = new Vector2(0.5f, 0.5f);

            switch (placement)
            {
                case Placement.Right:
                    pivot   = new Vector2(0f, 0.5f);
                    desired = new Vector2(targetMax.x + tooltipOffset, targetCenter.y);
                    break;
                case Placement.Left:
                    pivot   = new Vector2(1f, 0.5f);
                    desired = new Vector2(targetMin.x - tooltipOffset, targetCenter.y);
                    break;
                case Placement.Top:
                    pivot   = new Vector2(0.5f, 0f);
                    desired = new Vector2(targetCenter.x, targetMax.y + tooltipOffset);
                    break;
                case Placement.Bottom:
                    pivot   = new Vector2(0.5f, 1f);
                    desired = new Vector2(targetCenter.x, targetMin.y - tooltipOffset);
                    break;
            }

            tooltipRect.anchorMin      = new Vector2(0.5f, 0.5f);
            tooltipRect.anchorMax      = new Vector2(0.5f, 0.5f);
            tooltipRect.pivot          = pivot;
            tooltipRect.anchoredPosition = ClampByPivot(desired, tooltipSize, pivot, overlayBounds, screenPadding);

            PositionArrow(tooltipRect, targetCenter, placement);
        }

        private RectTransform GetTooltipRect()
        {
            return _currentTooltipObject != null
                ? _currentTooltipObject.GetComponent<RectTransform>()
                : null;
        }

        private static Vector2 WorldToOverlayLocal(RectTransform overlayRect, Vector3 worldPoint)
        {
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, worldPoint);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                overlayRect, screenPoint, null, out Vector2 localPoint);
            return localPoint;
        }

        private static Vector2 ClampByPivot(Vector2 desired, Vector2 size, Vector2 pivot, Rect bounds, Vector2 padding)
        {
            float minX = bounds.xMin + padding.x + size.x * pivot.x;
            float maxX = bounds.xMax - padding.x - size.x * (1f - pivot.x);
            float minY = bounds.yMin + padding.y + size.y * pivot.y;
            float maxY = bounds.yMax - padding.y - size.y * (1f - pivot.y);

            desired.x = Mathf.Clamp(desired.x, minX, maxX);
            desired.y = Mathf.Clamp(desired.y, minY, maxY);
            return desired;
        }

        private void PositionArrow(RectTransform tooltipRect, Vector2 targetCenter, Placement placement)
        {
            if (_currentTooltip == null || _currentTooltip.ArrowRect == null || overlayPanel == null)
                return;

            RectTransform arrowRect = _currentTooltip.ArrowRect;
            _currentTooltip.SetArrowVisible(true);

            Vector3 targetWorld = overlayPanel.transform.TransformPoint(targetCenter);
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, targetWorld);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                tooltipRect, screenPoint, null, out Vector2 localTarget);

            Vector2 size  = tooltipRect.rect.size;
            float halfW = size.x * 0.5f;
            float halfH = size.y * 0.5f;

            switch (placement)
            {
                case Placement.Right:
                    arrowRect.localEulerAngles = new Vector3(0f, 0f, 180f);
                    arrowRect.anchoredPosition = new Vector2(
                        -halfW,
                        Mathf.Clamp(localTarget.y, -halfH + 24f, halfH - 24f));
                    break;

                case Placement.Left:
                    arrowRect.localEulerAngles = Vector3.zero;
                    arrowRect.anchoredPosition = new Vector2(
                        halfW,
                        Mathf.Clamp(localTarget.y, -halfH + 24f, halfH - 24f));
                    break;

                case Placement.Top:
                    arrowRect.localEulerAngles = new Vector3(0f, 0f, -90f);
                    arrowRect.anchoredPosition = new Vector2(
                        Mathf.Clamp(localTarget.x, -halfW + 24f, halfW - 24f),
                        -halfH);
                    break;

                case Placement.Bottom:
                    arrowRect.localEulerAngles = new Vector3(0f, 0f, 90f);
                    arrowRect.anchoredPosition = new Vector2(
                        Mathf.Clamp(localTarget.x, -halfW + 24f, halfW - 24f),
                        halfH);
                    break;
            }
        }

        private static Placement PickLargestSpace(float right, float left, float top, float bottom)
        {
            float max = Mathf.Max(right, left, top, bottom);

            if (Mathf.Approximately(max, right))  return Placement.Right;
            if (Mathf.Approximately(max, left))   return Placement.Left;
            if (Mathf.Approximately(max, bottom)) return Placement.Bottom;
            return Placement.Top;
        }

        private enum Placement { Right, Left, Top, Bottom }
    }
}
