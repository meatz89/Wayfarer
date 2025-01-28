public class ChoiceSetSelector
{
    private readonly Random random = new();
    public GameState GameState { get; }

    public ChoiceSetSelector(GameState gameState)
    {
        GameState = gameState;
    }

    public EncounterChoiceTemplate SelectTemplate(List<EncounterChoiceTemplate> templates, EncounterContext context)
    {
        // Get valid sets
        List<EncounterChoiceTemplate> validSets = new();
        foreach (EncounterChoiceTemplate template in templates)
        {
            if (ChoiceMeetsConditions(template, context, GameState))
            {
                validSets.Add(template);
            }
        }

        // Pick randomly from top 2-3 scoring choices
        List<EncounterChoiceTemplate> topChoices = validSets.Take(3).ToList();
        if (topChoices.Count() < 1)
        {
            return null;
        }

        int randomValue = random.Next(topChoices.Count);
        EncounterChoiceTemplate randomTemplate = topChoices[randomValue];

        return randomTemplate;
    }

    private bool ChoiceMeetsConditions(EncounterChoiceTemplate template, EncounterContext encounterContext, GameState gameState)
    {
        // Check action type matches
        if (encounterContext.ActionType != template.ActionType) return false;

        // Check location conditions
        foreach (LocationPropertyCondition condition in template.LocationPropertyConditions)
        {
            if (!condition.IsMet(encounterContext.Location)) return false;
        }

        // Check location spot conditions
        foreach (LocationSpotPropertyCondition condition in template.LocationSpotPropertyConditions)
        {
            if (!condition.IsMet(encounterContext.LocationSpot)) return false;
        }

        // Check world conditions
        foreach (WorldStatePropertyCondition condition in template.WorldStatePropertyConditions)
        {
            if (!condition.IsMet(gameState.World)) return false;
        }

        // Check player conditions
        foreach (PlayerStatusPropertyCondition condition in template.PlayerStatusPropertyConditions)
        {
            if (!condition.IsMet(gameState.Player)) return false;
        }

        // Check state conditions
        foreach (EncounterStateCondition condition in template.StateConditions)
        {
            if (!condition.IsMet(encounterContext.CurrentValues)) return false;
        }

        return true;
    }

}
