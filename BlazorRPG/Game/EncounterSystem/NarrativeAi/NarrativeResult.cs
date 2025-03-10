namespace BlazorRPG.Game.EncounterManager.NarrativeAi
{
    /// <summary>
    /// Represents the result of a narrative-enhanced action
    /// </summary>
    public class NarrativeResult
    {
        public string Narrative { get; }
        public List<IChoice> Choices { get; }
        public List<ChoiceProjection> Projections { get; }
        public Dictionary<IChoice, string> ChoiceDescriptions { get; }
        public bool IsEncounterOver { get; }
        public EncounterOutcomes? Outcome { get; }

        public NarrativeResult(
            string narrative,
            List<IChoice> choices,
            List<ChoiceProjection> projections,
            Dictionary<IChoice, string> choiceDescriptions,
            bool isEncounterOver = false,
            EncounterOutcomes? outcome = null)
        {
            Narrative = narrative;
            Choices = choices;
            Projections = projections;
            ChoiceDescriptions = choiceDescriptions;
            IsEncounterOver = isEncounterOver;
            Outcome = outcome;
        }
    }
}