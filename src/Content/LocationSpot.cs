using System.Collections.Generic;

public class LocationSpot
{
    public string SpotID { get; set; }
    public string Name { get; set; }
    // Description removed - generated from SpotPropertyType combinations
    public string LocationId { get; set; }

    // Tier system (1-5) for difficulty/content progression
    public int Tier { get; set; } = 1;

    // Categorical spot properties that affect conversations
    public List<SpotPropertyType> SpotProperties { get; set; } = new List<SpotPropertyType>();

    // Comfort modifier based on spot properties
    public int ComfortModifier { get; set; } = 0;

    // Time-specific properties that activate only during certain time blocks
    public Dictionary<TimeBlocks, List<SpotPropertyType>> TimeSpecificProperties { get; set; } = new Dictionary<TimeBlocks, List<SpotPropertyType>>();

    public List<TimeBlocks> CurrentTimeBlocks { get; set; } = new List<TimeBlocks>();
    public string InitialState { get; set; }
    public bool PlayerKnowledge { get; set; }

    public List<string> DomainTags { get; set; } = new List<string>();

    // Access Requirements for this spot
    public AccessRequirement AccessRequirement { get; set; }

    // UI compatibility - converts SpotPropertyType enum to string list
    public List<string> Properties 
    { 
        get 
        { 
            return SpotProperties.Select(p => p.ToString()).ToList(); 
        } 
    }

    public LocationSpot(string id, string name)
    {
        SpotID = id;
        Name = name;
    }

    /// <summary>
    /// Get active properties for the current time, combining base and time-specific
    /// </summary>
    public List<SpotPropertyType> GetActiveProperties(TimeBlocks currentTime)
    {
        var activeProperties = new List<SpotPropertyType>(SpotProperties);

        if (TimeSpecificProperties.ContainsKey(currentTime))
        {
            activeProperties.AddRange(TimeSpecificProperties[currentTime]);
        }

        return activeProperties;
    }

    /// <summary>
    /// Calculate the comfort modifier for conversations at this spot
    /// Based on spot properties and the NPC's personality
    /// </summary>
    public int CalculateComfortModifier(PersonalityType npcPersonality, TimeBlocks currentTime)
    {
        int modifier = ComfortModifier; // Base modifier from spot

        // Get all active properties (base + time-specific)
        var activeProperties = new List<SpotPropertyType>(SpotProperties);
        if (TimeSpecificProperties.ContainsKey(currentTime))
        {
            activeProperties.AddRange(TimeSpecificProperties[currentTime]);
        }

        // Apply property-based modifiers
        foreach (var property in activeProperties)
        {
            switch (property)
            {
                case SpotPropertyType.Private:
                    modifier += 2;
                    break;
                case SpotPropertyType.Discrete:
                    modifier += 1;
                    break;
                case SpotPropertyType.Exposed:
                    modifier -= 1;
                    break;
                case SpotPropertyType.Quiet:
                    if (npcPersonality == PersonalityType.DEVOTED || npcPersonality == PersonalityType.CUNNING)
                        modifier += 1;
                    break;
                case SpotPropertyType.Loud:
                    if (npcPersonality == PersonalityType.MERCANTILE)
                        modifier += 1;
                    else
                        modifier -= 1;
                    break;
                case SpotPropertyType.NobleFavored:
                    if (npcPersonality == PersonalityType.PROUD)
                        modifier += 1;
                    break;
                case SpotPropertyType.CommonerHaunt:
                    if (npcPersonality == PersonalityType.STEADFAST)
                        modifier += 1;
                    break;
                case SpotPropertyType.MerchantHub:
                    if (npcPersonality == PersonalityType.MERCANTILE)
                        modifier += 1;
                    break;
                case SpotPropertyType.SacredGround:
                    if (npcPersonality == PersonalityType.DEVOTED)
                        modifier += 1;
                    break;
            }
        }

        return modifier;
    }
}