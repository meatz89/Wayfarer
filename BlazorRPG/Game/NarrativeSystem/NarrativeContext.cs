using System.Text;

/// <summary>
/// Stores the complete narrative context for an encounter
/// </summary>
public class NarrativeContext
{
    public string LocationName { get; }
    public string locationSpotName { get; }
    public CardTypes EncounterType { get; }
    public ActionImplementation ActionImplementation { get; }
    public List<NarrativeEvent> Events { get; } = new List<NarrativeEvent>();
    public PlayerState PlayerState { get; set; }

    public NarrativeContext(
        string location,
        string locationSpot,
        CardTypes encounterType,
        PlayerState playerState,
        ActionImplementation incitingAction)
    {
        LocationName = location;
        locationSpotName = locationSpot;
        EncounterType = encounterType;
        PlayerState = playerState;
        ActionImplementation = incitingAction;
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
        prompt.AppendLine($"Presentation Style: {EncounterType}");
        prompt.AppendLine();

        foreach (NarrativeEvent evt in Events)
        {
            prompt.AppendLine($"--- Turn {evt.TurnNumber} ---");
            prompt.AppendLine("Scene:");
            prompt.AppendLine(evt.Summary);
            prompt.AppendLine();

            if (evt.ChosenOption != null)
            {
                prompt.AppendLine("Player Choice:");
                prompt.AppendLine($"- {evt.ChosenOption.Id}: {evt.ChoiceNarrative}");
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
