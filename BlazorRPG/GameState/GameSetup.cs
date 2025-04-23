public class GameSetup
{
    public static GameState CreateNewGame()
    {
        GameState gameState = new GameState();
        GameRules gameRules = GameRules.StandardRuleset;

        PlayerState playerInfo = gameState.PlayerState;
        playerInfo.Coins = gameRules.StartingCoins;

        gameState.PlayerState = playerInfo;

        return gameState;
    }
}
