namespace BlazorRPG.Game.EncounterManager
{
    /// <summary>
    /// Represents the current status of an encounter
    /// </summary>
    public class EncounterStatus
    {
        public int CurrentTurn { get; }
        public int MaxTurns { get; }
        public int Momentum { get; }
        public int Pressure { get; }
        public IReadOnlyDictionary<EncounterStateTags, int> ApproachTags { get; }
        public IReadOnlyDictionary<FocusTags, int> FocusTags { get; }
        public List<string> ActiveTagNames { get; }

        public EncounterStatus(
            int currentTurn,
            int maxTurns,
            int momentum,
            int pressure,
            IReadOnlyDictionary<EncounterStateTags, int> approachTags,
            IReadOnlyDictionary<FocusTags, int> focusTags,
            List<string> activeTagNames)
        {
            CurrentTurn = currentTurn;
            MaxTurns = maxTurns;
            Momentum = momentum;
            Pressure = pressure;
            ApproachTags = approachTags;
            FocusTags = focusTags;
            ActiveTagNames = activeTagNames;
        }
    }
}
