using UnityEngine;

[CreateAssetMenu(fileName = "Card", menuName = "CardGame/CardData")]
public class CardData : ScriptableObject
{
    [Header("Основные параметры")]

    [Tooltip("Тип карты")]
    [InspectorName("Тип карты")]
    public CocktailType cocktailType;

    [Tooltip("Базовые очки карты")]
    [InspectorName("Очки")]
    [Range(-3, 6)]
    public int points = 1;

    [Tooltip("Готовый спрайт карты")]
    [InspectorName("Спрайт карты")]
    public Sprite cardSprite;

    [Header("Бонус если рядом")]

    [Tooltip("Какой тип карты должен быть слева")]
    [InspectorName("Если слева")]
    public CocktailType requiredLeft = CocktailType.None;

    [Tooltip("Какой тип карты должен быть справа")]
    [InspectorName("Если справа")]
    public CocktailType requiredRight = CocktailType.None;

    [Tooltip("Сколько очков добавить")]
    [InspectorName("Бонус")]
    [Range(-3, 6)]
    public int adjacencyBonus = 0;

    [Header("Особый эффект")]

    [Tooltip("Особая механика карты")]
    [InspectorName("Тип эффекта")]
    public CardEffectType effectType = CardEffectType.None;

    [Tooltip("Какой тип карты проверяет эффект")]
    [InspectorName("Проверяемый тип")]
    public CocktailType effectTarget = CocktailType.None;

    [Tooltip("Число для эффекта")]
    [InspectorName("Сила эффекта")]
    public int effectAmount = 0;
}

public enum CocktailType
{
    [InspectorName("Нет")]
    None = 0,

    [InspectorName("Биттер")]
    Bitter = 1,

    [InspectorName("Лимончелло")]
    Lemonchello = 2,

    [InspectorName("Абсент")]
    Absinthe = 3,

    [InspectorName("Испорченная")]
    Damaged = 4
}

public enum CardEffectType
{
    [InspectorName("Нет")]
    None = 0,

    [InspectorName("Ломает бонусы соседей")]
    BreakAdjacentBonuses = 1,

    [InspectorName("Не участвует в Радуге")]
    ExcludeFromRainbow = 2,

    [InspectorName("Отменяет бонус рецепта")]
    CancelRecipeBonus = 3,

    [InspectorName("Штраф в центре")]
    MoldCenterPenalty = 4,

    [InspectorName("Копирует тип соседа")]
    CopyNeighborType = 10,

    [InspectorName("Подходит к любому бонусу")]
    AnyTypeForAdjacency = 11,

    [InspectorName("+ очки соседу")]
    AddToNeighbor = 12,

    [InspectorName("Удваивает бонус справа")]
    DoubleRightAdjacency = 13,

    [InspectorName("0 очков без нужного соседа")]
    ZeroIfNoTargetNearby = 20,

    [InspectorName("Штраф если рядом нужный тип")]
    PenaltyIfTargetNearby = 21,

    [InspectorName("Проигрыш если рядом испорченная")]
    LoseIfDamagedNearby = 22
}