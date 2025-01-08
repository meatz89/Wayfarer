public class LocationContent
{
    // === RESIDENTIAL DISTRICT ===
    public static Location LionsHeadTavern => new LocationBuilder()
        .SetLocationType(LocationTypes.Social)
        .ForLocation(LocationNames.LionsHeadTavern)
        .SetLocationArchetype(LocationArchetypes.Tavern)
        .WithLocationProperties(properties => properties
            .WithScale(ScaleVariationTypes.Medium)
            .WithExposure(ExposureConditionTypes.Indoor)
            .WithCrowdLevel(CrowdLevelTypes.Crowded)
            .WithLegality(LegalityTypes.Legal)
            .WithPressure(PressureStateTypes.Relaxed)
            .WithComplexity(ComplexityTypes.Simple)
        )
        .AddLocationSpot(spot => spot
            .WithName("Tavern Bar")
            .ForActionType(BasicActionTypes.Labor))
        .AddLocationSpot(spot => spot
            .WithName("Tavern Table")
            .ForActionType(BasicActionTypes.Trade))
        .AddLocationSpot(spot => spot
            .WithName("Cellar Pantry")
            .ForActionType(BasicActionTypes.Gather))
        .AddLocationSpot(spot => spot
            .WithName("Tavern Kitchen")
            .ForActionType(BasicActionTypes.Labor))
        .AddLocationSpot(spot => spot
            .WithName("Common Room")
            .ForActionType(BasicActionTypes.Mingle))
        .AddLocationSpot(spot => spot
            .WithName("Fireplace")
            .ForActionType(BasicActionTypes.Perform))
        .AddLocationSpot(spot => spot
            .WithName("Backroom")
            .ForActionType(BasicActionTypes.Investigate))
        .AddLocationSpot(spot => spot
            .WithName("Rooms")
            .ForActionType(BasicActionTypes.Rest))
        .WithDifficultyLevel(1)
        .Build();
}