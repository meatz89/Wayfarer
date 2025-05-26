using System.Text;

/// <summary>
/// Stores the complete narrative context for an encounter
/// </summary>
public class NarrativeContext
{
    public string LocationName { get; }
    public string LocationSpotName { get; }
    public SkillCategories SkillCategory { get; }
    public ActionImplementation ActionImplementation { get; }
    public ApproachDefinition ChosenApproach { get; }
    public List<NarrativeEvent> Events { get; } = new List<NarrativeEvent>();
    public Player PlayerState { get; set; }

    public NarrativeContext(
        string location,
        string locationSpot,
        SkillCategories SkillCategory,
        Player playerState,
        ActionImplementation incitingAction,
        ApproachDefinition chosenApproach)
    {
        LocationName = location;
        LocationSpotName = locationSpot;
        SkillCategory = SkillCategory;
        PlayerState = playerState;
        ActionImplementation = incitingAction;
        ChosenApproach = chosenApproach;
    }

    public void AddEvent(NarrativeEvent narrativeEvent)
    {
        Events.Add(narrativeEvent);
    }

    public string GetLastScene()
    {
        return Events.Count > 0 ? Events[Events.Count - 1].Summary : string.Empty;
    }

    /// <summary>
    /// Convert the full narrative context to a prompt for the AI
    /// </summary>
    public string ToPrompt()
    {
        StringBuilder prompt = new StringBuilder();
        prompt.AppendLine($"Location: {LocationName}");
        prompt.AppendLine($"Inciting Action: {ActionImplementation}");
        prompt.AppendLine($"Presentation Style: {SkillCategory}");
        prompt.AppendLine();

        foreach (NarrativeEvent evt in Events)
        {
            prompt.AppendLine($"--- Turn {evt.Stage} ---");
            prompt.AppendLine("Scene:");
            prompt.AppendLine(evt.Summary);
            prompt.AppendLine();

            if (evt.ChosenOption != null)
            {
                prompt.AppendLine("Player Choice:");
                prompt.AppendLine($"- {evt.ChosenOption.ChoiceID}: {evt.ChosenOption.NarrativeText}");
                prompt.AppendLine();

                if (!string.IsNullOrEmpty(evt.Outcome))
                {
                    prompt.AppendLine("Outcome:");
                    prompt.AppendLine(evt.Outcome);
                    prompt.AppendLine();
                }
            }
        }

        return prompt.ToString();
    }
}
