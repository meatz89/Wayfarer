namespace TestProject1
{
    public class BlazorRPGFixture : IDisposable
    {
        public GameContentProvider GameContentProvider;
        public GameState GameState;

        public NarrativeSystem NarrativeSystem;
        public LocationSystem LocationSystem;

        public ActionManager ActionManager;

        public BlazorRPGFixture()
        {
            GameContentProvider = new GameContentProvider();

            GameState = GameSetup.CreateNewGame();

            NarrativeSystem = new NarrativeSystem(GameState, GameContentProvider);
            LocationSystem = new LocationSystem(GameState, GameContentProvider);

            ActionManager = new ActionManager(GameState, NarrativeSystem, LocationSystem);
            ActionManager.Initialize();
        }

        public void Dispose()
        {
            // clean up test data from the database
        }
    }
}