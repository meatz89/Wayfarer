using Wayfarer.Game.ActionSystem;

namespace Wayfarer.Game.MainSystem;

/// <summary>
/// Represents a piece of information that can be traded, learned, or required for actions
/// Information is treated as a categorical resource in the game economy
/// </summary>
public class Information
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }
    public string Source { get; set; } // Who provided this information

    // Categorical Properties
    public InformationType Type { get; set; }
    public InformationQuality Quality { get; set; }

    // Context and Relationships
    public string LocationId { get; set; } // Where this information is relevant
    public string NPCId { get; set; } // Which NPC this information concerns
    public List<string> RelatedItemIds { get; set; } = new();
    public List<string> RelatedLocationIds { get; set; } = new();

    // Game State
    public DateTime AcquiredDate { get; set; }
    public int Value { get; set; } // Economic value for trading
    public bool IsPublic { get; set; } // Whether this is common knowledge
    public int DaysToExpire { get; set; } // How long before this becomes stale

    public Information(string id, string title, InformationType type)
    {
        Id = id;
        Title = title;
        Type = type;
        Quality = InformationQuality.Reliable;
        AcquiredDate = DateTime.Now;
        DaysToExpire = GetDefaultExpirationDays(type);
    }

    // Helper methods for categorical matching
    public bool IsType(InformationType type)
    {
        return Type == type;
    }

    public bool IsQuality(InformationQuality quality)
    {
        return Quality == quality;
    }

    public bool IsAbout(string locationId)
    {
        return LocationId == locationId || RelatedLocationIds.Contains(locationId);
    }

    public bool ConcernsNPC(string npcId)
    {
        return NPCId == npcId;
    }

    public bool RelatesToItem(string itemId)
    {
        return RelatedItemIds.Contains(itemId);
    }

    /// <summary>
    /// Get the display description showing all categorical properties
    /// </summary>
    public string CategoricalDescription
    {
        get
        {
            List<string> descriptions = new()
            {
                $"Type: {Type.ToString().Replace('_', ' ')}",
                $"Quality: {Quality}",
            };

            if (!string.IsNullOrEmpty(Source))
                descriptions.Add($"Source: {Source}");

            return string.Join(" • ", descriptions);
        }
    }

    /// <summary>
    /// Calculate current information value based on categorical properties
    /// </summary>
    public int CalculateCurrentValue()
    {
        int baseValue = Value;

        // Quality multiplier
        float qualityMultiplier = Quality switch
        {
            InformationQuality.Rumor => 0.5f,
            InformationQuality.Reliable => 1.0f,
            InformationQuality.Verified => 1.5f,
            InformationQuality.Expert => 2.0f,
            InformationQuality.Authoritative => 3.0f,
            _ => 1.0f
        };
        return (int)(baseValue * qualityMultiplier);
    }

    /// <summary>
    /// Check if information meets categorical requirements
    /// </summary>
    public bool MeetsRequirements(InformationType? requiredType = null,
                                InformationQuality? minQuality = null)
    {
        if (requiredType.HasValue && Type != requiredType.Value)
            return false;

        if (minQuality.HasValue && Quality < minQuality.Value)
            return false;

        return true;
    }

    private int GetDefaultExpirationDays(InformationType type)
    {
        return type switch
        {
            InformationType.Market_Intelligence => 3,      // Market prices change frequently
            InformationType.Route_Conditions => 7,        // Travel conditions relatively stable
            InformationType.Social_Gossip => 14,          // Social news has medium lifespan
            InformationType.Professional_Knowledge => 30,  // Professional knowledge more stable
            InformationType.Location_Secrets => 90,       // Secrets don't change often
            InformationType.Political_News => 7,          // Political situations change
            InformationType.Personal_History => 365,      // Personal history very stable
            InformationType.Resource_Availability => 5,    // Resource locations change
            _ => 7
        };
    }
}