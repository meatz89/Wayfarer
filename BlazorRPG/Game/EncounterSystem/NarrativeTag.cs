namespace BlazorRPG.Game.EncounterManager
{
    /// <summary>
    /// Narrative tags control which choices appear in the player's hand
    /// </summary>
    public class NarrativeTag : IEncounterTag
    {
        public string Name { get; }
        public ApproachTypes? BlockedApproach { get; }

        private readonly Func<BaseTagSystem, bool> _activationCondition;

        public NarrativeTag(string name, Func<BaseTagSystem, bool> activationCondition, ApproachTypes? blockedApproach = null) 
        {
            Name = name;
            _activationCondition = activationCondition;
            BlockedApproach = blockedApproach;
        }

        public bool IsActive(BaseTagSystem tagSystem) => _activationCondition(tagSystem);

        public void ApplyEffect(EncounterState state)
        {
            // Narrative tags don't directly affect state, only card selection
        }
    }
}
