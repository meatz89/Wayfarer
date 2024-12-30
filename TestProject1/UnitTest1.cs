
namespace TestProject1
{
    public class UnitTest1 : IClassFixture<BlazorRPGFixture>
    {
        BlazorRPGFixture Fixture;

        public GameContentProvider GameContentProvider;
        public GameState GameState;

        public NarrativeSystem NarrativeSystem;
        public LocationSystem LocationSystem;

        public ActionManager ActionManager;

        public UnitTest1(BlazorRPGFixture fixture)
        {
            this.Fixture = fixture;

            GameContentProvider = new GameContentProvider();

            GameState = GameSetup.CreateNewGame();

            NarrativeSystem = new NarrativeSystem(GameState, GameContentProvider);
            LocationSystem = new LocationSystem(GameState, GameContentProvider);

            ActionManager = new ActionManager(GameState, NarrativeSystem, LocationSystem);
            ActionManager.Initialize();
        }

        [Fact]
        public void Player_Dies_Without_Food()
        {
            // Advance through days without eating
            for (int i = 0; i < 5; i++)
            {
                GameTestHelpers.AdvanceToTimeWindow(ActionManager, TimeWindows.Night);
            }

            Assert.Equal(GameState.Player.MinHealth, GameState.Player.Health);
        }

        [Fact]
        public void Player_Dies_Without_Shelter()
        {
            // Stay in location without shelter for multiple nights
            GameState.SetCurrentLocation(LocationNames.Docks);
            for (int i = 0; i < 5; i++)
            {
                GameTestHelpers.AdvanceToTimeWindow(ActionManager, TimeWindows.Night);
            }

            Assert.Equal(GameState.Player.MinHealth, GameState.Player.Health);
        }

        [Fact]
        public void Can_Generate_Basic_Resources()
        {
            // Test dock labor
            GameState.SetCurrentLocation(LocationNames.Docks);
            GameTestHelpers.AdvanceToTimeWindow(ActionManager, TimeWindows.Morning);
            int initialCoins = GameState.Player.Coins;
            GameTestHelpers.ExecuteActionSequence(ActionManager, BasicActionTypes.Labor);
            Assert.True(GameState.Player.Coins > initialCoins);

            // Test forest gathering
            GameState.SetCurrentLocation(LocationNames.DarkForest);
            int initialFood = GameState.Player.Inventory.Food;
            GameTestHelpers.ExecuteActionSequence(ActionManager, BasicActionTypes.Gather);

            Assert.True(GameState.Player.Inventory.Food > initialFood);
        }

        [Fact]
        public void Can_Survive_Through_Labor_Path()
        {
            // Execute optimal labor->buy food->shelter sequence
            for (int day = 0; day < 3; day++)
            {
                // Morning labor at docks
                GameState.SetCurrentLocation(LocationNames.Docks);
                GameTestHelpers.AdvanceToTimeWindow(ActionManager, TimeWindows.Morning);
                GameTestHelpers.ExecuteActionSequence(ActionManager, BasicActionTypes.Labor);

                // Afternoon market food purchase
                GameState.SetCurrentLocation(LocationNames.MarketSquare);
                GameTestHelpers.ExecuteActionSequence(ActionManager, BasicActionTypes.Trade);

                // Evening tavern shelter
                GameState.SetCurrentLocation(LocationNames.LionsHeadTavern);
                GameTestHelpers.AdvanceToTimeWindow(ActionManager, TimeWindows.Evening);
                GameTestHelpers.ExecuteActionSequence(ActionManager, BasicActionTypes.Rest);
            }

            Assert.True(GameState.Player.Health > GameState.Player.MinHealth);
        }

    }
}
