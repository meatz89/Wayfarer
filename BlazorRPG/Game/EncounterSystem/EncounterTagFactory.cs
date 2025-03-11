using BlazorRPG.Game.EncounterManager;

public static class EncounterTagFactory
{
    public const int MinorTagThreshold = 2;
    public const int MajorTagThreshold = 4;

    public static NarrativeTag CreateApproachThresholdTag(
        string name,
        ApproachTags tag,
        int threshold,
        ApproachTypes blockedApproach)
    {
        return new NarrativeTag(
            name,
            new ApproachThresholdCondition(tag, threshold),
            blockedApproach
        );
    }

    public static StrategicTag CreateApproachMomentumBonus(
        string name,
        ApproachTags requiredTag,
        int threshold,
        ApproachTypes affectedApproach)
    {
        return new StrategicTag(
            name,
            new ApproachThresholdCondition(requiredTag, threshold),
            StrategicEffectTypes.AddMomentumToApproach,
            1,
            affectedApproach
        );
    }

    public static StrategicTag CreateFocusMomentumBonus(
        string name,
        FocusTags requiredTag,
        int threshold,
        FocusTags affectedFocus)
    {
        return new StrategicTag(
            name,
            new FocusThresholdCondition(requiredTag, threshold),
            StrategicEffectTypes.AddMomentumToFocus,
            1,
            null,
            affectedFocus
        );
    }

    public static StrategicTag CreateEndOfTurnPressureReduction(
        string name,
        ApproachTags requiredTag,
        int threshold)
    {
        return new StrategicTag(
            name,
            new ApproachThresholdCondition(requiredTag, threshold),
            StrategicEffectTypes.ReducePressurePerTurn,
            1
        );
    }

    public static StrategicTag CreateFocusPressureReduction(
        string name,
        ApproachTags requiredTag,
        int threshold,
        FocusTags affectedFocus)
    {
        return new StrategicTag(
            name,
            new ApproachThresholdCondition(requiredTag, threshold),
            StrategicEffectTypes.ReducePressureFromFocus,
            1,
            null,
            affectedFocus
        );
    }

    public static StrategicTag CreateApproachPressureIncrease(
        string name,
        ApproachTags requiredTag,
        int threshold,
        ApproachTypes affectedApproach,
        int pressureAmount = 1)
    {
        return new StrategicTag(
            name,
            new ApproachThresholdCondition(requiredTag, threshold),
            StrategicEffectTypes.AddPressureFromApproach,
            pressureAmount,
            affectedApproach
        );
    }

    public static StrategicTag CreateMomentumOnActivation(
        string name,
        ApproachTags requiredTag,
        int threshold,
        int momentumAmount = 1)
    {
        return new StrategicTag(
            name,
            new ApproachThresholdCondition(requiredTag, threshold),
            StrategicEffectTypes.AddMomentumOnActivation,
            momentumAmount
        );
    }

    public static StrategicTag CreatePressureReductionOnActivation(
        string name,
        ApproachTags requiredTag,
        int threshold,
        int pressureReduction = 1)
    {
        return new StrategicTag(
            name,
            new ApproachThresholdCondition(requiredTag, threshold),
            StrategicEffectTypes.ReducePressureOnActivation,
            pressureReduction
        );
    }
}