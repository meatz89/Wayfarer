namespace Wayfarer.GameState.Enums;

/// <summary>
/// Categorical venue types that determine atmosphere and market pricing characteristics.
/// Used by LocationFacade for atmosphere modifiers and PriceManager for price calculations.
/// Replaces runtime string matching with strongly-typed enum property.
/// </summary>
public enum VenueType
{
    /// <summary>
    /// Market or town square - busy during day, empty at night
    /// Price modifiers favor common goods and materials
    /// </summary>
    Market,

    /// <summary>
    /// Tavern or inn - quiet during day, lively at night
    /// Price modifiers favor food and drink
    /// </summary>
    Tavern,

    /// <summary>
    /// Workshop, mill, or craft venue - specialized production
    /// Price modifiers favor materials and tools
    /// </summary>
    Workshop,

    /// <summary>
    /// Merchant quarter or trading district - commercial hub
    /// Price modifiers favor trade goods and valuables
    /// </summary>
    Merchant,

    /// <summary>
    /// Harbor or port - water transport and trade
    /// Price modifiers favor water transport and maritime goods
    /// </summary>
    Harbor,

    /// <summary>
    /// Noble district, manor, or estate - formal and private
    /// Atmosphere varies by time of day
    /// </summary>
    NobleDistrict,

    /// <summary>
    /// Wilderness, outdoor, or natural venues
    /// Default catch-all for non-urban venues
    /// </summary>
    Wilderness
}
