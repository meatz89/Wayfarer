public class ChoiceSetSelector
{
    private readonly Random random = new();
    public GameState GameState { get; }

    public ChoiceSetSelector(GameState gameState)
    {
        GameState = gameState;
    }

    public ChoiceSetTemplate SelectTemplate(List<ChoiceSetTemplate> templates, EncounterContext context)
    {
        // Get valid sets
        List<ChoiceSetTemplate> validSets = new();
        foreach (ChoiceSetTemplate template in templates)
        {
            if (MeetsConditions(template, context))
            {
                validSets.Add(template);
            }
        }

        //// Create ChoiceSetScore for each template and calculate
        //var scoredSets = validSets.Select(set => new
        //{
        //    Set = set,
        //    Score = CreateScoreForTemplate(set).GetContextScore(context)
        //}).OrderByDescending(x => x.Score);

        // Pick randomly from top 2-3 scoring sets
        List<ChoiceSetTemplate> topSets = validSets.Take(3).ToList();
        if (topSets.Count() < 1)
        {
            return null;
        }

        int randomValue = random.Next(topSets.Count);
        ChoiceSetTemplate randomTemplate = topSets[randomValue];

        return randomTemplate;
    }

    private bool MeetsConditions(ChoiceSetTemplate template, EncounterContext encounterContext)
    {
        // Check action type matches
        if (encounterContext.ActionType != template.ActionType) return false;

        // Check location conditions
        foreach (LocationPropertyCondition condition in template.AvailabilityConditions)
        {
            if (!condition.IsMet(encounterContext.LocationProperties)) return false;
        }

        // Check state conditions
        foreach (EncounterStateCondition condition in template.StateConditions)
        {
            if (!condition.IsMet(encounterContext.CurrentValues)) return false;
        }

        return true;
    }

}
