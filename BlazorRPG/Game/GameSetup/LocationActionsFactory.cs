public static class LocationActionsFactory
{
    public static List<BasicAction> Create(
        LocationTypes locationType,
        List<ActivityTypes> activityTypes,
        AccessTypes accessType,
        ShelterStates shelterState,
        DangerLevels dangerLevel,
        TradeDirectionTypes tradeDirectionType,
        LocationResourceTypes locationResourceType
    )
    {
        List<BasicAction> actions = new List<BasicAction>();

        switch (locationType)
        {
            case LocationTypes.Industry:
                if (!activityTypes.Contains(ActivityTypes.Labor)) activityTypes.Add(ActivityTypes.Labor);
                break;

            case LocationTypes.Commerce:
                if (!activityTypes.Contains(ActivityTypes.Trade)) activityTypes.Add(ActivityTypes.Trade);
                break;

            case LocationTypes.Social:
                if (!activityTypes.Contains(ActivityTypes.Mingle)) activityTypes.Add(ActivityTypes.Mingle);
                break;

            case LocationTypes.Nature:
                if (!activityTypes.Contains(ActivityTypes.Gather)) activityTypes.Add(ActivityTypes.Gather);
                break;
        }

        foreach (ActivityTypes activityType in activityTypes)
        {
            switch (activityType)
            {
                case ActivityTypes.Labor:
                    actions.AddRange(GetLaborActions(locationType, dangerLevel));
                    break;

                case ActivityTypes.Gather:
                    actions.AddRange(GetGatherActions(locationType, locationResourceType, dangerLevel));
                    break;

                case ActivityTypes.Trade:
                    actions.AddRange(GetTradeActions(locationType, tradeDirectionType, dangerLevel));
                    break;

                case ActivityTypes.Mingle:
                    actions.AddRange(GetMingleActions(locationType, dangerLevel));
                    break;
            }
        }

        actions.AddRange(GetRestActions(shelterState));
        return actions;
    }

    private static List<BasicAction> GetLaborActions(LocationTypes locationType, DangerLevels dangerLevel)
    {
        List<BasicAction> actions = new List<BasicAction>();
        switch (locationType)
        {
            case LocationTypes.Industry:
                actions.Add(
                    AddAction(action => action
                        .ForAction(BasicActionTypes.Labor)
                        .WithDescription("Load Cargo")
                        .AddTimeWindow(TimeWindows.Morning)
                        .AddTimeWindow(TimeWindows.Afternoon)
                        .ExpendsPhysicalEnergy(1)
                        .RewardsCoins(1)
                        , dangerLevel
                    )
                );
                break;

            case LocationTypes.Commerce:
                actions.Add(
                    AddAction(action => action
                        .ForAction(BasicActionTypes.Labor)
                        .WithDescription("Loading/Unloading Work")
                        .AddTimeWindow(TimeWindows.Morning)
                        .AddTimeWindow(TimeWindows.Afternoon)
                        .ExpendsPhysicalEnergy(1)
                        .RewardsCoins(1)
                        , dangerLevel
                    )
                );
                break;

            case LocationTypes.Social:
                actions.Add(
                    AddAction(action => action
                        .ForAction(BasicActionTypes.Labor)
                        .WithDescription("Service Work")
                        .AddTimeWindow(TimeWindows.Morning)
                        .AddTimeWindow(TimeWindows.Afternoon)
                        .ExpendsPhysicalEnergy(1)
                        .RewardsCoins(1)
                        , dangerLevel
                    )
                );
                break;

            case LocationTypes.Nature:
                break;
        }

        return actions;
    }

    private static List<BasicAction> GetGatherActions(LocationTypes locationType, LocationResourceTypes locationResourceType, DangerLevels dangerLevel)
    {
        List<BasicAction> actions = new List<BasicAction>();

        ResourceTypes resourceType;
        switch(locationResourceType)
        {
            case LocationResourceTypes.Forage:
                resourceType = ResourceTypes.Food;
                break;

            case LocationResourceTypes.Fish:
                resourceType = ResourceTypes.Food;
                break;

            case LocationResourceTypes.Game:
                resourceType = ResourceTypes.Food;
                break;

            case LocationResourceTypes.Wood:
                resourceType = ResourceTypes.Wood;
                break;

            default:
                resourceType = ResourceTypes.Wood;
            break;
        }

        switch (locationType)
        {
            case LocationTypes.Industry:
                actions.Add(
                    AddAction(action => action
                        .ForAction(BasicActionTypes.Gather)
                        .WithDescription("Pick up Trash")
                        .AddTimeWindow(TimeWindows.Afternoon)
                        .AddTimeWindow(TimeWindows.Evening)
                        .ExpendsPhysicalEnergy(1)
                        , dangerLevel
                    )
                );
                break;

            case LocationTypes.Commerce:
                actions.Add(
                    AddAction(action => action
                        .ForAction(BasicActionTypes.Gather)
                        .WithDescription("Market Scrounging")
                        .AddTimeWindow(TimeWindows.Afternoon)
                        .AddTimeWindow(TimeWindows.Evening)
                        .ExpendsSocialEnergy(1)
                        .RewardsFood(1)
                        , dangerLevel
                    )
                );
                break;

            case LocationTypes.Social:
                actions.Add(
                    AddAction(action => action
                        .ForAction(BasicActionTypes.Gather)
                        .WithDescription("Rumor Collection")
                        .AddTimeWindow(TimeWindows.Evening)
                        .AddTimeWindow(TimeWindows.Night)
                        .ExpendsPhysicalEnergy(1)
                        .RewardsCoins(1)
                        , dangerLevel
                    )
                );
                break;

            case LocationTypes.Nature:
                actions.Add(
                    AddAction(action => action
                        .ForAction(BasicActionTypes.Gather)
                        .WithDescription("Gather Wood")
                        .AddTimeWindow(TimeWindows.Morning)
                        .AddTimeWindow(TimeWindows.Afternoon)
                        .ExpendsFocusEnergy(2)
                        .RewardsItem(resourceType)
                        , dangerLevel
                    )
                );
                break;
        }

        return actions;
    }

    private static List<BasicAction> GetTradeActions(LocationTypes locationType, TradeDirectionTypes tradeDirection, DangerLevels dangerLevel)
    {
        List<BasicAction> actions = new List<BasicAction>();
        switch (locationType)
        {
            case LocationTypes.Industry:
                actions.Add(
                    AddAction(action => action
                        .ForAction(BasicActionTypes.Trade)
                        .WithDescription("Sell Food")
                        .AddTimeWindow(TimeWindows.Afternoon)
                        .AddTimeWindow(TimeWindows.Evening)
                        .ExpendsFood(1)
                        .RewardsCoins(1)
                        , dangerLevel
                    )
                );
                break;

            case LocationTypes.Commerce:
                actions.Add(
                    AddAction(action => action
                        .ForAction(BasicActionTypes.Trade)
                        .WithDescription("Buy Food")
                        .AddTimeWindow(TimeWindows.Morning)
                        .AddTimeWindow(TimeWindows.Afternoon)
                        .ExpendsCoins(2)
                        .RewardsFood(1)
                        , dangerLevel
                    )
                );
                break;

            case LocationTypes.Social:
                actions.Add(
                    AddAction(action => action
                        .ForAction(BasicActionTypes.Trade)
                        .WithDescription("Information Trading")
                        .AddTimeWindow(TimeWindows.Afternoon)
                        .AddTimeWindow(TimeWindows.Evening)
                        .ExpendsPhysicalEnergy(1)
                        .RewardsCoins(1)
                        , dangerLevel
                    )
                );
                break;

            case LocationTypes.Nature:
                if (tradeDirection != TradeDirectionTypes.Sell) break;

                actions.Add(
                   AddAction(action => action
                       .ForAction(BasicActionTypes.Trade)
                       .WithDescription("Sell Wood")
                       .AddTimeWindow(TimeWindows.Morning)
                       .AddTimeWindow(TimeWindows.Afternoon)
                       .AddTimeWindow(TimeWindows.Evening)
                       .ExpendsItem(ResourceTypes.Wood)
                       .RewardsCoins(2)
                       , dangerLevel
                    )
               );
                break;
        }

        return actions;
    }

    private static List<BasicAction> GetMingleActions(LocationTypes locationType, DangerLevels dangerLevel)
    {
        List<BasicAction> actions = new List<BasicAction>();
        switch (locationType)
        {
            case LocationTypes.Industry:
                actions.Add(
                    AddAction(action => action
                        .ForAction(BasicActionTypes.Discuss)
                        .WithDescription("Talk to Workers")
                        .AddTimeWindow(TimeWindows.Evening)
                        .ExpendsSocialEnergy(1)
                        .RewardsTrust(1)
                        , dangerLevel
                    )
                );
                break;

            case LocationTypes.Commerce:
                actions.Add(
                    AddAction(action => action
                        .ForAction(BasicActionTypes.Discuss)
                        .WithDescription("Merchant Interaction")
                        .AddTimeWindow(TimeWindows.Evening)
                        .ExpendsSocialEnergy(1)
                        .RewardsTrust(1)
                        , dangerLevel
                    )
                );
                break;

            case LocationTypes.Social:
                actions.Add(
                    AddAction(action => action
                        .ForAction(BasicActionTypes.Discuss)
                        .WithDescription("Talk To Strangers")
                        .AddTimeWindow(TimeWindows.Morning)
                        .AddTimeWindow(TimeWindows.Afternoon)
                        .AddTimeWindow(TimeWindows.Evening)
                        .ExpendsSocialEnergy(1)
                        .RewardsTrust(1)
                        , dangerLevel
                    )
                );
                break;

            case LocationTypes.Nature:
                actions.Add(
                    AddAction(action => action
                        .ForAction(BasicActionTypes.Discuss)
                        .WithDescription("Traveler Interaction")
                        .AddTimeWindow(TimeWindows.Morning)
                        .AddTimeWindow(TimeWindows.Afternoon)
                        .AddTimeWindow(TimeWindows.Evening)
                        .RewardsSocialEnergy(1)
                        , dangerLevel
                    )
                );
                break;
        }

        return actions;
    }

    private static List<BasicAction> GetRestActions(ShelterStates shelterState)
    {
        List<BasicAction> actions = new List<BasicAction>();

        switch (shelterState)
        {
            case ShelterStates.None:
                break;

            case ShelterStates.Bad:
                actions.Add(
                    AddAction(action => action
                        .ForAction(BasicActionTypes.Rest)
                        .WithDescription("Bad Shelter")
                        .AddTimeWindow(TimeWindows.Evening)
                        .AddTimeWindow(TimeWindows.Night)
                        .ExpendsFood(1)
                        , DangerLevels.Safe
                    )
                );
                break;

            case ShelterStates.Good:
                actions.Add(
                    AddAction(action => action
                        .ForAction(BasicActionTypes.Rest)
                        .WithDescription("Good Shelter")
                        .AddTimeWindow(TimeWindows.Evening)
                        .AddTimeWindow(TimeWindows.Night)
                        .ExpendsCoins(1)
                        .ExpendsFood(1)
                        .RewardsHealth(1)
                        .RewardsPhysicalEnergy(3)
                        .RewardsPhysicalEnergy(3)
                        .RewardsPhysicalEnergy(3)
                        , DangerLevels.Safe
                    )
                );
                break;
        }
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