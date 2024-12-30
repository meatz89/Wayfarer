public class SmokeTests : IClassFixture<BlazorRPGFixture>
{
    public GameContentProvider GameContentProvider;

    public NarrativeSystem NarrativeSystem;
    public LocationSystem LocationSystem;

    public GameState GameState;
    public ActionManager ActionManager;

    public SmokeTests(BlazorRPGFixture fixture)
    {
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
            GameTestHelpers.AdvanceToTimeWindow(ActionManager, TimeWindows.Morning);
        }

        Assert.True(GameState.Player.MinHealth >= GameState.Player.Health);
    }

    [Fact]
    public void Can_Generate_Basic_Resources()
    {
        // Test dock labor
        ActionManager.MoveToLocation(LocationNames.Docks);
        GameTestHelpers.AdvanceToTimeWindow(ActionManager, TimeWindows.Morning);
        int initialCoins = GameState.Player.Coins;
        GameTestHelpers.ExecuteActionSequence(ActionManager, BasicActionTypes.Labor);

        Assert.True(GameState.Player.Coins > initialCoins);

        // Test forest gathering
        ActionManager.MoveToLocation(LocationNames.DarkForest);
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
            ActionManager.MoveToLocation(LocationNames.Docks);

            GameTestHelpers.AdvanceToTimeWindow(ActionManager, TimeWindows.Morning);
            GameTestHelpers.ExecuteActionSequence(ActionManager, BasicActionTypes.Labor);

            // Afternoon market food purchase
            ActionManager.MoveToLocation(LocationNames.MarketSquare);

            GameTestHelpers.ExecuteActionSequence(ActionManager, BasicActionTypes.Trade);

            // Evening tavern shelter
            ActionManager.MoveToLocation(LocationNames.LionsHeadTavern);

            GameTestHelpers.AdvanceToTimeWindow(ActionManager, TimeWindows.Evening);
            GameTestHelpers.ExecuteActionSequence(ActionManager, BasicActionTypes.Rest);
        }

        Assert.True(GameState.Player.Health > GameState.Player.MinHealth);
    }

    [Fact]
    public void Can_Survive_Through_Gathering_Path()
    {
        for (int day = 0; day < 3; day++)
        {
            // Morning gathering
            ActionManager.MoveToLocation(LocationNames.DarkForest);
            GameTestHelpers.AdvanceToTimeWindow(ActionManager, TimeWindows.Morning);
            GameTestHelpers.ExecuteActionSequence(ActionManager, BasicActionTypes.Gather);

            // Afternoon market trading
            ActionManager.MoveToLocation(LocationNames.MarketSquare);
            GameTestHelpers.ExecuteActionSequence(ActionManager, BasicActionTypes.Trade);

            // Evening rest at bad shelter
            ActionManager.MoveToLocation(LocationNames.HarborStreets);
            GameTestHelpers.AdvanceToTimeWindow(ActionManager, TimeWindows.Evening);
            GameTestHelpers.ExecuteActionSequence(ActionManager, BasicActionTypes.Rest);
        }
        Assert.True(GameState.Player.Health > GameState.Player.MinHealth);
    }

    [Fact]
    public void Energy_Depletes_Correctly()
    {
        ActionManager.MoveToLocation(LocationNames.Docks);
        GameTestHelpers.AdvanceToTimeWindow(ActionManager, TimeWindows.Morning);

        int initialEnergy = GameState.Player.PhysicalEnergy;
        GameTestHelpers.ExecuteActionSequence(ActionManager, BasicActionTypes.Labor);

        Assert.True(GameState.Player.PhysicalEnergy < initialEnergy);
    }

    [Fact]
    public void Good_Shelter_Recovers_Energy()
    {
        // Deplete energy first
        ActionManager.MoveToLocation(LocationNames.Docks);
        GameTestHelpers.AdvanceToTimeWindow(ActionManager, TimeWindows.Morning);
        GameTestHelpers.ExecuteActionSequence(ActionManager, BasicActionTypes.Labor);

        int depletedEnergy = GameState.Player.PhysicalEnergy;

        // Rest at good shelter
        ActionManager.MoveToLocation(LocationNames.LionsHeadTavern);
        GameTestHelpers.AdvanceToTimeWindow(ActionManager, TimeWindows.Evening);
        GameTestHelpers.ExecuteActionSequence(ActionManager, BasicActionTypes.Rest);

        Assert.True(GameState.Player.PhysicalEnergy > depletedEnergy);
    }

    [Fact]
    public void Bad_Shelter_No_Energy_Recovery()
    {
        // Deplete energy first
        ActionManager.MoveToLocation(LocationNames.Docks);
        GameTestHelpers.AdvanceToTimeWindow(ActionManager, TimeWindows.Morning);
        GameTestHelpers.ExecuteActionSequence(ActionManager, BasicActionTypes.Labor);

        int depletedEnergy = GameState.Player.PhysicalEnergy;

        // Rest at bad shelter
        ActionManager.MoveToLocation(LocationNames.HarborStreets);
        GameTestHelpers.AdvanceToTimeWindow(ActionManager, TimeWindows.Evening);
        GameTestHelpers.ExecuteActionSequence(ActionManager, BasicActionTypes.Rest);

        Assert.Equal(depletedEnergy, GameState.Player.PhysicalEnergy);
    }

    [Fact]
    public void Cannot_Execute_Actions_Without_Energy()
    {
        // Deplete physical energy
        ActionManager.MoveToLocation(LocationNames.Docks);
        while (GameState.Player.PhysicalEnergy > 0)
        {
            GameTestHelpers.ExecuteActionSequence(ActionManager, BasicActionTypes.Labor);
        }

        // Try to execute another labor action
        var action = GameState.ValidUserActions
            .First(a => a.BasicAction.ActionType == BasicActionTypes.Labor).BasicAction;
        var result = ActionManager.ExecuteBasicAction(action);

        Assert.False(result.IsSuccess);
    }
}
