public class ActionNameGenerator
{
    private readonly ActionGenerationContext context;
    private readonly ActionNameCombinationData combinationData;

    public ActionNameGenerator(ActionGenerationContext context)
    {
        this.context = context;
        this.combinationData = new ActionNameCombinationData();
    }

    public string GenerateName()
    {
        // Retrieve name parts using the combination data
        ActionVerb? verb = combinationData.GetVerb(context);
        LocationDescriptor? locationDescriptor = combinationData.GetLocationDescriptor(context);
        SpaceDescriptor? spaceDescriptor = combinationData.GetSpaceDescriptor(context);
        ActivityDescriptor? activityDescriptor = combinationData.GetActivityDescriptor(context);

        // Build the name from the retrieved parts
        List<string> parts = new List<string>();
        if (locationDescriptor.HasValue) parts.Add(locationDescriptor.Value.ToString());
        if (activityDescriptor.HasValue) parts.Add(activityDescriptor.Value.ToString());
        if (verb.HasValue) parts.Add(verb.Value.ToString());
        if (spaceDescriptor.HasValue) parts.Add(spaceDescriptor.Value.ToString());

        return string.Join(" ", parts);
    }
}

public class ActionNameCombinationData
{
    public List<ActionNamePart> Combinations { get; } = new List<ActionNamePart>()
    {
        // Define all valid combinations here

        // Gather
        new ActionNamePart(baseAction: BasicActionTypes.Gather, locationType: LocationTypes.Nature, exposure: ExposureConditions.Outdoor, complexity: ComplexityTypes.Complex, verbResult: ActionVerb.Hunt),
        new ActionNamePart(baseAction: BasicActionTypes.Gather, locationType: LocationTypes.Nature, exposure: ExposureConditions.Outdoor, verbResult: ActionVerb.Forage),
        new ActionNamePart(baseAction: BasicActionTypes.Gather, locationType: LocationTypes.Commercial, verbResult: ActionVerb.Browse),
        new ActionNamePart(baseAction: BasicActionTypes.Gather, verbResult: ActionVerb.Collect),

        // Labor
        new ActionNamePart(baseAction: BasicActionTypes.Labor, locationType: LocationTypes.Industrial, verbResult: ActionVerb.Labor),
        new ActionNamePart(baseAction: BasicActionTypes.Labor, locationType: LocationTypes.Social, verbResult: ActionVerb.Serve),
        new ActionNamePart(baseAction: BasicActionTypes.Labor, verbResult: ActionVerb.Labor),

        // Trade
        new ActionNamePart(baseAction: BasicActionTypes.Trade, verbResult: ActionVerb.Negotiate),
        new ActionNamePart(baseAction: BasicActionTypes.Trade, locationType: LocationTypes.Commercial, verbResult: ActionVerb.Barter),
        new ActionNamePart(baseAction: BasicActionTypes.Trade, verbResult: ActionVerb.Trade),

        // Mingle
        new ActionNamePart(baseAction: BasicActionTypes.Mingle, locationType: LocationTypes.Social, verbResult: ActionVerb.Socialize),
        new ActionNamePart(baseAction: BasicActionTypes.Mingle, verbResult: ActionVerb.Network),
        new ActionNamePart(baseAction: BasicActionTypes.Mingle, verbResult: ActionVerb.Chat),

        // --- Location Descriptor Combinations ---
        new ActionNamePart(locationType: LocationTypes.Industrial, locationDescriptorResult: LocationDescriptor.Dockside),
        new ActionNamePart(locationType: LocationTypes.Commercial, locationDescriptorResult: LocationDescriptor.Market),
        new ActionNamePart(locationType: LocationTypes.Social, scale: ScaleVariations.Intimate, locationDescriptorResult: LocationDescriptor.Tavern),
        new ActionNamePart(locationType: LocationTypes.Nature, exposure: ExposureConditions.Outdoor, locationDescriptorResult: LocationDescriptor.Forest),

        // --- Space Descriptor Combinations ---
        new ActionNamePart(baseAction: BasicActionTypes.Gather, locationType: LocationTypes.Nature, complexity: ComplexityTypes.Complex, spaceDescriptorResult: SpaceDescriptor.Game),
        new ActionNamePart(baseAction: BasicActionTypes.Gather, locationType: LocationTypes.Nature, spaceDescriptorResult: SpaceDescriptor.Resources),
        new ActionNamePart(scale: ScaleVariations.Intimate, spaceDescriptorResult: SpaceDescriptor.CloseQuarters),
        new ActionNamePart(exposure: ExposureConditions.Outdoor, spaceDescriptorResult: SpaceDescriptor.Wilderness),

        // --- Activity Descriptor Combinations ---
        new ActionNamePart(complexity: ComplexityTypes.Complex, activityDescriptorResult: ActivityDescriptor.Expert),
        new ActionNamePart(activityDescriptorResult: ActivityDescriptor.Intensive),
    };

    public ActionVerb? GetVerb(ActionGenerationContext context)
    {
        foreach (ActionNamePart combo in Combinations)
        {
            if (combo.BaseAction == context.BaseAction &&
                (combo.LocationType == null || combo.LocationType == context.LocationType) &&
                (combo.Exposure == null || combo.Exposure == context.Space.Exposure) &&
                (combo.Complexity == null || combo.Complexity == context.Activity.Complexity))
            {
                return combo.VerbResult;
            }
        }
        return null; // Or a default verb
    }

    public LocationDescriptor? GetLocationDescriptor(ActionGenerationContext context)
    {
        foreach (ActionNamePart combo in Combinations)
        {
            if ((combo.LocationType == null || combo.LocationType == context.LocationType) &&
                (combo.Scale == null || combo.Scale == context.Space.Scale))
            {
                return combo.LocationDescriptorResult;
            }
        }
        return null;
    }

    public SpaceDescriptor? GetSpaceDescriptor(ActionGenerationContext context)
    {
        foreach (ActionNamePart combo in Combinations)
        {
            if ((combo.BaseAction == null || combo.BaseAction == context.BaseAction) &&
                (combo.LocationType == null || combo.LocationType == context.LocationType) &&
                (combo.Complexity == null || combo.Complexity == context.Activity.Complexity) &&
                (combo.Scale == null || combo.Scale == context.Space.Scale) &&
                (combo.Exposure == null || combo.Exposure == context.Space.Exposure))
            {
                return combo.SpaceDescriptorResult;
            }
        }
        return null;
    }

    public ActivityDescriptor? GetActivityDescriptor(ActionGenerationContext context)
    {
        foreach (ActionNamePart combo in Combinations)
        {
            if ((combo.Complexity == null || combo.Complexity == context.Activity.Complexity))
            {
                return combo.ActivityDescriptorResult;
            }
        }
        return null;
    }
}

public class ActionNamePart
{
    public BasicActionTypes? BaseAction { get; set; }
    public LocationTypes? LocationType { get; set; }
    public ScaleVariations? Scale { get; set; }
    public ExposureConditions? Exposure { get; set; }
    public LegalityTypes? Legality { get; set; }
    public TensionState? Tension { get; set; }
    public ComplexityTypes? Complexity { get; set; }
    
    public ActionVerb? VerbResult { get; set; }
    public LocationDescriptor? LocationDescriptorResult { get; set; }
    public SpaceDescriptor? SpaceDescriptorResult { get; set; }
    public ActivityDescriptor? ActivityDescriptorResult { get; set; }

    public ActionNamePart(
        BasicActionTypes? baseAction = null,
        LocationTypes? locationType = null,
        ScaleVariations? scale = null,
        ExposureConditions? exposure = null,
        LegalityTypes? legality = null,
        TensionState? tension = null,
        ComplexityTypes? complexity = null,
        ActionVerb? verbResult = null,
        LocationDescriptor? locationDescriptorResult = null,
        SpaceDescriptor? spaceDescriptorResult = null,
        ActivityDescriptor? activityDescriptorResult = null)
    {
        BaseAction = baseAction;
        LocationType = locationType;
        Scale = scale;
        Exposure = exposure;
        Legality = legality;
        Tension = tension;
        Complexity = complexity;
        VerbResult = verbResult;
        LocationDescriptorResult = locationDescriptorResult;
        SpaceDescriptorResult = spaceDescriptorResult;
        ActivityDescriptorResult = activityDescriptorResult;
    }
}

// In ActionNameGenerator.cs

public enum ActionVerb
{
    // Gather
    Forage,
    Hunt,
    Browse,
    Collect,

    // Labor
    Labor,
    Serve,

    // Trade
    Negotiate,
    Barter,
    Trade,

    // Mingle
    Socialize,
    Network,
    Chat
}

public enum LocationDescriptor
{
    Dockside,
    Market,
    Tavern,
    Forest,
    // ... add more as needed
}

public enum SpaceDescriptor
{
    Game,
    Resources,
    CloseQuarters,
    Wilderness,
    // ... add more as needed
}

public enum ActivityDescriptor
{
    Expert,
    Intensive,
    // ... add more as needed
}