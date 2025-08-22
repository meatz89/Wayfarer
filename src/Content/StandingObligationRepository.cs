public class StandingObligationRepository
{
    private readonly GameWorld _gameWorld;

    public StandingObligationRepository(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }

    // Get all obligation templates available in the game
    public List<StandingObligation> GetAllObligationTemplates()
    {
        return _gameWorld.WorldState.StandingObligationTemplates ?? new List<StandingObligation>();
    }

}