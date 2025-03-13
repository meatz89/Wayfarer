namespace BlazorRPG.Game.EncounterManager
{
    /// <summary>
    /// Represents the outcome of the player character (PC)'s choice
    /// </summary>
    public class ChoiceOutcome
    {
        public int MomentumGain { get; }
        public int PressureGain { get; }
        public string Description { get; }
        public bool IsEncounterOver { get; }
        public EncounterOutcomes Outcome { get; }

        // Added tag-related fields
        public Dictionary<ApproachTags, int> ApproachTagChanges { get; }
        public Dictionary<FocusTags, int> FocusTagChanges { get; }
        public Dictionary<EncounterStateTags, int> EncounterStateTagChanges { get; }
        public List<string> NewlyActivatedTags { get; }
        public List<string> DeactivatedTags { get; }

        // Add resource change fields directly to ChoiceOutcome
        public int HealthChange { get; }
        public int ConcentrationChange { get; }
        public int ReputationChange { get; }

        public ChoiceOutcome(
            int momentumGained,
            int pressureBuilt,
            string description,
            bool isEncounterOver,
            EncounterOutcomes outcome,
            int healthChange = 0,
            int concentrationChange = 0,
            int reputationChange = 0)
        {
            MomentumGain = momentumGained;
            PressureGain = pressureBuilt;
            Description = description;
            IsEncounterOver = isEncounterOver;
            Outcome = outcome;
            HealthChange = healthChange;
            ConcentrationChange = concentrationChange;
            ReputationChange = reputationChange;

            // Initialize empty collections
            ApproachTagChanges = new Dictionary<ApproachTags, int>();
            FocusTagChanges = new Dictionary<FocusTags, int>();
            EncounterStateTagChanges = new Dictionary<EncounterStateTags, int>();
            NewlyActivatedTags = new List<string>();
            DeactivatedTags = new List<string>();
        }
    }
}