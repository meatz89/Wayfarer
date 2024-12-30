
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
        actions.AddRange(GetLocationTypeActions(locationType));

        switch (locationType)
        {
            case LocationTypes.Industry:
                foreach (ActivityTypes activityType in activityTypes)
                {
                    actions.AddRange(GetActivityTypeActionsForIndustry(activityType));
                }
                break;

            case LocationTypes.Commerce:
                foreach (ActivityTypes activityType in activityTypes)
                {
                    actions.AddRange(GetActivityTypeActionsForCommerce(activityType));
                }
                break;

            case LocationTypes.Social:
                foreach (ActivityTypes activityType in activityTypes)
                {
                    actions.AddRange(GetActivityTypeActionsForSocial(activityType));
                }
                break;

            case LocationTypes.Nature:
                foreach (ActivityTypes activityType in activityTypes)
                {
                    actions.AddRange(GetActivityTypeActionsForNature(activityType));
                }
                break;
        }

        actions.AddRange(GetRestActions(shelterState));
        return actions;
    }

    private static List<BasicAction> GetActivityTypeActionsForIndustry(ActivityTypes activityType)
    {
        List<BasicAction> actions = new List<BasicAction>();
        switch (activityType)
        {
            case ActivityTypes.Trade:
                actions.Add(
                    AddAction(action => action
                        .ForAction(BasicActionTypes.Trade)
                        .WithDescription("Sell Food")
                        .AddTimeWindow(TimeWindows.Afternoon)
                        .AddTimeWindow(TimeWindows.Evening)
                        .ExpendsFood(1)
                        .RewardsCoins(1)
                        )
                );
                break;

            case ActivityTypes.Mingle:
                actions.Add(
                    AddAction(action => action
                        .ForAction(BasicActionTypes.Discuss)
                        .WithDescription("Talk to Workers")
                        .AddTimeWindow(TimeWindows.Evening)
                        .ExpendsSocialEnergy(1)
                        .RewardsTrust(1)
                    )
                );
                break;
        }
        return actions;
    }


    private static List<BasicAction> GetActivityTypeActionsForCommerce(ActivityTypes activityType)
    {
        List<BasicAction> actions = new List<BasicAction>();
        switch (activityType)
        {
            case ActivityTypes.Labor:
                actions.Add(
                    AddAction(action => action
                        .ForAction(BasicActionTypes.Labor)
                        .WithDescription("Loading/Unloading Work")
                        .AddTimeWindow(TimeWindows.Morning)
                        .AddTimeWindow(TimeWindows.Afternoon)
                        .ExpendsPhysicalEnergy(1)
                        .RewardsCoins(1)
                        )
                );
                break;

            case ActivityTypes.Gather:
                actions.Add(
                    AddAction(action => action
                        .ForAction(BasicActionTypes.Gather)
                        .WithDescription("Market Scrounging")
                        .AddTimeWindow(TimeWindows.Afternoon)
                        .AddTimeWindow(TimeWindows.Evening)
                        .ExpendsSocialEnergy(1)
                        .RewardsFood(1)
                        )
                );
                break;

            case ActivityTypes.Mingle:
                actions.Add(
                    AddAction(action => action
                        .ForAction(BasicActionTypes.Discuss)
                        .WithDescription("Merchant Interaction")
                        .AddTimeWindow(TimeWindows.Evening)
                        .ExpendsSocialEnergy(1)
                        .RewardsTrust(1)
                    )
                );
                break;
        }
        return actions;
    }

    private static List<BasicAction> GetActivityTypeActionsForSocial(ActivityTypes activityType)
    {
        List<BasicAction> actions = new List<BasicAction>();
        switch (activityType)
        {
            case ActivityTypes.Labor:
                actions.Add(
                    AddAction(action => action
                        .ForAction(BasicActionTypes.Labor)
                        .WithDescription("Service Work")
                        .AddTimeWindow(TimeWindows.Morning)
                        .AddTimeWindow(TimeWindows.Afternoon)
                        .ExpendsPhysicalEnergy(1)
                        .RewardsCoins(1)
                        )
                );
                break;

            case ActivityTypes.Trade:
                actions.Add(
                    AddAction(action => action
                        .ForAction(BasicActionTypes.Trade)
                        .WithDescription("Information Trading")
                        .AddTimeWindow(TimeWindows.Afternoon)
                        .AddTimeWindow(TimeWindows.Evening)
                        .ExpendsPhysicalEnergy(1)
                        .RewardsCoins(1)
                        )
                );
                break;

            case ActivityTypes.Gather:
                actions.Add(
                    AddAction(action => action
                        .ForAction(BasicActionTypes.Gather)
                        .WithDescription("Rumor Collection")
                        .AddTimeWindow(TimeWindows.Evening)
                        .AddTimeWindow(TimeWindows.Night)
                        .ExpendsPhysicalEnergy(1)
                        .RewardsCoins(1)
                        )
                );
                break;

            case ActivityTypes.Mingle:
                actions.Add(
                    AddAction(action => action
                        .ForAction(BasicActionTypes.Discuss)
                        .WithDescription("Drink")
                        .AddTimeWindow(TimeWindows.Evening)
                        .ExpendsCoins(1)
                        .RewardsSocialEnergy(1)
                    )
                );
                break;
        }
        return actions;
    }


    private static List<BasicAction> GetActivityTypeActionsForNature(ActivityTypes activityType)
    {
        List<BasicAction> actions = new List<BasicAction>();
        switch (activityType)
        {
            case ActivityTypes.Trade:
                actions.Add(
                    AddAction(action => action
                        .ForAction(BasicActionTypes.Trade)
                        .WithDescription("Forager Trading")
                        .AddTimeWindow(TimeWindows.Morning)
                        .AddTimeWindow(TimeWindows.Afternoon)
                        .AddTimeWindow(TimeWindows.Evening)
                        .ExpendsCoins(1)
                        .RewardsFood(1)
                        )
                );
                break;

            case ActivityTypes.Mingle:
                actions.Add(
                    AddAction(action => action
                        .ForAction(BasicActionTypes.Discuss)
                        .WithDescription("Traveler Interaction")
                        .AddTimeWindow(TimeWindows.Morning)
                        .AddTimeWindow(TimeWindows.Afternoon)
                        .AddTimeWindow(TimeWindows.Evening)
                        .RewardsSocialEnergy(1)
                    )
                );
                break;
        }
        return actions;
    }
    private static List<BasicAction> GetLocationTypeActions(LocationTypes locationType)
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
                    )
                );
                break;

            case LocationTypes.Nature:
                actions.Add(
                    AddAction(action => action
                        .ForAction(BasicActionTypes.Gather)
                        .WithDescription("Hunt")
                        .AddTimeWindow(TimeWindows.Morning)
                        .AddTimeWindow(TimeWindows.Afternoon)
                        .ExpendsPhysicalEnergy(2)
                        .RewardsFood(2)
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
                        .RewardsPhysicalEnergy(3)
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
                        .RewardsPhysicalEnergy(5)
                        .RewardsPhysicalEnergy(5)
                        .RewardsPhysicalEnergy(5)
                    )
                );
                break;
        }
        return actions;
    }

    private static BasicAction AddAction(Action<BasicActionDefinitionBuilder> buildBasicAction)
    {
        BasicActionDefinitionBuilder builder = new BasicActionDefinitionBuilder();
        buildBasicAction(builder);
        BasicAction action = builder.Build();
        return action;
    }
}