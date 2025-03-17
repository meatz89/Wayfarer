

public class ResourceManager
{
    private readonly PlayerState _playerState;
    private readonly TagManager _tagManager;
    private readonly LocationInfo _location;

    public ResourceManager(PlayerState playerState, TagManager tagManager, LocationInfo location)
    {
        _playerState = playerState;
        _tagManager = tagManager;
        _location = location;
    }

    public void ApplyPressureResourceDamage(int currentPressure)
    {
        // Skip if no pressure or location doesn't apply pressure damage
        if (currentPressure <= 0)
            return;

        // Different resource affected based on encounter type
        switch (_location.EncounterType)
        {
            case EncounterTypes.Physical:
                _playerState.ModifyHealth(-currentPressure);
                break;

            case EncounterTypes.Intellectual:
                _playerState.ModifyFocus(-currentPressure);
                break;

            case EncounterTypes.Social:
                _playerState.ModifyConfidence(-currentPressure);
                break;
        }
    }

    public int CalculatePressureResourceDamage(ResourceTypes resourceType, int currentPressure)
    {
        // Skip if no pressure or location doesn't apply pressure damage
        if (currentPressure <= 0)
            return 0;

        // Only return a value for the resource type affected by this encounter type
        switch (_location.EncounterType)
        {
            case EncounterTypes.Physical:
                if (resourceType == ResourceTypes.Health)
                    return -currentPressure;
                break;

            case EncounterTypes.Intellectual:
                if (resourceType == ResourceTypes.Focus)
                    return -currentPressure;
                break;

            case EncounterTypes.Social:
                if (resourceType == ResourceTypes.Confidence)
                    return -currentPressure;
                break;
        }

        return 0;
    }

    public int CalculateTagResourceChange(IChoice choice, ResourceTypes resourceType, int currentPressure, List<IEncounterTag> excludedTags = null)
    {
        int change = 0;

        foreach (IEncounterTag tag in _tagManager.ActiveTags)
        {
            // Skip excluded tags (like newly activated ones)
            if (excludedTags != null && excludedTags.Contains(tag))
                continue;

            if (tag is StrategicTag strategicTag)
            {
                if (strategicTag.AffectedFocus.HasValue && choice.Focus != strategicTag.AffectedFocus.Value)
                    continue;

                switch (resourceType)
                {
                    //case ResourceTypes.Health:
                    //    if (strategicTag.EffectType == StrategicEffectTypes.ReduceHealthByPressure)
                    //        change -= currentPressure;
                    //    else if (strategicTag.EffectType == StrategicEffectTypes.ReduceHealthByApproachValue &&
                    //             strategicTag.ScalingApproachTag.HasValue)
                    //        change -= _tagManager.TagSystem.GetEncounterStateTagValue(strategicTag.ScalingApproachTag.Value);
                    //    break;

                    //case ResourceTypes.Focus:
                    //    if (strategicTag.EffectType == StrategicEffectTypes.ReduceFocusByPressure)
                    //        change -= currentPressure;
                    //    else if (strategicTag.EffectType == StrategicEffectTypes.ReduceFocusByApproachValue &&
                    //             strategicTag.ScalingApproachTag.HasValue)
                    //        change -= _tagManager.TagSystem.GetEncounterStateTagValue(strategicTag.ScalingApproachTag.Value);
                    //    break;

                    //case ResourceTypes.Confidence:
                    //    if (strategicTag.EffectType == StrategicEffectTypes.ReduceConfidenceByPressure)
                    //        change -= currentPressure;
                    //    else if (strategicTag.EffectType == StrategicEffectTypes.ReduceConfidenceByApproachValue &&
                    //             strategicTag.ScalingApproachTag.HasValue)
                    //        change -= _tagManager.TagSystem.GetEncounterStateTagValue(strategicTag.ScalingApproachTag.Value);
                    //    break;
                }
            }
        }

        return change;
    }
    public void ApplyResourceChanges(int healthChange, int focusChange, int confidenceChange)
    {
        if (healthChange != 0)
            _playerState.ModifyHealth(healthChange);

        if (focusChange != 0)
            _playerState.ModifyFocus(focusChange);

        if (confidenceChange != 0)
            _playerState.ModifyConfidence(confidenceChange);
    }
}