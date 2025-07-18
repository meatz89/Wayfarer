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
/// Knowledge, skill level, or education required
/// </summary>
public enum KnowledgeRequirement
{
    None,            // No special knowledge needed
    Local,           // Specific local knowledge
    Commercial,      // Trade and business knowledge
    Academic,        // Scholarly education
    Technical,       // Engineering or technical skills
    Cultural,        // Social customs and etiquette
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
    Information_Exchange,  // Share or receive information

    // Economic Effects
    Economic_Opportunity,  // Create trade or profit chances
    Economic_Investment,   // Spend resources for future gain
    Resource_Acquisition,  // Gain materials or tools
    Market_Intelligence,   // Learn about prices and opportunities
    Contract_Discovery,    // Reveal available contracts from NPCs

    // Environmental Effects
    Location_Access,       // Unlock new areas or permissions
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
/// Categories of contracts affecting NPC relationships and strategic progression
/// </summary>
public enum ContractCategory
{
    General,              // Basic contracts available to all players
    Merchant,            // Trade-focused contracts, affect merchant relationships
    Craftsman,           // Craft and delivery contracts, affect artisan relationships
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
