using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Wayfarer.GameState.Enums;

/// <summary>
/// Tests for the categorical card system implementation
/// Verifies that the property-based system has been fully replaced with categorical enums
/// </summary>
public class CategoricalCardSystemTest
{
    private readonly CategoricalEffectResolver _effectResolver;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly AtmosphereManager _atmosphereManager;
    private readonly FocusManager _focusManager;
    private readonly CardDeckManager _deckManager;
    private readonly GameWorld _gameWorld;

    public CategoricalCardSystemTest()
    {
        _gameWorld = new GameWorld();
        _tokenManager = new TokenMechanicsManager(_gameWorld);
        _atmosphereManager = new AtmosphereManager();
        _focusManager = new FocusManager();
        _deckManager = new CardDeckManager(_gameWorld, null);
        _effectResolver = new CategoricalEffectResolver(_tokenManager, _atmosphereManager, _focusManager, _deckManager);
    }

    [Fact]
    public void TestPersistenceTypes()
    {
        // Test Thought persistence (survives LISTEN)
        var thoughtCard = new ConversationCard
        {
            Id = "thought_card",
            Description = "A persistent thought",
            Persistence = PersistenceType.Thought,
            SuccessType = SuccessEffectType.Rapport,
            Difficulty = Difficulty.Medium
        };
        Assert.Equal(PersistenceType.Thought, thoughtCard.Persistence);

        // Test Impulse persistence (removed after SPEAK)
        var impulseCard = new ConversationCard
        {
            Id = "impulse_card",
            Description = "An impulsive action",
            Persistence = PersistenceType.Impulse,
            SuccessType = SuccessEffectType.Threading,
            ExhaustType = ExhaustEffectType.Focusing,
            Difficulty = Difficulty.Hard
        };
        Assert.Equal(PersistenceType.Impulse, impulseCard.Persistence);

        // Test Opening persistence (removed after LISTEN)
        var openingCard = new ConversationCard
        {
            Id = "opening_card",
            Description = "A fleeting opportunity",
            Persistence = PersistenceType.Opening,
            SuccessType = SuccessEffectType.Atmospheric,
            ExhaustType = ExhaustEffectType.Regret,
            Difficulty = Difficulty.Easy
        };
        Assert.Equal(PersistenceType.Opening, openingCard.Persistence);
    }

    [Fact]
    public void TestSuccessEffectTypes()
    {
        var session = CreateTestSession();

        // Test Rapport effect
        var rapportCard = CreateCardInstance(SuccessEffectType.Rapport, Difficulty.Medium);
        var rapportResult = _effectResolver.ProcessSuccessEffect(rapportCard, session);
        Assert.Equal(2, rapportResult.RapportChange); // Medium difficulty = magnitude 2

        // Test Threading effect (card draw)
        var threadingCard = CreateCardInstance(SuccessEffectType.Threading, Difficulty.Hard);
        var threadingResult = _effectResolver.ProcessSuccessEffect(threadingCard, session);
        Assert.Equal(3, threadingResult.CardsToAdd.Count); // Hard difficulty = magnitude 3

        // Test Focusing effect
        var focusingCard = CreateCardInstance(SuccessEffectType.Focusing, Difficulty.Easy);
        var focusingResult = _effectResolver.ProcessSuccessEffect(focusingCard, session);
        Assert.Equal(1, focusingResult.FocusAdded); // Easy difficulty = magnitude 1

        // Test Atmospheric effect
        var atmosphericCard = CreateCardInstance(SuccessEffectType.Atmospheric, Difficulty.VeryHard);
        var atmosphericResult = _effectResolver.ProcessSuccessEffect(atmosphericCard, session);
        Assert.Equal(AtmosphereType.Synchronized, atmosphericResult.AtmosphereTypeChange); // VeryHard = magnitude 4 = Synchronized

        // Test Promising effect (double rapport)
        var promisingCard = CreateCardInstance(SuccessEffectType.Promising, Difficulty.Medium);
        var promisingResult = _effectResolver.ProcessSuccessEffect(promisingCard, session);
        Assert.Equal(4, promisingResult.RapportChange); // Medium difficulty = magnitude 2, doubled for promising

        // Test Advancing effect (ignores magnitude)
        var advancingCard = CreateCardInstance(SuccessEffectType.Advancing, Difficulty.VeryEasy);
        var advancingResult = _effectResolver.ProcessSuccessEffect(advancingCard, session);
        Assert.Contains("advanced", advancingResult.SpecialEffect.ToLower());
    }

    [Fact]
    public void TestFailureEffectTypes()
    {
        var session = CreateTestSession();

        // Test Overreach (clears hand)
        session.ActiveCards.AddRange(new[] {
            CreateCardInstance(SuccessEffectType.None, Difficulty.Easy),
            CreateCardInstance(SuccessEffectType.None, Difficulty.Medium),
            CreateCardInstance(SuccessEffectType.None, Difficulty.Hard)
        });
        var overreachCard = CreateCardInstance(SuccessEffectType.Rapport, Difficulty.Medium, FailureEffectType.Overreach);
        var overreachResult = _effectResolver.ProcessFailureEffect(overreachCard, session);
        Assert.Equal(0, session.ActiveCards.Count);
        Assert.Contains("Overreach", overreachResult.SpecialEffect);

        // Test Backfire (negative rapport)
        var backfireCard = CreateCardInstance(SuccessEffectType.Rapport, Difficulty.Hard, FailureEffectType.Backfire);
        var backfireResult = _effectResolver.ProcessFailureEffect(backfireCard, session);
        Assert.Equal(-3, backfireResult.RapportChange); // Hard difficulty = magnitude 3, negative

        // Test Disrupting (removes high-focus cards)
        session.ActiveCards.Clear();
        session.ActiveCards.AddRange(new[] {
            CreateCardInstanceWithFocus(1),
            CreateCardInstanceWithFocus(2),
            CreateCardInstanceWithFocus(3),
            CreateCardInstanceWithFocus(4)
        });
        var disruptingCard = CreateCardInstance(SuccessEffectType.Rapport, Difficulty.Medium, FailureEffectType.Disrupting);
        var disruptingResult = _effectResolver.ProcessFailureEffect(disruptingCard, session);
        Assert.Equal(2, session.ActiveCards.Count); // Only focus 1 and 2 remain
        Assert.All(session.ActiveCards.Cards, c => Assert.True(c.Focus < 3));
    }

    [Fact]
    public void TestExhaustEffectTypes()
    {
        var session = CreateTestSession();

        // Test Threading exhaust (draw cards)
        var threadingExhaustCard = CreateCardInstance(
            SuccessEffectType.None,
            Difficulty.Medium,
            exhaustType: ExhaustEffectType.Threading,
            persistence: PersistenceType.Impulse);
        var threadingResult = _effectResolver.ProcessExhaustEffect(threadingExhaustCard, session);
        Assert.Equal(2, threadingResult.CardsToAdd.Count); // Medium = magnitude 2

        // Test Focusing exhaust (restore focus)
        var focusingExhaustCard = CreateCardInstance(
            SuccessEffectType.None,
            Difficulty.Hard,
            exhaustType: ExhaustEffectType.Focusing,
            persistence: PersistenceType.Opening);
        var focusingResult = _effectResolver.ProcessExhaustEffect(focusingExhaustCard, session);
        Assert.Equal(3, focusingResult.FocusAdded); // Hard = magnitude 3

        // Test Regret exhaust (lose rapport)
        var regretExhaustCard = CreateCardInstance(
            SuccessEffectType.None,
            Difficulty.Easy,
            exhaustType: ExhaustEffectType.Regret,
            persistence: PersistenceType.Opening);
        var regretResult = _effectResolver.ProcessExhaustEffect(regretExhaustCard, session);
        Assert.Equal(-1, regretResult.RapportChange); // Easy = magnitude 1, negative

        // Test that Thought cards don't trigger exhaust effects
        var thoughtCard = CreateCardInstance(
            SuccessEffectType.None,
            Difficulty.VeryHard,
            exhaustType: ExhaustEffectType.Regret,
            persistence: PersistenceType.Thought);
        var thoughtResult = _effectResolver.ProcessExhaustEffect(thoughtCard, session);
        Assert.Equal(0, thoughtResult.RapportChange); // No effect for Thought cards
    }

    [Fact]
    public void TestMagnitudeFromDifficulty()
    {
        Assert.Equal(1, _effectResolver.GetMagnitudeFromDifficulty(Difficulty.VeryEasy));
        Assert.Equal(1, _effectResolver.GetMagnitudeFromDifficulty(Difficulty.Easy));
        Assert.Equal(2, _effectResolver.GetMagnitudeFromDifficulty(Difficulty.Medium));
        Assert.Equal(3, _effectResolver.GetMagnitudeFromDifficulty(Difficulty.Hard));
        Assert.Equal(4, _effectResolver.GetMagnitudeFromDifficulty(Difficulty.VeryHard));
    }

    [Fact]
    public void TestCardTypeCategorization()
    {
        // Test standard conversation card
        var conversationCard = new ConversationCard
        {
            CardType = CardType.Conversation,
            Persistence = PersistenceType.Thought
        };
        Assert.Equal(CardType.Conversation, conversationCard.CardType);

        // Test Letter card (goal card)
        var letterCard = new ConversationCard
        {
            CardType = CardType.Letter,
            RapportThreshold = 5,
            Persistence = PersistenceType.Thought
        };
        Assert.Equal(CardType.Letter, letterCard.CardType);
        Assert.Equal(5, letterCard.RapportThreshold);

        // Test Promise card
        var promiseCard = new ConversationCard
        {
            CardType = CardType.Promise,
            RapportThreshold = 3,
            QueuePosition = 1,
            InstantRapport = 2
        };
        Assert.Equal(CardType.Promise, promiseCard.CardType);
        Assert.Equal(1, promiseCard.QueuePosition);

        // Test BurdenGoal card
        var burdenCard = new ConversationCard
        {
            CardType = CardType.BurdenGoal,
            RapportThreshold = 7
        };
        Assert.Equal(CardType.BurdenGoal, burdenCard.CardType);
    }

    [Fact]
    public void TestAtmosphereModifiers()
    {
        var session = CreateTestSession();

        // Test Focused atmosphere (+1 magnitude)
        session.CurrentAtmosphere = AtmosphereType.Focused;
        var focusedCard = CreateCardInstance(SuccessEffectType.Rapport, Difficulty.Medium);
        var focusedResult = _effectResolver.ProcessSuccessEffect(focusedCard, session);
        Assert.Equal(3, focusedResult.RapportChange); // Base 2 + 1 for focused

        // Test Exposed atmosphere (double magnitude)
        session.CurrentAtmosphere = AtmosphereType.Exposed;
        var exposedCard = CreateCardInstance(SuccessEffectType.Rapport, Difficulty.Medium);
        var exposedResult = _effectResolver.ProcessSuccessEffect(exposedCard, session);
        Assert.Equal(4, exposedResult.RapportChange); // Base 2 * 2 for exposed

        // Test Synchronized atmosphere (effect happens twice)
        session.CurrentAtmosphere = AtmosphereType.Synchronized;
        var synchronizedCard = CreateCardInstance(SuccessEffectType.Rapport, Difficulty.Medium);
        var synchronizedResult = _effectResolver.ProcessSuccessEffect(synchronizedCard, session);
        Assert.Equal(4, synchronizedResult.RapportChange); // Base 2 * 2 for synchronized
        Assert.Contains("synchronized", synchronizedResult.SpecialEffect.ToLower());
    }

    [Fact]
    public void TestSuccessPercentageCalculation()
    {
        var session = CreateTestSession();

        // Test base success percentages
        var veryEasyCard = CreateCardInstance(SuccessEffectType.None, Difficulty.VeryEasy);
        Assert.Equal(85, _effectResolver.CalculateSuccessPercentage(veryEasyCard, session));

        var easyCard = CreateCardInstance(SuccessEffectType.None, Difficulty.Easy);
        Assert.Equal(70, _effectResolver.CalculateSuccessPercentage(easyCard, session));

        var mediumCard = CreateCardInstance(SuccessEffectType.None, Difficulty.Medium);
        Assert.Equal(50, _effectResolver.CalculateSuccessPercentage(mediumCard, session));

        var hardCard = CreateCardInstance(SuccessEffectType.None, Difficulty.Hard);
        Assert.Equal(30, _effectResolver.CalculateSuccessPercentage(hardCard, session));

        var veryHardCard = CreateCardInstance(SuccessEffectType.None, Difficulty.VeryHard);
        Assert.Equal(15, _effectResolver.CalculateSuccessPercentage(veryHardCard, session));

        // Test Focused atmosphere bonus (+20%)
        session.CurrentAtmosphere = AtmosphereType.Focused;
        Assert.Equal(70, _effectResolver.CalculateSuccessPercentage(mediumCard, session)); // 50 + 20
    }

    [Fact]
    public void TestRequestCardPlayability()
    {
        // Request cards (Letter/Promise/BurdenGoal) always succeed
        var letterCard = new CardInstance
        {
            CardType = CardType.Letter,
            Difficulty = Difficulty.VeryHard // Even with VeryHard, should be 100%
        };
        Assert.Equal(100, letterCard.CalculateSuccessChance());

        var promiseCard = new CardInstance
        {
            CardType = CardType.Promise,
            Difficulty = Difficulty.Hard
        };
        Assert.Equal(100, promiseCard.CalculateSuccessChance());

        var burdenCard = new CardInstance
        {
            CardType = CardType.BurdenGoal,
            Difficulty = Difficulty.Medium
        };
        Assert.Equal(100, burdenCard.CalculateSuccessChance());
    }

    // Helper methods
    private ConversationSession CreateTestSession()
    {
        var npc = new NPC { ID = "test_npc", Name = "Test NPC" };
        var deck = new SessionCardDeck();

        // Add some test cards to the deck
        for (int i = 0; i < 10; i++)
        {
            deck.AddCard(new ConversationCard
            {
                Id = $"deck_card_{i}",
                Description = $"Test card {i}",
                Persistence = PersistenceType.Thought,
                Difficulty = Difficulty.Medium
            });
        }

        return new ConversationSession
        {
            NPC = npc,
            CurrentState = ConnectionState.NEUTRAL,
            CurrentAtmosphere = AtmosphereType.Neutral,
            Deck = deck,
            ActiveCards = new Pile(),
            DrawPile = new Pile(),
            ExhaustPile = new Pile()
        };
    }

    private CardInstance CreateCardInstance(
        SuccessEffectType successType,
        Difficulty difficulty,
        FailureEffectType failureType = FailureEffectType.None,
        ExhaustEffectType exhaustType = ExhaustEffectType.None,
        PersistenceType persistence = PersistenceType.Thought)
    {
        return new CardInstance
        {
            Id = Guid.NewGuid().ToString(),
            Description = "Test card",
            SuccessType = successType,
            FailureType = failureType,
            ExhaustType = exhaustType,
            Persistence = persistence,
            Difficulty = difficulty,
            Focus = 2,
            TokenType = ConnectionType.Trust
        };
    }

    private CardInstance CreateCardInstanceWithFocus(int focus)
    {
        return new CardInstance
        {
            Id = Guid.NewGuid().ToString(),
            Description = $"Card with focus {focus}",
            Focus = focus,
            Difficulty = Difficulty.Medium,
            TokenType = ConnectionType.Trust
        };
    }
}