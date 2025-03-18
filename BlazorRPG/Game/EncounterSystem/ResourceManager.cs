public class ResourceManager
{
    private readonly PlayerState _playerState;
    private readonly EncounterInfo _location;

    public ResourceManager(PlayerState playerState, EncounterInfo location)
    {
        _playerState = playerState;
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
                _playerState.ModifyConcentratin(-currentPressure);
                break;

            case EncounterTypes.Social:
                _playerState.ModifyConfidence(-currentPressure);
                break;
        }
    }

    public int CalculatePressureResourceDamage(ResourceTypes resourceType, int choicePressureValue)
    {
        switch (_location.EncounterType)
        {
            case EncounterTypes.Physical:
                if (resourceType == ResourceTypes.Health)
                    return (int)-choicePressureValue;
                break;

            case EncounterTypes.Intellectual:
                if (resourceType == ResourceTypes.Concentration)
                    return (int)-choicePressureValue;
                break;

            case EncounterTypes.Social:
                if (resourceType == ResourceTypes.Confidence)
                    return (int)-choicePressureValue;
                break;
        }

        return 0;
    }

    public void ApplyResourceChanges(int healthChange, int focusChange, int confidenceChange)
    {
        if (healthChange != 0)
            _playerState.ModifyHealth(healthChange);

        if (focusChange != 0)
            _playerState.ModifyConcentratin(focusChange);

        if (confidenceChange != 0)
            _playerState.ModifyConfidence(confidenceChange);
    }
}