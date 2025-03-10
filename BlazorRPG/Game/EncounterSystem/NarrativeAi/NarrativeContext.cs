using System.Text;

namespace BlazorRPG.Game.EncounterManager.NarrativeAi
{
    /// <summary>
    /// Stores the complete narrative context for an encounter
    /// </summary>
    public class NarrativeContext
    {
        public string Location { get; }
        public string IncitingAction { get; }
        public List<NarrativeEvent> Events { get; } = new List<NarrativeEvent>();
        public PresentationStyles CurrentStyle { get; set; }

        public NarrativeContext(string location, string incitingAction, PresentationStyles style)
        {
            Location = location;
            IncitingAction = incitingAction;
            CurrentStyle = style;
        }

        public void AddEvent(NarrativeEvent narrativeEvent)
        {
            Events.Add(narrativeEvent);
        }

        public string GetLastScene()
        {
            return Events.Count > 0 ? Events[Events.Count - 1].SceneDescription : string.Empty;
        }

        /// <summary>
        /// Convert the full narrative context to a prompt for the AI
        /// </summary>
        public string ToPrompt()
        {
            StringBuilder prompt = new StringBuilder();
            prompt.AppendLine($"Location: {Location}");
            prompt.AppendLine($"Inciting Action: {IncitingAction}");
            prompt.AppendLine($"Presentation Style: {CurrentStyle}");
            prompt.AppendLine();

            foreach (NarrativeEvent evt in Events)
            {
                prompt.AppendLine($"--- Turn {evt.TurnNumber} ---");
                prompt.AppendLine("Scene:");
                prompt.AppendLine(evt.SceneDescription);
                prompt.AppendLine();

                if (evt.ChosenOption != null)
                {
                    prompt.AppendLine("Player Choice:");
                    prompt.AppendLine($"- {evt.ChosenOption.Name}: {evt.ChosenOptionDescription}");
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
}
