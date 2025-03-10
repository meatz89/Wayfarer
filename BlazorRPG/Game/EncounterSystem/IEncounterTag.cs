namespace BlazorRPG.Game.EncounterManager
{
    public interface IEncounterTag
    {
        string Name { get; }
        bool IsActive(BaseTagSystem tagSystem);
        void ApplyEffect(EncounterState state);
    }

    public class StrategicTag : IEncounterTag
    {
        public string Name { get; }
        private readonly Func<BaseTagSystem, bool> _activationCondition;
        private readonly Action<EncounterState> _effectAction;

        public StrategicEffectTypes EffectType { get; }
        public ApproachTypes? AffectedApproach { get; }
        public FocusTags? AffectedFocus { get; }
        public int EffectValue { get; }

        // Track if this tag has already applied its activation effect
        private bool _activationEffectApplied = false;

        // Constructor remains largely the same
        public StrategicTag(
            string name,
            Func<BaseTagSystem, bool> activationCondition,
            Action<EncounterState> effectAction,
            StrategicEffectTypes effectType = StrategicEffectTypes.None,
            int effectValue = 1,
            ApproachTypes? affectedApproach = null,
            FocusTags? affectedFocus = null)
        {
            Name = name;
            _activationCondition = activationCondition;
            _effectAction = effectAction;
            EffectType = effectType;
            EffectValue = effectValue;
            AffectedApproach = affectedApproach;
            AffectedFocus = affectedFocus;
        }

        public bool IsActive(BaseTagSystem tagSystem) => _activationCondition(tagSystem);

        public void ApplyEffect(EncounterState state)
        {
            // Call the original effect action
            _effectAction(state);

            // Also apply the typed effects
            switch (EffectType)
            {
                case StrategicEffectTypes.AddMomentumToApproach:
                    if (AffectedApproach.HasValue)
                        state.AddApproachMomentumBonus(AffectedApproach.Value, EffectValue);
                    break;

                case StrategicEffectTypes.AddMomentumToFocus:
                    if (AffectedFocus.HasValue)
                        state.AddFocusMomentumBonus(AffectedFocus.Value, EffectValue);
                    break;

                case StrategicEffectTypes.ReducePressurePerTurn:
                    state.AddEndOfTurnPressureReduction(EffectValue);
                    break;

                case StrategicEffectTypes.AddPressurePerTurn:
                    state.AddEndOfTurnPressureReduction(-EffectValue);
                    break;

                case StrategicEffectTypes.AddPressureFromApproach:
                    if (AffectedApproach.HasValue)
                        state.AddApproachPressureModifier(AffectedApproach.Value, EffectValue);
                    break;

                case StrategicEffectTypes.AddPressureFromFocus:
                    if (AffectedFocus.HasValue)
                        state.AddFocusPressureModifier(AffectedFocus.Value, EffectValue);
                    break;

                case StrategicEffectTypes.ReducePressureFromApproach:
                    if (AffectedApproach.HasValue)
                        state.AddApproachPressureModifier(AffectedApproach.Value, -EffectValue);
                    break;

                case StrategicEffectTypes.ReducePressureFromFocus:
                    if (AffectedFocus.HasValue)
                        state.AddFocusPressureModifier(AffectedFocus.Value, -EffectValue);
                    break;
            }
        }

        // Apply one-time activation effects
        public void ApplyActivationEffect(EncounterState state)
        {
            if (_activationEffectApplied)
                return;

            switch (EffectType)
            {
                case StrategicEffectTypes.AddMomentumOnActivation:
                    state.BuildMomentum(EffectValue);
                    break;

                case StrategicEffectTypes.ReducePressureOnActivation:
                    state.ReducePressure(EffectValue);
                    break;
            }

            _activationEffectApplied = true;
        }

        // Get momentum modifier for a specific choice
        public int GetMomentumModifierForChoice(IChoice choice)
        {
            switch (EffectType)
            {
                case StrategicEffectTypes.AddMomentumToApproach:
                    return AffectedApproach.HasValue && choice.Approach == AffectedApproach.Value
                        ? EffectValue : 0;

                case StrategicEffectTypes.AddMomentumToFocus:
                    return AffectedFocus.HasValue && choice.Focus == AffectedFocus.Value
                        ? EffectValue : 0;

                case StrategicEffectTypes.ReduceMomentumFromApproach:
                    return AffectedApproach.HasValue && choice.Approach == AffectedApproach.Value
                        ? -EffectValue : 0;

                case StrategicEffectTypes.ReduceMomentumFromFocus:
                    return AffectedFocus.HasValue && choice.Focus == AffectedFocus.Value
                        ? -EffectValue : 0;

                default:
                    return 0;
            }
        }

        // Get pressure modifier for a specific choice
        public int GetPressureModifierForChoice(IChoice choice)
        {
            switch (EffectType)
            {
                case StrategicEffectTypes.ReducePressureFromApproach:
                    return AffectedApproach.HasValue && choice.Approach == AffectedApproach.Value
                        ? -EffectValue : 0;

                case StrategicEffectTypes.ReducePressureFromFocus:
                    return AffectedFocus.HasValue && choice.Focus == AffectedFocus.Value
                        ? -EffectValue : 0;

                case StrategicEffectTypes.AddPressureFromApproach:
                    return AffectedApproach.HasValue && choice.Approach == AffectedApproach.Value
                        ? EffectValue : 0;

                case StrategicEffectTypes.AddPressureFromFocus:
                    return AffectedFocus.HasValue && choice.Focus == AffectedFocus.Value
                        ? EffectValue : 0;

                default:
                    return 0;
            }
        }

        public string GetEffectDescription()
        {
            switch (EffectType)
            {
                case StrategicEffectTypes.AddMomentumToApproach:
                    return $"+{EffectValue} momentum to {AffectedApproach} approaches";

                case StrategicEffectTypes.AddMomentumToFocus:
                    return $"+{EffectValue} momentum to {AffectedFocus} choices";

                case StrategicEffectTypes.ReducePressureFromApproach:
                    return $"-{EffectValue} pressure from {AffectedApproach} approaches";

                case StrategicEffectTypes.ReducePressureFromFocus:
                    return $"-{EffectValue} pressure from {AffectedFocus} choices";

                case StrategicEffectTypes.ReducePressurePerTurn:
                    return $"-{EffectValue} pressure at end of each turn";

                case StrategicEffectTypes.AddMomentumPerTurn:
                    return $"+{EffectValue} momentum at end of each turn";

                case StrategicEffectTypes.ReduceMomentumFromApproach:
                    return $"-{EffectValue} momentum from {AffectedApproach} approaches";

                case StrategicEffectTypes.ReduceMomentumFromFocus:
                    return $"-{EffectValue} momentum from {AffectedFocus} choices";

                case StrategicEffectTypes.AddPressurePerTurn:
                    return $"+{EffectValue} pressure at end of each turn";

                case StrategicEffectTypes.AddPressureFromApproach:
                    return $"+{EffectValue} pressure from {AffectedApproach} approaches";

                case StrategicEffectTypes.AddPressureFromFocus:
                    return $"+{EffectValue} pressure from {AffectedFocus} choices";

                case StrategicEffectTypes.AddMomentumOnActivation:
                    return $"+{EffectValue} momentum when activated";

                case StrategicEffectTypes.ReducePressureOnActivation:
                    return $"-{EffectValue} pressure when activated";

                default:
                    return "Affects encounter mechanics";
            }
        }
    }
}
