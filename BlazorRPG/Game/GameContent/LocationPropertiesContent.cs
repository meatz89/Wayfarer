public class LocationPropertiesContent
{
    public static LocationProperties HarborStreets => new LocationPropertiesBuilder()
        .ForLocation(LocationNames.HarborStreets)
        .SetLocationType(LocationTypes.Social)
        .SetActivityType(ActivityTypes.Mingle)
        .SetAccessType(AccessTypes.Open)
        .SetShelterStatus(ShelterStates.Bad)
        .SetDangerLevel(DangerLevels.Safe)
        .Build();

    public static LocationProperties Docks => new LocationPropertiesBuilder()
        .ForLocation(LocationNames.Docks)
        .SetLocationType(LocationTypes.Industry)
        .SetActivityType(ActivityTypes.Labor)
        .SetAccessType(AccessTypes.Open)
        .SetShelterStatus(ShelterStates.None)
        .SetDangerLevel(DangerLevels.Safe)
        .AddAction(action => action
            .ForAction(BasicActionTypes.Labor)
            .WithDescription("Load Cargo")
            .AddTimeWindow(TimeWindows.Morning)
            .AddTimeWindow(TimeWindows.Afternoon)
            .ExpendsPhysicalEnergy(1)
            .RewardsCoins(1)
        )
        .AddAction(action => action
            .ForAction(BasicActionTypes.Labor)
            .WithDescription("Move Crates")
            .AddTimeWindow(TimeWindows.Morning)
            .AddTimeWindow(TimeWindows.Afternoon)
            .ExpendsPhysicalEnergy(2)
            .RewardsCoins(2)
        )
        .Build();

    public static LocationProperties Market => new LocationPropertiesBuilder()
        .ForLocation(LocationNames.MarketSquare)
        .SetLocationType(LocationTypes.Commerce)
        .SetActivityType(ActivityTypes.Trade)
        .SetAccessType(AccessTypes.Open)
        .SetShelterStatus(ShelterStates.None)
        .SetDangerLevel(DangerLevels.Safe)
        .AddAction(action => action
            .ForAction(BasicActionTypes.Trade)
            .WithDescription("Buy Food")
            .AddTimeWindow(TimeWindows.Morning)
            .AddTimeWindow(TimeWindows.Afternoon)
            .ExpendsCoins(2)
            .RewardsFood(1)
        )
        .Build();

    public static LocationProperties Tavern => new LocationPropertiesBuilder()
        .ForLocation(LocationNames.LionsHeadTavern)
        .SetLocationType(LocationTypes.Social)
        .SetActivityType(ActivityTypes.Mingle)
        .SetAccessType(AccessTypes.Open)
        .SetShelterStatus(ShelterStates.None)
        .SetDangerLevel(DangerLevels.Safe)
        .AddAction(action => action
            .ForAction(BasicActionTypes.Discuss)
            .WithDescription("Listen")
            .AddTimeWindow(TimeWindows.Evening)
            .ExpendsFocusEnergy(1)
        )
        .AddAction(action => action
            .ForAction(BasicActionTypes.Discuss)
            .WithDescription("Drink")
            .AddTimeWindow(TimeWindows.Evening)
            .ExpendsCoins(1)
            .RewardsSocialEnergy(1)
        )
        .AddAction(action => action
            .ForAction(BasicActionTypes.Rest)
            .WithDescription("Rest in Good Shelter")
            .AddTimeWindow(TimeWindows.Evening)
            .AddTimeWindow(TimeWindows.Night)
            .ExpendsCoins(1)
            .ExpendsFood(1)
        )
        .Build();
}
