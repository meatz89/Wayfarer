public class GameSetup
{
    private const LocationNames StartingLocation = LocationNames.Village;

    public static GameState CreateNewGame()
    {
        GameState gameState = new GameState();
        gameState.WorldState.SetCurrentTime(22);
        gameState.WorldState.ChangeWeather(WeatherTypes.Clear);

        GameRules gameRules = GameRules.StandardRuleset;

        PlayerState playerInfo = new PlayerState();
        playerInfo.SetStartingLocation(StartingLocation.ToString());

        playerInfo.Coins = gameRules.StartingCoins;

        playerInfo.MinHealth = gameRules.MinimumHealth;
        playerInfo.Health = gameRules.StartingHealth;
        playerInfo.MaxHealth = 20;

        playerInfo.PhysicalEnergy = gameRules.StartingPhysicalEnergy;
        playerInfo.MaxPhysicalEnergy = 20;

        playerInfo.Concentration = gameRules.StartingConcentration;
        playerInfo.MaxConcentration = 20;

        playerInfo.Confidence = gameRules.StartingConfidence;
        playerInfo.MaxConfidence = 20;

        playerInfo.Inventory.AddItems(ItemTypes.Food, 5);

        gameState.PlayerState = playerInfo;

        return gameState;
    }
}
