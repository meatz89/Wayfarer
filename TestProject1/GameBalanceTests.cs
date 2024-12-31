using FluentAssertions;
using Xunit.Abstractions;

public class GameBalanceTests : IClassFixture<BlazorRPGFixture>
{
    private readonly ITestOutputHelper output;
    private ActionManager ActionManager;
    private GameState GameState;

    public GameBalanceTests(BlazorRPGFixture fixture, ITestOutputHelper output)
    {
        this.output = output;

        GameContentProvider GameContentProvider = new GameContentProvider();

        GameState = GameSetup.CreateNewGame();

        NarrativeSystem NarrativeSystem = new NarrativeSystem(GameState, GameContentProvider);
        LocationSystem LocationSystem = new LocationSystem(GameState, GameContentProvider);
        CharacterSystem CharacterSystem = new CharacterSystem(GameState, GameContentProvider);

        ActionManager = new ActionManager(GameState, NarrativeSystem, LocationSystem, CharacterSystem);
        ActionManager.Initialize();
    }


    [Fact]
    public void SubOptimal_Path_Leads_To_Death()
    {
        int startingHealth = GameState.Player.Health;
        ActionManager.MoveToLocation(LocationNames.ForestEdge);

        List<string> healthLog = new List<string>();

        for (int day = 0; day < 3; day++)
        {
            // Track daily stats
            healthLog.Add($"Day {day} Start - Health: {GameState.Player.Health}, Food: {GameState.Player.Inventory.GetItemCount(ResourceTypes.Food)}");

            // Just gather berries all day without proper shelter
            GameTestHelpers.AdvanceToTimeWindow(ActionManager, TimeWindows.Morning);
            GameTestHelpers.ExecuteActionSequence(ActionManager, BasicActionTypes.Gather);

            healthLog.Add($"Day {day} After Gather - Health: {GameState.Player.Health}, Food: {GameState.Player.Inventory.GetItemCount(ResourceTypes.Food)}");

            // No shelter at night, should lose health
            GameTestHelpers.AdvanceToTimeWindow(ActionManager, TimeWindows.Night);

            healthLog.Add($"Day {day} End - Health: {GameState.Player.Health}, Food: {GameState.Player.Inventory.GetItemCount(ResourceTypes.Food)}");
        }

        Assert.True(GameState.Player.Health < startingHealth,
            $"Health should decrease due to lack of shelter. Health log:\n{string.Join("\n", healthLog)}");
    }


    [Fact]
    public void No_Action_Sequence_Creates_Unwinnable_State()
    {
        // Track resources through worst-case sequence
        List<string> resourceLog = new List<string>();
        string initialState = $"Initial - Health: {GameState.Player.Health}, Food: {GameState.Player.Inventory.GetItemCount(ResourceTypes.Food)}, " +
                          $"Coins: {GameState.Player.Coins}, Energy: {GameState.Player.PhysicalEnergy}";
        resourceLog.Add(initialState);

        // Deplete resources
        ActionManager.MoveToLocation(LocationNames.Docks);
        while (GameState.Player.PhysicalEnergy > 0)
        {
            GameTestHelpers.ExecuteActionSequence(ActionManager, BasicActionTypes.Labor);
            resourceLog.Add($"After Labor - Health: {GameState.Player.Health}, Food: {GameState.Player.Inventory.GetItemCount(ResourceTypes.Food)}, " +
                           $"Coins: {GameState.Player.Coins}, Energy: {GameState.Player.PhysicalEnergy}");
        }

        // Check available actions
        IEnumerable<UserActionOption> validActions = GameState.ValidUserActions
            .Where(a => !a.IsDisabled)
            .Where(a => a.BasicAction.Id != BasicActionTypes.CheckStatus)
            .Where(a => a.BasicAction.Id != BasicActionTypes.Wait);

        string availableActions = string.Join(", ", validActions.Select(a => a.BasicAction.Id));
        Assert.True(validActions.Any(),
            $"No valid actions available after resource depletion.\nResource history:\n{string.Join("\n", resourceLog)}\n" +
            $"Available actions: {availableActions}");
    }


    [Fact]
    public void Every_Action_Can_Be_Taken()
    {
        HashSet<BasicActionTypes> executedActions = new HashSet<BasicActionTypes>();
        List<string> actionLog = new List<string>();

        foreach (LocationNames location in Enum.GetValues<LocationNames>())
        {
            actionLog.Add($"\nTesting {location}:");
            ActionManager.MoveToLocation(location);

            foreach (TimeWindows timeWindow in Enum.GetValues<TimeWindows>())
            {
                GameTestHelpers.AdvanceToTimeWindow(ActionManager, timeWindow);
                // Log ALL actions with their disabled state
                IEnumerable<UserActionOption> allActions = GameState.ValidUserActions
                    .Where(a => a.BasicAction.Id != BasicActionTypes.CheckStatus)
                    .Where(a => a.BasicAction.Id != BasicActionTypes.Travel)
                    .Where(a => a.BasicAction.Id != BasicActionTypes.Wait);

                actionLog.Add($"  {timeWindow} actions:");
                foreach (UserActionOption? action in allActions)
                {
                    actionLog.Add($"    {action.BasicAction.Id} - Disabled: {action.IsDisabled}, " +
                                $"ValidTimeSlots: [{string.Join(",", action.BasicAction.TimeSlots)}], " +
                                $"CurrentTime: {GameState.CurrentTimeSlot}");
                }

                IEnumerable<UserActionOption> availableActions = allActions.Where(a => !a.IsDisabled);
                foreach (UserActionOption? action in availableActions)
                {
                    executedActions.Add(action.BasicAction.Id);
                }
            }
        }

        BasicActionTypes[] expectedActions = new[] {
        BasicActionTypes.Labor,
        BasicActionTypes.Gather,
        BasicActionTypes.Trade,
        BasicActionTypes.Discuss,
        BasicActionTypes.Rest
    };

        IEnumerable<BasicActionTypes> missingActions = expectedActions.Where(a => !executedActions.Contains(a));
        Assert.True(missingActions.Count() == 0,
            $"Missing actions: {string.Join(", ", missingActions)}\n" +
            $"Action log:\n{string.Join("\n", actionLog)}");
    }

    [Fact]
    public void Debug_Market_Actions()
    {
        ActionManager.MoveToLocation(LocationNames.MarketSquare);
        GameTestHelpers.AdvanceToTimeWindow(ActionManager, TimeWindows.Morning);

        foreach (UserActionOption action in GameState.ValidUserActions)
        {
            output.WriteLine($"Action: {action.BasicAction.Id}, " +
                           $"TimeSlots: [{string.Join(",", action.BasicAction.TimeSlots)}], " +
                           $"CurrentTime: {GameState.CurrentTimeSlot}, " +
                           $"IsDisabled: {action.IsDisabled}");
        }

        Assert.True(
            GameState.ValidUserActions.Any(a => !a.IsDisabled && a.BasicAction.Id == BasicActionTypes.Trade),
            "Trade action should be available in morning"
        );
    }

    [Fact]
    public void Sub_Optimal_Play_Leads_To_Death_1()
    {
        // Test 1: Only doing labor without buying food
        ActionManager.MoveToLocation(LocationNames.Docks);
        for (int i = 0; i < 5; i++)
        {
            GameTestHelpers.AdvanceToTimeWindow(ActionManager, TimeWindows.Morning);
            GameTestHelpers.ExecuteActionSequence(ActionManager, BasicActionTypes.Labor);
        }
        Assert.True(GameState.Player.Health <= GameState.Player.MinHealth);
    }


    [Fact]
    public void Sub_Optimal_Play_Leads_To_Death_2()
    {
        // Test 2: Only gathering without shelter
        ActionManager.MoveToLocation(LocationNames.ForestEdge);
        for (int i = 0; i < 5; i++)
        {
            GameTestHelpers.AdvanceToTimeWindow(ActionManager, TimeWindows.Morning);
            GameTestHelpers.ExecuteActionSequence(ActionManager, BasicActionTypes.Gather);
        }
        Assert.True(GameState.Player.Health <= GameState.Player.MinHealth);
    }

    [Fact]
    public void Always_Has_Valid_Action_Available()
    {
        // For each location and time window, player should either have:
        // 1. A valid gameplay action OR
        // 2. The ability to travel to a location that does have valid actions
        foreach (LocationNames location in Enum.GetValues<LocationNames>())
        {
            ActionManager.MoveToLocation(location);
            foreach (TimeWindows timeWindow in Enum.GetValues<TimeWindows>())
            {
                GameTestHelpers.AdvanceToTimeWindow(ActionManager, timeWindow);

                // Count gameplay actions
                List<UserActionOption> validGameplayActions = GameState.ValidUserActions
                    .Where(a => !a.IsDisabled)
                    .Where(a => a.BasicAction.Id != BasicActionTypes.Travel)
                    .Where(a => a.BasicAction.Id != BasicActionTypes.Wait)
                    .Where(a => a.BasicAction.Id != BasicActionTypes.CheckStatus)
                    .ToList();

                // If no valid gameplay actions, verify we can travel somewhere
                if (validGameplayActions.Count == 0)
                {
                    bool canTravel = GameState.ValidUserActions
                        .Any(a => a.BasicAction.Id == BasicActionTypes.Travel && !a.IsDisabled);

                    Assert.True(canTravel,
                        $"No valid actions or travel options at {location} during {timeWindow}");
                }
            }
        }
    }

    [Fact]
    public void All_Locations_Reachable()
    {
        LocationNames[] allLocations = Enum.GetValues<LocationNames>();
        HashSet<LocationNames> reachedLocations = new();
        Queue<LocationNames> toVisit = new();
        toVisit.Enqueue(GameState.CurrentLocation);

        while (toVisit.Count > 0)
        {
            LocationNames location = toVisit.Dequeue();
            if (reachedLocations.Add(location))
            {
                ActionManager.MoveToLocation(location);
                List<LocationNames> connections = ActionManager.GetConnectedLocations();
                foreach (LocationNames connection in connections)
                {
                    toVisit.Enqueue(connection);
                }
            }
        }

        Assert.Equal(allLocations.Length, reachedLocations.Count);
    }

}