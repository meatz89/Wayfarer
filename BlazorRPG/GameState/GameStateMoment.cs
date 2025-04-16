public class GameStateMoment
{
    public PlayerStateMoment PlayerState { get; }

    public GameStateMoment(GameState gameState)
    {
        PlayerState = new PlayerStateMoment(gameState.PlayerState);
    }
}
