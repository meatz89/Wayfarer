using Xunit;

namespace Wayfarer.Tests;

/// <summary>
/// Tests for CardAnimationManager after Dictionary → List refactoring.
/// Validates that List<CardAnimationState> pattern correctly manages animation states.
/// </summary>
public class CardAnimationManagerTests
{
    [Fact]
    public void CardStates_InitiallyEmpty()
    {
        CardAnimationManager manager = new CardAnimationManager();

        Assert.Empty(manager.CardStates);
    }

    [Fact]
    public void CardStates_ReturnsCorrectType()
    {
        CardAnimationManager manager = new CardAnimationManager();

        List<CardAnimationState> states = manager.CardStates;

        Assert.IsType<List<CardAnimationState>>(states);
    }

    [Fact]
    public void CardStates_CanAddState()
    {
        CardAnimationManager manager = new CardAnimationManager();
        CardAnimationState state = new CardAnimationState
        {
            CardId = "test_card_1",
            State = "new",
            StateChangedAt = DateTime.Now,
            AnimationDelayMs = 200, // DDR-007: int milliseconds instead of double seconds
            SequenceIndex = 0,
            AnimationDirection = "up"
        };

        manager.CardStates.Add(state);

        Assert.Single(manager.CardStates);
        Assert.Equal("test_card_1", manager.CardStates[0].CardId);
    }

    [Fact]
    public void CardStates_CanFindByCardId_Linq()
    {
        CardAnimationManager manager = new CardAnimationManager();
        manager.CardStates.Add(new CardAnimationState
        {
            CardId = "card_1",
            State = "new"
        });
        manager.CardStates.Add(new CardAnimationState
        {
            CardId = "card_2",
            State = "played-success"
        });

        CardAnimationState found = manager.CardStates.FirstOrDefault(s => s.CardId == "card_2");

        Assert.NotNull(found);
        Assert.Equal("card_2", found.CardId);
        Assert.Equal("played-success", found.State);
    }

    [Fact]
    public void CardStates_FindNonExistent_ReturnsNull()
    {
        CardAnimationManager manager = new CardAnimationManager();
        manager.CardStates.Add(new CardAnimationState
        {
            CardId = "card_1",
            State = "new"
        });

        CardAnimationState found = manager.CardStates.FirstOrDefault(s => s.CardId == "nonexistent");

        Assert.Null(found);
    }

    [Fact]
    public void CardStates_CanRemoveByCardId()
    {
        CardAnimationManager manager = new CardAnimationManager();
        CardAnimationState state1 = new CardAnimationState { CardId = "card_1", State = "new" };
        CardAnimationState state2 = new CardAnimationState { CardId = "card_2", State = "new" };
        manager.CardStates.Add(state1);
        manager.CardStates.Add(state2);

        CardAnimationState toRemove = manager.CardStates.FirstOrDefault(s => s.CardId == "card_1");
        manager.CardStates.Remove(toRemove);

        Assert.Single(manager.CardStates);
        Assert.Equal("card_2", manager.CardStates[0].CardId);
    }

    [Fact]
    public void CardStates_CanFilterByState()
    {
        CardAnimationManager manager = new CardAnimationManager();
        manager.CardStates.Add(new CardAnimationState { CardId = "card_1", State = "new" });
        manager.CardStates.Add(new CardAnimationState { CardId = "card_2", State = "played-success" });
        manager.CardStates.Add(new CardAnimationState { CardId = "card_3", State = "new" });

        List<CardAnimationState> newStates = manager.CardStates.Where(s => s.State == "new").ToList();

        Assert.Equal(2, newStates.Count);
        Assert.All(newStates, state => Assert.Equal("new", state.State));
    }

    [Fact]
    public void CardStates_CanOrderBySequenceIndex()
    {
        CardAnimationManager manager = new CardAnimationManager();
        manager.CardStates.Add(new CardAnimationState { CardId = "card_3", SequenceIndex = 2 });
        manager.CardStates.Add(new CardAnimationState { CardId = "card_1", SequenceIndex = 0 });
        manager.CardStates.Add(new CardAnimationState { CardId = "card_2", SequenceIndex = 1 });

        List<CardAnimationState> ordered = manager.CardStates.OrderBy(s => s.SequenceIndex).ToList();

        Assert.Equal("card_1", ordered[0].CardId);
        Assert.Equal("card_2", ordered[1].CardId);
        Assert.Equal("card_3", ordered[2].CardId);
    }

    [Fact]
    public void CardStates_MultipleCardsWithSameState_AllFound()
    {
        CardAnimationManager manager = new CardAnimationManager();
        manager.CardStates.Add(new CardAnimationState { CardId = "card_1", State = "exhausting" });
        manager.CardStates.Add(new CardAnimationState { CardId = "card_2", State = "exhausting" });
        manager.CardStates.Add(new CardAnimationState { CardId = "card_3", State = "normal" });

        List<CardAnimationState> exhausting = manager.CardStates
            .Where(s => s.State == "exhausting")
            .ToList();

        Assert.Equal(2, exhausting.Count);
    }
}
