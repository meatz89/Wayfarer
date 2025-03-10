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
                state => state.AddApproachMomentumBonus(affectedApproach, 1) // Standard +1 bonus
            );
        }

        // Create a strategic tag that adds momentum to choices with a specific focus
        public static StrategicTag CreateFocusMomentumBonus(string name, FocusTags requiredTag, int threshold, FocusTags affectedFocus)
        {
            return new StrategicTag(
                name,
                tagSystem => tagSystem.GetFocusTagValue(requiredTag) >= threshold,
                state => state.AddFocusMomentumBonus(affectedFocus, 1) // Standard +1 bonus
            );
        }

        // Create a strategic tag that reduces pressure at the end of each turn
        public static StrategicTag CreateEndOfTurnPressureReduction(string name, ApproachTags requiredTag, int threshold)
        {
            return new StrategicTag(
                name,
                tagSystem => tagSystem.GetApproachTagValue(requiredTag) >= threshold,
                state => state.AddEndOfTurnPressureReduction(1) // Standard -1 pressure
            );
        }
    }
}
