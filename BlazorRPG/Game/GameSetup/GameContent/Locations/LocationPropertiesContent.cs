public class LocationPropertiesContent
{
    public static LocationProperties HarborStreets => new LocationPropertiesBuilder()
        .ForLocation(LocationNames.HarborStreets)
        .SetLocationType(LocationTypes.Social)
        .SetAccessType(AccessTypes.Open)
        .SetShelterStatus(ShelterStates.Bad)
        .SetDangerLevel(DangerLevels.Safe)
        .Build();

    public static LocationProperties Docks => new LocationPropertiesBuilder()
        .ForLocation(LocationNames.Docks)
        .SetLocationType(LocationTypes.Industry)
        .SetAccessType(AccessTypes.Open)
        .SetShelterStatus(ShelterStates.None)
        .SetDangerLevel(DangerLevels.Dangerous)
        .Build();

    public static LocationProperties Market => new LocationPropertiesBuilder()
        .ForLocation(LocationNames.MarketSquare)
        .SetLocationType(LocationTypes.Commerce)
        .SetAccessType(AccessTypes.Open)
        .SetShelterStatus(ShelterStates.None)
        .SetDangerLevel(DangerLevels.Safe)
        .Build();

    public static LocationProperties Tavern => new LocationPropertiesBuilder()
        .ForLocation(LocationNames.LionsHeadTavern)
        .SetLocationType(LocationTypes.Social)
        .SetAccessType(AccessTypes.Open)
        .SetShelterStatus(ShelterStates.Good)
        .SetDangerLevel(DangerLevels.Safe)
        .Build();

    public static LocationProperties Forest => new LocationPropertiesBuilder()
        .ForLocation(LocationNames.DarkForest)
        .SetLocationType(LocationTypes.Nature)
        .SetAccessType(AccessTypes.Open)
        .SetShelterStatus(ShelterStates.None)
        .SetDangerLevel(DangerLevels.Dangerous)
        .Build();
}
