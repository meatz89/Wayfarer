public class LocationContent
{
    // === RESIDENTIAL DISTRICT ===
    public static Location LionsHeadTavern => new LocationBuilder()
        .SetLocationType(LocationTypes.Residential)
        .ForLocation(LocationNames.LionsHeadTavern)
        .WithLocationProperties(properties => properties
            .WithArchetype(LocationArchetypes.Tavern)
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
            .ForActionType(BasicActionTypes.Reflect))
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

    public static Location BusyMarketplace => new LocationBuilder()
    .SetLocationType(LocationTypes.Commercial)
    .ForLocation(LocationNames.BusyMarketplace)
    .WithLocationProperties(properties => properties
        .WithArchetype(LocationArchetypes.Market)
        .WithScale(ScaleVariationTypes.Large)
        .WithExposure(ExposureConditionTypes.Outdoor)
        .WithCrowdLevel(CrowdLevelTypes.Crowded)
        .WithLegality(LegalityTypes.Legal)
        .WithPressure(PressureStateTypes.Alert)
        .WithComplexity(ComplexityTypes.Complex)
    )
    .AddLocationSpot(spot => spot
        .WithName("Merchant Stalls")
        .ForActionType(BasicActionTypes.Persuade))
    .AddLocationSpot(spot => spot
        .WithName("Food Court")
        .ForActionType(BasicActionTypes.Gather))
    .AddLocationSpot(spot => spot
        .WithName("Market Square")
        .ForActionType(BasicActionTypes.Perform))
    .AddLocationSpot(spot => spot
        .WithName("Auction Block")
        .ForActionType(BasicActionTypes.Investigate))
    .WithDifficultyLevel(2)
    .Build();

    public static Location QuietBookshop => new LocationBuilder()
        .SetLocationType(LocationTypes.Commercial)
        .ForLocation(LocationNames.QuietBookshop)
        .WithLocationProperties(properties => properties
            .WithArchetype(LocationArchetypes.Library)
            .WithScale(ScaleVariationTypes.Intimate)
            .WithExposure(ExposureConditionTypes.Indoor)
            .WithCrowdLevel(CrowdLevelTypes.Sparse)
            .WithLegality(LegalityTypes.Legal)
            .WithPressure(PressureStateTypes.Relaxed)
            .WithComplexity(ComplexityTypes.Complex)
        )
        .AddLocationSpot(spot => spot
            .WithName("Reading Nook")
            .ForActionType(BasicActionTypes.Study))
        .AddLocationSpot(spot => spot
            .WithName("Shop Counter")
            .ForActionType(BasicActionTypes.Labor))
        .AddLocationSpot(spot => spot
            .WithName("Rare Books Section")
            .ForActionType(BasicActionTypes.Investigate))
        .WithDifficultyLevel(1)
        .Build();


    public static Location BusyDockyard => new LocationBuilder()
        .SetLocationType(LocationTypes.Industrial)
        .ForLocation(LocationNames.BusyDockyard)
        .WithLocationProperties(properties => properties
            .WithArchetype(LocationArchetypes.Docks)
            .WithScale(ScaleVariationTypes.Large)
            .WithExposure(ExposureConditionTypes.Outdoor)
            .WithCrowdLevel(CrowdLevelTypes.Busy)
            .WithLegality(LegalityTypes.Gray)
            .WithPressure(PressureStateTypes.Alert)
            .WithComplexity(ComplexityTypes.Complex)
            .WithResource(ResourceTypes.Fish)
        )
        .AddLocationSpot(spot => spot
            .WithName("Loading Area")
            .ForActionType(BasicActionTypes.Labor))
        .AddLocationSpot(spot => spot
            .WithName("Fish Market")
            .ForActionType(BasicActionTypes.Gather))
        .AddLocationSpot(spot => spot
            .WithName("Harbormaster's Office")
            .ForActionType(BasicActionTypes.Investigate))
        .AddLocationSpot(spot => spot
            .WithName("Workers' Rest Area")
            .ForActionType(BasicActionTypes.Mingle))
        .WithDifficultyLevel(3)
        .Build();

    public static Location CraftsmanWorkshop => new LocationBuilder()
        .SetLocationType(LocationTypes.Industrial)
        .ForLocation(LocationNames.CraftsmanWorkshop)
        .WithLocationProperties(properties => properties
            .WithArchetype(LocationArchetypes.CraftsmanWorkshop)
            .WithScale(ScaleVariationTypes.Intimate)
            .WithExposure(ExposureConditionTypes.Indoor)
            .WithCrowdLevel(CrowdLevelTypes.Sparse)
            .WithLegality(LegalityTypes.Legal)
            .WithPressure(PressureStateTypes.Relaxed)
            .WithComplexity(ComplexityTypes.Complex)
            .WithResource(ResourceTypes.Cloth)
        )
        .AddLocationSpot(spot => spot
            .WithName("Workbench")
            .ForActionType(BasicActionTypes.Labor))
        .AddLocationSpot(spot => spot
            .WithName("Storage Area")
            .ForActionType(BasicActionTypes.Gather))
        .AddLocationSpot(spot => spot
            .WithName("Apprentice Corner")
            .ForActionType(BasicActionTypes.Study))
        .WithDifficultyLevel(2)
        .Build();


    public static Location WildForest => new LocationBuilder()
    .SetLocationType(LocationTypes.Natural)
    .ForLocation(LocationNames.WildForest)
    .WithLocationProperties(properties => properties
        .WithArchetype(LocationArchetypes.Forest)
        .WithScale(ScaleVariationTypes.Large)
        .WithExposure(ExposureConditionTypes.Outdoor)
        .WithCrowdLevel(CrowdLevelTypes.Empty)
        .WithLegality(LegalityTypes.Gray)
        .WithPressure(PressureStateTypes.Alert)
        .WithComplexity(ComplexityTypes.Complex)
        .WithResource(ResourceTypes.Herbs)
    )
    .AddLocationSpot(spot => spot
        .WithName("Herb Garden")
        .ForActionType(BasicActionTypes.Gather))
    .AddLocationSpot(spot => spot
        .WithName("Meditation Grove")
        .ForActionType(BasicActionTypes.Reflect))
    .AddLocationSpot(spot => spot
        .WithName("Hidden Trail")
        .ForActionType(BasicActionTypes.Travel))
    .AddLocationSpot(spot => spot
        .WithName("Ancient Ruins")
        .ForActionType(BasicActionTypes.Investigate))
    .WithDifficultyLevel(3)
    .Build();

    public static Location TendedGarden => new LocationBuilder()
        .SetLocationType(LocationTypes.Natural)
        .ForLocation(LocationNames.TendedGarden)
        .WithLocationProperties(properties => properties
            .WithArchetype(LocationArchetypes.Garden)
            .WithScale(ScaleVariationTypes.Medium)
            .WithExposure(ExposureConditionTypes.Outdoor)
            .WithCrowdLevel(CrowdLevelTypes.Sparse)
            .WithLegality(LegalityTypes.Legal)
            .WithPressure(PressureStateTypes.Relaxed)
            .WithComplexity(ComplexityTypes.Simple)
            .WithResource(ResourceTypes.Food)
        )
        .AddLocationSpot(spot => spot
            .WithName("Vegetable Beds")
            .ForActionType(BasicActionTypes.Labor))
        .AddLocationSpot(spot => spot
            .WithName("Herb Section")
            .ForActionType(BasicActionTypes.Gather))
        .AddLocationSpot(spot => spot
            .WithName("Garden Bench")
            .ForActionType(BasicActionTypes.Reflect))
        .WithDifficultyLevel(1)
        .Build();

}