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
    Commercial,     // Enables work actions for earning coins

    // Service properties
    Social,         // Social hub, enables social interactions
    Service,        // Service location, various services available
    Rest,           // Rest area, enables rest actions
    Lodging,        // Lodging available, sleep/room actions

    // Location type properties
    Tavern,         // Tavern location type
    Market,         // Market location type
    Temple,         // Temple/religious location
    Noble,          // Noble district location
    
    // Access/Transit properties
    Transit,        // Transit hub, connections to other areas  
    TransitHub,     // Major transit point
    Transport,      // Transport services available
    Checkpoint,     // Official checkpoint, guards present
    Edge,           // Edge of settlement/boundary
    Gateway,        // Gateway to other regions
    
    // Authority properties
    Official,       // Official/government location
    Authority,      // Authority presence
    Guarded,        // Guarded location
    Secure,         // Secure/protected area
    Watched,        // Under surveillance
    
    // Social class properties  
    Wealthy,        // Wealthy area
    Exclusive,      // Exclusive/restricted access
    Prestigious,    // Prestigious location
    Political,      // Political significance
    
    // Environmental properties
    Water,          // Near water/waterfront
    River,          // River access
    ViewsRiver,     // Views of river
    Urban,          // Urban environment
    Rural,          // Rural environment
    
    // Activity level properties
    Busy,           // Generally busy location
    Crowded,        // Crowded space
    Central,        // Central/hub location
    Hidden,         // Hidden/secret location
    Intimate,       // Intimate/cozy atmosphere
    Gathering,      // Gathering place
    
    // Time-based properties
    Dark,           // Dark at certain times
    Dim,            // Dimly lit
    Cozy,           // Cozy atmosphere
    
    // Functional properties
    Restful,        // Good for resting
    Commerce        // Commerce hub (duplicate of Commercial, will consolidate)
}