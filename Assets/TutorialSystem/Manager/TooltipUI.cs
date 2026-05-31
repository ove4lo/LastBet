using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SentryToolkit
{
    public class TooltipUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private RectTransform arrowImage;
        [SerializeField] private TextMeshProUGUI textMeshPro;
        [SerializeField] private RectTransform backgroundRectTransform;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button skipButton;

        [Header("Text")]
        [SerializeField] private string nextButtonText = "Далее";
        [SerializeField] private string skipButtonText = "Пропустить";

        [Header("Optional Style")]
        [SerializeField] private int fontSize = 18;
        [SerializeField] private Color textColor = new Color(0.78f, 0.63f, 0.38f, 1f);
        [SerializeField] private Color backgroundColor = new Color(0.045f, 0.025f, 0.03f, 0.98f);

        public RectTransform ArrowRect => arrowImage;
        public Button NextButton => nextButton;
        public Button SkipButton => skipButton;

        private void Awake()
        {
            AutoBind();
            ApplyStyle();
            SetControlsVisible(false);
        }

        private void OnValidate()
        {
            AutoBind();
        }

        public void AutoBind()
        {
            if (arrowImage == null)
            {
                Transform arrow = transform.Find("Arrow");
                if (arrow != null)
                    arrowImage = arrow.GetComponent<RectTransform>();
            }

            if (textMeshPro == null)
            {
                Transform text = transform.Find("Text");
                if (text != null)
                    textMeshPro = text.GetComponent<TextMeshProUGUI>();
            }

            if (backgroundRectTransform == null)
            {
                Transform background = transform.Find("Background");
                if (background != null)
                    backgroundRectTransform = background.GetComponent<RectTransform>();
            }

            if (nextButton == null)
            {
                Transform next = transform.Find("NextButton");
                if (next != null)
                    nextButton = next.GetComponent<Button>();
            }

            if (skipButton == null)
            {
                Transform skip = transform.Find("SkipButton");
                if (skip != null)
                    skipButton = skip.GetComponent<Button>();
            }
        }

        public void BindButtons(Action onNext, Action onSkip)
        {
            AutoBind();

            if (nextButton != null)
            {
                nextButton.onClick.RemoveAllListeners();

                if (onNext != null)
                    nextButton.onClick.AddListener(() => onNext());

                SetButtonLabel(nextButton, nextButtonText);
            }

            if (skipButton != null)
            {
                skipButton.onClick.RemoveAllListeners();

                if (onSkip != null)
                    skipButton.onClick.AddListener(() => onSkip());

                SetButtonLabel(skipButton, skipButtonText);
            }
        }

        public void SetText(string tooltipText)
        {
            AutoBind();

            if (textMeshPro == null)
                return;

            textMeshPro.SetText(tooltipText ?? string.Empty);
            textMeshPro.fontSize = fontSize;
            textMeshPro.color = textColor;
            textMeshPro.textWrappingMode = TextWrappingModes.Normal;
            textMeshPro.alignment = TextAlignmentOptions.Left;
            textMeshPro.ForceMeshUpdate();
        }

        public void SetControlsVisible(bool visible)
        {
            AutoBind();

            if (nextButton != null)
                nextButton.gameObject.SetActive(visible);

            if (skipButton != null)
                skipButton.gameObject.SetActive(visible);
        }

        public void SetArrowVisible(bool visible)
        {
            AutoBind();

            if (arrowImage != null)
                arrowImage.gameObject.SetActive(visible);
        }

        private void ApplyStyle()
        {
            AutoBind();

            if (textMeshPro != null)
            {
                textMeshPro.fontSize = fontSize;
                textMeshPro.color = textColor;
                textMeshPro.textWrappingMode = TextWrappingModes.Normal;
                textMeshPro.alignment = TextAlignmentOptions.Left;
            }

            Image bg = backgroundRectTransform != null
                ? backgroundRectTransform.GetComponent<Image>()
                : GetComponent<Image>();

            if (bg != null)
            {
                bg.color = backgroundColor;
                bg.raycastTarget = false;
            }

            Image arrow = arrowImage != null ? arrowImage.GetComponent<Image>() : null;
            if (arrow != null)
            {
                arrow.color = textColor;
                arrow.raycastTarget = false;
            }

            if (nextButton != null)
                SetButtonLabel(nextButton, nextButtonText);

            if (skipButton != null)
                SetButtonLabel(skipButton, skipButtonText);
        }

        private static void SetButtonLabel(Button button, string value)
        {
            if (button == null)
                return;

            TMP_Text tmpText = button.GetComponentInChildren<TMP_Text>(true);
            if (tmpText != null)
            {
                tmpText.text = value;
                return;
            }

            Text legacyText = button.GetComponentInChildren<Text>(true);
            if (legacyText != null)
                legacyText.text = value;
        }
    }
}