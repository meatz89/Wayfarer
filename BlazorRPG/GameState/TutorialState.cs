public class TutorialState
{
    // Current tutorial objective
    public TutorialObjective CurrentObjective { get; private set; } = TutorialObjective.ExploreClearing;

    // Flags for tutorial progression
    private Dictionary<TutorialFlags, bool> TutorialFlagProgress { get; set; } = new Dictionary<TutorialFlags, bool>();

    // Tutorial objectives in sequence
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

    // Check if a tutorial flag is set
    public bool CheckFlag(TutorialFlags flagName)
    {
        if (!TutorialFlagProgress.ContainsKey(flagName))
            TutorialFlagProgress[flagName] = false;
        return TutorialFlagProgress[flagName];
    }

    // Set a tutorial flag
    public void SetFlag(TutorialFlags flagName)
    {
        TutorialFlagProgress[flagName] = true;
        UpdateObjectiveProgress();
    }

    // Update tutorial objective based on flags
    private void UpdateObjectiveProgress()
    {
        switch (CurrentObjective)
        {
            case TutorialObjective.ExploreClearing:
                if (CheckFlag(TutorialFlags.FoundStream))
                    CurrentObjective = TutorialObjective.FindStream;
                break;

            case TutorialObjective.FindStream:
                if (CheckFlag(TutorialFlags.VisitedStream))
                    CurrentObjective = TutorialObjective.GatherHerbs;
                break;

            case TutorialObjective.GatherHerbs:
                if (CheckFlag(TutorialFlags.GatheredHerbs))
                    CurrentObjective = TutorialObjective.UseHerbs;
                break;

            case TutorialObjective.UseHerbs:
                if (CheckFlag(TutorialFlags.UsedHerbs))
                    CurrentObjective = TutorialObjective.FindFood;
                break;

            case TutorialObjective.FindFood:
                if (CheckFlag(TutorialFlags.GatheredFood))
                    CurrentObjective = TutorialObjective.EatFood;
                break;

            case TutorialObjective.EatFood:
                if (CheckFlag(TutorialFlags.UsedFood))
                    CurrentObjective = TutorialObjective.ReachHighGround;
                break;

            case TutorialObjective.ReachHighGround:
                if (CheckFlag(TutorialFlags.VisitedHighGround))
                    CurrentObjective = TutorialObjective.FindPathOut;
                break;

            case TutorialObjective.FindPathOut:
                if (CheckFlag(TutorialFlags.FoundPathOut))
                    CurrentObjective = TutorialObjective.TutorialComplete;
                break;
        }
    }

    // Get the current objective description
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

    // Get an optional hint for the current objective
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