namespace BlazorRPG.Game.EncounterManager.NarrativeAi
{
    /// <summary>
    /// Represents a single narrative event in the encounter timeline
    /// </summary>
    public class NarrativeEvent
    {
        public int TurnNumber { get; }
        public string SceneDescription { get; }
        public IChoice ChosenOption { get; }
        public string ChosenOptionDescription { get; }
        public string Outcome { get; }
        public Dictionary<IChoice, string> AvailableChoiceDescriptions { get; }

        public NarrativeEvent(
            int turnNumber,
            string sceneDescription,
            IChoice chosenOption = null,
            string chosenOptionDescription = null,
            string outcome = null,
            Dictionary<IChoice, string> availableChoiceDescriptions = null)
        {
            TurnNumber = turnNumber;
            SceneDescription = sceneDescription;
            ChosenOption = chosenOption;
            ChosenOptionDescription = chosenOptionDescription;
            Outcome = outcome;
            AvailableChoiceDescriptions = availableChoiceDescriptions ?? new Dictionary<IChoice, string>();
        }
    }
}