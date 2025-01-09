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
        playerInfo.MaxHealth = 40;

        playerInfo.PhysicalEnergy = gameRules.StartingPhysicalEnergy;
        playerInfo.MaxPhysicalEnergy = 40;

        playerInfo.FocusEnergy = gameRules.StartingFocusEnergy;
        playerInfo.MaxFocusEnergy = 40;

        playerInfo.SocialEnergy = gameRules.StartingSocialEnergy;
        playerInfo.MaxSocialEnergy = 40;

        playerInfo.Inventory.AddResources(ResourceTypes.Food, 10);

        gameState.Player = playerInfo;


        return gameState;
    }
}
