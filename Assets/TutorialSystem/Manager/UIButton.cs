using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace SentryToolkit
{
    /// <summary>
    /// Кнопка-цель для onboarding.
    /// Исправление базового пакета:
    /// - Canvas добавляется корректно, если его не было.
    /// - Для прозрачных proxy-кнопок не появляется белая заливка.
    /// - При снятии фокуса временные компоненты удаляются.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class UIButton : MonoBehaviour
    {
        public ButtonID buttonID;

        [Header("Proxy Target")]
        [SerializeField] private bool forceTransparentVisual = false;

        private GraphicRaycaster graphicRaycaster;
        private Canvas focusCanvas;
        private Button button;
        private Graphic targetGraphic;
        private Selectable.Transition originalTransition;
        private ColorBlock originalColors;
        private bool isFocused;
        private bool addedCanvas;
        private bool addedRaycaster;

        private void Awake()
        {
            button = GetComponent<Button>();
            targetGraphic = GetComponent<Graphic>();

            if (button != null)
            {
                originalTransition = button.transition;
                originalColors = button.colors;
            }

            if (forceTransparentVisual)
                MakeTransparent();
        }

        private void Start()
        {
            if (button == null)
                button = GetComponent<Button>();

            button.onClick.RemoveListener(Click);
            button.onClick.AddListener(Click);
        }

        private void OnEnable()
        {
            UITutorialManager.OnButtonFocus += TriggerButtonFocus;
        }

        private void OnDisable()
        {
            UITutorialManager.OnButtonFocus -= TriggerButtonFocus;
            ResetFocus();
        }

        public void TriggerButtonFocus(ButtonID id)
        {
            if (id == buttonID && buttonID != ButtonID.None)
            {
                ApplyFocus();
            }
            else if (isFocused)
            {
                ResetFocus();
            }
        }

        private void ApplyFocus()
        {
            if (isFocused)
                return;

            focusCanvas = GetComponent<Canvas>();
            if (focusCanvas == null)
            {
                focusCanvas = gameObject.AddComponent<Canvas>();
                addedCanvas = true;
            }

            focusCanvas.overrideSorting = true;
            focusCanvas.sortingOrder = 50;

            graphicRaycaster = GetComponent<GraphicRaycaster>();
            if (graphicRaycaster == null)
            {
                graphicRaycaster = gameObject.AddComponent<GraphicRaycaster>();
                addedRaycaster = true;
            }

            if (forceTransparentVisual)
                MakeTransparent();

            isFocused = true;
        }

        private void ResetFocus()
        {
            if (!isFocused && !addedCanvas && !addedRaycaster)
                return;

            if (button != null)
            {
                button.transition = originalTransition;
                button.colors = originalColors;
            }

            if (forceTransparentVisual)
                MakeTransparent();

            if (addedRaycaster && graphicRaycaster != null)
                Destroy(graphicRaycaster);

            if (addedCanvas && focusCanvas != null)
                StartCoroutine(DestroyCanvasAfterFrame());

            graphicRaycaster = null;
            isFocused = false;
            addedRaycaster = false;
            addedCanvas = false;
        }

        private IEnumerator DestroyCanvasAfterFrame()
        {
            yield return null;

            if (focusCanvas != null)
                Destroy(focusCanvas);

            focusCanvas = null;
        }

        private void MakeTransparent()
        {
            if (button != null)
            {
                button.transition = Selectable.Transition.None;

                ColorBlock colors = button.colors;
                colors.normalColor = TransparentWhite();
                colors.highlightedColor = TransparentWhite();
                colors.pressedColor = TransparentWhite();
                colors.selectedColor = TransparentWhite();
                colors.disabledColor = TransparentWhite();
                button.colors = colors;
            }

            if (targetGraphic != null)
            {
                Color c = targetGraphic.color;
                c.a = 0f;
                targetGraphic.color = c;
                targetGraphic.raycastTarget = true;
            }
        }

        private static Color TransparentWhite()
        {
            return new Color(1f, 1f, 1f, 0f);
        }

        private void Click()
        {
            // В улучшенной версии tutorial шагает кнопкой "Далее".
            // Клик по target больше не обязан завершать шаг.
        }
    }
}
