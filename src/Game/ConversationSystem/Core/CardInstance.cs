using System;
using System.Collections.Generic;

/// <summary>
/// Represents a specific instance of a card in play.
/// Each instance has a unique ID and references an immutable card template.
/// </summary>
public class CardInstance
{
    private static int _nextInstanceId = 1;
    
    /// <summary>
    /// Unique identifier for this specific card instance
    /// </summary>
    public string InstanceId { get; }
    
    /// <summary>
    /// The immutable card template this instance represents
    /// </summary>
    public ConversationCard Template { get; }
    
    /// <summary>
    /// Session ID this instance belongs to (for debugging)
    /// </summary>
    public string SessionId { get; }
    
    /// <summary>
    /// When this instance was created (for debugging)
    /// </summary>
    public DateTime CreatedAt { get; }
    
    public CardInstance(ConversationCard template, string sessionId = null)
    {
        if (template == null)
            throw new ArgumentNullException(nameof(template));
            
        Template = template;
        SessionId = sessionId ?? "default";
        InstanceId = $"{template.Id}_{SessionId}_{_nextInstanceId++}";
        CreatedAt = DateTime.UtcNow;
    }
    
    // Convenience accessors to template properties
    public string TemplateId => Template.Id;
    public CardMechanics Mechanics => Template.Mechanics;
    public CardCategory Category => Template.Category;
    public CardContext Context => Template.Context;
    public CardType Type => Template.Type;
    public PersistenceType Persistence => Template.Persistence;
    public int Weight => Template.Weight;
    public int BaseComfort => Template.BaseComfort;
    public EmotionalState? SuccessState => Template.SuccessState;
    public EmotionalState? FailureState => Template.FailureState;
    public bool IsStateCard => Template.IsStateCard;
    public bool GrantsToken => Template.GrantsToken;
    public bool IsExchange => Template.IsExchange;
    public bool IsObservation => Template.IsObservation;
    public string ObservationSource => Template.ObservationSource;
    public bool CanDeliverLetter => Template.CanDeliverLetter;
    public string DeliveryObligationId => Template.DeliveryObligationId;
    public bool ManipulatesObligations => Template.ManipulatesObligations;
    public int? SuccessRate => Template.SuccessRate;
    public string DisplayName => Template.DisplayName;
    public string Description => Template.Description;
    public bool IsGoalCard => Template.IsGoalCard;
    public ConversationType? GoalCardType => Template.GoalCardType;
    public List<EmotionalState> DrawableStates => Template.DrawableStates;
    public int PatienceBonus => Template.PatienceBonus;
    
    /// <summary>
    /// Get effective weight considering state rules
    /// </summary>
    public int GetEffectiveWeight(EmotionalState state)
    {
        return Template.GetEffectiveWeight(state);
    }
    
    /// <summary>
    /// Calculate success chance based on weight and tokens
    /// </summary>
    public int CalculateSuccessChance(Dictionary<ConnectionType, int> tokens = null)
    {
        return Template.CalculateSuccessChance(tokens);
    }
    
    /// <summary>
    /// Get the connection type this card builds
    /// </summary>
    public ConnectionType GetConnectionType()
    {
        return Template.GetConnectionType();
    }
    
    /// <summary>
    /// Get CSS class for card category
    /// </summary>
    public string GetCategoryClass()
    {
        return Template.GetCategoryClass();
    }
    
    public override string ToString()
    {
        return $"CardInstance[{InstanceId}] of {TemplateId}";
    }
    
    public override bool Equals(object obj)
    {
        return obj is CardInstance other && InstanceId == other.InstanceId;
    }
    
    public override int GetHashCode()
    {
        return InstanceId.GetHashCode();
    }
}