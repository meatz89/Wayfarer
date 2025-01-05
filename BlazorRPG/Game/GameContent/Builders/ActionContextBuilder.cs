public class ActionContextBuilder
{
    private readonly ActionGenerationContext context = new();

    public ActionContextBuilder(LocationTypes locationType)
    {
        context.LocationType = locationType;
    }

    public ActionContextBuilder WithBaseAction(BasicActionTypes baseAction)
    {
        context.BaseAction = baseAction;
        return this;
    }

    public ActionContextBuilder WithSpace(Action<SpacePropertiesBuilder> buildSpace)
    {
        var builder = new SpacePropertiesBuilder();
        buildSpace(builder);
        context.Space = builder.Build();
        return this;
    }

    public ActionContextBuilder WithSocial(Action<SocialContextBuilder> buildSocial)
    {
        var builder = new SocialContextBuilder();
        buildSocial(builder);
        context.Social = builder.Build();
        return this;
    }

    public ActionContextBuilder WithActivity(Action<ActivityPropertiesBuilder> buildActivity)
    {
        var builder = new ActivityPropertiesBuilder();
        buildActivity(builder);
        context.Activity = builder.Build();
        return this;
    }

    public ActionGenerationContext Build() => context;
}
