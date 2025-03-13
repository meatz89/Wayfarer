// Creates concise narrative summaries from context
using BlazorRPG.Game.EncounterManager.NarrativeAi;

public class NarrativeSummaryBuilder
{
    public string CreateSummary(NarrativeContext context)
    {
        // Get only the most recent event to keep things concise
        NarrativeEvent? mostRecentEvent = context.Events
            .OrderByDescending(e => e.TurnNumber)
            .FirstOrDefault();

        if (mostRecentEvent == null)
            return $"I've just {context.IncitingAction} at {context.LocationName}.";

        // Keep the summary extremely focused
        if (mostRecentEvent.TurnNumber == 0)
        {
            // Initial scene
            return mostRecentEvent.SceneDescription.Trim();
        }
        else
        {
            // Latest action and result
            return $"I just {mostRecentEvent.ChosenOption?.Description}. {mostRecentEvent.SceneDescription.Trim()}";
        }
    }
}