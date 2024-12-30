public class LocationPropertiesContent
{
    public static LocationProperties HarborStreets => new LocationPropertiesBuilder()
        .ForLocation(LocationNames.HarborStreets)
        .SetLocationType(LocationTypes.Social)
        .SetActivityType(ActivityTypes.Mingle)
        .AddTimeSlot(TimeSlots.Morning)
        .AddTimeSlot(TimeSlots.Afternoon)
        .AddTimeSlot(TimeSlots.Evening)
        .AddTimeSlot(TimeSlots.Night)
        .SetAccessType(AccessTypes.Open)
        .SetShelterStatus(ShelterStates.Bad)
        .SetDangerLevel(DangerLevels.Safe)
        .Build();

    public static LocationProperties Docks => new LocationPropertiesBuilder()
        .ForLocation(LocationNames.Docks)
        .SetLocationType(LocationTypes.Industry)
        .SetActivityType(ActivityTypes.Labor)
        .AddTimeSlot(TimeSlots.Morning)
        .AddTimeSlot(TimeSlots.Afternoon)
        .SetAccessType(AccessTypes.Open)
        .SetShelterStatus(ShelterStates.None)
        .SetDangerLevel(DangerLevels.Safe)
        .AddLaborAction()
        .Build();

    public static LocationProperties Market => new LocationPropertiesBuilder()
        .ForLocation(LocationNames.MarketSquare)
        .SetLocationType(LocationTypes.Commerce)
        .SetActivityType(ActivityTypes.Trade)
        .AddTimeSlot(TimeSlots.Morning)
        .AddTimeSlot(TimeSlots.Afternoon)
        .SetAccessType(AccessTypes.Open)
        .SetShelterStatus(ShelterStates.None)
        .SetDangerLevel(DangerLevels.Safe)
        .AddTradeAction(TradeDirections.Buy, TradeResourceTypes.Food)
        .Build();

    public static LocationProperties Tavern => new LocationPropertiesBuilder()
        .ForLocation(LocationNames.LionsHeadTavern)
        .SetLocationType(LocationTypes.Social)
        .SetActivityType(ActivityTypes.Mingle)
        .AddTimeSlot(TimeSlots.Evening)
        .AddTimeSlot(TimeSlots.Night)
        .SetAccessType(AccessTypes.Open)
        .SetShelterStatus(ShelterStates.None)
        .SetDangerLevel(DangerLevels.Safe)
        .AddDiscussAction()
        .AddRestAction()
        .Build();
}
