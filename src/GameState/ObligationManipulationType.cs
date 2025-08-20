/// <summary>
/// Types of obligation manipulation available through conversation
/// </summary>
public enum ObligationManipulationType
{
    /// <summary>
    /// Move an obligation to position 1 (prioritize)
    /// </summary>
    Prioritize,
    
    /// <summary>
    /// Burn tokens to clear queue slots above a specific obligation
    /// </summary>
    BurnToClear,
    
    /// <summary>
    /// Purge an obligation using tokens
    /// </summary>
    Purge,
    
    /// <summary>
    /// Negotiate deadline extension with sender
    /// </summary>
    ExtendDeadline,
    
    /// <summary>
    /// Transfer obligation to another NPC
    /// </summary>
    Transfer,
    
    /// <summary>
    /// Cancel obligation (requires high relationship)
    /// </summary>
    Cancel
}