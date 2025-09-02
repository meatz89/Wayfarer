using System;

/// <summary>
/// Properties that determine card behavior and exhaustion mechanics.
/// Cards can have multiple properties in any combination.
/// </summary>
public enum CardProperty
{
    /// <summary>
    /// Default - card stays in hand until played
    /// </summary>
    Persistent,
    
    /// <summary>
    /// Card exhausts after SPEAK action if unplayed
    /// </summary>
    Fleeting,
    
    /// <summary>
    /// Card exhausts after LISTEN action if unplayed
    /// </summary>
    Opportunity,
    
    /// <summary>
    /// System-generated placeholder card
    /// </summary>
    Skeleton,
    
    /// <summary>
    /// Card blocks deck slots and cannot be removed easily
    /// </summary>
    Burden,
    
    /// <summary>
    /// Card comes from observation system
    /// </summary>
    Observable
}