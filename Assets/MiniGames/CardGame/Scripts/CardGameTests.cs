using NUnit.Framework;
using UnityEngine;

public class CardGameTests
{
    private static CardData Card(
        CocktailType type,
        int points,
        CocktailType requiredLeft = CocktailType.None,
        CocktailType requiredRight = CocktailType.None,
        int adjacencyBonus = 0,
        CardEffectType effectType = CardEffectType.None,
        CocktailType effectTarget = CocktailType.None,
        int effectAmount = 0)
    {
        var card = ScriptableObject.CreateInstance<CardData>();

        card.cocktailType = type;
        card.points = points;

        card.requiredLeft = requiredLeft;
        card.requiredRight = requiredRight;
        card.adjacencyBonus = adjacencyBonus;

        card.effectType = effectType;
        card.effectTarget = effectTarget;
        card.effectAmount = effectAmount;

        return card;
    }

    private static CustomerData Customer(
        CocktailType required = CocktailType.None,
        CocktailType preferred = CocktailType.None,
        CustomerRuleType rule = CustomerRuleType.None,
        int bonusForPreferred = 1)
    {
        var customer = ScriptableObject.CreateInstance<CustomerData>();

        customer.requiredType = required;
        customer.preferredType = preferred;
        customer.ruleType = rule;
        customer.bonusForPreferred = bonusForPreferred;

        return customer;
    }

    // ─────────────────────────────────────────────
    // RECIPE BONUS
    // ─────────────────────────────────────────────

    [Test]
    public void Triplet_SameType_GivesBonus3()
    {
        var cards = new[]
        {
            Card(CocktailType.Bitter, 1),
            Card(CocktailType.Bitter, 2),
            Card(CocktailType.Bitter, 1)
        };

        int bonus = CardGameScoring.RecipeBonus(cards, CocktailType.Lemonchello);

        Assert.AreEqual(3, bonus);
    }

    [Test]
    public void Triplet_PreferredType_GivesBonus4()
    {
        var cards = new[]
        {
            Card(CocktailType.Lemonchello, 2),
            Card(CocktailType.Lemonchello, 1),
            Card(CocktailType.Lemonchello, 2)
        };

        int bonus = CardGameScoring.RecipeBonus(cards, CocktailType.Lemonchello);

        Assert.AreEqual(4, bonus);
    }

    [Test]
    public void Twin_Adjacent_GivesBonus1()
    {
        var cards = new[]
        {
            Card(CocktailType.Bitter, 2),
            Card(CocktailType.Bitter, 1),
            Card(CocktailType.Absinthe, 2)
        };

        int bonus = CardGameScoring.RecipeBonus(cards, CocktailType.Lemonchello);

        Assert.AreEqual(1, bonus);
    }

    [Test]
    public void Twin_Preferred_GivesBonus2()
    {
        var cards = new[]
        {
            Card(CocktailType.Absinthe, 2),
            Card(CocktailType.Absinthe, 2),
            Card(CocktailType.Bitter, 1)
        };

        int bonus = CardGameScoring.RecipeBonus(cards, CocktailType.Absinthe);

        Assert.AreEqual(2, bonus);
    }

    [Test]
    public void Twin_NotAdjacent_GivesNoBonus()
    {
        var cards = new[]
        {
            Card(CocktailType.Bitter, 2),
            Card(CocktailType.Absinthe, 1),
            Card(CocktailType.Bitter, 1)
        };

        int bonus = CardGameScoring.RecipeBonus(cards, CocktailType.Lemonchello);

        Assert.AreEqual(0, bonus);
    }

    [Test]
    public void Rainbow_AllDifferent_GivesBonus2()
    {
        var cards = new[]
        {
            Card(CocktailType.Bitter, 1),
            Card(CocktailType.Lemonchello, 1),
            Card(CocktailType.Absinthe, 1)
        };

        int bonus = CardGameScoring.RecipeBonus(cards, CocktailType.Damaged);

        Assert.AreEqual(2, bonus);
    }

    [Test]
    public void Rainbow_PreferredPresent_GivesBonus3()
    {
        var cards = new[]
        {
            Card(CocktailType.Bitter, 1),
            Card(CocktailType.Lemonchello, 1),
            Card(CocktailType.Absinthe, 1)
        };

        int bonus = CardGameScoring.RecipeBonus(cards, CocktailType.Absinthe);

        Assert.AreEqual(3, bonus);
    }

    // ─────────────────────────────────────────────
    // ADJACENCY BONUS
    // ─────────────────────────────────────────────

    [Test]
    public void Adjacency_LeftConditionMet_GivesBonus()
    {
        var cards = new[]
        {
            Card(CocktailType.Lemonchello, 2),
            Card(CocktailType.Bitter, 2, requiredLeft: CocktailType.Lemonchello, adjacencyBonus: 1),
            Card(CocktailType.Absinthe, 2)
        };

        int bonus = CardGameScoring.AdjacencyBonus(cards);

        Assert.AreEqual(1, bonus);
    }

    [Test]
    public void Adjacency_LeftConditionNotMet_GivesNoBonus()
    {
        var cards = new[]
        {
            Card(CocktailType.Absinthe, 2),
            Card(CocktailType.Bitter, 2, requiredLeft: CocktailType.Lemonchello, adjacencyBonus: 1),
            Card(CocktailType.Lemonchello, 2)
        };

        int bonus = CardGameScoring.AdjacencyBonus(cards);

        Assert.AreEqual(0, bonus);
    }

    [Test]
    public void Adjacency_RightConditionMet_GivesBonus()
    {
        var cards = new[]
        {
            Card(CocktailType.Bitter, 2),
            Card(CocktailType.Lemonchello, 3, requiredRight: CocktailType.Absinthe, adjacencyBonus: 2),
            Card(CocktailType.Absinthe, 2)
        };

        int bonus = CardGameScoring.AdjacencyBonus(cards);

        Assert.AreEqual(2, bonus);
    }

    [Test]
    public void RottenBitter_BreaksAdjacentBonuses()
    {
        var cards = new[]
        {
            Card(CocktailType.Damaged, -1, effectType: CardEffectType.BreakAdjacentBonuses),
            Card(CocktailType.Bitter, 2, requiredLeft: CocktailType.Lemonchello, adjacencyBonus: 1),
            Card(CocktailType.Absinthe, 2)
        };

        int bonus = CardGameScoring.AdjacencyBonus(cards);

        Assert.AreEqual(0, bonus);
    }

    [Test]
    public void OrangePeel_DoublesRightAdjacencyBonus()
    {
        var cards = new[]
        {
            Card(CocktailType.None, 0, effectType: CardEffectType.DoubleRightAdjacency),
            Card(CocktailType.Bitter, 2, requiredRight: CocktailType.Absinthe, adjacencyBonus: 1),
            Card(CocktailType.Absinthe, 2)
        };

        int bonus = CardGameScoring.AdjacencyBonus(cards);

        Assert.AreEqual(2, bonus);
    }

    // ─────────────────────────────────────────────
    // FULL ROUND SCORE
    // ─────────────────────────────────────────────

    [Test]
    public void RoundScore_NoRequiredType_FailsRound()
    {
        var cards = new[]
        {
            Card(CocktailType.Bitter, 2),
            Card(CocktailType.Absinthe, 1),
            Card(CocktailType.Bitter, 1)
        };

        var result = CardGameScoring.CalculateRoundScore(
            cards,
            Customer(required: CocktailType.Lemonchello, preferred: CocktailType.Absinthe)
        );

        Assert.IsTrue(result.IsFailed);
        Assert.IsFalse(result.IsFatal);
        Assert.AreEqual(0, result.Score);
    }

    [Test]
    public void RoundScore_RequiredPresent_ReturnsScore()
    {
        var cards = new[]
        {
            Card(CocktailType.Bitter, 2),
            Card(CocktailType.Bitter, 2),
            Card(CocktailType.Absinthe, 1)
        };

        var result = CardGameScoring.CalculateRoundScore(
            cards,
            Customer(required: CocktailType.Absinthe, preferred: CocktailType.Bitter)
        );

        // base 2+2+1 = 5
        // twin Bitter preferred = +2
        // preferred present customer bonus = +1
        // total = 8
        Assert.IsFalse(result.IsFailed);
        Assert.IsFalse(result.IsFatal);
        Assert.AreEqual(8, result.Score);
    }

    [Test]
    public void RoundScore_NoDamagedCustomerRule_FailsIfDamagedPresent()
    {
        var cards = new[]
        {
            Card(CocktailType.Bitter, 2),
            Card(CocktailType.Damaged, -1),
            Card(CocktailType.Absinthe, 2)
        };

        var result = CardGameScoring.CalculateRoundScore(
            cards,
            Customer(required: CocktailType.Bitter, rule: CustomerRuleType.NoDamagedCards)
        );

        Assert.IsTrue(result.IsFailed);
        Assert.IsFalse(result.IsFatal);
        Assert.AreEqual(0, result.Score);
    }

    [Test]
    public void RoundScore_WantsRainbow_FailsIfNotRainbow()
    {
        var cards = new[]
        {
            Card(CocktailType.Bitter, 2),
            Card(CocktailType.Bitter, 1),
            Card(CocktailType.Absinthe, 2)
        };

        var result = CardGameScoring.CalculateRoundScore(
            cards,
            Customer(required: CocktailType.Bitter, rule: CustomerRuleType.WantsRainbow)
        );

        Assert.IsTrue(result.IsFailed);
        Assert.IsFalse(result.IsFatal);
    }

    [Test]
    public void RoundScore_WantsTriplet_FailsIfNotTriplet()
    {
        var cards = new[]
        {
            Card(CocktailType.Bitter, 2),
            Card(CocktailType.Lemonchello, 1),
            Card(CocktailType.Absinthe, 2)
        };

        var result = CardGameScoring.CalculateRoundScore(
            cards,
            Customer(required: CocktailType.Bitter, rule: CustomerRuleType.WantsTriplet)
        );

        Assert.IsTrue(result.IsFailed);
        Assert.IsFalse(result.IsFatal);
    }

    [Test]
    public void ToxicMix_DamagedNearby_IsFatal()
    {
        var cards = new[]
        {
            Card(CocktailType.Damaged, 6, effectType: CardEffectType.LoseIfDamagedNearby),
            Card(CocktailType.Damaged, -1),
            Card(CocktailType.Bitter, 2)
        };

        var result = CardGameScoring.CalculateRoundScore(
            cards,
            Customer(required: CocktailType.Bitter)
        );

        Assert.IsTrue(result.IsFailed);
        Assert.IsTrue(result.IsFatal);
        Assert.AreEqual(0, result.Score);
    }

    // ─────────────────────────────────────────────
    // SPECIAL CARDS
    // ─────────────────────────────────────────────

    [Test]
    public void GoldenBitter_NoLemonchelloNearby_GivesZeroBasePoints()
    {
        var cards = new[]
        {
            Card(CocktailType.Bitter, 5,
                effectType: CardEffectType.ZeroIfNoTargetNearby,
                effectTarget: CocktailType.Lemonchello),
            Card(CocktailType.Absinthe, 1),
            Card(CocktailType.Bitter, 1)
        };

        var result = CardGameScoring.CalculateRoundScore(
            cards,
            Customer(required: CocktailType.Absinthe)
        );

        // Golden_Bitter = 0, Absinthe = 1, Bitter = 1
        // twin Bitter not adjacent? positions 0 and 2 are not adjacent => no twin
        Assert.IsFalse(result.IsFailed);
        Assert.AreEqual(2, result.Score);
    }

    [Test]
    public void GoldenBitter_WithLemonchelloNearby_KeepsBasePoints()
    {
        var cards = new[]
        {
            Card(CocktailType.Bitter, 5,
                effectType: CardEffectType.ZeroIfNoTargetNearby,
                effectTarget: CocktailType.Lemonchello),
            Card(CocktailType.Lemonchello, 1),
            Card(CocktailType.Absinthe, 1)
        };

        var result = CardGameScoring.CalculateRoundScore(
            cards,
            Customer(required: CocktailType.Lemonchello)
        );

        // base 5+1+1 = 7
        // rainbow = +2
        // preferred none = +0
        Assert.IsFalse(result.IsFailed);
        Assert.AreEqual(9, result.Score);
    }

    [Test]
    public void SharpLimonchello_WithAbsintheNearby_GetsPenalty()
    {
        var cards = new[]
        {
            Card(CocktailType.Lemonchello, 4,
                effectType: CardEffectType.PenaltyIfTargetNearby,
                effectTarget: CocktailType.Absinthe,
                effectAmount: -2),
            Card(CocktailType.Absinthe, 1),
            Card(CocktailType.Bitter, 1)
        };

        var result = CardGameScoring.CalculateRoundScore(
            cards,
            Customer(required: CocktailType.Lemonchello)
        );

        // base: (4-2)+1+1 = 4
        // rainbow +2
        Assert.IsFalse(result.IsFailed);
        Assert.AreEqual(6, result.Score);
    }

    [Test]
    public void Mold_InCenter_GetsPenalty()
    {
        var cards = new[]
        {
            Card(CocktailType.Bitter, 2),
            Card(CocktailType.Damaged, -2,
                effectType: CardEffectType.MoldCenterPenalty,
                effectAmount: -1),
            Card(CocktailType.Absinthe, 2)
        };

        var result = CardGameScoring.CalculateRoundScore(
            cards,
            Customer(required: CocktailType.Bitter)
        );

        // base: 2 + (-3) + 2 = 1
        Assert.IsFalse(result.IsFailed);
        Assert.AreEqual(1, result.Score);
    }
}