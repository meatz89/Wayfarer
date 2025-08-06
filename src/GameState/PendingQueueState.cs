using System.Collections.Generic;

/// <summary>
/// Strongly typed state for pending queue actions, replacing unsafe metadata dictionary
/// </summary>
public class PendingQueueState
{
    // Pending queue management actions
    public QueueActionType? PendingAction { get; set; }
    public int? PendingSkipPosition { get; set; }
    public int? PendingPurgePosition { get; set; }
    
    // Token selection for purge action - strongly typed
    public PurgeTokenSelection PendingPurgeTokens { get; set; } = new PurgeTokenSelection();
    
    // NPC-specific secrets (using HashSet is fine - it's not a dictionary)
    public HashSet<string> NPCsWithSecrets { get; set; } = new HashSet<string>();
    
    public void ClearPendingAction()
    {
        PendingAction = null;
        PendingSkipPosition = null;
        PendingPurgePosition = null;
        PendingPurgeTokens.Clear();
    }
    
    public bool HasPendingAction()
    {
        return PendingAction.HasValue;
    }
}

/// <summary>
/// Strongly typed token selection for purge actions
/// </summary>
public class PurgeTokenSelection
{
    public int TrustTokens { get; set; }
    public int CommerceTokens { get; set; }
    public int StatusTokens { get; set; }
    public int ShadowTokens { get; set; }
    
    public void Clear()
    {
        TrustTokens = 0;
        CommerceTokens = 0;
        StatusTokens = 0;
        ShadowTokens = 0;
    }
    
    public int GetTokenCount(ConnectionType tokenType)
    {
        return tokenType switch
        {
            ConnectionType.Trust => TrustTokens,
            ConnectionType.Commerce => CommerceTokens,
            ConnectionType.Status => StatusTokens,
            ConnectionType.Shadow => ShadowTokens,
            _ => 0
        };
    }
    
    public void SetTokenCount(ConnectionType tokenType, int count)
    {
        switch (tokenType)
        {
            case ConnectionType.Trust:
                TrustTokens = count;
                break;
            case ConnectionType.Commerce:
                CommerceTokens = count;
                break;
            case ConnectionType.Status:
                StatusTokens = count;
                break;
            case ConnectionType.Shadow:
                ShadowTokens = count;
                break;
        }
    }
    
    public int TotalTokens => TrustTokens + CommerceTokens + StatusTokens + ShadowTokens;
}

public enum QueueActionType
{
    Skip,
    Purge
}