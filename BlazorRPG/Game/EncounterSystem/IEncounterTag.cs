namespace BlazorRPG.Game.EncounterManager
{
    public interface IEncounterTag
    {
        string Name { get; }
        bool IsActive(BaseTagSystem tagSystem);
        void ApplyEffect(EncounterState state);
    }

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

    /// <summary>
    /// Strategic tags provide mechanical bonuses to choices
    /// </summary>
    public class StrategicTag : IEncounterTag
    {
        public string Name { get; }

        private readonly Func<BaseTagSystem, bool> _activationCondition;
        private readonly Action<EncounterState> _effectAction;

        public StrategicTag(string name, Func<BaseTagSystem, bool> activationCondition, Action<EncounterState> effectAction)
        {
            Name = name;
            _activationCondition = activationCondition;
            _effectAction = effectAction;
        }

        public bool IsActive(BaseTagSystem tagSystem) => _activationCondition(tagSystem);

        public void ApplyEffect(EncounterState state)
        {
            _effectAction(state);
        }
    }
}