using System;

/// <summary>
/// Decay states for observation cards
/// Models how information becomes outdated over time
/// </summary>
public enum ObservationDecayState
{
    Active,
    Expired   // 6+ hours: Must discard, unplayable
}

/// <summary>
/// Observation card that tracks when it was created and its decay state
/// Observations naturally expire through three stages, modeling how information becomes outdated
/// </summary>
public class ObservationCard
{
    /// <summary>
    /// Unique identifier for this observation card
    /// </summary>
    public string Id { get; init; }
    
    /// <summary>
    /// The underlying conversation card
    /// </summary>
    public ConversationCard ConversationCard { get; init; }
    
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
    
    /// <summary>
    /// Create an observation card from a conversation card
    /// </summary>
    public static ObservationCard FromConversationCard(ConversationCard conversationCard, string sourceObservationId, DateTime createdAt)
    {
        return new ObservationCard
        {
            Id = $"obscard_{sourceObservationId}_{DateTime.Now.Ticks}",
            ConversationCard = conversationCard,
            CreatedAt = createdAt,
            SourceObservationId = sourceObservationId,
            BaseComfortValue = conversationCard.BaseComfort,
            DecayState = ObservationDecayState.Active // Always starts fresh
        };
    }
}