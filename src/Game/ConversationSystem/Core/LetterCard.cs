using System;
using System.Collections.Generic;

/// <summary>
/// Eligibility requirements for letter cards
/// </summary>
public class LetterEligibility
{
    /// <summary>
    /// Minimum token count required by type
    /// </summary>
    public Dictionary<ConnectionType, int> RequiredTokens { get; init; } = new();
    
    /// <summary>
    /// Emotional states that make this letter eligible
    /// </summary>
    public List<EmotionalState> RequiredStates { get; init; } = new();
    
    /// <summary>
    /// Check if current token balance and state meet requirements
    /// </summary>
    public bool IsEligible(Dictionary<ConnectionType, int> tokens, EmotionalState currentState)
    {
        // Check token requirements
        foreach (var requirement in RequiredTokens)
        {
            var currentTokens = tokens.GetValueOrDefault(requirement.Key, 0);
            if (currentTokens < requirement.Value)
                return false;
        }
        
        // Check state requirements
        if (RequiredStates.Count > 0 && !RequiredStates.Contains(currentState))
            return false;
            
        return true;
    }
}

/// <summary>
/// Negotiation terms for letter acceptance
/// </summary>
public class LetterNegotiationTerms
{
    /// <summary>
    /// Deadline in hours
    /// </summary>
    public int DeadlineHours { get; init; }
    
    /// <summary>
    /// Preferred queue position (0 = front of queue)
    /// </summary>
    public int QueuePosition { get; init; }
    
    /// <summary>
    /// Payment in coins
    /// </summary>
    public int Payment { get; init; }
    
    /// <summary>
    /// Whether this forces position 1 (crisis letters)
    /// </summary>
    public bool ForcesPositionOne { get; init; }
}

/// <summary>
/// A letter card represents an opportunity to negotiate letter delivery terms.
/// Letters become eligible based on token requirements and emotional state.
/// Playing the card determines success/failure for negotiation terms.
/// </summary>
public class LetterCard
{
    /// <summary>
    /// Unique identifier for this letter
    /// </summary>
    public string Id { get; init; }
    
    /// <summary>
    /// Letter title for display
    /// </summary>
    public string Title { get; init; }
    
    /// <summary>
    /// Description of the letter content
    /// </summary>
    public string Description { get; init; }
    
    /// <summary>
    /// Letter template type for narrative generation
    /// </summary>
    public string TemplateType { get; init; }
    
    /// <summary>
    /// NPC personality type (affects narrative framing)
    /// </summary>
    public PersonalityType NPCPersonality { get; init; }
    
    /// <summary>
    /// Card depth/power level for comfort requirements
    /// </summary>
    public int Depth { get; init; }
    
    /// <summary>
    /// Card weight for speak action limits
    /// </summary>
    public int Weight { get; init; }
    
    /// <summary>
    /// Eligibility requirements to offer this letter
    /// </summary>
    public LetterEligibility Eligibility { get; init; }
    
    /// <summary>
    /// Terms if negotiation succeeds
    /// </summary>
    public LetterNegotiationTerms SuccessTerms { get; init; }
    
    /// <summary>
    /// Terms if negotiation fails
    /// </summary>
    public LetterNegotiationTerms FailureTerms { get; init; }
    
    /// <summary>
    /// Connection type this letter card builds (usually Trust)
    /// </summary>
    public ConnectionType ConnectionType { get; init; } = ConnectionType.Trust;
    
    /// <summary>
    /// Base success rate for negotiation (before token modifiers)
    /// </summary>
    public int BaseSuccessRate { get; init; } = 60;
    
    /// <summary>
    /// Check if this letter is eligible with current tokens and state
    /// </summary>
    public bool IsEligible(Dictionary<ConnectionType, int> tokens, EmotionalState currentState)
    {
        return Eligibility.IsEligible(tokens, currentState);
    }
    
    /// <summary>
    /// Calculate success chance based on tokens
    /// </summary>
    public int CalculateSuccessChance(Dictionary<ConnectionType, int> tokens)
    {
        var chance = BaseSuccessRate;
        
        // Add 5% per relevant token
        var relevantTokens = tokens.GetValueOrDefault(ConnectionType, 0);
        chance += relevantTokens * 5;
        
        return Math.Clamp(chance, 10, 95);
    }
    
    /// <summary>
    /// Convert to ConversationCard for hand play
    /// </summary>
    public ConversationCard ToConversationCard(string npcId, string npcName)
    {
        return new ConversationCard
        {
            Id = Id,
            Template = CardTemplateType.NegotiateDeadline, // Letter negotiation template
            Context = new CardContext
            {
                NPCName = npcName,
                NPCPersonality = NPCPersonality,
                LetterId = Id,
                TargetNpcId = npcId,
                HasDeadline = true,
                MinutesUntilDeadline = SuccessTerms.DeadlineHours * 60,
                UrgencyLevel = SuccessTerms.DeadlineHours <= 2 ? 3 : SuccessTerms.DeadlineHours <= 6 ? 2 : 1
            },
            Type = CardType.Trust, // Letter cards are typically Trust type
            Persistence = PersistenceType.Fleeting, // Letters are opportunities that can be lost
            Weight = Weight,
            BaseComfort = 0, // Letters don't give comfort, they create obligations
            Category = CardCategory.LETTER, // New category for letter cards
            Depth = Depth,
            DisplayName = Title,
            Description = Description,
            CanDeliverLetter = false, // This IS the letter negotiation
            ManipulatesObligations = true, // Creates new obligations
            SuccessRate = CalculateSuccessChance(new Dictionary<ConnectionType, int>()) // Will be recalculated with actual tokens
        };
    }
}

/// <summary>
/// Factory for creating letter cards based on NPC and scenario
/// </summary>
public static class LetterCardFactory
{
    /// <summary>
    /// Create Elena's letter deck from POC specification
    /// </summary>
    public static List<LetterCard> CreateElenaLetterDeck(string npcId)
    {
        var deck = new List<LetterCard>();
        
        // "Crisis Letter to Lord Blackwood" - POC Main Scenario
        // Eligible: Trust tokens ≥ 0 (available immediately in DESPERATE state)
        // Success: 3-hour deadline, queue position 2-3, 12 coins payment
        // Failure: 1-hour deadline, forces position 1 (displaces Marcus), 15 coins payment
        deck.Add(new LetterCard
        {
            Id = $"{npcId}_crisis_letter_blackwood",
            Title = "Crisis Letter to Lord Blackwood",
            Description = "Elena desperately needs to send this urgent refusal before Lord Blackwood's deadline",
            TemplateType = "crisis_letter",
            NPCPersonality = PersonalityType.DEVOTED,
            Depth = 7,  // Accessible with moderate comfort building
            Weight = 0,  // Free to play in DESPERATE state (crisis mechanics)
            Eligibility = new LetterEligibility
            {
                RequiredTokens = new() { },  // No token requirement - urgency overrides trust barriers
                RequiredStates = new() { EmotionalState.DESPERATE, EmotionalState.TENSE }  // Only available when Elena is stressed
            },
            SuccessTerms = new LetterNegotiationTerms
            {
                DeadlineHours = 3,  // Reasonable but urgent deadline
                QueuePosition = 2,  // Gets position 2 on good negotiation
                Payment = 12,
                ForcesPositionOne = false
            },
            FailureTerms = new LetterNegotiationTerms
            {
                DeadlineHours = 1,  // Crisis deadline!
                QueuePosition = 1,  // Forces position 1
                Payment = 15,       // Higher payment for urgent delivery
                ForcesPositionOne = true  // This triggers automatic displacement
            },
            ConnectionType = ConnectionType.Trust,
            BaseSuccessRate = 50  // 50/50 chance, modified by Trust tokens
        });
        
        // "Formal Refusal to Lord Blackwood"
        // Eligible: Trust tokens ≥ 3, Open/Connected state
        // Success: 8-hour deadline, position 3, 8 coins
        // Failure: 2-hour deadline, position 2, 10 coins
        deck.Add(new LetterCard
        {
            Id = $"{npcId}_formal_refusal_blackwood",
            Title = "Formal Refusal to Lord Blackwood", 
            Description = "Elena wants to formally decline Lord Blackwood's proposition",
            TemplateType = "formal_refusal",
            NPCPersonality = PersonalityType.DEVOTED,
            Depth = 12,
            Weight = 2,
            Eligibility = new LetterEligibility
            {
                RequiredTokens = new() { { ConnectionType.Trust, 3 } },
                RequiredStates = new() { EmotionalState.OPEN, EmotionalState.CONNECTED }
            },
            SuccessTerms = new LetterNegotiationTerms
            {
                DeadlineHours = 8,
                QueuePosition = 3,
                Payment = 8,
                ForcesPositionOne = false
            },
            FailureTerms = new LetterNegotiationTerms
            {
                DeadlineHours = 2,
                QueuePosition = 2,
                Payment = 10,
                ForcesPositionOne = false
            },
            ConnectionType = ConnectionType.Trust,
            BaseSuccessRate = 60
        });
        
        return deck;
    }
}