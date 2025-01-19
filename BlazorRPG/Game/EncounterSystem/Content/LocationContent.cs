public class LocationContent
{
    // The Wanderer's Welcome represents our social challenge
    public static Location WaysideInn => new LocationBuilder()
        .SetLocationType(LocationTypes.Residential)
        .ForLocation(LocationNames.WaysideInn)
        .WithLocationProperties(properties => properties
            .WithArchetype(LocationArchetypes.Tavern)
            .WithResource(ResourceTypes.Ale)
            .WithActivityLevel(ActivityLevelTypes.Bustling)
            .WithAccessibility(AccessibilityTypes.Public)
            .WithSupervision(SupervisionTypes.Unsupervised)
            .WithAtmosphere(AtmosphereTypes.Relaxed)
            .WithSpace(SpaceTypes.Cramped)
            .WithLighting(LightingTypes.Dim)
            .WithExposure(ExposureTypes.Indoor))
        .AddLocationSpot(spot => spot
            .WithName("Bar Counter")
            .ForActionType(BasicActionTypes.Persuade))
        .AddLocationSpot(spot => spot
            .WithName("Common Room")
            .ForActionType(BasicActionTypes.Mingle))
        .WithDifficultyLevel(1)
        .Build();

}