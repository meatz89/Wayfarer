using System.Text;
public class NarrativeContext
{
    public string LocationName { get; }
    public string LocationSpotName { get; }
    public SkillCategories SkillCategory { get; }
    public Player PlayerState { get; }
    public ActionImplementation ActionImplementation { get; }
    public ApproachDefinition Approach { get; }

    private List<NarrativeEvent> events = new List<NarrativeEvent>();
    public List<NarrativeEvent> Events
    {
        get
        {
            return events;
        }
    }

    public NarrativeContext(
        string locationName,
        string locationSpotName,
        SkillCategories skillCategory,
        Player playerState,
        ActionImplementation actionImplementation,
        ApproachDefinition approach)
    {
        LocationName = locationName;
        LocationSpotName = locationSpotName;
        SkillCategory = skillCategory;
        PlayerState = playerState;
        ActionImplementation = actionImplementation;
        Approach = approach;

        events = new List<NarrativeEvent>();
    }

    public void AddEvent(NarrativeEvent narrativeEvent)
    {
        events.Add(narrativeEvent);
    }

    public List<NarrativeEvent> GetEvents()
    {
        return events;
    }

    public NarrativeEvent GetLatestEvent()
    {
        if (events.Count > 0)
        {
            return events[events.Count - 1];
        }

        return null;
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

        foreach (NarrativeEvent evt in events)
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
