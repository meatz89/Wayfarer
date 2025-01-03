public class CharacterActionsFactory
{
    public static List<BasicAction> Create(
        )
    {
        List<BasicAction> actions = new List<BasicAction>();

        return actions;
    }

    private static BasicAction AddAction(Action<ActionBuilder> buildBasicAction, DangerLevels dangerLevel)
    {
        ActionBuilder builder = new ActionBuilder();
        buildBasicAction(builder);

        if (dangerLevel == DangerLevels.Dangerous)
        {
            builder.ExpendsHealth(1);
        }

        BasicAction action = builder.Build();
        return action;
    }
}