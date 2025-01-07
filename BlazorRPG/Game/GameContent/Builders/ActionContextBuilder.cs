public class ActionContextBuilder
{
    public LocationTypes LocationType { get; private set; }
    public BasicActionTypes BaseAction { get; private set; }
    public SpaceProperties Space { get; private set; } = new();
    public SocialContext Social { get; private set; } = new();
    public ActivityProperties Activity { get; private set; } = new();
    public string LocationSpotName { get; private set; }

    public ActionContextBuilder(LocationTypes locationType)
    {
        LocationType = locationType;
    }

    public ActionContextBuilder WithBaseAction(BasicActionTypes baseAction)
    {
        BaseAction = baseAction;
        return this;
    }

    public ActionContextBuilder WithSpace(Action<SpacePropertiesBuilder> buildSpace)
    {
        SpacePropertiesBuilder builder = new SpacePropertiesBuilder();
        buildSpace(builder);
        Space = builder.Build();
        return this;
    }

    public ActionContextBuilder WithSocial(Action<SocialContextBuilder> buildSocial)
    {
        SocialContextBuilder builder = new SocialContextBuilder();
        buildSocial(builder);
        Social = builder.Build();
        return this;
    }

    public ActionContextBuilder WithActivity(Action<ActivityPropertiesBuilder> buildActivity)
    {
        ActivityPropertiesBuilder builder = new ActivityPropertiesBuilder();
        buildActivity(builder);
        Activity = builder.Build();
        return this;
    }

    public ActionContextBuilder WithLocationSpotName(string locationSpotName)
    {
        LocationSpotName = locationSpotName;
        return this;
    }

    public ActionGenerationContext Build()
    {
        ActionGenerationContext context = new ActionGenerationContext(
            LocationType,
            BaseAction,
            Space,
            Social,
            Activity,
            LocationSpotName
        );

        return context;
    }

}

public class ActionGenerationContext
{
    public LocationTypes LocationType { get; }
    public BasicActionTypes BaseAction { get; }
    public SpaceProperties Space { get; }
    public SocialContext Social { get; }
    public ActivityProperties Activity { get; }
    public string LocationSpotName { get; }


    public ActionGenerationContext(
        LocationTypes locationType,
        BasicActionTypes baseAction,
        SpaceProperties space,
        SocialContext social,
        ActivityProperties activity,
        string locationSpotName)
    {
        LocationType = locationType;
        BaseAction = baseAction;
        Space = space;
        Social = social;
        Activity = activity;
        LocationSpotName = locationSpotName;
    }
}