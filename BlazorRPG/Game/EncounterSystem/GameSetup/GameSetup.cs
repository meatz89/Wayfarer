public class GameSetup
{
    public static GameState CreateNewGame()
    {
        GameState gameState = new GameState();
        gameState.World.SetNewLocation(LocationContent.LionsHeadTavern);

        GameRules gameRules = GameRules.StandardRuleset;

        PlayerState playerInfo = new PlayerState();
        playerInfo.Coins = gameRules.StartingCoins;

        playerInfo.Health = gameRules.StartingHealth;
        playerInfo.MinHealth = gameRules.MinimumHealth;
        playerInfo.MaxHealth = 100;

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
