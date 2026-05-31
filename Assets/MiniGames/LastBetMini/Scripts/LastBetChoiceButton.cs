using UnityEngine;
using UnityEngine.UI;

// Визуальное состояние кнопки выбора подозреваемого
public class LastBetChoiceButton : MonoBehaviour
{
    [Header("Фон кнопки")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Sprite idleBackground;
    [SerializeField] private Sprite activeBackground;

    [Header("Цвета (fallback, если спрайты не назначены)")]
    [SerializeField] private Color idleColor = Color.white;
    [SerializeField] private Color activeColor = new Color(1f, 0.78f, 0.35f, 1f);

    /// <summary>
    /// Привязывает фоновый Image если он не назначен в Inspector.
    /// Запоминает текущий цвет как цвет «обычного» состояния.
    /// </summary>
    public void BindDefaults()
    {
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();

        // Если idle-спрайт не назначен — запоминаем текущий цвет объекта
        // как цвет неактивного состояния, чтобы не сбивать настройку сцены.
        if (backgroundImage != null && idleBackground == null)
            idleColor = backgroundImage.color;
    }

    /// <summary>
    /// Переключает визуальное состояние кнопки.
    /// Вызывается из LastBetSuspectPanel при выборе/сбросе подозреваемого.
    /// </summary>
    public void SetSelected(bool selected)
    {
        BindDefaults();

        if (backgroundImage == null)
            return;

        Sprite target = selected ? activeBackground : idleBackground;
        if (target != null)
            backgroundImage.sprite = target;

        backgroundImage.color = selected ? activeColor : idleColor;
    }
}
