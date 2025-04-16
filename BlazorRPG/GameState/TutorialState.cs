public class TutorialState
{
    public TutorialObjective CurrentObjective { get; private set; } = TutorialObjective.ExploreClearing;
    private Dictionary<TutorialObjective, bool> completedObjectives = new Dictionary<TutorialObjective, bool>();
    private readonly List<TutorialCondition> conditions = new List<TutorialCondition>();

    // For backward compatibility
    private Dictionary<TutorialFlags, bool> tutorialFlagProgress = new Dictionary<TutorialFlags, bool>();

    public enum TutorialObjective
    {
        ExploreClearing,
        FindStream,
        GatherHerbs,
        UseHerbs,
        FindFood,
        EatFood,
        ReachHighGround,
        FindPathOut,
        TutorialComplete
    }

    public enum TutorialFlags
    {
        FoundStream,
        VisitedStream,
        GatheredHerbs,
        UsedHerbs,
        GatheredFood,
        UsedFood,
        VisitedHighGround,
        FoundPathOut,
        Rested,
        OutOfFood,
        OutOfHerbs,
        TutorialStarted
    }

    public TutorialState()
    {
        InitializeConditions();
    }

    private void InitializeConditions()
    {
        // Define progression conditions based on game state
        conditions.Add(new TutorialCondition(
            state => state.PlayerState.KnownLocationSpots.Contains("Forest Stream"),
            TutorialObjective.FindStream
        ));

        conditions.Add(new TutorialCondition(
            state => state.WorldState.CurrentLocationSpot?.Name == "Forest Stream",
            TutorialObjective.GatherHerbs
        ));

        conditions.Add(new TutorialCondition(
            state => state.PlayerState.MedicinalHerbs > 0,
            TutorialObjective.UseHerbs
        ));

        conditions.Add(new TutorialCondition(
            state => state.ActionStateTracker.PreviousState != null &&
                   state.PlayerState.MedicinalHerbs < state.ActionStateTracker.PreviousState.PlayerState.MedicinalHerbs,
            TutorialObjective.FindFood
        ));

        conditions.Add(new TutorialCondition(
            state => state.PlayerState.Food > 0,
            TutorialObjective.EatFood
        ));

        conditions.Add(new TutorialCondition(
            state => state.ActionStateTracker.PreviousState != null &&
                   state.PlayerState.Food < state.ActionStateTracker.PreviousState.PlayerState.Food,
            TutorialObjective.ReachHighGround
        ));

        conditions.Add(new TutorialCondition(
            state => state.WorldState.CurrentLocationSpot?.Name == "High Ground",
            TutorialObjective.FindPathOut
        ));

        conditions.Add(new TutorialCondition(
            state => state.WorldState.IsEncounterCompleted("FindPathOut"),
            TutorialObjective.TutorialComplete
        ));
    }

    public void CheckConditions(GameState gameState)
    {
        // Check all conditions that lead to objective unlocks
        foreach (var condition in conditions)
        {
            // Skip if this would lead to an already completed objective
            if (completedObjectives.ContainsKey(condition.UnlocksObjective))
                continue;

            // Check if condition is met
            if (condition.Check(gameState))
            {
                // If this is the next objective in sequence, update current objective
                if (condition.UnlocksObjective == GetNextObjective(CurrentObjective))
                {
                    completedObjectives[CurrentObjective] = true;
                    CurrentObjective = condition.UnlocksObjective;

                    // Set corresponding flags for backward compatibility
                    UpdateFlagsForObjective(CurrentObjective);
                }
            }
        }
    }

    private void UpdateFlagsForObjective(TutorialObjective objective)
    {
        // Set appropriate flags for each objective for backward compatibility
        switch (objective)
        {
            case TutorialObjective.FindStream:
                SetFlag(TutorialFlags.FoundStream);
                break;
            case TutorialObjective.GatherHerbs:
                SetFlag(TutorialFlags.VisitedStream);
                break;
            case TutorialObjective.UseHerbs:
                SetFlag(TutorialFlags.GatheredHerbs);
                break;
            case TutorialObjective.FindFood:
                SetFlag(TutorialFlags.UsedHerbs);
                break;
            case TutorialObjective.EatFood:
                SetFlag(TutorialFlags.GatheredFood);
                break;
            case TutorialObjective.ReachHighGround:
                SetFlag(TutorialFlags.UsedFood);
                break;
            case TutorialObjective.FindPathOut:
                SetFlag(TutorialFlags.VisitedHighGround);
                break;
            case TutorialObjective.TutorialComplete:
                SetFlag(TutorialFlags.FoundPathOut);
                break;
        }
    }

    private TutorialObjective GetNextObjective(TutorialObjective current)
    {
        return current switch
        {
            TutorialObjective.ExploreClearing => TutorialObjective.FindStream,
            TutorialObjective.FindStream => TutorialObjective.GatherHerbs,
            TutorialObjective.GatherHerbs => TutorialObjective.UseHerbs,
            TutorialObjective.UseHerbs => TutorialObjective.FindFood,
            TutorialObjective.FindFood => TutorialObjective.EatFood,
            TutorialObjective.EatFood => TutorialObjective.ReachHighGround,
            TutorialObjective.ReachHighGround => TutorialObjective.FindPathOut,
            TutorialObjective.FindPathOut => TutorialObjective.TutorialComplete,
            _ => TutorialObjective.TutorialComplete
        };
    }

    // Maintained for backward compatibility
    public bool CheckFlag(TutorialFlags flagName)
    {
        if (!tutorialFlagProgress.ContainsKey(flagName))
            tutorialFlagProgress[flagName] = false;
        return tutorialFlagProgress[flagName];
    }

    // Maintained for backward compatibility
    public void SetFlag(TutorialFlags flagName)
    {
        tutorialFlagProgress[flagName] = true;
    }

    public string GetCurrentObjectiveText()
    {
        return CurrentObjective switch
        {
            TutorialObjective.ExploreClearing => "Explore the forest clearing to find a way forward",
            TutorialObjective.FindStream => "Make your way to the forest stream",
            TutorialObjective.GatherHerbs => "Search for medicinal herbs along the stream",
            TutorialObjective.UseHerbs => "Use the medicinal herbs to restore health",
            TutorialObjective.FindFood => "Forage for food to maintain your energy",
            TutorialObjective.EatFood => "Consume food to restore energy",
            TutorialObjective.ReachHighGround => "Find high ground to survey the area",
            TutorialObjective.FindPathOut => "Find the path out of the forest before nightfall",
            TutorialObjective.TutorialComplete => "You've completed the tutorial! Explore freely.",
            _ => "Explore the forest"
        };
    }

    public string GetCurrentHint()
    {
        return CurrentObjective switch
        {
            TutorialObjective.ExploreClearing => "Try using the 'Search Surroundings' action",
            TutorialObjective.FindStream => "Look for a path leading to water",
            TutorialObjective.GatherHerbs => "Use the 'Gather Herbs' action at the stream",
            TutorialObjective.UseHerbs => "Click the 'Use' button next to Herbs in your inventory",
            TutorialObjective.FindFood => "Look for berries and roots in the forest clearing",
            TutorialObjective.EatFood => "Click the 'Use' button next to Food in your inventory",
            TutorialObjective.ReachHighGround => "Find a path leading upward for a better view",
            TutorialObjective.FindPathOut => "Use the 'Find Path Out' action at the forest edge",
            _ => ""
        };
    }
}