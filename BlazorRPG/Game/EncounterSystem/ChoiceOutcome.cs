namespace BlazorRPG.Game.EncounterManager
{
    /// <summary>
    /// Represents the outcome of a player's choice
    /// </summary>
    public class ChoiceOutcome
    {
        public int MomentumGain { get; }
        public int PressureGain { get; }
        public string Description { get; }
        public bool IsEncounterOver { get; }
        public EncounterOutcomes Outcome { get; }

        public ChoiceOutcome(
            int momentumGained,
            int pressureBuilt,
            string description,
            bool isEncounterOver,
            EncounterOutcomes outcome)
        {
            MomentumGain = momentumGained;
            PressureGain = pressureBuilt;
            Description = description;
            IsEncounterOver = isEncounterOver;
            Outcome = outcome;
        }
    }
}