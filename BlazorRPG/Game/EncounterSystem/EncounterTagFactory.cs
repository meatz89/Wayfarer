namespace BlazorRPG.Game.EncounterManager
{
    /// <summary>
    /// Factory for creating common encounter tags
    /// </summary>
    public static class EncounterTagFactory
    {
        // Minor tags activate at 2+, Major tags at 4+
        public const int MinorTagThreshold = 2;
        public const int MajorTagThreshold = 4;

        // Create a narrative tag that blocks an approach when an approach tag reaches a threshold
        public static NarrativeTag CreateApproachThresholdTag(string name, ApproachTags tag, int threshold, ApproachTypes blockedApproach)
        {
            return new NarrativeTag(
                name,
                tagSystem => tagSystem.GetApproachTagValue(tag) >= threshold,
                blockedApproach
            );
        }

        // Create a strategic tag that adds momentum to choices with a specific approach
        public static StrategicTag CreateApproachMomentumBonus(string name, ApproachTags requiredTag, int threshold, ApproachTypes affectedApproach)
        {
            return new StrategicTag(
                name,
                tagSystem => tagSystem.GetApproachTagValue(requiredTag) >= threshold,
                state => state.AddApproachMomentumBonus(affectedApproach, 1),
                StrategicEffectTypes.AddMomentumToApproach,
                1,  // Standard +1 bonus
                affectedApproach
            );
        }

        // Create a strategic tag that adds momentum to choices with a specific focus
        public static StrategicTag CreateFocusMomentumBonus(string name, FocusTags requiredTag, int threshold, FocusTags affectedFocus)
        {
            return new StrategicTag(
                name,
                tagSystem => tagSystem.GetFocusTagValue(requiredTag) >= threshold,
                state => state.AddFocusMomentumBonus(affectedFocus, 1),
                StrategicEffectTypes.AddMomentumToFocus,
                1,  // Standard +1 bonus
                null,
                affectedFocus
            );
        }

        // Create a strategic tag that reduces pressure at the end of each turn
        public static StrategicTag CreateEndOfTurnPressureReduction(string name, ApproachTags requiredTag, int threshold)
        {
            return new StrategicTag(
                name,
                tagSystem => tagSystem.GetApproachTagValue(requiredTag) >= threshold,
                state => state.AddEndOfTurnPressureReduction(1),
                StrategicEffectTypes.ReducePressurePerTurn,
                1  // Standard -1 pressure
            );
        }

        // Create a strategic tag that reduces pressure from choices with a specific focus
        public static StrategicTag CreateFocusPressureReduction(string name, ApproachTags requiredTag, int threshold, FocusTags affectedFocus)
        {
            return new StrategicTag(
                name,
                tagSystem => tagSystem.GetApproachTagValue(requiredTag) >= threshold,
                state => {
                    // This would need implementation in EncounterState
                },
                StrategicEffectTypes.ReducePressureFromFocus,
                1,  // Standard -1 pressure
                null,
                affectedFocus
            );
        }

        // Create a negative strategic tag that adds pressure each turn
        public static StrategicTag CreateEndOfTurnPressureIncrease(string name, ApproachTags requiredTag, int threshold)
        {
            return new StrategicTag(
                name,
                tagSystem => tagSystem.GetApproachTagValue(requiredTag) >= threshold,
                state => {
                    // This would add pressure rather than reduce it
                    state.AddEndOfTurnPressureReduction(-1);
                },
                StrategicEffectTypes.AddPressurePerTurn,
                1
            );
        }

        // Create a strategic tag that adds pressure to choices with a specific approach
        public static StrategicTag CreateApproachPressureIncrease(string name, ApproachTags requiredTag, int threshold, ApproachTypes affectedApproach)
        {
            return new StrategicTag(
                name,
                tagSystem => tagSystem.GetApproachTagValue(requiredTag) >= threshold,
                state => {
                    // Implementation would be handled in ApplyChoice via GetPressureModifierForChoice
                },
                StrategicEffectTypes.AddPressureFromApproach,
                1, // Standard +1 pressure
                affectedApproach
            );
        }

        // Create a strategic tag that adds pressure to choices with a specific focus
        public static StrategicTag CreateFocusPressureIncrease(string name, ApproachTags requiredTag, int threshold, FocusTags affectedFocus)
        {
            return new StrategicTag(
                name,
                tagSystem => tagSystem.GetApproachTagValue(requiredTag) >= threshold,
                state => {
                    // Implementation would be handled in ApplyChoice via GetPressureModifierForChoice
                },
                StrategicEffectTypes.AddPressureFromFocus,
                1, // Standard +1 pressure
                null,
                affectedFocus
            );
        }

        // Create a strategic tag that adds momentum when activated
        public static StrategicTag CreateMomentumOnActivation(string name, ApproachTags requiredTag, int threshold, int momentumAmount = 1)
        {
            return new StrategicTag(
                name,
                tagSystem => tagSystem.GetApproachTagValue(requiredTag) >= threshold,
                state => {
                    // Implementation handled through ApplyActivationEffect
                },
                StrategicEffectTypes.AddMomentumOnActivation,
                momentumAmount
            );
        }

        // Create a strategic tag that reduces pressure when activated
        public static StrategicTag CreatePressureReductionOnActivation(string name, ApproachTags requiredTag, int threshold, int pressureReduction = 1)
        {
            return new StrategicTag(
                name,
                tagSystem => tagSystem.GetApproachTagValue(requiredTag) >= threshold,
                state => {
                    // Implementation handled through ApplyActivationEffect
                },
                StrategicEffectTypes.ReducePressureOnActivation,
                pressureReduction
            );
        }
    }
}