namespace Wayfarer.Game.ActionSystem;

/// <summary>
/// Physical effort required to perform an action
/// </summary>
public enum PhysicalDemand
{
    None,       // No physical effort (reading, talking)
    Light,      // Minimal effort (light crafting, writing)  
    Moderate,   // Some effort (walking, basic labor)
    Heavy,      // Significant effort (heavy lifting, climbing)
    Extreme     // Maximum effort (combat, emergency situations)
}

/// <summary>
/// Social standing or permissions required to perform an action
/// </summary>
public enum SocialRequirement
{
    Any,                // Anyone can perform this action
    Commoner,          // Basic citizenship required
    Merchant_Class,    // Commercial credentials needed
    Artisan_Class,     // Guild membership or craft training
    Minor_Noble,       // Noble status or special permission
    Major_Noble,       // High noble rank or royal access
    Guild_Member,      // Specific guild membership required
    Professional       // Professional credentials in relevant field
}

/// <summary>
/// Categories of tools or equipment needed for actions (non-overlapping with EquipmentCategory)
/// </summary>
public enum ToolCategory
{
    None,
    Basic_Tools,           // Simple hand tools
    Specialized_Equipment, // Professional equipment for specific trades
    Trade_Samples,         // Quality goods to demonstrate in negotiations
    Documentation,         // Contracts, permits, credentials
    Quality_Materials,     // Fine materials for crafting
    Writing_Materials,     // Ink, parchment, quills
    Measurement_Tools,     // Scales, rulers, measuring devices
    Safety_Equipment,      // Protection gear for dangerous activities
    Social_Attire,         // Appropriate clothing for social situations
    Crafting_Supplies      // Raw materials and tools for creation
}

/// <summary>
/// Environmental conditions required for actions
/// </summary>
public enum EnvironmentCategory
{
    Any,                  // Can be performed anywhere
    Indoor,              // Requires enclosed space
    Outdoor,             // Must be performed outside
    Workshop,            // Specialized crafting space needed
    Commercial_Setting,  // Market, shop, or trade location
    Private_Space,       // Quiet, private area required
    Good_Light,          // Adequate lighting needed
    Quiet,               // Low noise environment
    Weather_public,   // Shelter from rain/wind needed
    Specific_Location,   // Unique location requirement
    Hearth,              // Requires fireplace or heating
    Library,             // Books and study materials available
    Market_Square,       // Active trading environment
    Noble_Court,         // Formal court setting
    Sacred_Space         // Religious or ceremonial location
}

/// <summary>
/// Knowledge, skill level, or education required
/// </summary>
public enum KnowledgeRequirement
{
    None,            // No special knowledge needed
    Basic,           // Basic literacy and common knowledge
    Professional,    // Trade or professional training
    Advanced,        // Specialized expertise
    Expert,          // Master-level knowledge
    Master,          // Highest level of skill/knowledge
    Local,           // Specific local knowledge
    Commercial,      // Trade and business knowledge
    Academic,        // Scholarly education
    Technical,       // Engineering or technical skills
    Cultural,        // Social customs and etiquette
    Legal           // Law and legal procedures
}

/// <summary>
/// Time commitment required for the action
/// </summary>
public enum TimeInvestment
{
    Instant,            // Immediate action (seconds)
    Quick,              // Brief activity (few minutes)
    Standard,           // Normal activity (15-30 minutes)
    Extended,           // Longer activity (1-2 hours)
    Major_Undertaking,  // Significant time (half day or more)
    Multi_Day          // Requires multiple days
}

/// <summary>
/// Categories of effects that actions can produce
/// </summary>
public enum EffectCategory
{
    // Physical Effects
    Physical_Recovery,     // Restores stamina, removes fatigue
    Physical_Exertion,     // Causes tiredness or exhaustion
    Health_Improvement,    // Healing or health benefits
    Health_Risk,          // Potential for injury or illness

    // Mental/Knowledge Effects  
    Knowledge_Gain,        // Learn new information or skills
    Mental_Fatigue,        // Concentration strain
    Skill_Practice,        // Improve existing abilities
    Research_Progress,     // Advance ongoing studies

    // Social Effects
    Relationship_Building, // Improve relations with NPCs
    Reputation_Change,     // Alter standing with groups
    Social_Standing,       // Change social class perception
    Information_Exchange,  // Share or receive information

    // Economic Effects
    Economic_Opportunity,  // Create trade or profit chances
    Economic_Investment,   // Spend resources for future gain
    Resource_Acquisition,  // Gain materials or tools
    Market_Intelligence,   // Learn about prices and opportunities
    Contract_Discovery,    // Reveal available contracts from NPCs

    // Environmental Effects
    Location_Access,       // Unlock new areas or permissions
    Environmental_Change,  // Alter the current location
    Weather_Protection,    // Shelter from weather effects
    Time_Advancement,      // Progress game time

    // Item/Equipment Effects
    Item_Creation,         // Craft or make new items
    Equipment_Maintenance, // Repair or improve equipment
    Item_Discovery,        // Find hidden or rare items
    Equipment_Access       // Gain access to specialized tools
}

/// <summary>
/// Player's current physical condition affecting action capability
/// </summary>
public enum PhysicalCondition
{
    Excellent,    // Peak performance
    Good,         // Normal condition
    Tired,        // Somewhat fatigued
    Exhausted,    // Significantly fatigued
    Injured,      // Physical impairment
    Sick,         // Illness affecting performance
    Recovered     // Recently recovered from exertion
}

/// <summary>
/// Categories of information that can be traded, learned, or required
/// </summary>
public enum InformationType
{
    Market_Intelligence,    // Prices, demand, supply chain information
    Route_Conditions,      // Road safety, weather, travel requirements
    Social_Gossip,         // Relationships, reputations, social events
    Professional_Knowledge, // Trade secrets, craft techniques, business practices
    Location_Secrets,      // Hidden spots, special access, local customs
    Political_News,        // Government actions, policy changes, conflicts
    Personal_History,      // Individual backgrounds, motivations, connections
    Resource_Availability  // Where to find specific items, services, opportunities
}

/// <summary>
/// Quality and reliability of information
/// </summary>
public enum InformationQuality
{
    Rumor,         // Unverified, possibly false information
    Reliable,      // Generally trustworthy but may have gaps
    Verified,      // Confirmed through multiple sources
    Expert,        // From authoritative professional source
    Authoritative  // From definitive, unquestionable source
}

/// <summary>
/// How current and time-sensitive the information is
/// </summary>
public enum InformationFreshness
{
    Stale,      // Old information, may no longer be accurate
    Recent,     // Information from last few days, mostly current
    Current,    // Up-to-date information from today
    Breaking,   // Very recent developments, high value
    Real_Time   // Immediate, as-it-happens information
}

/// <summary>
/// Data class for specifying information requirements in action definitions
/// </summary>
public class InformationRequirementData
{
    public InformationType RequiredType { get; set; }
    public InformationQuality MinimumQuality { get; set; } = InformationQuality.Reliable;
    public InformationFreshness MinimumFreshness { get; set; } = InformationFreshness.Recent;
    public string SpecificInformationId { get; set; } = "";
    
    public InformationRequirementData() { }
    
    public InformationRequirementData(InformationType type, InformationQuality quality = InformationQuality.Reliable, 
                                     InformationFreshness freshness = InformationFreshness.Recent, string specificId = "")
    {
        RequiredType = type;
        MinimumQuality = quality;
        MinimumFreshness = freshness;
        SpecificInformationId = specificId;
    }
}

/// <summary>
/// Data class for specifying information effects in action definitions
/// </summary>
public class InformationEffectData
{
    public string InformationId { get; set; } = "";
    public string Title { get; set; } = "";
    public string Content { get; set; } = "";
    public InformationType Type { get; set; }
    public InformationQuality Quality { get; set; } = InformationQuality.Reliable;
    public InformationFreshness Freshness { get; set; } = InformationFreshness.Current;
    public string Source { get; set; } = "";
    public int Value { get; set; } = 10;
    public bool UpgradeExisting { get; set; } = false;
    public string LocationId { get; set; } = "";
    public string NPCId { get; set; } = "";
    public List<string> RelatedItemIds { get; set; } = new();
    public List<string> RelatedLocationIds { get; set; } = new();
    
    public InformationEffectData() { }
    
    public InformationEffectData(string id, string title, InformationType type, string content = "", 
                                InformationQuality quality = InformationQuality.Reliable, string source = "")
    {
        InformationId = id;
        Title = title;
        Type = type;
        Content = content;
        Quality = quality;
        Source = source;
    }
}

/// <summary>
/// Categories of contracts affecting NPC relationships and strategic progression
/// </summary>
public enum ContractCategory
{
    General,              // Basic contracts available to all players
    Merchant,            // Trade-focused contracts, affect merchant relationships
    Craftsman,           // Craft and delivery contracts, affect artisan relationships
    Noble,               // High-status contracts, affect noble relationships
    Exploration,         // Discovery and reconnaissance contracts
    Professional,        // Guild and professional organization contracts
    Emergency,           // Time-critical contracts with high stakes
    Diplomatic,          // Social and political contracts
    Research             // Information gathering and academic contracts
}

/// <summary>
/// Priority levels affecting payment multipliers and reputation impact
/// </summary>
public enum ContractPriority
{
    Low,                 // 0.8x payment, minimal reputation impact
    Standard,            // 1.0x payment, normal reputation impact
    High,                // 1.5x payment, increased reputation impact
    Urgent,              // 2.0x payment, major reputation consequences
    Critical             // 3.0x payment, severe reputation consequences for failure
}

/// <summary>
/// Risk levels determining potential negative consequences and equipment loss
/// </summary>
public enum ContractRisk
{
    None,                // No risk of equipment loss or injury
    Low,                 // Minimal risk, small chance of equipment damage
    Moderate,            // Some risk, possible equipment loss or injury
    High,                // Significant risk, high chance of negative consequences
    Extreme              // Major risk, severe consequences for failure
}

/// <summary>
/// Result of checking whether a player can perform an action
/// </summary>
public class ActionAccessResult
{
    public bool IsAllowed { get; set; }
    public List<string> BlockingReasons { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Requirements { get; set; } = new();

    public static ActionAccessResult Allowed()
    {
        return new ActionAccessResult { IsAllowed = true };
    }

    public static ActionAccessResult Blocked(string reason)
    {
        return new ActionAccessResult 
        { 
            IsAllowed = false, 
            BlockingReasons = new List<string> { reason }
        };
    }

    public static ActionAccessResult Blocked(List<string> reasons)
    {
        return new ActionAccessResult 
        { 
            IsAllowed = false, 
            BlockingReasons = reasons
        };
    }

    public void AddWarning(string warning)
    {
        Warnings.Add(warning);
    }

    public void AddRequirement(string requirement)
    {
        Requirements.Add(requirement);
    }
}

/// <summary>
/// Result of checking whether a player can accept or complete a contract
/// </summary>
public class ContractAccessResult
{
    public bool CanAccept { get; set; }
    public bool CanComplete { get; set; }
    public List<string> AcceptanceBlockers { get; set; } = new();
    public List<string> CompletionBlockers { get; set; } = new();
    public List<string> MissingRequirements { get; set; } = new();
    public List<string> Warnings { get; set; } = new();

    public static ContractAccessResult Allowed()
    {
        return new ContractAccessResult { CanAccept = true, CanComplete = true };
    }

    public static ContractAccessResult AcceptanceBlocked(string reason)
    {
        return new ContractAccessResult 
        { 
            CanAccept = false, 
            CanComplete = false,
            AcceptanceBlockers = new List<string> { reason }
        };
    }

    public static ContractAccessResult CompletionBlocked(string reason)
    {
        return new ContractAccessResult 
        { 
            CanAccept = true, 
            CanComplete = false,
            CompletionBlockers = new List<string> { reason }
        };
    }
}