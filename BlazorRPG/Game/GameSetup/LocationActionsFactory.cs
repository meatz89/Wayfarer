public static class LocationActionsFactory
{
    public static List<BasicAction> Create(
        LocationTypes locationType,
        List<ActivityTypes> activityTypes,
        AccessTypes accessType,
        ShelterStates shelterState,
        DangerLevels dangerLevel
    )
    {
        List<BasicAction> actions = new List<BasicAction>();

        switch(locationType)
        {
            case LocationTypes.Industry:
                if(!activityTypes.Contains(ActivityTypes.Labor)) activityTypes.Add(ActivityTypes.Labor);
                break;

            case LocationTypes.Commerce:
                if(!activityTypes.Contains(ActivityTypes.Trade)) activityTypes.Add(ActivityTypes.Trade);
                break;

            case LocationTypes.Social:
                if(!activityTypes.Contains(ActivityTypes.Mingle)) activityTypes.Add(ActivityTypes.Mingle);
                break;

            case LocationTypes.Nature:
                if(!activityTypes.Contains(ActivityTypes.Gather)) activityTypes.Add(ActivityTypes.Gather);
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
                    actions.AddRange(GetGatherActions(locationType, dangerLevel));
                    break;

                case ActivityTypes.Trade:
                    actions.AddRange(GetTradeActions(locationType, dangerLevel));
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


    private static List<BasicAction> GetGatherActions(LocationTypes locationType, DangerLevels dangerLevel)
    {
        List<BasicAction> actions = new List<BasicAction>();
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
                        .WithDescription("Pick Berries")
                        .AddTimeWindow(TimeWindows.Morning)
                        .AddTimeWindow(TimeWindows.Afternoon)
                        .ExpendsFocusEnergy(2)
                        .RewardsFood(2)
                        , dangerLevel
                    )
                );
                break;
        }

        return actions;
    }

    private static List<BasicAction> GetTradeActions(LocationTypes locationType, DangerLevels dangerLevel)
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
                actions.Add(
                   AddAction(action => action
                       .ForAction(BasicActionTypes.Trade)
                       .WithDescription("Forager Trading")
                       .AddTimeWindow(TimeWindows.Morning)
                       .AddTimeWindow(TimeWindows.Afternoon)
                       .AddTimeWindow(TimeWindows.Evening)
                       .ExpendsCoins(1)
                       .RewardsFood(1)
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
                        .RewardsPhysicalEnergy(1)
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