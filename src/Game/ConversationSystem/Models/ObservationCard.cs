using System;
using System.Collections.Generic;

/// <summary>
/// Decay states for observation cards
/// Models how information becomes outdated over time
/// </summary>
public enum ObservationDecayState
{
    Active,
    Expired,   // 6+ hours: Must discard, unplayable
    // Compatibility aliases
    Fresh = Active,
    Stale = Expired
}

/// <summary>
/// Observation card that tracks when it was created and its decay state
/// Observations naturally expire through three stages, modeling how information becomes outdated
/// </summary>
public class ObservationCard : ConversationCard
{
    /// <summary>
    /// Unique identifier for this observation card
    /// </summary>
    public string Id { get; init; }
    
    /// <summary>
    /// Card template type for frontend text generation
    /// </summary>
    public CardTemplateType Template { get; init; }
    
    /// <summary>
    /// ConversationCard interface compatibility - same as Template
    /// </summary>
    public CardTemplateType TemplateType => Template;
    
    /// <summary>
    /// Context data for template rendering
    /// </summary>
    public CardContext Context { get; init; }

    /// <summary>
    /// Which relationship type this card builds
    /// </summary>
    public CardType Type { get; init; }

    /// <summary>
    /// Observation cards are always fleeting
    /// </summary>
    public PersistenceType Persistence => PersistenceType.Fleeting;

    /// <summary>
    /// Emotional bandwidth required (usually low for observations)
    /// </summary>
    public int Weight { get; init; }

    /// <summary>
    /// Base comfort gained on success (varies with decay)
    /// </summary>
    public int BaseComfort { get; init; }

    /// <summary>
    /// Card depth/power level (usually 0 - observations are always accessible)
    /// </summary>
    public int Depth { get; init; } = 0;

    /// <summary>
    /// Override success rate for special cards (null uses calculated rate)
    /// </summary>
    public int? SuccessRate { get; init; }

    /// <summary>
    /// Display name for special cards (null uses template-generated name)
    /// </summary>
    public string DisplayName { get; init; }

    /// <summary>
    /// Description for special cards
    /// </summary>
    public string Description { get; init; }
    
    /// <summary>
    /// When this observation was made (game time)
    /// </summary>
    public DateTime CreatedAt { get; init; }
    
    /// <summary>
    /// Source observation ID from the observation system
    /// </summary>
    public string SourceObservationId { get; init; }
    
    /// <summary>
    /// Current decay state based on time elapsed
    /// </summary>
    public ObservationDecayState DecayState { get; private set; }
    
    /// <summary>
    /// Base comfort value when fresh
    /// </summary>
    public int BaseComfortValue { get; init; }
    
    /// <summary>
    /// ConversationCard interface - stub cost value
    /// </summary>
    public int Cost => 0;
    
    /// <summary>
    /// ConversationCard interface - stub reward value
    /// </summary>
    public int Reward => BaseComfort;
    
    /// <summary>
    /// ConversationCard interface - calculated success rate
    /// </summary>
    public int BaseSuccessRate => CalculateSuccessChance();
    
    /// <summary>
    /// ConversationCard interface - stub success terms for UI
    /// </summary>
    public List<string> SuccessTerms => new List<string> { "Shared observation" };
    
    /// <summary>
    /// ConversationCard interface - stub failure terms for UI
    /// </summary>
    public List<string> FailureTerms => new List<string> { "Awkward silence" };
    
    /// <summary>
    /// ConversationCard interface properties
    /// </summary>
    public bool IsExchange => false;
    public bool IsObservation => true;
    public bool IsGoalCard => false;
    public bool IsStateCard => false;
    public bool CanDeliverLetter => false;
    public bool ManipulatesObligations => false;
    
    /// <summary>
    /// Whether this card is currently playable
    /// </summary>
    public bool IsPlayable => DecayState != ObservationDecayState.Expired;
    
    /// <summary>
    /// Get the effective comfort value based on decay state
    /// </summary>
    public int EffectiveComfortValue => DecayState switch
    {
        ObservationDecayState.Active => BaseComfortValue,
        ObservationDecayState.Expired => 0, // No comfort when expired
        _ => 0
    };
    
    /// <summary>
    /// Update the decay state based on current game time
    /// </summary>
    public void UpdateDecayState(DateTime currentGameTime)
    {
        var hoursElapsed = (currentGameTime - CreatedAt).TotalHours;
        
        DecayState = hoursElapsed switch
        {
            < 6.0 => ObservationDecayState.Active,
            _ => ObservationDecayState.Expired
        };
    }
    
    /// <summary>
    /// Get hours elapsed since creation
    /// </summary>
    public double GetHoursElapsed(DateTime currentGameTime)
    {
        return (currentGameTime - CreatedAt).TotalHours;
    }
    
    /// <summary>
    /// Get decay state description for UI
    /// </summary>
    public string GetDecayStateDescription(DateTime currentGameTime)
    {
        var hoursElapsed = GetHoursElapsed(currentGameTime);
        var hoursRemaining = DecayState switch
        {
            ObservationDecayState.Active => 6.0 - hoursElapsed,
            ObservationDecayState.Expired => 0.0,
            _ => 0.0
        };
        
        return DecayState switch
        {
            ObservationDecayState.Active => $"Active (expires in {Math.Max(0, hoursRemaining):F1}h)",
            ObservationDecayState.Expired => "Expired",
            _ => "Unknown"
        };
    }
    
    /// <summary>
    /// Get CSS class for decay state styling
    /// </summary>
    public string GetDecayStateClass()
    {
        return DecayState switch
        {
            ObservationDecayState.Active => "observation-fresh",
            ObservationDecayState.Expired => "observation-expired",
            _ => "observation-unknown"
        };
    }
    
    // GetEffectiveWeight is inherited from ConversationCard and uses Category-based logic

    /// <summary>
    /// Calculate success chance based on weight and tokens
    /// </summary>
    public int CalculateSuccessChance(Dictionary<ConnectionType, int> tokens = null)
    {
        // Use override if specified (for special cards)
        if (SuccessRate.HasValue)
            return SuccessRate.Value;
            
        var baseChance = 70;
        baseChance -= Weight * 10;
        
        // Decay affects success chance
        if (DecayState == ObservationDecayState.Expired)
            return 5; // Minimal success chance for expired observations
        
        // Apply linear token bonus: +5% per token for cards
        if (tokens != null)
        {
            var tokenCount = tokens.GetValueOrDefault(GetConnectionType(), 0);
            var bonusPerToken = 5;
            var tokenBonus = tokenCount * bonusPerToken;
            baseChance += tokenBonus;
        }
        
        return Math.Clamp(baseChance, 5, 95);
    }

    /// <summary>
    /// Get the connection type this card builds
    /// </summary>
    public ConnectionType GetConnectionType()
    {
        return Type switch
        {
            CardType.Trust => ConnectionType.Trust,
            CardType.Commerce => ConnectionType.Commerce,
            CardType.Status => ConnectionType.Status,
            CardType.Shadow => ConnectionType.Shadow,
            _ => ConnectionType.Trust
        };
    }

    /// <summary>
    /// Get CSS class for card category
    /// </summary>
    public string GetCategoryClass()
    {
        return "observation";
    }

    /// <summary>
    /// Create an observation card from card properties
    /// </summary>
    public static ObservationCard FromCardData(
        string cardId,
        CardTemplateType template,
        CardContext context,
        CardType type,
        int weight,
        int baseComfort,
        string displayName,
        string description,
        string sourceObservationId,
        DateTime createdAt)
    {
        return new ObservationCard
        {
            Id = $"obscard_{sourceObservationId}_{DateTime.Now.Ticks}",
            Template = template,
            Context = context,
            Type = type,
            Weight = weight,
            BaseComfort = baseComfort,
            DisplayName = displayName,
            Description = description,
            CreatedAt = createdAt,
            SourceObservationId = sourceObservationId,
            BaseComfortValue = baseComfort,
            DecayState = ObservationDecayState.Active // Always starts fresh
        };
    }
    
    /// <summary>
    /// Static method for compatibility with old code
    /// </summary>
    public static ObservationCard FromConversationCard(ConversationCard card)
    {
        return new ObservationCard
        {
            Id = card.Id,
            Template = card.Template,
            Context = card.Context,
            Type = card.Type,
            Weight = card.Weight,
            BaseComfort = card.BaseComfort,
            DisplayName = card.DisplayName,
            Description = card.Description,
            CreatedAt = DateTime.Now,
            SourceObservationId = card.Id,
            BaseComfortValue = card.BaseComfort,
            DecayState = ObservationDecayState.Active
        };
    }
    
    /// <summary>
    /// Property for compatibility with old code  
    /// </summary>
    public ConversationCard ConversationCard => new ConversationCard
    {
        Id = Id,
        Template = Template,
        Context = Context,
        Type = Type,
        Persistence = Persistence,
        Weight = Weight,
        BaseComfort = BaseComfort,
        Depth = Depth,
        DisplayName = DisplayName,
        Description = Description,
        IsObservation = true,
        ObservationSource = SourceObservationId
    };
}