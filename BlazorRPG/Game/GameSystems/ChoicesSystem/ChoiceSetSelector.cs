public class ChoiceSetSelector
{
    private readonly Random random = new();

    public ChoiceSetTemplate SelectTemplate(List<ChoiceSetTemplate> templates, EncounterContext context)
    {
        // Get valid sets
        List<ChoiceSetTemplate> validSets = templates.Where(t => MeetsConditions(t, context)).ToList();

        // Create ChoiceSetScore for each template and calculate
        var scoredSets = validSets.Select(set => new
        {
            Set = set,
            Score = CreateScoreForTemplate(set).GetContextScore(context)
        }).OrderByDescending(x => x.Score);

        // Pick randomly from top 2-3 scoring sets
        var topSets = scoredSets.Take(3).ToList();
        if (topSets.Count() < 1)
        {
            return null;
        }
        int randomValue = random.Next(topSets.Count);
        return topSets[randomValue].Set;
    }

    private bool MeetsConditions(ChoiceSetTemplate template, EncounterContext encounterContext)
    {
        // Check action type matches
        if (encounterContext.ActionType != template.ActionType) return false;

        // Check location archetype matches
        if (encounterContext.LocationArchetype != template.LocationArchetype) return false;

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

    private ChoiceSetScore CreateScoreForTemplate(ChoiceSetTemplate template)
    {
        // Create a ChoiceSetScore and initialize its properties based on the template
        ChoiceSetScore score = new ChoiceSetScore();
        score.BaseScore = 0; // Can be adjusted based on template properties
        score.ChoicePatterns = template.ChoicePatterns;
        // Set any other needed properties
        return score;
    }
}