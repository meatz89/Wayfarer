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
        LocationSpotNames locationSpotName = context.LocationSpotName;

        // 2. Get Verb based on Location Context, BaseAction, Complexity, and Tension
        Verb? verb = actionNameData.GetVerb(locationSpotName, context.BaseAction, context.Activity.Complexity, context.Social.Tension);

        // 3. Use Default Verb if no specific verb is found
        if (verb == null || verb == Verb.None)
        {
            verb = locationSpotName.DefaultVerb();
        }

        // 4. Get Adjective based on context (optional)
        Adjective? adjective = actionNameData.GetAdjective(context);

        // 5. Build Name
        List<string> parts = new List<string>();
        if (adjective.HasValue && adjective != Adjective.None)
        {
            parts.Add(adjective.Value.ToString());
        }
        if (verb.HasValue)
        {
            parts.Add(verb.Value.ToString());
        }

        string name = string.Join(" ", parts);

        // 6. Handle Legality (optional)
        if (context.Social.Legality == LegalityTypes.Illegal)
        {
            name = "Stealthily " + name;
        }

        return name;
    }
}
