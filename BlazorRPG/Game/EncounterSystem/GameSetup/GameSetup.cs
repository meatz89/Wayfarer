public class GameSetup
{
    public static GameState CreateNewGame()
    {
        GameState gameState = new GameState();
        gameState.World.SetNewLocation(LocationContent.ForestRoad);

        GameRules gameRules = GameRules.StandardRuleset;

        PlayerState playerInfo = new PlayerState();
        playerInfo.Coins = gameRules.StartingCoins;

        playerInfo.MinHealth = gameRules.MinimumHealth;
        playerInfo.Health = gameRules.StartingHealth;
        playerInfo.MaxHealth = 100;

        playerInfo.MinConcentration = 0;
        playerInfo.Concentration = 50;
        playerInfo.MaxConcentration = 100;

        playerInfo.MinReputation = 0;
        playerInfo.Reputation = 50;
        playerInfo.MaxReputation = 100;

        playerInfo.PhysicalEnergy = gameRules.StartingPhysicalEnergy;
        playerInfo.MaxPhysicalEnergy = 50;

        playerInfo.FocusEnergy = gameRules.StartingFocusEnergy;
        playerInfo.MaxFocusEnergy = 50;

        playerInfo.SocialEnergy = gameRules.StartingSocialEnergy;
        playerInfo.MaxSocialEnergy = 50;

        playerInfo.Inventory.AddResources(ResourceTypes.Food, 5);

        gameState.Player = playerInfo;


        return gameState;
    }
}
