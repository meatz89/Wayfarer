namespace BlazorRPG.Game.EncounterManager
{
    /// <summary>
    /// Formats player choices and encounter state in different narrative styles
    /// </summary>
    public class NarrativePresenter
    {
        // Format a choice description based on the presentation style
        public string FormatChoiceDescription(IChoice choice, PresentationStyles style)
        {
            string baseDescription = GetBaseDescription(choice);

            switch (style)
            {
                case PresentationStyles.Social:
                    return FormatAsSocialDialogue(choice, baseDescription);
                case PresentationStyles.Intellectual:
                    return FormatAsInternalMonologue(choice, baseDescription);
                case PresentationStyles.Physical:
                    return FormatAsActionDescription(choice, baseDescription);
                default:
                    return baseDescription;
            }
        }

        private string GetBaseDescription(IChoice choice)
        {
            // This would be a basic description of what the choice accomplishes
            return choice.Description;
        }

        private string FormatAsSocialDialogue(IChoice choice, string baseDescription)
        {
            // Format as direct speech
            return $"\"{baseDescription}\"";
        }

        private string FormatAsInternalMonologue(IChoice choice, string baseDescription)
        {
            // Format as internal thoughts
            return $"I consider: {baseDescription}";
        }

        private string FormatAsActionDescription(IChoice choice, string baseDescription)
        {
            // Format as physical action
            return $"You {baseDescription.ToLower()}";
        }

        // Format the outcome of a choice based on presentation style
        public string FormatOutcome(IChoice choice, PresentationStyles style, int momentumGained, int pressureBuilt)
        {
            string result = "";

            switch (style)
            {
                case PresentationStyles.Social:
                    result = $"Your words ";
                    break;
                case PresentationStyles.Intellectual:
                    result = $"Your analysis ";
                    break;
                case PresentationStyles.Physical:
                    result = $"Your actions ";
                    break;
            }

            if (choice.EffectType == EffectTypes.Momentum)
            {
                result += $"move the situation forward";
                if (momentumGained > 0)
                    result += $", gaining {momentumGained} momentum";
            }
            else
            {
                result += "increase the complexity of the situation";
                if (pressureBuilt > 0)
                    result += $", adding {pressureBuilt} pressure";
            }

            return result + ".";
        }
    }
}