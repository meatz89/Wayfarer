public class LocationContent
{
    // === RESIDENTIAL DISTRICT ===
    public static Location LionsHeadTavern => new LocationBuilder()
        .SetLocationType(LocationTypes.Social)
        .ForLocation(LocationNames.LionsHeadTavern)
        .SetLocationArchetype(LocationArchetype.Tavern)
        .SetSpace(space => space
            .WithScale(ScaleVariations.Medium)
            .WithExposure(ExposureConditions.Indoor)
            .WithCrowdLevel(CrowdLevel.Crowded))
        .SetSocial(social => social
            .WithLegality(LegalityTypes.Legal)
            .WithTension(TensionState.Relaxed))
        .SetActivity(activity => activity
            .WithComplexity(ComplexityTypes.Simple))
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
        .Build();
}