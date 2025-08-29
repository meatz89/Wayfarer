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
    private Dictionary<ConnectionType, int> npcTokens;

    public CardSelectionManager(EmotionalState state, Dictionary<ConnectionType, int> tokens = null)
    {
        currentState = state;
        rules = ConversationRules.States[state];
        selectedCards = new HashSet<ConversationCard>();
        random = new Random();
        npcTokens = tokens ?? new Dictionary<ConnectionType, int>();
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
        // HARD RULE: SPEAK plays exactly ONE card - this is core design
        if (selectedCards.Any())
            return false;

        // Check if individual card weight exceeds state's max weight
        // (Weight represents emotional intensity the state can process)
        var cardWeight = card.GetEffectiveWeight(currentState);
        if (cardWeight > rules.MaxWeight)
        {
            return false;
        }

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
        var letterNegotiations = new List<LetterNegotiationResult>();
        
        foreach (var card in selectedCards)
        {
            // Exchange cards ALWAYS succeed - they're simple trades
            if (card.Mechanics == CardMechanics.Exchange)
            {
                results.Add(new SingleCardResult
                {
                    Card = card,
                    Success = true, // Exchanges always succeed if affordable
                    Comfort = 0, // Exchanges don't generate comfort
                    Roll = 100, // Not used for exchanges
                    SuccessChance = 100 // 100% success rate
                });
                // Exchanges don't contribute to comfort
                continue;
            }
            
            // Promise cards (including letters) create negotiation results and obligations
            if (card.IsGoalCard && card.Mechanics == CardMechanics.Promise)
            {
                var letterSuccessChance = card.CalculateSuccessChance(npcTokens);
                var letterRoll = random.Next(100);
                var letterSuccess = letterRoll < letterSuccessChance;
                
                // Extract promise card ID from the conversation card context
                var promiseCardId = card.Context?.LetterId ?? card.Id;
                
                // Create a basic negotiation result - will be completed by ConversationSession
                var negotiationResult = new LetterNegotiationResult
                {
                    PromiseCardId = promiseCardId,
                    NegotiationSuccess = letterSuccess,
                    FinalTerms = null,        // Will be set by ConversationSession
                    SourcePromiseCard = null,  // Will be set by ConversationSession
                    CreatedObligation = null  // Will be set by ConversationSession
                };
                letterNegotiations.Add(negotiationResult);
                
                results.Add(new SingleCardResult
                {
                    Card = card,
                    Success = letterSuccess,
                    Comfort = 0, // Letter cards don't generate comfort, they create obligations
                    Roll = letterRoll,
                    SuccessChance = letterSuccessChance
                });
                
                // Letter cards don't contribute to comfort
                continue;
            }
            
            // Normal cards use success/failure mechanics with token bonuses
            var successChance = card.CalculateSuccessChance(npcTokens);
            var roll = random.Next(100);
            var success = roll < successChance;
            
            // Comfort is NOT used here - it's calculated based on weight in ConversationSession
            // We just track success/failure for the weight-based comfort calculation
            results.Add(new SingleCardResult
            {
                Card = card,
                Success = success,
                Comfort = 0, // Will be calculated based on weight in ConversationSession
                Roll = roll,
                SuccessChance = successChance
            });
        }

        // NO SET BONUSES - ONE-CARD RULE means no sets possible
        // NO CONNECTED BONUS - comfort is purely weight-based now

        // State changes work naturally with single card
        var singleCard = selectedCards.First();
        var singleResult = results.First();

        if (singleCard.IsStateCard)
        {
            newState = singleResult.Success ? singleCard.SuccessState : singleCard.FailureState;
        }

        return new CardPlayResult
        {
            TotalComfort = 0,  // Comfort is calculated from weight in ConversationSession
            NewState = newState,
            Results = results,
            SetBonus = 0,  // No set bonuses possible with ONE-CARD RULE
            ConnectedBonus = 0,  // No connected bonus in new system
            EagerBonus = 0,  // No eager bonus possible with ONE-CARD RULE
            LetterNegotiations = letterNegotiations,
            ManipulatedObligations = letterNegotiations.Any()
        };
    }

    /// <summary>
    /// Check if current selection is valid for playing
    /// </summary>
    public bool CanPlaySelection()
    {
        // ONE-CARD RULE: Must have exactly one card selected to play
        return selectedCards.Count == 1;
    }

    /// <summary>
    /// Get descriptive text for current selection
    /// </summary>
    public string GetSelectionDescription()
    {
        if (!selectedCards.Any())
            return "No card selected";

        // ONE-CARD RULE: Always exactly one card
        var card = selectedCards.First();
        
        if (card.BaseComfort <= 0 && card.Weight >= 2)
            return "DESPERATE ACTION";
        
        if (card.IsStateCard)
            return "Emotional Shift";
        
        if (card.Mechanics == CardMechanics.Exchange)
            return "Exchange Offer";
            
        if (card.IsGoalCard && card.Mechanics == CardMechanics.Promise)
            return card.IsGoalCard && card.GoalCardType == ConversationType.Promise ? "Letter Negotiation" : "Promise Negotiation";
        
        // Default for normal conversation cards
        return $"{card.Type} Expression";
    }
}