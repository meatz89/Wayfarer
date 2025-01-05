public partial class ActionNameCombinations
{
    public List<ActionNameCombination> GetCombinations(LocationSpotNames context, BasicActionTypes actionType)
    {
        // Returns a list of combinations that match the provided context and action type
        return ValidCombinations.Where(c => c.LocationContext == context && c.BaseAction == actionType).ToList();
    }

    public Verb? GetVerb(LocationSpotNames locationContext, BasicActionTypes baseAction, ComplexityTypes? complexity = null, TensionState? tension = null)
    {
        foreach (var combo in ValidCombinations)
        {
            if (combo.LocationContext == locationContext &&
                combo.BaseAction == baseAction &&
                (combo.Complexity == null || combo.Complexity == complexity) &&
                (combo.Tension == null || combo.Tension == tension))
            {
                return combo.Verb;
            }
        }
        return null; // Or a default verb
    }

    public Adjective? GetAdjective(ActionGenerationContext context)
    {
        foreach (var combo in ValidCombinations)
        {
            if ((combo.Tension == null || combo.Tension == context.Social.Tension) &&
                (combo.Complexity == null || combo.Complexity == context.Activity.Complexity))
            {
                return combo.Adjective;
            }
        }
        return Adjective.None;
    }

    public void CheckForMissingCombinations()
    {
        var allLocationContexts = Enum.GetValues(typeof(LocationSpotNames)).Cast<LocationSpotNames>();
        var allBaseActions = Enum.GetValues(typeof(BasicActionTypes)).Cast<BasicActionTypes>();
        var allComplexities = Enum.GetValues(typeof(ComplexityTypes)).Cast<ComplexityTypes>();
        var allTensions = Enum.GetValues(typeof(TensionState)).Cast<TensionState>();

        foreach (var locationContext in allLocationContexts)
        {
            foreach (var baseAction in allBaseActions)
            {
                foreach (var complexity in allComplexities)
                {
                    foreach (var tension in allTensions)
                    {
                        bool found = ValidCombinations.Any(c => c.LocationContext == locationContext &&
                                                               c.BaseAction == baseAction &&
                                                               (c.Complexity == null || c.Complexity == complexity) &&
                                                               (c.Tension == null || c.Tension == tension));
                        if (!found)
                        {
                            // Only print if it's a combination we haven't explicitly excluded
                            if (!ShouldSkipCombination(locationContext, baseAction, complexity, tension))
                            {
                                Console.WriteLine($"Missing combination: LocationContext={locationContext}, BaseAction={baseAction}, Complexity={complexity}, Tension={tension}");
                            }
                        }
                    }
                }
            }
        }
    }

    // Add logic to exclude combinations that are not relevant based on your game's rules
    private bool ShouldSkipCombination(LocationSpotNames locationContext, BasicActionTypes baseAction, ComplexityTypes complexity, TensionState tension)
    {
        // Example: Skip combinations where Labor is attempted in a Field
        if (locationContext == LocationSpotNames.Field && baseAction == BasicActionTypes.Labor)
        {
            return true;
        }

        // Example: Skip combinations where Trade is attempted in a Forest
        if (locationContext == LocationSpotNames.Forest && baseAction == BasicActionTypes.Trade)
        {
            return true;
        }

        // Example: Skip combinations where the action is Wait or Rest and the tension is Hostile
        if ((baseAction == BasicActionTypes.Wait || baseAction == BasicActionTypes.Rest) && tension == TensionState.Hostile)
        {
            return true;
        }
        // Add more skip conditions here based on your game's logic

        return false; // Do not skip by default
    }
}