public class LocationContent
{
    // === INDUSTRIAL CHOICES PLAYGROUND ===
    public static Location Industrial => new LocationBuilder()
        .SetLocationType(LocationTypes.Industrial)
        .ForLocation(LocationNames.GenericDocks)
        .AddTravelConnection(LocationNames.LionsHeadTavern)
        // PLAYGROUND LABOR ACTION
        .AddLocationSpot(feature => feature
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Labor)
                .WithSpace(space => space
                    .WithScale(ScaleVariations.Medium)
                    .WithExposure(ExposureConditions.Outdoor))
                .WithSocial(social => social
                    .WithTension(TensionState.Relaxed)
                    .WithLegality(LegalityTypes.Legal))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple))
            )
        )
        // PLAYGROUND GATHER ACTION
        .AddLocationSpot(feature => feature
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Gather)
                .WithSpace(space => space
                    .WithScale(ScaleVariations.Medium)
                    .WithExposure(ExposureConditions.Outdoor))
                .WithSocial(social => social
                    .WithTension(TensionState.Alert))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple)
            )
        ))
        // PLAYGROUND TRADE ACTION
        .AddLocationSpot(feature => feature
            .WithCharacter(CharacterNames.Shopkeeper)
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Trade)
                .WithSpace(space => space
                    .WithScale(ScaleVariations.Medium)
                    .WithExposure(ExposureConditions.Outdoor))
                .WithSocial(social => social
                    .WithTension(TensionState.Alert)
                    .WithLegality(LegalityTypes.Legal))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple)
            )
        ))
        // PLAYGROUND MINGLE ACTION
        .AddLocationSpot(feature => feature
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Mingle)
                .WithSpace(space => space
                    .WithScale(ScaleVariations.Intimate)
                    .WithExposure(ExposureConditions.Indoor))
                .WithSocial(social => social
                    .WithTension(TensionState.Relaxed)
                    .WithLegality(LegalityTypes.Legal)
                )
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple)
            )
        ))
        .Build();

    // === COMMERCIAL CHOICES PLAYGROUND ===
    public static Location Commercial => new LocationBuilder()
        .SetLocationType(LocationTypes.Commercial)
        .ForLocation(LocationNames.GenericMarket)
        .AddTravelConnection(LocationNames.LionsHeadTavern)
        // PLAYGROUND LABOR ACTION
        .AddLocationSpot(feature => feature
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Labor)
                .WithSpace(space => space
                    .WithScale(ScaleVariations.Intimate)
                    .WithExposure(ExposureConditions.Outdoor))
                .WithSocial(social => social
                    .WithTension(TensionState.Relaxed)
                    .WithLegality(LegalityTypes.Legal)
                )
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple))
        ))
        // PLAYGROUND GATHER ACTION
        .AddLocationSpot(feature => feature
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Gather)
                .WithSpace(space => space
                    .WithScale(ScaleVariations.Intimate)
                    .WithExposure(ExposureConditions.Outdoor))
                .WithSocial(social => social
                    .WithTension(TensionState.Relaxed))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple))
        ))
        // PLAYGROUND TRADE ACTION
        .AddLocationSpot(feature => feature
            .WithCharacter(CharacterNames.Shopkeeper)
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Trade)
                .WithSpace(space => space
                    .WithScale(ScaleVariations.Intimate)
                    .WithExposure(ExposureConditions.Outdoor))
                .WithSocial(social => social
                    .WithTension(TensionState.Relaxed))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple))
        ))
        // PLAYGROUND MINGLE ACTION
        .AddLocationSpot(feature => feature
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Mingle)
                .WithSpace(space => space
                    .WithScale(ScaleVariations.Intimate)
                    .WithExposure(ExposureConditions.Outdoor))
                .WithSocial(social => social
                    .WithTension(TensionState.Relaxed))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple))
        ))
        .Build();

    // === SOCIAL CHOICES PLAYGROUND ===
    public static Location Social => new LocationBuilder()
        .SetLocationType(LocationTypes.Social)
        .ForLocation(LocationNames.GenericTavern)
        .AddTravelConnection(LocationNames.LionsHeadTavern)
        // PLAYGROUND LABOR ACTION
        .AddLocationSpot(feature => feature
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Labor)
                .WithSpace(space => space
                    .WithExposure(ExposureConditions.Indoor)
                    .WithScale(ScaleVariations.Intimate)
                    .WithExposure(ExposureConditions.Outdoor))
                .WithSocial(social => social
                    .WithTension(TensionState.Relaxed))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple))
        ))
        // PLAYGROUND GATHER ACTION
        .AddLocationSpot(feature => feature
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Gather)
                .WithSpace(space => space
                    .WithScale(ScaleVariations.Intimate)
                    .WithExposure(ExposureConditions.Outdoor))
                .WithSocial(social => social
                    .WithTension(TensionState.Relaxed))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple))
        ))
        // PLAYGROUND TRADE ACTION
        .AddLocationSpot(feature => feature
            .WithCharacter(CharacterNames.Bartender)
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Trade)
                .WithSpace(space => space
                    .WithScale(ScaleVariations.Intimate)
                    .WithExposure(ExposureConditions.Outdoor))
                .WithSocial(social => social
                    .WithTension(TensionState.Relaxed))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple))
        ))
        // PLAYGROUND MINGLE ACTION
        .AddLocationSpot(feature => feature
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Mingle)
                .WithSpace(space => space
                    .WithScale(ScaleVariations.Intimate)
                    .WithExposure(ExposureConditions.Outdoor))
                .WithSocial(social => social
                    .WithTension(TensionState.Relaxed))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple))
        ))
        .Build();

    // === NATURE CHOICES PLAYGROUND ===
    public static Location Nature => new LocationBuilder()
        .SetLocationType(LocationTypes.Nature)
        .ForLocation(LocationNames.GenericForest)
        .AddTravelConnection(LocationNames.LionsHeadTavern)
        // PLAYGROUND LABOR ACTION
        .AddLocationSpot(feature => feature
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Labor)
                .WithSpace(space => space
                    .WithScale(ScaleVariations.Intimate)
                    .WithExposure(ExposureConditions.Outdoor))
                .WithSocial(social => social
                    .WithTension(TensionState.Relaxed))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple))
        ))
        // PLAYGROUND GATHER ACTION
        .AddLocationSpot(feature => feature
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Gather)
                .WithSpace(space => space
                    .WithScale(ScaleVariations.Intimate)
                    .WithExposure(ExposureConditions.Outdoor))
                .WithSocial(social => social
                    .WithTension(TensionState.Relaxed))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple))
        ))
        // PLAYGROUND TRADE ACTION
        .AddLocationSpot(feature => feature
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Trade)
                .WithSpace(space => space
                    .WithScale(ScaleVariations.Intimate)
                    .WithExposure(ExposureConditions.Outdoor))
                .WithSocial(social => social
                    .WithTension(TensionState.Relaxed))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple)))
        )
        // PLAYGROUND MINGLE ACTION
        .AddLocationSpot(feature => feature
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Mingle)
                .WithSpace(space => space
                    .WithScale(ScaleVariations.Intimate)
                    .WithExposure(ExposureConditions.Outdoor))
                .WithSocial(social => social
                    .WithTension(TensionState.Relaxed))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple))
        ))
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
            .WithContext(context => context
                .WithBaseAction(BasicActionTypes.Mingle)
                .WithSpace(space => space
                    .WithScale(ScaleVariations.Intimate))
                .WithSocial(social => social
                    .WithTension(TensionState.Relaxed))
                .WithActivity(activity => activity
                    .WithComplexity(ComplexityTypes.Simple)))
        )
        .Build();
}
