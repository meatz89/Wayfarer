public class ActionNameGenerator
{
    private readonly ActionGenerationContext context;
    private readonly ActionNameCombinations actionNameData;

    public ActionNameGenerator(ActionGenerationContext context)
    {
        this.context = context;
        this.actionNameData = new ActionNameCombinations();

        actionNameData.CheckForMissingCombinations();
    }

    public string GenerateName()
    {
        // 1. Determine Location Context
        LocationContext locationContext = LocationContextMapper.GetLocationContext(
            context.LocationType,
            context.Space.Exposure,
            context.Space.Scale);

        // 2. Get Possible Verbs and Adjectives based on LocationContext and BaseAction
        List<ActionNameCombination> matchingCombinations = actionNameData.GetCombinations(locationContext, context.BaseAction);

        if (matchingCombinations.Count == 0)
        {
            return "Unknown Action"; // Or some default action name
        }

        // 3. Choose a Combination (e.g., randomly, based on additional context, or use the first one)
        // For simplicity, we'll just use the first matching combination here
        ActionNameCombination chosenCombination = matchingCombinations[0];

        // 4. Build Name
        List<string> parts = new List<string>();
        if (chosenCombination.Adjective != Adjective.None)
        {
            parts.Add(chosenCombination.Adjective.ToString());
        }
        parts.Add(chosenCombination.Verb.ToString());

        string name = string.Join(" ", parts);

        // 5. Handle Legality (optional)
        if (context.Social.Legality == LegalityTypes.Illegal)
        {
            name = "Stealthily " + name;
        }

        return name;
    }
}

public partial class ActionNameCombinations
{
    public List<ActionNameCombination> GetCombinations(LocationContext context, BasicActionTypes actionType)
    {
        // Returns a list of combinations that match the provided context and action type
        return ValidCombinations.Where(c => c.LocationContext == context && c.BaseAction == actionType).ToList();
    }

    public void CheckForMissingCombinations()
    {
        var allLocationContexts = Enum.GetValues(typeof(LocationContext)).Cast<LocationContext>();
        var allBaseActions = Enum.GetValues(typeof(BasicActionTypes)).Cast<BasicActionTypes>();

        foreach (var locationContext in allLocationContexts)
        {
            foreach (var baseAction in allBaseActions)
            {
                bool found = ValidCombinations.Any(c => c.LocationContext == locationContext && c.BaseAction == baseAction);
                if (!found)
                {
                    Console.WriteLine($"Missing combination: LocationContext={locationContext}, BaseAction={baseAction}");
                }
            }
        }
    }
}
