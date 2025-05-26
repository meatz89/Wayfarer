using System.Text;

public class NarrativeSummaryBuilder
{
    public string CreateCompleteHistory(NarrativeContext context)
    {
        if (context.Events.Count == 0)
        {
            return $"Beginning a new encounter at {context.LocationName} after {context.ActionImplementation}.";
        }

        StringBuilder history = new StringBuilder();
        history.AppendLine("# Complete Encounter History");
        history.AppendLine($"Location: {context.LocationName} | Encounter Type: {context.SkillCategory} | Goal: {context.ActionImplementation}");
        history.AppendLine();

        // Create detailed history of all events
        for (int i = 0; i < context.Events.Count; i++)
        {
            NarrativeEvent evt = context.Events[i];

            history.AppendLine($"## Turn {evt.Stage}");

            // Add initial scene description
            if (i == 0)
            {
                history.AppendLine("### Initial Scene");
                history.AppendLine(evt.Summary);
                history.AppendLine();
            }
            else
            {
                // For other turns, add chosen option and outcome
                if (evt.ChosenOption != null)
                {
                    history.AppendLine($"### Choice: {evt.ChosenOption.NarrativeText}");

                    // Add outcome
                    if (!string.IsNullOrEmpty(evt.Outcome))
                    {
                        history.AppendLine($"Outcome: {evt.Outcome}");
                    }
                }

                // Add scene description
                history.AppendLine("### Scene");
                history.AppendLine(evt.Summary);
                history.AppendLine();
            }
        }

        return history.ToString();
    }

    public string CreateSummary(NarrativeContext context)
    {
        // Get only the most recent event to keep things concise
        NarrativeEvent? mostRecentEvent = context.Events
            .OrderByDescending(e =>
            {
                return e.Stage;
            })
            .FirstOrDefault();

        if (mostRecentEvent == null)
            return $"{context.ActionImplementation} at {context.LocationName}.";

        // Keep the summary extremely focused
        if (mostRecentEvent.Stage == 0)
        {
            // Initial scene
            return mostRecentEvent.Summary.Trim();
        }
        else
        {
            // Latest action and result
            return $"{mostRecentEvent.ChosenOption?.NarrativeText}. {mostRecentEvent.Summary.Trim()}";
        }
    }
}