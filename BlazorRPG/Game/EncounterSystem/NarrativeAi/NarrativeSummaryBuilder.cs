public class NarrativeSummaryBuilder
{
    public string CreateSummary(NarrativeContext context)
    {
        // Get only the most recent event to keep things concise
        NarrativeEvent? mostRecentEvent = context.Events
            .OrderByDescending(e => e.TurnNumber)
            .FirstOrDefault();

        if (mostRecentEvent == null)
            return $"{context.ActionImplementation} at {context.LocationName}.";

        // Keep the summary extremely focused
        if (mostRecentEvent.TurnNumber == 0)
        {
            // Initial scene
            return mostRecentEvent.SceneDescription.Trim();
        }
        else
        {
            // Latest action and result
            return $"{mostRecentEvent.ChosenOption?.Description}. {mostRecentEvent.SceneDescription.Trim()}";
        }
    }
}