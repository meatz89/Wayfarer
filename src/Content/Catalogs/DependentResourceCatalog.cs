/// <summary>
/// PARSE-TIME ONLY CATALOGUE
///
/// Procedurally generates PlacementFilter and item specifications based on
/// service activity type. Enables reusable scene archetypes to work across
/// multiple service domains without hardcoding resource specs.
///
/// CATALOGUE PATTERN COMPLIANCE:
/// - Called ONLY from SceneArchetypeCatalog at PARSE TIME
/// - NEVER called from facades, managers, or runtime code
/// - NEVER imported except in catalog classes
/// - Generates specs that EntityResolver uses at spawn time (find or create)
/// - Translation happens ONCE at game initialization
///
/// HIGHLANDER: Uses PlacementFilter for ALL location specifications
/// EntityResolver.FindOrCreateLocation handles find-first-then-create pattern
/// </summary>
public static class DependentResourceCatalog
{
    /// <summary>
    /// Result container for dependent resource generation
    /// Contains PlacementFilter for location
    /// </summary>
    public class DependentResources
    {
        /// <summary>
        /// PlacementFilter for the location (used by EntityResolver.FindOrCreateLocation)
        /// HIGHLANDER: Single structure for all location specifications
        /// </summary>
        public PlacementFilter LocationFilter { get; set; }
    }

    /// <summary>
    /// Generate PlacementFilter for service activity type
    ///
    /// Returns LocationFilter: Categorical filter for private space (find existing or create)
    ///
    /// All specs use generic, descriptive names until AI narrative system implemented.
    /// </summary>
    public static DependentResources GenerateForActivity(ServiceActivityType activityType)
    {
        return activityType switch
        {
            ServiceActivityType.Lodging => GenerateLodgingResources(),
            ServiceActivityType.Bathing => GenerateBathingResources(),
            ServiceActivityType.Training => GenerateTrainingResources(),
            ServiceActivityType.Healing => GenerateHealingResources(),
            ServiceActivityType.Crafting => GenerateCraftingResources(),
            ServiceActivityType.Studying => GenerateStudyingResources(),
            _ => throw new InvalidDataException($"Unknown ServiceActivityType: {activityType}")
        };
    }

    private static DependentResources GenerateLodgingResources()
    {
        PlacementFilter locationFilter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            LocationRole = LocationRole.Rest,
            Privacy = LocationPrivacy.Private,
            Safety = LocationSafety.Safe,
            Activity = LocationActivity.Quiet,
            Purpose = LocationPurpose.Dwelling,
            SelectionStrategy = PlacementSelectionStrategy.First
        };

        return new DependentResources
        {
            LocationFilter = locationFilter
        };
    }

    private static DependentResources GenerateBathingResources()
    {
        PlacementFilter locationFilter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            Privacy = LocationPrivacy.Private,
            Safety = LocationSafety.Safe,
            Activity = LocationActivity.Quiet,
            Purpose = LocationPurpose.Dwelling,
            SelectionStrategy = PlacementSelectionStrategy.First
        };

        return new DependentResources
        {
            LocationFilter = locationFilter
        };
    }

    private static DependentResources GenerateTrainingResources()
    {
        PlacementFilter locationFilter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            Privacy = LocationPrivacy.Private,
            Safety = LocationSafety.Neutral,
            Activity = LocationActivity.Busy,
            Purpose = LocationPurpose.Commerce,
            SelectionStrategy = PlacementSelectionStrategy.First
        };

        return new DependentResources
        {
            LocationFilter = locationFilter
        };
    }

    private static DependentResources GenerateHealingResources()
    {
        PlacementFilter locationFilter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            Privacy = LocationPrivacy.Private,
            Safety = LocationSafety.Safe,
            Activity = LocationActivity.Quiet,
            Purpose = LocationPurpose.Dwelling,
            SelectionStrategy = PlacementSelectionStrategy.First
        };

        return new DependentResources
        {
            LocationFilter = locationFilter
        };
    }

    private static DependentResources GenerateCraftingResources()
    {
        PlacementFilter locationFilter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            Privacy = LocationPrivacy.Private,
            Safety = LocationSafety.Neutral,
            Activity = LocationActivity.Busy,
            Purpose = LocationPurpose.Commerce,
            SelectionStrategy = PlacementSelectionStrategy.First
        };

        return new DependentResources
        {
            LocationFilter = locationFilter
        };
    }

    private static DependentResources GenerateStudyingResources()
    {
        PlacementFilter locationFilter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            Privacy = LocationPrivacy.Private,
            Safety = LocationSafety.Safe,
            Activity = LocationActivity.Quiet,
            Purpose = LocationPurpose.Learning,
            SelectionStrategy = PlacementSelectionStrategy.First
        };

        return new DependentResources
        {
            LocationFilter = locationFilter
        };
    }
}
