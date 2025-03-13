public class GameSetup
{
    private const LocationNames StartingLocation = LocationNames.WaysideInn;

    public static GameState CreateNewGame()
    {
        GameState gameState = new GameState();
        gameState.World.SetCurrentTime(22);
        gameState.World.ChangeWeather(WeatherTypes.Clear);

        GameRules gameRules = GameRules.StandardRuleset;

        PlayerState playerInfo = new PlayerState();
        playerInfo.SetStartingLocation(StartingLocation);

        playerInfo.Coins = gameRules.StartingCoins;

        playerInfo.MinHealth = gameRules.MinimumHealth;
        playerInfo.Health = gameRules.StartingHealth;
        playerInfo.MaxHealth = 20;

        playerInfo.PhysicalEnergy = gameRules.StartingPhysicalEnergy;
        playerInfo.MaxPhysicalEnergy = 10;

        playerInfo.Focus = gameRules.StartingFocus;
        playerInfo.MaxFocus = 10;

        playerInfo.Confidence = gameRules.StartingConfidence;
        playerInfo.MaxConfidence = 10;

        playerInfo.Inventory.AddItems(ItemTypes.Food, 5);

        gameState.Player = playerInfo;

        return gameState;
    }
}
