public class CharacterActionsFactory
{
    public static List<ActionImplementation> Create(
        )
    {
        List<ActionImplementation> actions = new List<ActionImplementation>();

        return actions;
    }

    private static ActionImplementation AddAction(Action<ActionBuilder> buildBasicAction)
    {
        ActionBuilder builder = new ActionBuilder();
        buildBasicAction(builder);

        ActionImplementation action = builder.Build();
        return action;
    }
}