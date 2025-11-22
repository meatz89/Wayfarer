/// <summary>
/// CATEGORICAL SEMANTIC BRIDGE: Maps location activity purposes to compatible venue atmosphere types.
///
/// MANY-TO-MANY by design:
/// - One LocationPurpose (Commerce) matches MULTIPLE VenueTypes (Market, Merchant, Workshop)
/// - This enables flexible procedural placement while maintaining semantic coherence
/// - LocationPurpose = abstract functional category (what activity happens)
/// - VenueType = concrete spatial archetype (what kind of place)
///
/// HIGHLANDER: Single source of truth for purpose→type mapping.
/// Alternative rejected: Unifying enums would lose semantic distinction between abstraction levels.
///
/// Used by: LocationPlacementService.FindMatchingVenues()
/// </summary>
public static class VenuePurposeCompatibility
{
    /// <summary>
    /// Compatibility lookup table: LocationPurpose → Compatible VenueTypes
    /// Deterministic, immutable, centralized mapping.
    /// </summary>
    private static readonly Dictionary<LocationPurpose, List<VenueType>> _compatibilityTable = new Dictionary<LocationPurpose, List<VenueType>>
    {
        // Transit: Movement and passage locations match wilderness (roads, outdoor paths)
        [LocationPurpose.Transit] = new List<VenueType>
        {
            VenueType.Wilderness
        },

        // Dwelling: Rest and lodging locations match taverns (inns, residential areas)
        [LocationPurpose.Dwelling] = new List<VenueType>
        {
            VenueType.Tavern
        },

        // Commerce: Trade and services match commercial venues (markets, merchants, workshops)
        [LocationPurpose.Commerce] = new List<VenueType>
        {
            VenueType.Market,
            VenueType.Merchant,
            VenueType.Workshop
        },

        // Civic: Law and authority match noble districts (official buildings, courts)
        [LocationPurpose.Civic] = new List<VenueType>
        {
            VenueType.NobleDistrict
        },

        // Defense: Military and security match fortresses and guard stations
        [LocationPurpose.Defense] = new List<VenueType>
        {
            VenueType.Fortress,
            VenueType.Guard
        },

        // Governance: Administrative and bureaucratic match administrative venues
        [LocationPurpose.Governance] = new List<VenueType>
        {
            VenueType.Administrative
        },

        // Worship: Religious services match temples
        [LocationPurpose.Worship] = new List<VenueType>
        {
            VenueType.Temple
        },

        // Learning: Education and research match academies
        [LocationPurpose.Learning] = new List<VenueType>
        {
            VenueType.Academy
        },

        // Entertainment: Performances and recreation match theaters and arenas
        [LocationPurpose.Entertainment] = new List<VenueType>
        {
            VenueType.Theater,
            VenueType.Arena
        },

        // Generic: Wildcard purpose matches ALL venue types (catch-all fallback)
        [LocationPurpose.Generic] = new List<VenueType>
        {
            VenueType.Market,
            VenueType.Tavern,
            VenueType.Workshop,
            VenueType.Merchant,
            VenueType.Harbor,
            VenueType.NobleDistrict,
            VenueType.Wilderness,
            VenueType.Fortress,
            VenueType.Guard,
            VenueType.Administrative,
            VenueType.Temple,
            VenueType.Academy,
            VenueType.Theater,
            VenueType.Arena
        }
    };

    /// <summary>
    /// Get all VenueTypes compatible with a given LocationPurpose.
    /// Used by LocationPlacementService Phase 2 (Venue Matching).
    /// Returns empty list if purpose not found (should never happen with complete enum coverage).
    /// </summary>
    public static List<VenueType> GetCompatibleTypes(LocationPurpose purpose)
    {
        if (_compatibilityTable.TryGetValue(purpose, out List<VenueType> compatibleTypes))
        {
            return compatibleTypes;
        }

        // Fallback: Return empty list (placement will fail with clear error message)
        return new List<VenueType>();
    }

    /// <summary>
    /// Check if a specific Venue Type is compatible with a LocationPurpose.
    /// Alternative query method for direct compatibility checks.
    /// </summary>
    public static bool IsCompatible(LocationPurpose purpose, VenueType venueType)
    {
        List<VenueType> compatibleTypes = GetCompatibleTypes(purpose);
        return compatibleTypes.Contains(venueType);
    }
}
