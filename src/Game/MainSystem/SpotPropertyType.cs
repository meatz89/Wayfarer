/// <summary>
/// Categorical properties of location spots that affect conversation mechanics
/// These properties influence comfort modifiers, card availability, and NPC willingness
/// </summary>
public enum SpotPropertyType
{
    // Privacy properties
    Private,        // +2 comfort, enables intimate cards
    Discrete,       // +1 comfort, enables shadow cards
    Public,         // No modifier, standard cards only
    Exposed,        // -1 comfort, restricts sensitive cards

    // Atmosphere properties
    Quiet,          // +1 comfort for thoughtful personalities
    Loud,           // -1 comfort, but +1 for boisterous personalities
    Warm,           // +1 comfort in cold weather
    Shaded,         // +1 comfort in hot weather

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
    NobleFavored,   // +1 comfort for proud personalities
    CommonerHaunt,  // +1 comfort for steadfast personalities
    MerchantHub,    // +1 comfort for mercantile personalities
    SacredGround,   // +1 comfort for devoted personalities

    // Work properties
    Commercial      // Enables work actions for earning coins
}