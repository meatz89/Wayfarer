public class LocationPropertiesContent
{
    public static Location HarborStreets => new LocationPropertiesBuilder()
        .ForLocation(LocationNames.HarborStreets)
        .AddTravelConnection(LocationNames.Docks)
        .AddTravelConnection(LocationNames.MarketSquare)
        .AddTravelConnection(LocationNames.LionsHeadTavern)
        .AddTravelConnection(LocationNames.ForestEdge)
        .SetLocationType(LocationTypes.Social)
        .SetAccessType(AccessTypes.Open)
        .SetShelterStatus(ShelterStates.Bad)
        .SetDangerLevel(DangerLevels.Safe)
        .Build();

    public static Location Docks => new LocationPropertiesBuilder()
        .ForLocation(LocationNames.Docks)
        .AddTravelConnection(LocationNames.ForestEdge)
        .AddTravelConnection(LocationNames.HarborStreets)
        .SetLocationType(LocationTypes.Industry)
        .SetAccessType(AccessTypes.Open)
        .SetShelterStatus(ShelterStates.None)
        .SetDangerLevel(DangerLevels.Dangerous)
        .Build();

    public static Location Market => new LocationPropertiesBuilder()
        .ForLocation(LocationNames.MarketSquare)
        .AddTravelConnection(LocationNames.LionsHeadTavern)
        .AddTravelConnection(LocationNames.HarborStreets)
        .SetLocationType(LocationTypes.Commerce)
        .SetAccessType(AccessTypes.Open)
        .SetShelterStatus(ShelterStates.None)
        .SetDangerLevel(DangerLevels.Safe)
        .Build();

    public static Location Tavern => new LocationPropertiesBuilder()
        .ForLocation(LocationNames.LionsHeadTavern)
        .AddTravelConnection(LocationNames.MarketSquare)
        .AddTravelConnection(LocationNames.HarborStreets)
        .SetLocationType(LocationTypes.Social)
        .SetAccessType(AccessTypes.Open)
        .SetShelterStatus(ShelterStates.Good)
        .SetDangerLevel(DangerLevels.Safe)
        .Build();

    public static Location ForestEdge => new LocationPropertiesBuilder()
        .ForLocation(LocationNames.ForestEdge)
        .AddTravelConnection(LocationNames.Docks)
        .AddTravelConnection(LocationNames.HarborStreets)
        .SetLocationType(LocationTypes.Nature)
        .SetResourceType(LocationResourceTypes.Wood)
        .SetTradeDirectionType(TradeDirectionTypes.Sell)
        .SetAccessType(AccessTypes.Open)
        .SetShelterStatus(ShelterStates.None)
        .SetDangerLevel(DangerLevels.Safe)
        .Build();
}
