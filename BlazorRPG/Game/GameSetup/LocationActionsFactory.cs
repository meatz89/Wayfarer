public static class LocationActionsFactory
{
    public static List<BasicAction> Create(
        LocationTypes locationType,
        AccessTypes accessType,
        DangerLevels dangerLevel)
    {
        List<BasicAction> actions = new();

        // Add core type-specific actions
        actions.AddRange(GetCoreLocationActions(locationType, dangerLevel));

        return actions;
    }

    private static List<BasicAction> GetCoreLocationActions(
    LocationTypes locationType,
    DangerLevels dangerLevel)
    {
        List<BasicAction> actions = new();

        // Every location type gets one basic action that's always available
        BasicAction coreAction = locationType switch
        {
            LocationTypes.Industry => AddAction(action => action
                .ForAction(BasicActionTypes.Labor)
                .WithDescription("Basic Labor")
                .AddTimeWindow(TimeWindows.Morning)
                .AddTimeWindow(TimeWindows.Afternoon)
                .ExpendsPhysicalEnergy(1)
                .RewardsCoins(1),
                dangerLevel),

            LocationTypes.Commerce => AddAction(action => action
                .ForAction(BasicActionTypes.Trade)
                .WithDescription("Simple Trading")
                .AddTimeWindow(TimeWindows.Morning)
                .AddTimeWindow(TimeWindows.Afternoon)
                .ExpendsSocialEnergy(1)
                .RewardsCoins(1),
                dangerLevel),

            LocationTypes.Social => AddAction(action => action
                .ForAction(BasicActionTypes.Discuss)
                .WithDescription("Basic Conversation")
                .AddTimeWindow(TimeWindows.Evening)
                .ExpendsSocialEnergy(1)
                .RewardsTrust(1),
                dangerLevel),

            LocationTypes.Nature => AddAction(action => action
                .ForAction(BasicActionTypes.Gather)
                .WithDescription("Basic Gathering")
                .AddTimeWindow(TimeWindows.Morning)
                .AddTimeWindow(TimeWindows.Afternoon)
                .ExpendsFocusEnergy(1)
                .RewardsFood(1),
                dangerLevel),

            _ => throw new ArgumentException($"Unknown location type: {locationType}")
        };

        actions.Add(coreAction);
        return actions;
    }

    private static BasicAction AddAction(Action<BasicActionDefinitionBuilder> buildBasicAction, DangerLevels dangerLevel)
    {
        BasicActionDefinitionBuilder builder = new BasicActionDefinitionBuilder();
        buildBasicAction(builder);

        if (dangerLevel == DangerLevels.Dangerous)
        {
            builder.ExpendsHealth(1);
        }

        BasicAction action = builder.Build();
        return action;
    }
}