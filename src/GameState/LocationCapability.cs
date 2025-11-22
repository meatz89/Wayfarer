/// <summary>
/// Functional capabilities of locations - what they can DO (not what they ARE).
/// Uses Flags enum for efficient bitwise operations and combination checking.
///
/// ARCHITECTURE PRINCIPLE: Separated from categorical dimensions.
/// - LocationCapability = What location CAN DO (enables game mechanics)
/// - Privacy/Safety/Activity/Purpose = What location IS (categorical identity)
///
/// Examples:
/// - Crossroads → Enables Travel action (route selection UI)
/// - Commercial → Enables Work action (earn coins)
/// - SleepingSpace → Enables Rest action (restore health/stamina)
/// - Indoor/Outdoor → Environmental context (weather affects conversation flow)
/// </summary>
[Flags]
public enum LocationCapability
{
    None = 0,

    // ========== NAVIGATION/ROUTING CAPABILITIES ==========
    /// <summary>Route selection point - enables Travel action, connects multiple routes</summary>
    Crossroads = 1 << 0,

    /// <summary>Major transit hub - significant route connections</summary>
    TransitHub = 1 << 1,

    /// <summary>Gateway to other regions - district/area boundary crossing</summary>
    Gateway = 1 << 2,

    /// <summary>Transit location - general travel infrastructure</summary>
    Transit = 1 << 3,

    /// <summary>Transport services available - coaches, boats, etc.</summary>
    Transport = 1 << 4,

    // ========== ECONOMIC/WORK CAPABILITIES ==========
    /// <summary>Commercial activity available - enables Work action for earning coins</summary>
    Commercial = 1 << 5,

    /// <summary>Market venue type - affects pricing (1.1x modifier), enables trading</summary>
    Market = 1 << 6,

    /// <summary>Tavern venue type - affects pricing (0.9x general), social hub</summary>
    Tavern = 1 << 7,

    // ========== REST/SERVICE CAPABILITIES ==========
    /// <summary>Sleeping space available - enables Rest action (restore health/stamina)</summary>
    SleepingSpace = 1 << 8,

    /// <summary>Restful atmosphere - good rest quality, enhanced restoration</summary>
    Restful = 1 << 9,

    /// <summary>Lodging provider - offers accommodation services (business conversation topic)</summary>
    LodgingProvider = 1 << 10,

    /// <summary>Service location - various services available (not specific type)</summary>
    Service = 1 << 11,

    /// <summary>Rest area - designated for resting (may not have sleeping space)</summary>
    Rest = 1 << 12,

    // ========== ENVIRONMENTAL CAPABILITIES ==========
    /// <summary>Indoor enclosed space - shelter from weather, affects conversation flow</summary>
    Indoor = 1 << 13,

    /// <summary>Outdoor open-air space - exposed to weather, different conversation dynamics</summary>
    Outdoor = 1 << 14,

    // ========== SOCIAL/GATHERING CAPABILITIES ==========
    /// <summary>Social hub - enables social interactions, gathering place</summary>
    Social = 1 << 15,

    /// <summary>Gathering place - meeting point, community center</summary>
    Gathering = 1 << 16,

    // ========== SPECIAL/FUNCTIONAL CAPABILITIES ==========
    /// <summary>Temple/religious location - spiritual services, conversation topics</summary>
    Temple = 1 << 17,

    /// <summary>Noble district location - affects conversation tone, pricing</summary>
    Noble = 1 << 18,

    /// <summary>Water/waterfront access - river transport, water-related activities</summary>
    Water = 1 << 19,

    /// <summary>River access - specific waterway connection</summary>
    River = 1 << 20,

    // ========== AUTHORITY/SECURITY CAPABILITIES ==========
    /// <summary>Official/government location - authority presence</summary>
    Official = 1 << 21,

    /// <summary>Authority presence - guards, officials</summary>
    Authority = 1 << 22,

    /// <summary>Guarded location - active security</summary>
    Guarded = 1 << 23,

    /// <summary>Checkpoint - official checkpoint, inspections</summary>
    Checkpoint = 1 << 24,

    // ========== WEALTH/STATUS CAPABILITIES ==========
    /// <summary>Wealthy area - affects pricing, conversation tone</summary>
    Wealthy = 1 << 25,

    /// <summary>Prestigious location - high social standing</summary>
    Prestigious = 1 << 26,

    // ========== ENVIRONMENTAL CONTEXT ==========
    /// <summary>Urban environment - city setting</summary>
    Urban = 1 << 27,

    /// <summary>Rural environment - countryside setting</summary>
    Rural = 1 << 28,

    // ========== SPECIAL VIEWS/OBSERVATIONS ==========
    /// <summary>Views main entrance - enables authority observations</summary>
    ViewsMainEntrance = 1 << 29,

    /// <summary>Views back alley - enables shadow observations</summary>
    ViewsBackAlley = 1 << 30
}
