public class LocationContent
{
    // === INDUSTRIAL CHOICES PLAYGROUND ===
    public static Location Industrial => new LocationBuilder()
        .SetLocationType(LocationTypes.Industrial)
        .ForLocation(LocationNames.GenericDocks)
        .AddTravelConnection(LocationNames.LionsHeadTavern)
        // PLAYGROUND LABOR ACTION
        .AddLocationSpot(feature => feature
            .ForLocationSpot(LocationSpotNames.DocksideWarehouse)
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Labor)
                .WithSpace(space => space
                    .WithAccess(AccessTypes.Open)
                    .WithScale(ScaleVariations.Medium)
                    .WithPopulation(PopulationDensity.Sparse)
                    .WithExposure(ExposureConditions.Outdoor))
                .WithSocial(social => social
                    .WithAuthority(AuthorityTypes.Official)
                    .WithFormality(FormalityTypes.Formal)
                    .WithTension(TensionState.Alert))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple)
                    .WithIntensity(IntensityTypes.Medium)
                    .WithDuration(DurationTypes.Brief)
                    .WithNoise(NoiseTypes.Quiet))
            )
        )
        // PLAYGROUND GATHER ACTION
        .AddLocationSpot(feature => feature
            .ForLocationSpot(LocationSpotNames.FishingWharf)
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Gather)
                .WithSpace(space => space
                    .WithAccess(AccessTypes.Open)
                    .WithScale(ScaleVariations.Medium)
                    .WithPopulation(PopulationDensity.Sparse)
                    .WithExposure(ExposureConditions.Outdoor))
                .WithSocial(social => social
                    .WithAuthority(AuthorityTypes.Official)
                    .WithFormality(FormalityTypes.Formal)
                    .WithTension(TensionState.Alert))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple)
                    .WithIntensity(IntensityTypes.Medium)
                    .WithDuration(DurationTypes.Brief)
                    .WithNoise(NoiseTypes.Quiet))
            )
        )
        // PLAYGROUND TRADE ACTION
        .AddLocationSpot(feature => feature
            .ForLocationSpot(LocationSpotNames.WharfMerchant)
            .WithCharacter(CharacterNames.Shopkeeper)
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Trade)
                .WithSpace(space => space
                    .WithAccess(AccessTypes.Open)
                    .WithScale(ScaleVariations.Medium)
                    .WithPopulation(PopulationDensity.Sparse)
                    .WithExposure(ExposureConditions.Outdoor))
                .WithSocial(social => social
                    .WithAuthority(AuthorityTypes.Official)
                    .WithFormality(FormalityTypes.Formal)
                    .WithTension(TensionState.Alert))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple)
                    .WithIntensity(IntensityTypes.Low)
                    .WithDuration(DurationTypes.Brief)
                    .WithNoise(NoiseTypes.Quiet))
            )
        )
        // PLAYGROUND MINGLE ACTION
        .AddLocationSpot(feature => feature
            .ForLocationSpot(LocationSpotNames.DocksidePub)
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Mingle)
                .WithSpace(space => space
                    .WithAccess(AccessTypes.Public)
                    .WithScale(ScaleVariations.Intimate)
                    .WithPopulation(PopulationDensity.Busy)
                    .WithExposure(ExposureConditions.Indoor))
                .WithSocial(social => social
                    .WithAuthority(AuthorityTypes.Official)
                    .WithFormality(FormalityTypes.Casual)
                    .WithTension(TensionState.Relaxed))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple)
                    .WithIntensity(IntensityTypes.Low)
                    .WithDuration(DurationTypes.Brief)
                    .WithNoise(NoiseTypes.Quiet))
            )
        )
        .Build();

    // === COMMERCIAL CHOICES PLAYGROUND ===
    public static Location Commercial => new LocationBuilder()
        .SetLocationType(LocationTypes.Commercial)
        .ForLocation(LocationNames.GenericMarket)
        .AddTravelConnection(LocationNames.LionsHeadTavern)
        // PLAYGROUND LABOR ACTION
        .AddLocationSpot(feature => feature
            .ForLocationSpot(LocationSpotNames.MarketPorters)
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Mingle)
                .WithSpace(space => space
                    .WithAccess(AccessTypes.Public)
                    .WithScale(ScaleVariations.Intimate))
                .WithSocial(social => social
                    .WithFormality(FormalityTypes.Casual)
                    .WithTension(TensionState.Relaxed))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple)
                    .WithIntensity(IntensityTypes.Low)))
        )
        // PLAYGROUND GATHER ACTION
        .AddLocationSpot(feature => feature
            .ForLocationSpot(LocationSpotNames.HerbGarden)
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Mingle)
                .WithSpace(space => space
                    .WithAccess(AccessTypes.Public)
                    .WithScale(ScaleVariations.Intimate))
                .WithSocial(social => social
                    .WithFormality(FormalityTypes.Casual)
                    .WithTension(TensionState.Relaxed))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple)
                    .WithIntensity(IntensityTypes.Low)))
        )
        // PLAYGROUND TRADE ACTION
        .AddLocationSpot(feature => feature
            .ForLocationSpot(LocationSpotNames.MarketBazaar)
            .WithCharacter(CharacterNames.Shopkeeper)
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Mingle)
                .WithSpace(space => space
                    .WithAccess(AccessTypes.Public)
                    .WithScale(ScaleVariations.Intimate))
                .WithSocial(social => social
                    .WithFormality(FormalityTypes.Casual)
                    .WithTension(TensionState.Relaxed))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple)
                    .WithIntensity(IntensityTypes.Low)))
        )
        // PLAYGROUND MINGLE ACTION
        .AddLocationSpot(feature => feature
            .ForLocationSpot(LocationSpotNames.MarketSquare)
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Mingle)
                .WithSpace(space => space
                    .WithAccess(AccessTypes.Public)
                    .WithScale(ScaleVariations.Intimate))
                .WithSocial(social => social
                    .WithFormality(FormalityTypes.Casual)
                    .WithTension(TensionState.Relaxed))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple)
                    .WithIntensity(IntensityTypes.Low)))
        )
        .Build();

    // === SOCIAL CHOICES PLAYGROUND ===
    public static Location Social => new LocationBuilder()
        .SetLocationType(LocationTypes.Social)
        .ForLocation(LocationNames.GenericTavern)
        .AddTravelConnection(LocationNames.LionsHeadTavern)
        // PLAYGROUND LABOR ACTION
        .AddLocationSpot(feature => feature
            .ForLocationSpot(LocationSpotNames.TavernKitchen)
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Mingle)
                .WithSpace(space => space
                    .WithAccess(AccessTypes.Public)
                    .WithScale(ScaleVariations.Intimate))
                .WithSocial(social => social
                    .WithFormality(FormalityTypes.Casual)
                    .WithTension(TensionState.Relaxed))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple)
                    .WithIntensity(IntensityTypes.Low)))
        )
        // PLAYGROUND GATHER ACTION
        .AddLocationSpot(feature => feature
            .ForLocationSpot(LocationSpotNames.CellarPantry)
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Mingle)
                .WithSpace(space => space
                    .WithAccess(AccessTypes.Public)
                    .WithScale(ScaleVariations.Intimate))
                .WithSocial(social => social
                    .WithFormality(FormalityTypes.Casual)
                    .WithTension(TensionState.Relaxed))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple)
                    .WithIntensity(IntensityTypes.Low)))
        )
        // PLAYGROUND TRADE ACTION
        .AddLocationSpot(feature => feature
            .ForLocationSpot(LocationSpotNames.TavernBarterTable)
            .WithCharacter(CharacterNames.Bartender)
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Mingle)
                .WithSpace(space => space
                    .WithAccess(AccessTypes.Public)
                    .WithScale(ScaleVariations.Intimate))
                .WithSocial(social => social
                    .WithFormality(FormalityTypes.Casual)
                    .WithTension(TensionState.Relaxed))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple)
                    .WithIntensity(IntensityTypes.Low)))
        )
        // PLAYGROUND MINGLE ACTION
        .AddLocationSpot(feature => feature
            .ForLocationSpot(LocationSpotNames.InnFireplace)
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Mingle)
                .WithSpace(space => space
                    .WithAccess(AccessTypes.Public)
                    .WithScale(ScaleVariations.Intimate))
                .WithSocial(social => social
                    .WithFormality(FormalityTypes.Casual)
                    .WithTension(TensionState.Relaxed))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple)
                    .WithIntensity(IntensityTypes.Low)))
        )
        .Build();

    // === NATURE CHOICES PLAYGROUND ===
    public static Location Nature => new LocationBuilder()
        .SetLocationType(LocationTypes.Nature)
        .ForLocation(LocationNames.GenericForest)
        .AddTravelConnection(LocationNames.LionsHeadTavern)
        // PLAYGROUND LABOR ACTION
        .AddLocationSpot(feature => feature
            .ForLocationSpot(LocationSpotNames.LumberYard)
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Mingle)
                .WithSpace(space => space
                    .WithAccess(AccessTypes.Public)
                    .WithScale(ScaleVariations.Intimate))
                .WithSocial(social => social
                    .WithFormality(FormalityTypes.Casual)
                    .WithTension(TensionState.Relaxed))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple)
                    .WithIntensity(IntensityTypes.Low)))
        )
        // PLAYGROUND GATHER ACTION
        .AddLocationSpot(feature => feature
            .ForLocationSpot(LocationSpotNames.MysticGrove)
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Mingle)
                .WithSpace(space => space
                    .WithAccess(AccessTypes.Public)
                    .WithScale(ScaleVariations.Intimate))
                .WithSocial(social => social
                    .WithFormality(FormalityTypes.Casual)
                    .WithTension(TensionState.Relaxed))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple)
                    .WithIntensity(IntensityTypes.Low)))
        )
        // PLAYGROUND TRADE ACTION
        .AddLocationSpot(feature => feature
            .ForLocationSpot(LocationSpotNames.WoodworkerCabin)
            .WithCharacter(CharacterNames.ForestTrader)
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Mingle)
                .WithSpace(space => space
                    .WithAccess(AccessTypes.Public)
                    .WithScale(ScaleVariations.Intimate))
                .WithSocial(social => social
                    .WithFormality(FormalityTypes.Casual)
                    .WithTension(TensionState.Relaxed))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple)
                    .WithIntensity(IntensityTypes.Low)))
        )
        // PLAYGROUND MINGLE ACTION
        .AddLocationSpot(feature => feature
            .ForLocationSpot(LocationSpotNames.GroveShrine)
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Mingle)
                .WithSpace(space => space
                    .WithAccess(AccessTypes.Public)
                    .WithScale(ScaleVariations.Intimate))
                .WithSocial(social => social
                    .WithFormality(FormalityTypes.Casual)
                    .WithTension(TensionState.Relaxed))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple)
                    .WithIntensity(IntensityTypes.Low)))
        )
        .Build();

    // === RESIDENTIAL DISTRICT ===
    public static Location LionsHeadTavern => new LocationBuilder()
        .SetLocationType(LocationTypes.Social)
        .ForLocation(LocationNames.LionsHeadTavern)
        .AddTravelConnection(LocationNames.GenericDocks)
        .AddTravelConnection(LocationNames.GenericTavern)
        .AddTravelConnection(LocationNames.GenericMarket)
        .AddTravelConnection(LocationNames.GenericForest)
        .AddLocationSpot(feature => feature
            .ForLocationSpot(LocationSpotNames.TavernKitchen)
            .WithEnergyCost(1, EnergyTypes.Social)
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Mingle)
                .WithSpace(space => space
                    .WithAccess(AccessTypes.Public)
                    .WithScale(ScaleVariations.Intimate))
                .WithSocial(social => social
                    .WithFormality(FormalityTypes.Casual)
                    .WithTension(TensionState.Relaxed))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple)
                    .WithIntensity(IntensityTypes.Low)))
        )
        .Build();
}
