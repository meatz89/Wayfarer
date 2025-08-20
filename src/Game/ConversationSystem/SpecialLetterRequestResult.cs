/// <summary>
/// Categorical results for special letter requests (Epic 7)
/// Frontend maps these to appropriate UI messages and styling
/// </summary>
public enum SpecialLetterRequestResult
{
    /// <summary>
    /// Request succeeded - special letter generated and added to queue/satchel
    /// </summary>
    Success,
    
    /// <summary>
    /// Player doesn't have enough tokens for this special letter type
    /// </summary>
    InsufficientTokens,
    
    /// <summary>
    /// NPC considered the request but is not quite ready (neutral conversation outcome)
    /// </summary>
    Neutral,
    
    /// <summary>
    /// NPC declined the request (failed conversation outcome) 
    /// </summary>
    Declined,
    
    /// <summary>
    /// Technical processing failure - backend couldn't generate the letter
    /// </summary>
    ProcessingFailed
}