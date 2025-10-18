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
        if (_gameWorld.StandingObligationTemplates == null)
        {throw new InvalidOperationException("StandingObligationTemplates not initialized - data loading failed");
        }
        return _gameWorld.StandingObligationTemplates;
    }

}