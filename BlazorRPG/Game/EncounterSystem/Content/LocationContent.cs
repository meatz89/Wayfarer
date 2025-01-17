public class LocationContent
{
    public static Location ForestRoad => new LocationBuilder()
        .SetLocationType(LocationTypes.Natural)
        .ForLocation(LocationNames.ForestRoad)
        .WithLocationProperties(properties => properties
            .WithArchetype(LocationArchetypes.Forest)
            .WithAccessibility(AccessibilityTypes.Public)
            .WithExposure(ExposureTypes.Outdoor)
            .WithActivityLevel(ActivityLevelTypes.Deserted)
            .WithSupervision(SupervisionTypes.Unsupervised)
            .WithAtmosphere(AtmosphereTypes.Tense)
            .WithSpace(SpaceTypes.Open))
        .AddLocationSpot(spot => spot
            .WithName("Fallen Tree")
            .ForActionType(BasicActionTypes.Labor))
        .AddLocationSpot(spot => spot
            .WithName("Forest Edge")
            .ForActionType(BasicActionTypes.Reflect))
        .WithDifficultyLevel(1)
        .Build();

    // The crossroads represents our mental challenge - a focus encounter
    public static Location AncientCrossroads => new LocationBuilder()
        .SetLocationType(LocationTypes.Natural)
        .ForLocation(LocationNames.AncientCrossroads)
        .WithLocationProperties(properties => properties
            .WithArchetype(LocationArchetypes.Crossroads)
            .WithAccessibility(AccessibilityTypes.Public)
            .WithExposure(ExposureTypes.Outdoor)
            .WithActivityLevel(ActivityLevelTypes.Deserted)
            .WithSupervision(SupervisionTypes.Unsupervised)
            .WithAtmosphere(AtmosphereTypes.Mysterious)
            .WithSpace(SpaceTypes.Open))
        .AddLocationSpot(spot => spot
            .WithName("Weathered Signpost")
            .ForActionType(BasicActionTypes.Investigate))
        .AddLocationSpot(spot => spot
            .WithName("Stone Circle")
            .ForActionType(BasicActionTypes.Study))
        .WithDifficultyLevel(1)
        .Build();

    // The Wanderer's Welcome represents our social challenge
    public static Location WanderersWelcome => new LocationBuilder()
        .SetLocationType(LocationTypes.Residential)
        .ForLocation(LocationNames.WanderersWelcome)
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
            .WithName("Common Room")
            .ForActionType(BasicActionTypes.Mingle))
        .AddLocationSpot(spot => spot
            .WithName("Bar Counter")
            .ForActionType(BasicActionTypes.Persuade))
        .WithDifficultyLevel(1)
        .Build();

}