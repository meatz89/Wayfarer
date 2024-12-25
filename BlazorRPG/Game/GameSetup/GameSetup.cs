public class GameSetup
{
    public static GameState CreateNewGame()
    {
        GameState gameState = new GameState();

        gameState.Locations = new List<Location>
        {
            new Location() { Index = 1, Name = "Docks", Description = "Docks" },
            new Location() { Index = 2, Name = "Market", Description = "Market" }
        };
        gameState.CurrentLocation = "Docks";

        var playerInfo = new PlayerInfo();
        playerInfo.Money = 10;

        playerInfo.Health = 10;
        playerInfo.MaxHealth = 10;

        playerInfo.PhysicalEnergy = 10;
        playerInfo.MaxPhysicalEnergy = 10;

        playerInfo.FocusEnergy = 10;
        playerInfo.MaxFocusEnergy = 10;

        playerInfo.SocialEnergy = 10;
        playerInfo.MaxSocialEnergy = 10;

        gameState.PlayerInfo = playerInfo;

        return gameState;
    }
}
