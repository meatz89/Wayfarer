/// <summary>
/// Population density context of a location. Orthogonal categorical dimension.
/// Determines NPC availability, pricing modifiers, and social dynamics.
/// </summary>
public enum LocationSetting
{
    /// <summary>Dense city environment - high population, diverse NPCs, higher prices</summary>
    Urban,

    /// <summary>Town outskirts or small settlement - moderate population</summary>
    Suburban,

    /// <summary>Countryside or farmland - sparse population, lower prices</summary>
    Rural,

    /// <summary>Uninhabited natural area - no permanent population, survival focus</summary>
    Wilderness
}
