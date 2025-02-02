public class GameSetup
{
    private const LocationNames StartingLocation = LocationNames.Market;

    public static GameState CreateNewGame()
    {
        GameState gameState = new GameState();
        gameState.World.SetCurrentTime(22);
        gameState.World.ChangeWeather(WeatherTypes.Clear);

        GameRules gameRules = GameRules.StandardRuleset;

        PlayerState playerInfo = new PlayerState();
        playerInfo.StartingLocation = StartingLocation;
        playerInfo.AddLocationKnowledge(StartingLocation);

        playerInfo.Coins = gameRules.StartingCoins;

        playerInfo.MinHealth = gameRules.MinimumHealth;
        playerInfo.Health = gameRules.StartingHealth;
        playerInfo.MaxHealth = 20;

        playerInfo.PhysicalEnergy = gameRules.StartingPhysicalEnergy;
        playerInfo.MaxPhysicalEnergy = 10;

        playerInfo.Concentration = gameRules.StartingConcentration;
        playerInfo.MaxConcentration = 10;

        playerInfo.Reputation = gameRules.StartingReputation;
        playerInfo.MaxReputation = 10;

        playerInfo.Inventory.AddResources(ResourceTypes.Food, 5);

        gameState.Player = playerInfo;

        return gameState;
    }
}
