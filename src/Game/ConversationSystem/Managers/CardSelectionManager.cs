using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages card selection rules and multi-card play.
/// Enforces weight limits and combination rules.
/// </summary>
public class CardSelectionManager
{
    private readonly EmotionalState currentState;
    private readonly StateRuleset rules;
    private readonly HashSet<ConversationCard> selectedCards;
    private readonly Random random;

    public CardSelectionManager(EmotionalState state)
    {
        currentState = state;
        rules = ConversationRules.States[state];
        selectedCards = new HashSet<ConversationCard>();
        random = new Random();
    }

    /// <summary>
    /// Current total weight of selected cards
    /// </summary>
    public int TotalWeight => selectedCards.Sum(c => c.GetEffectiveWeight(currentState));

    /// <summary>
    /// Maximum weight allowed by current state
    /// </summary>
    public int MaxWeight => rules.MaxWeight;

    /// <summary>
    /// Currently selected cards
    /// </summary>
    public IReadOnlyCollection<ConversationCard> SelectedCards => selectedCards;

    /// <summary>
    /// Check if a card can be selected given current selection
    /// </summary>
    public bool CanSelectCard(ConversationCard card)
    {
        // State cards must be played alone
        if (card.Category == CardCategory.STATE && selectedCards.Any())
            return false;
        if (selectedCards.Any(c => c.Category == CardCategory.STATE))
            return false;

        // Crisis cards must be played alone
        if (card.Category == CardCategory.CRISIS && selectedCards.Any())
            return false;
        if (selectedCards.Any(c => c.Category == CardCategory.CRISIS))
            return false;

        // OVERWHELMED state: max 1 card only
        if (rules.MaxCards.HasValue && selectedCards.Count >= rules.MaxCards.Value)
            return false;

        // Check weight limit
        var newWeight = TotalWeight + card.GetEffectiveWeight(currentState);
        if (newWeight > rules.MaxWeight)
            return false;

        return true;
    }

    /// <summary>
    /// Toggle card selection
    /// </summary>
    public bool ToggleCard(ConversationCard card)
    {
        if (selectedCards.Contains(card))
        {
            selectedCards.Remove(card);
            return false; // Card deselected
        }
        else if (CanSelectCard(card))
        {
            selectedCards.Add(card);
            return true; // Card selected
        }
        return false; // Can't select
    }

    /// <summary>
    /// Clear all selections
    /// </summary>
    public void ClearSelection()
    {
        selectedCards.Clear();
    }

    /// <summary>
    /// Play the selected cards and calculate results
    /// </summary>
    public CardPlayResult PlaySelectedCards()
    {
        if (!selectedCards.Any())
        {
            throw new InvalidOperationException("No cards selected to play");
        }

        var results = new List<SingleCardResult>();
        var totalComfort = 0;
        EmotionalState? newState = null;

        // Roll for each card individually
        foreach (var card in selectedCards)
        {
            var successChance = card.CalculateSuccessChance();
            var roll = random.Next(100);
            var success = roll < successChance;
            
            var comfort = success ? card.BaseComfort : 1;
            
            results.Add(new SingleCardResult
            {
                Card = card,
                Success = success,
                Comfort = comfort,
                Roll = roll,
                SuccessChance = successChance
            });
            
            totalComfort += comfort;
        }

        // Apply set bonuses for same type
        var types = selectedCards.Select(c => c.Type).Distinct();
        if (types.Count() == 1 && selectedCards.Count > 1)
        {
            var setBonus = GetSetBonus(selectedCards.Count);
            totalComfort += setBonus;

            // EAGER state gives additional bonus
            if (currentState == EmotionalState.EAGER && selectedCards.Count >= 2)
            {
                totalComfort += rules.SetBonus;
            }
        }

        // CONNECTED state gives bonus to all comfort
        if (currentState == EmotionalState.CONNECTED)
        {
            totalComfort += 2;
        }

        // State changes ONLY for single cards
        if (selectedCards.Count == 1)
        {
            var card = selectedCards.First();
            var result = results.First();

            if (card.Category == CardCategory.STATE)
            {
                newState = result.Success ? card.SuccessState : card.FailureState;
            }
        }

        return new CardPlayResult
        {
            TotalComfort = totalComfort,
            NewState = newState,
            Results = results,
            SetBonus = types.Count() == 1 && selectedCards.Count > 1 ? GetSetBonus(selectedCards.Count) : 0,
            ConnectedBonus = currentState == EmotionalState.CONNECTED ? 2 : 0,
            EagerBonus = currentState == EmotionalState.EAGER && selectedCards.Count >= 2 ? rules.SetBonus : 0
        };
    }

    /// <summary>
    /// Calculate set bonus for playing multiple cards of same type
    /// </summary>
    private int GetSetBonus(int count)
    {
        return count switch
        {
            2 => 2,
            3 => 5,
            _ => 8 // 4+
        };
    }

    /// <summary>
    /// Check if current selection is valid for playing
    /// </summary>
    public bool CanPlaySelection()
    {
        if (!selectedCards.Any())
            return false;

        // EAGER state requires playing 2+ cards
        if (rules.RequiredCards.HasValue && selectedCards.Count < rules.RequiredCards.Value)
            return false;

        return true;
    }

    /// <summary>
    /// Get descriptive text for current selection
    /// </summary>
    public string GetSelectionDescription()
    {
        if (!selectedCards.Any())
            return "No cards selected";

        var count = selectedCards.Count;
        var types = selectedCards.Select(c => c.Type).Distinct();
        
        if (selectedCards.Any(c => c.Category == CardCategory.CRISIS))
            return "DESPERATE ACTION";
        
        if (selectedCards.Any(c => c.Category == CardCategory.STATE))
            return "Emotional Shift";
        
        if (types.Count() == 1)
        {
            if (count == 1)
                return "Simple Expression";
            return $"Coherent Expression ({count} {types.First()})";
        }
        
        return "Scattered Expression";
    }
}