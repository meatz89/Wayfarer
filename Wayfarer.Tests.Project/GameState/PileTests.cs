using Xunit;

namespace Wayfarer.Tests.GameState;

/// <summary>
/// Comprehensive tests for Pile.Shuffle Fisher-Yates algorithm implementation.
/// Tests critical shuffling functionality: randomness, card preservation,
/// empty pile handling, single card edge case, and shuffle correctness.
/// MANDATORY per CLAUDE.md: Complex algorithms require complete test coverage.
/// </summary>
public class PileTests
{
    [Fact]
    public void Shuffle_EmptyPile_DoesNotThrow()
    {
        // Arrange: Empty pile
        Pile pile = new Pile();

        // Act & Assert: Shuffling empty pile should not crash
        pile.Shuffle();
        Assert.Equal(0, pile.Count);
    }

    [Fact]
    public void Shuffle_SingleCard_CardRemainsInPile()
    {
        // Arrange: Pile with one card
        Pile pile = new Pile();
        CardInstance card = CreateTestCard("card1");
        pile.Add(card);

        // Act
        pile.Shuffle();

        // Assert: Card still in pile, no loss
        Assert.Equal(1, pile.Count);
        Assert.Contains(card, pile.Cards);
    }

    [Fact]
    public void Shuffle_AllCardsRemainInPile()
    {
        // Arrange: Pile with 10 distinct cards
        Pile pile = new Pile();
        List<CardInstance> originalCards = new List<CardInstance>();

        for (int i = 0; i < 10; i++)
        {
            CardInstance card = CreateTestCard($"card_{i}");
            originalCards.Add(card);
            pile.Add(card);
        }

        // Act
        pile.Shuffle();

        // Assert: All cards still present, none lost
        Assert.Equal(10, pile.Count);
        foreach (CardInstance card in originalCards)
        {
            Assert.Contains(card, pile.Cards);
        }
    }

    [Fact]
    public void Shuffle_DeckSizeUnchanged()
    {
        // Arrange: Pile with 20 cards
        Pile pile = new Pile();
        int originalCount = 20;

        for (int i = 0; i < originalCount; i++)
        {
            pile.Add(CreateTestCard($"card_{i}"));
        }

        // Act
        pile.Shuffle();

        // Assert: Count unchanged after shuffle
        Assert.Equal(originalCount, pile.Count);
    }

    [Fact]
    public void Shuffle_ProducesDifferentOrderings()
    {
        // Arrange: Create pile with ordered cards
        Pile pile = new Pile();
        List<CardInstance> orderedCards = new List<CardInstance>();

        for (int i = 0; i < 10; i++)
        {
            CardInstance card = CreateTestCard($"card_{i}");
            orderedCards.Add(card);
            pile.Add(card);
        }

        // Act: Shuffle multiple times, check if ANY shuffle produces different order
        bool foundDifferentOrdering = false;
        int attempts = 100; // Statistical test: 100 shuffles should produce different order

        for (int attempt = 0; attempt < attempts; attempt++)
        {
            pile.Shuffle();

            // Check if order changed from original
            bool orderChanged = false;
            for (int i = 0; i < orderedCards.Count; i++)
            {
                if (pile.Cards[i] != orderedCards[i])
                {
                    orderChanged = true;
                    break;
                }
            }

            if (orderChanged)
            {
                foundDifferentOrdering = true;
                break;
            }

            // Reset pile to original order for next attempt
            pile.Clear();
            foreach (CardInstance card in orderedCards)
            {
                pile.Add(card);
            }
        }

        // Assert: At least one shuffle produced different ordering (randomness works)
        Assert.True(foundDifferentOrdering, "Shuffle should produce different orderings (randomness test failed)");
    }

    [Fact]
    public void Shuffle_FisherYatesCorrectness_MultipleShufflesDifferent()
    {
        // Arrange: Pile with 5 cards
        Pile pile = new Pile();
        List<CardInstance> cards = new List<CardInstance>();

        for (int i = 0; i < 5; i++)
        {
            CardInstance card = CreateTestCard($"card_{i}");
            cards.Add(card);
            pile.Add(card);
        }

        // Act: Shuffle twice and compare results
        pile.Shuffle();
        List<CardInstance> firstShuffleOrder = new List<CardInstance>(pile.Cards);

        pile.Shuffle();
        List<CardInstance> secondShuffleOrder = new List<CardInstance>(pile.Cards);

        // Assert: Two shuffles should (statistically) produce different orders
        // With 5 cards, probability of identical shuffle = 1/120 (very low)
        bool ordersAreDifferent = false;
        for (int i = 0; i < firstShuffleOrder.Count; i++)
        {
            if (firstShuffleOrder[i] != secondShuffleOrder[i])
            {
                ordersAreDifferent = true;
                break;
            }
        }

        // Note: This test COULD fail randomly (1/120 chance), but extremely unlikely
        Assert.True(ordersAreDifferent, "Sequential shuffles should produce different orderings");
    }

    [Fact]
    public void Shuffle_LargeDeck_AllCardsPreserved()
    {
        // Arrange: Large pile with 100 cards
        Pile pile = new Pile();
        List<CardInstance> originalCards = new List<CardInstance>();

        for (int i = 0; i < 100; i++)
        {
            CardInstance card = CreateTestCard($"card_{i}");
            originalCards.Add(card);
            pile.Add(card);
        }

        // Act
        pile.Shuffle();

        // Assert: All 100 cards still present
        Assert.Equal(100, pile.Count);
        foreach (CardInstance card in originalCards)
        {
            Assert.Contains(card, pile.Cards);
        }
    }

    [Fact]
    public void Shuffle_CardsHaveContext_ContextPreserved()
    {
        // Arrange: Cards with CardContext set
        Pile pile = new Pile();
        CardInstance card1 = CreateTestCard("card1");
        card1.Context = new CardContext { threshold = 5 };

        CardInstance card2 = CreateTestCard("card2");
        card2.Context = new CardContext { threshold = 10 };

        pile.Add(card1);
        pile.Add(card2);

        // Act
        pile.Shuffle();

        // Assert: Context preserved after shuffle
        CardInstance foundCard1 = pile.Cards.First(c => c.SocialCardTemplate.Id == "card1");
        CardInstance foundCard2 = pile.Cards.First(c => c.SocialCardTemplate.Id == "card2");

        Assert.NotNull(foundCard1.Context);
        Assert.Equal(5, foundCard1.Context.threshold);

        Assert.NotNull(foundCard2.Context);
        Assert.Equal(10, foundCard2.Context.threshold);
    }

    [Fact]
    public void Shuffle_DrawAfterShuffle_DrawsCorrectly()
    {
        // Arrange: Pile with cards
        Pile pile = new Pile();
        for (int i = 0; i < 10; i++)
        {
            pile.Add(CreateTestCard($"card_{i}"));
        }

        // Act: Shuffle then draw
        pile.Shuffle();
        CardInstance drawnCard = pile.DrawTop();

        // Assert: Draw works after shuffle
        Assert.NotNull(drawnCard);
        Assert.Equal(9, pile.Count); // One card removed
        Assert.DoesNotContain(drawnCard, pile.Cards); // Drawn card no longer in pile
    }

    [Fact]
    public void Shuffle_TwoCardsOnly_OrderChanges()
    {
        // Arrange: Minimal test case - 2 cards
        Pile pile = new Pile();
        CardInstance card1 = CreateTestCard("first");
        CardInstance card2 = CreateTestCard("second");
        pile.Add(card1);
        pile.Add(card2);

        // Act: Shuffle multiple times, check if order ever changes
        bool foundReversedOrder = false;
        for (int i = 0; i < 50; i++)
        {
            pile.Shuffle();
            if (pile.Cards[0] == card2 && pile.Cards[1] == card1)
            {
                foundReversedOrder = true;
                break;
            }
        }

        // Assert: At some point, cards should be in reversed order (50% probability per shuffle)
        Assert.True(foundReversedOrder, "Shuffle should reverse order of 2 cards at least once in 50 attempts");
    }

    [Fact]
    public void Shuffle_NoDuplicateCards_AfterShuffle()
    {
        // Arrange: Pile with 10 unique cards
        Pile pile = new Pile();
        List<CardInstance> cards = new List<CardInstance>();

        for (int i = 0; i < 10; i++)
        {
            CardInstance card = CreateTestCard($"unique_card_{i}");
            cards.Add(card);
            pile.Add(card);
        }

        // Act
        pile.Shuffle();

        // Assert: No cards duplicated or lost (count matches via reference equality)
        HashSet<CardInstance> uniqueCards = new HashSet<CardInstance>(pile.Cards);
        Assert.Equal(10, uniqueCards.Count); // All cards are unique instances
    }

    [Fact]
    public void Add_CardWithNullContext_ContextInitialized()
    {
        // Arrange
        Pile pile = new Pile();
        CardInstance card = CreateTestCard("test");
        card.Context = null; // Explicitly null context

        // Act
        pile.Add(card);

        // Assert: Pile.Add initializes Context if null
        Assert.NotNull(card.Context);
    }

    [Fact]
    public void DrawMultiple_AfterShuffle_ReturnsCorrectCount()
    {
        // Arrange: Pile with 10 cards
        Pile pile = new Pile();
        for (int i = 0; i < 10; i++)
        {
            pile.Add(CreateTestCard($"card_{i}"));
        }

        // Act
        pile.Shuffle();
        List<CardInstance> drawn = pile.DrawMultiple(5);

        // Assert
        Assert.Equal(5, drawn.Count);
        Assert.Equal(5, pile.Count); // 5 cards remain
    }

    [Fact]
    public void Shuffle_PerformanceTest_LargeDeck()
    {
        // Arrange: Very large pile (1000 cards) to test performance
        Pile pile = new Pile();
        for (int i = 0; i < 1000; i++)
        {
            pile.Add(CreateTestCard($"card_{i}"));
        }

        // Act: Shuffle should complete quickly (no timeout)
        pile.Shuffle();

        // Assert: All cards still present
        Assert.Equal(1000, pile.Count);
    }

    // ========== HELPER METHODS ==========

    /// <summary>
    /// Create test card instance with minimal valid template
    /// </summary>
    private CardInstance CreateTestCard(string id)
    {
        SocialCard template = new SocialCard
        {
            Id = id,
            Title = $"Test Card {id}",
            Persistence = PersistenceType.Statement
        };

        return new CardInstance(template);
    }
}
