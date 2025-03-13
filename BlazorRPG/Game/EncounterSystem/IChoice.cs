namespace BlazorRPG.Game.EncounterManager
{
    /// <summary>
    /// Base interface for all choices
    /// </summary>
    public interface IChoice
    {
        string Name { get; }
        string Description { get; }
        ApproachTypes Approach { get; }
        FocusTags Focus { get; }
        EffectTypes EffectType { get; }
        IReadOnlyList<EncounterStateModification> TagModifications { get; }
        void ApplyChoice(EncounterState state);

    }

    /// <summary>
    /// Standard choice implementation - either builds momentum or pressure
    /// </summary>
    public class Choice : IChoice
    {
        public string Name { get; }
        public string Description { get; }
        public ApproachTypes Approach { get; }
        public FocusTags Focus { get; }
        public EffectTypes EffectType { get; }
        public IReadOnlyList<EncounterStateModification> TagModifications { get; }

        public Choice(string name, string description, ApproachTypes approach, FocusTags focus,
                      EffectTypes effectType, IReadOnlyList<EncounterStateModification> tagModifications)
        {
            Name = name;
            Description = description;
            Approach = approach;
            Focus = focus;
            EffectType = effectType;
            TagModifications = tagModifications;
        }

        public virtual void ApplyChoice(EncounterState state)
        {
            // Apply tag modifications
            foreach (EncounterStateModification mod in TagModifications)
            {
                if (mod.Type == EncounterStateModification.TagTypes.Approach)
                    state.TagSystem.ModifyApproachTag((EncounterStateTags)mod.Tag, mod.Delta);
                else
                    state.TagSystem.ModifyFocusTag((FocusTags)mod.Tag, mod.Delta);
            }

            // Apply momentum or pressure effect
            if (EffectType == EffectTypes.Momentum)
            {
                int baseMomentum = 2; // Standard choices build 2 momentum
                int totalMomentum = state.GetTotalMomentum(this, baseMomentum);
                state.BuildMomentum(totalMomentum);
            }
            else // Pressure
            {
                state.BuildPressure(3); // Standard choices build 2 pressure
            }
        }

        public override string ToString()
        {
            return $"{Name} - {Approach.ToString()} - {Focus.ToString()} - {EffectType.ToString()}";
        }
    }

    /// <summary>
    /// Special choice that requires specific tag values and builds more momentum
    /// </summary>
    public class SpecialChoice : Choice
    {
        public IReadOnlyList<Func<BaseTagSystem, bool>> Requirements { get; }

        public SpecialChoice(string name, string description, ApproachTypes approach, FocusTags focus,
                            IReadOnlyList<EncounterStateModification> tagModifications,
                            IReadOnlyList<Func<BaseTagSystem, bool>> requirements)
            : base(name, description, approach, focus, EffectTypes.Momentum, tagModifications)
        {
            Requirements = requirements;
        }

        public bool CanBeSelected(BaseTagSystem tagSystem)
        {
            return Requirements.All(req => req(tagSystem));
        }

        public override void ApplyChoice(EncounterState state)
        {
            // Apply tag modifications
            foreach (EncounterStateModification mod in TagModifications)
            {
                if (mod.Type == EncounterStateModification.TagTypes.Approach)
                    state.TagSystem.ModifyApproachTag((EncounterStateTags)mod.Tag, mod.Delta);
                else
                    state.TagSystem.ModifyFocusTag((FocusTags)mod.Tag, mod.Delta);
            }

            // Special choices build 3 momentum (plus bonuses)
            int baseMomentum = 3;
            int totalMomentum = state.GetTotalMomentum(this, baseMomentum);
            state.BuildMomentum(totalMomentum);
        }

        public override string ToString()
        {
            return $"{Name} - {Approach.ToString()} - {Focus.ToString()} - {EffectType.ToString()}";
        }
    }

    /// <summary>
    /// Emergency choice for when approaches are blocked
    /// </summary>
    public class EmergencyChoice : IChoice
    {
        public string Name { get; }
        public string Description { get; }
        public ApproachTypes Approach { get; }
        public FocusTags Focus { get; }
        public EffectTypes EffectType => EffectTypes.Momentum; // Always momentum
        public IReadOnlyList<EncounterStateModification> TagModifications { get; }
        public ApproachTypes BlockedApproach { get; }

        public EmergencyChoice(string name, string description, ApproachTypes approach,
                              IReadOnlyList<EncounterStateModification> tagModifications,
                              ApproachTypes blockedApproach)
        {
            Name = name;
            Description = description;
            Approach = approach;
            Focus = FocusTags.Physical; // Default
            TagModifications = tagModifications;
            BlockedApproach = blockedApproach;
        }

        public void ApplyChoice(EncounterState state)
        {
            // Apply tag modifications
            foreach (EncounterStateModification mod in TagModifications)
            {
                if (mod.Type == EncounterStateModification.TagTypes.Approach)
                    state.TagSystem.ModifyApproachTag((EncounterStateTags)mod.Tag, mod.Delta);
                else
                    state.TagSystem.ModifyFocusTag((FocusTags)mod.Tag, mod.Delta);
            }

            // Emergency choices build 1 momentum and 5 pressure
            int baseMomentum = 1;
            int totalMomentum = state.GetTotalMomentum(this, baseMomentum);
            state.BuildMomentum(totalMomentum);
            state.BuildPressure(5);
        }

        public override string ToString()
        {
            return $"{Name} - {Approach.ToString()} - {Focus.ToString()} - {EffectType.ToString()}";
        }
    }
}
