public class GameSetup
{
    public static GameState CreateNewGame()
    {
        GameState gameState = new GameState();
        gameState.World.SetCurrentTime(0);

        GameRules gameRules = GameRules.StandardRuleset;

        PlayerState playerInfo = new PlayerState();
        playerInfo.StartingLocation = LocationNames.WaysideInn;

        playerInfo.Coins = gameRules.StartingCoins;

        playerInfo.MinHealth = gameRules.MinimumHealth;
        playerInfo.Health = gameRules.StartingHealth;
        playerInfo.MaxHealth = 20;

        playerInfo.MinConcentration = 0;
        playerInfo.Concentration = 10;
        playerInfo.MaxConcentration = 20;

        playerInfo.MinReputation = 0;
        playerInfo.Reputation = 10;
        playerInfo.MaxReputation = 20;

        playerInfo.PhysicalEnergy = gameRules.StartingPhysicalEnergy;
        playerInfo.MaxPhysicalEnergy = 10;

        playerInfo.FocusEnergy = gameRules.StartingFocusEnergy;
        playerInfo.MaxFocusEnergy = 10;

        playerInfo.SocialEnergy = gameRules.StartingSocialEnergy;
        playerInfo.MaxSocialEnergy = 10;

        playerInfo.Inventory.AddResources(ResourceTypes.Food, 5);

        gameState.Player = playerInfo;

        return gameState;
    }
}
