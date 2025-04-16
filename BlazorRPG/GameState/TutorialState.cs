public class TutorialState
{
    public TutorialObjective CurrentObjective { get; private set; } = TutorialObjective.ExploreClearing;
    private Dictionary<TutorialFlags, bool> tutorialFlagProgress = new Dictionary<TutorialFlags, bool>();
    private Dictionary<TutorialObjective, bool> completedObjectives = new Dictionary<TutorialObjective, bool>();

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

    public bool CheckFlag(TutorialFlags flagName)
    {
        if (!tutorialFlagProgress.ContainsKey(flagName))
            tutorialFlagProgress[flagName] = false;
        return tutorialFlagProgress[flagName];
    }

    public void SetFlag(TutorialFlags flagName)
    {
        tutorialFlagProgress[flagName] = true;
    }

    public void AdvanceToObjective(TutorialObjective newObjective)
    {
        // Only advance if it's the next objective in sequence
        if (newObjective == GetNextObjective(CurrentObjective))
        {
            completedObjectives[CurrentObjective] = true;
            CurrentObjective = newObjective;
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