public class LocationContent
{
    // === RESIDENTIAL DISTRICT ===
    public static Location Tavern => new LocationBuilder()
        .ForLocation(LocationNames.LionsHeadTavern)
        .SetLocationType(LocationTypes.Social)
        .AddTravelConnection(LocationNames.HarborStreets)
        .AddTravelConnection(LocationNames.MarketSquare)
        // Tutorial entrance spot
        .AddLocationSpot(feature => feature
            .ForLocation(LocationNames.LionsHeadTavern)
            .ForLocationSpot(LocationSpotNames.ServingArea)
            .WithEnergyCost(1, EnergyTypes.Physical)
            .WithEnergyCost(1, EnergyTypes.Social)
            .WithCoinReward(3)
        )
        // Tutorial common tables
        .AddLocationSpot(feature => feature
            .ForLocation(LocationNames.LionsHeadTavern)
            .ForLocationSpot(LocationSpotNames.CommonArea)
            .WithEnergyCost(1, EnergyTypes.Social)
        )
        // Tutorial bar counter
        .AddLocationSpot(feature => feature
            .ForLocation(LocationNames.LionsHeadTavern)
            .WithCharacter(CharacterNames.Bartender)
            .ForLocationSpot(LocationSpotNames.TavernBar)
            .WithCoinCost(3)
            .WithOutputResource(ResourceTypes.Food, 1)
        )
        // Tutorial corner
        .AddLocationSpot(feature => feature
            .ForLocation(LocationNames.LionsHeadTavern)
            .ForLocationSpot(LocationSpotNames.PrivateCorner)
            .WithEnergyCost(1, EnergyTypes.Focus)
        )
        // Tutorial back room (initially locked)
        .AddLocationSpot(feature => feature
            .ForLocation(LocationNames.LionsHeadTavern)
            .ForLocationSpot(LocationSpotNames.GoodShelter)
            .WithCoinCost(1)
            .WithEnergyReward(5, EnergyTypes.Physical)
            .WithEnergyReward(5, EnergyTypes.Focus)
            .WithEnergyReward(5, EnergyTypes.Social)
            .SetAccessType(AccessTypes.Restricted)
        )
        .SetAccessType(AccessTypes.Open)
        .SetDangerLevel(DangerLevels.Safe)
        .Build();

}