/// <summary>
/// Categorical properties of location spots that affect conversation mechanics
/// These properties influence flow modifiers, card availability, and NPC willingness
/// </summary>
public enum SpotPropertyType
{
    // Privacy properties
    Private,        // +2 flow, enables intimate cards
    Discrete,       // +1 flow, enables shadow cards
    Public,         // No modifier, standard cards only
    Exposed,        // -1 flow, restricts sensitive cards

    // Atmosphere properties
    Quiet,          // +1 flow for thoughtful personalities
    Loud,           // -1 flow, but +1 for boisterous personalities
    Warm,           // +1 flow in cold weather
    Shaded,         // +1 flow in hot weather

    // View properties
    ViewsMainEntrance,  // Enables authority observations
    ViewsBackAlley,     // Enables shadow observations
    ViewsMarket,        // Enables commerce observations
    ViewsTemple,        // Enables trust observations

    // Social properties
    Crossroads,     // More NPCs available
    Isolated,       // Fewer NPCs, but deeper conversations
    BusyEvening,    // Time-specific, more activity
    QuietMorning,   // Time-specific, peaceful

    // Special properties
    NobleFavored,   // +1 flow for proud personalities
    CommonerHaunt,  // +1 flow for steadfast personalities
    MerchantHub,    // +1 flow for mercantile personalities
    SacredGround,   // +1 flow for devoted personalities

    // Work properties
    Commercial      // Enables work actions for earning coins
}