public class GameSetup
{
    public static GameState CreateNewGame()
    {
        GameState gameState = new GameState();
        gameState.World.SetCurrentTime(22);
        gameState.World.ChangeWeather(WeatherTypes.Stormy);

        GameRules gameRules = GameRules.StandardRuleset;

        PlayerState playerInfo = new PlayerState();
        playerInfo.StartingLocation = LocationNames.WaysideInn;
        playerInfo.AddLocationKnowledge(LocationNames.WaysideInn);

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
