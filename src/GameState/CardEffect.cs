using System.Collections.Generic;

/// <summary>
/// Represents a single effect that a card can trigger.
/// Cards can have up to three effects: Success, Failure, and Exhaust.
/// </summary>
public class CardEffect
{
    /// <summary>
    /// The type of effect to apply
    /// </summary>
    public CardEffectType Type { get; set; } = CardEffectType.None;
    
    /// <summary>
    /// The primary value for the effect.
    /// For fixed effects: numeric value (e.g., "3" for +3 flow)
    /// For scaling effects: formula (e.g., "Trust" for Trust tokens, "4 - flow")
    /// For atmosphere: atmosphere name (e.g., "Focused", "Volatile")
    /// </summary>
    public string Value { get; set; }
    
    /// <summary>
    /// Additional data needed for complex effects.
    /// Used for EndConversation effects to store obligation details,
    /// or any other effect-specific metadata.
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
    
    /// <summary>
    /// Strongly typed exchange data for Exchange effects.
    /// Populated by the parser when Type is Exchange.
    /// </summary>
    public ExchangeData ExchangeData { get; set; }
    
    /// <summary>
    /// Helper to check if this effect is empty/none
    /// </summary>
    public bool IsEmpty => Type == CardEffectType.None || string.IsNullOrEmpty(Value);
    
    /// <summary>
    /// Creates a deep clone of this effect
    /// </summary>
    public CardEffect DeepClone()
    {
        return new CardEffect
        {
            Type = this.Type,
            Value = this.Value,
            Data = this.Data != null ? new Dictionary<string, object>(this.Data) : new Dictionary<string, object>()
        };
    }
    
    /// <summary>
    /// Helper to create a no-effect instance
    /// </summary>
    public static CardEffect None => new CardEffect { Type = CardEffectType.None };
}