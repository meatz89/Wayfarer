using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Generates game endings based on relationship patterns after 30 days
/// </summary>
public class EndingGenerator
{
    private readonly GameWorld _gameWorld;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly NPCRepository _npcRepository;

    public EndingGenerator(
        GameWorld gameWorld,
        TokenMechanicsManager tokenManager,
        NPCRepository npcRepository)
    {
        _gameWorld = gameWorld;
        _tokenManager = tokenManager;
        _npcRepository = npcRepository;
    }

    /// <summary>
    /// Analyze relationship patterns and generate appropriate ending
    /// </summary>
    public GameEnding GenerateEnding()
    {
        RelationshipAnalysis analysis = AnalyzeRelationships();
        EndingType endingType = DetermineEndingType(analysis);
        
        return new GameEnding
        {
            Type = endingType,
            Title = GetEndingTitle(endingType),
            Description = GenerateEndingDescription(endingType, analysis),
            RelationshipSummary = analysis,
            Day = _gameWorld.CurrentDay
        };
    }

    /// <summary>
    /// Check if player has reached the 30-day ending
    /// </summary>
    public bool HasReachedEnding()
    {
        return _gameWorld.CurrentDay >= 30;
    }

    private RelationshipAnalysis AnalyzeRelationships()
    {
        List<NPC> allNpcs = _npcRepository.GetAllNPCs();
        RelationshipAnalysis analysis = new RelationshipAnalysis();

        foreach (NPC npc in allNpcs)
        {
            Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npc.ID);
            
            int trustLevel = tokens.GetValueOrDefault(ConnectionType.Trust, 0);
            int commerceLevel = tokens.GetValueOrDefault(ConnectionType.Commerce, 0);
            int statusLevel = tokens.GetValueOrDefault(ConnectionType.Status, 0);
            int shadowLevel = tokens.GetValueOrDefault(ConnectionType.Shadow, 0);

            analysis.TrustRelationships.Add(npc.ID, trustLevel);
            analysis.CommerceRelationships.Add(npc.ID, commerceLevel);
            analysis.StatusRelationships.Add(npc.ID, statusLevel);
            analysis.ShadowRelationships.Add(npc.ID, shadowLevel);

            // Track highest single relationship
            int maxRelationship = Math.Max(Math.Max(trustLevel, commerceLevel), Math.Max(statusLevel, shadowLevel));
            if (maxRelationship > analysis.DeepestRelationshipLevel)
            {
                analysis.DeepestRelationshipLevel = maxRelationship;
                analysis.DeepestRelationshipNPC = npc.Name;
                analysis.DeepestRelationshipType = GetDominantConnectionType(tokens);
            }
        }

        // Calculate totals and averages
        analysis.TotalTrustAcrossNPCs = analysis.TrustRelationships.Values.Sum();
        analysis.TotalCommerceAcrossNPCs = analysis.CommerceRelationships.Values.Sum();
        analysis.TotalStatusAcrossNPCs = analysis.StatusRelationships.Values.Sum();
        analysis.TotalShadowAcrossNPCs = analysis.ShadowRelationships.Values.Sum();

        analysis.BalancedRelationships = CountBalancedRelationships(allNpcs);
        analysis.SpecializedConnections = CountSpecializedConnections(allNpcs);

        return analysis;
    }

    private EndingType DetermineEndingType(RelationshipAnalysis analysis)
    {
        // Specialist ending - one very deep relationship (6+ in one type)
        if (analysis.DeepestRelationshipLevel >= 6)
        {
            return analysis.DeepestRelationshipType switch
            {
                ConnectionType.Trust => EndingType.TrustedConfidant,
                ConnectionType.Commerce => EndingType.MerchantPartner,
                ConnectionType.Status => EndingType.NoblePatron,
                ConnectionType.Shadow => EndingType.ShadowOperative,
                _ => EndingType.Specialist
            };
        }

        // Noble ending - high Status across NPCs
        if (analysis.TotalStatusAcrossNPCs >= 15)
            return EndingType.Noble;

        // Economic ending - high Commerce across NPCs  
        if (analysis.TotalCommerceAcrossNPCs >= 15)
            return EndingType.Economic;

        // Shadow ending - high Shadow across NPCs
        if (analysis.TotalShadowAcrossNPCs >= 12)
            return EndingType.Shadow;

        // Community ending - balanced relationships
        if (analysis.BalancedRelationships >= 3)
            return EndingType.Community;

        // Default - struggled courier
        return EndingType.StrugglingCourier;
    }

    private string GetEndingTitle(EndingType type)
    {
        return type switch
        {
            EndingType.Community => "Beloved Community Courier",
            EndingType.Economic => "Prosperous Merchant Courier",
            EndingType.Noble => "Court-Connected Courier",
            EndingType.Shadow => "Underground Network Courier",
            EndingType.TrustedConfidant => "The Trusted Confidant",
            EndingType.MerchantPartner => "The Business Partner",
            EndingType.NoblePatron => "The Noble's Right Hand",
            EndingType.ShadowOperative => "The Shadow Operative",
            EndingType.Specialist => "The Specialist",
            EndingType.StrugglingCourier => "The Struggling Courier",
            _ => "The Courier's Tale"
        };
    }

    private string GenerateEndingDescription(EndingType type, RelationshipAnalysis analysis)
    {
        return type switch
        {
            EndingType.Community => $"After thirty days, you've become the heart of this community. Everyone knows your name, trusts your discretion, and values your service. You've built {analysis.BalancedRelationships} strong, balanced relationships that will sustain you for years to come.",
            
            EndingType.Economic => $"Your business acumen has paid off handsomely. With {analysis.TotalCommerceAcrossNPCs} commerce connections across the city, you're now more than just a courier - you're a vital link in the economic network. Opportunities for expansion await.",
            
            EndingType.Noble => $"You've earned the attention and respect of the city's elite. Your {analysis.TotalStatusAcrossNPCs} status connections open doors that were once barred to common folk. The noble courts now see you as one of their own.",
            
            EndingType.Shadow => $"You've become part of the city's hidden networks. With {analysis.TotalShadowAcrossNPCs} shadow connections, you know secrets others can only guess at. Information flows through you like water through stone.",
            
            EndingType.TrustedConfidant => $"Your deepest bond is with {analysis.DeepestRelationshipNPC}. This trust runs deeper than mere business - you've become their most valued confidant, someone they turn to in their darkest hours.",
            
            EndingType.MerchantPartner => $"Your commercial relationship with {analysis.DeepestRelationshipNPC} has evolved into true partnership. Together, you're building something that will outlast both of you.",
            
            EndingType.NoblePatron => $"You've become indispensable to {analysis.DeepestRelationshipNPC}. Your service has earned not just respect, but genuine regard from one of the city's most influential figures.",
            
            EndingType.ShadowOperative => $"Your connection to {analysis.DeepestRelationshipNPC} has drawn you into the city's hidden world. You're no longer just a courier - you're an operative in games most people never even suspect.",
            
            EndingType.StrugglingCourier => "Thirty days have passed, and while you've survived, prosperity remains elusive. Your relationships remain shallow, your reputation modest. Perhaps with more time and different choices, your story might have been different.",
            
            _ => "Your thirty days as a courier have come to an end. The relationships you've built - or failed to build - will shape whatever comes next."
        };
    }

    private ConnectionType GetDominantConnectionType(Dictionary<ConnectionType, int> tokens)
    {
        return tokens.OrderByDescending(kvp => kvp.Value).First().Key;
    }

    private int CountBalancedRelationships(List<NPC> npcs)
    {
        int count = 0;
        foreach (NPC npc in npcs)
        {
            Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npc.ID);
            int total = tokens.Values.Sum();
            int max = tokens.Values.Max();
            
            // Balanced if total >= 8 and no single type dominates (max <= total/2)
            if (total >= 8 && max <= total / 2)
                count++;
        }
        return count;
    }

    private int CountSpecializedConnections(List<NPC> npcs)
    {
        int count = 0;
        foreach (NPC npc in npcs)
        {
            Dictionary<ConnectionType, int> tokens = _tokenManager.GetTokensWithNPC(npc.ID);
            if (tokens.Values.Max() >= 5) // Specialized if any single type >= 5
                count++;
        }
        return count;
    }
}

/// <summary>
/// Types of endings based on relationship patterns
/// </summary>
public enum EndingType
{
    Community,          // Balanced relationships across multiple NPCs
    Economic,           // High commerce across NPCs
    Noble,              // High status across NPCs
    Shadow,             // High shadow across NPCs
    TrustedConfidant,   // Deep trust with one NPC
    MerchantPartner,    // Deep commerce with one NPC
    NoblePatron,        // Deep status with one NPC
    ShadowOperative,    // Deep shadow with one NPC
    Specialist,         // Very deep relationship of any type
    StrugglingCourier   // Low relationships overall
}

/// <summary>
/// Complete analysis of player's relationships
/// </summary>
public class RelationshipAnalysis
{
    public Dictionary<string, int> TrustRelationships { get; set; } = new();
    public Dictionary<string, int> CommerceRelationships { get; set; } = new();
    public Dictionary<string, int> StatusRelationships { get; set; } = new();
    public Dictionary<string, int> ShadowRelationships { get; set; } = new();

    public int TotalTrustAcrossNPCs { get; set; }
    public int TotalCommerceAcrossNPCs { get; set; }
    public int TotalStatusAcrossNPCs { get; set; }
    public int TotalShadowAcrossNPCs { get; set; }

    public int DeepestRelationshipLevel { get; set; }
    public string DeepestRelationshipNPC { get; set; } = "";
    public ConnectionType DeepestRelationshipType { get; set; }

    public int BalancedRelationships { get; set; }
    public int SpecializedConnections { get; set; }
}

/// <summary>
/// Represents a complete game ending
/// </summary>
public class GameEnding
{
    public EndingType Type { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public RelationshipAnalysis RelationshipSummary { get; set; } = new();
    public int Day { get; set; }
    public bool EndlessMode { get; set; } = false;
}