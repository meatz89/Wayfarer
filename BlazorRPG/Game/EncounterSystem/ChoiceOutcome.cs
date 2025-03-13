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

            // Initialize empty collections
            ApproachTagChanges = new Dictionary<ApproachTags, int>();
            FocusTagChanges = new Dictionary<FocusTags, int>();
            EncounterStateTagChanges = new Dictionary<EncounterStateTags, int>();
            NewlyActivatedTags = new List<string>();
            DeactivatedTags = new List<string>();
        }

        /// <summary>
        /// Creates a ChoiceOutcome from a ChoiceProjection, including all tag changes
        /// </summary>
        public ChoiceOutcome(ChoiceProjection projection) : this(
            projection.MomentumGained,
            projection.PressureBuilt,
            projection.NarrativeDescription,
            projection.EncounterWillEnd,
            projection.ProjectedOutcome)
        {
            // Copy all tag changes from the projection
            foreach (var kvp in projection.ApproachTagChanges)
            {
                ApproachTagChanges[kvp.Key] = kvp.Value;
            }

            foreach (var kvp in projection.FocusTagChanges)
            {
                FocusTagChanges[kvp.Key] = kvp.Value;
            }

            foreach (var kvp in projection.EncounterStateTagChanges)
            {
                EncounterStateTagChanges[kvp.Key] = kvp.Value;
            }

            NewlyActivatedTags.AddRange(projection.NewlyActivatedTags);
            DeactivatedTags.AddRange(projection.DeactivatedTags);
        }
    }
}