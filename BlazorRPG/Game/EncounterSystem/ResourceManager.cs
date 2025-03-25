
public class ResourceManager
{
    public void ApplyPressureResourceDamage(PlayerState playerState, EncounterInfo encounterInfo, int currentPressure)
    {
        // Skip if no pressure or location doesn't apply pressure damage
        if (currentPressure <= 0)
            return;

        // Different resource affected based on encounter type
        switch (encounterInfo.EncounterType)
        {
            case EncounterTypes.Physical:
                playerState.ModifyHealth(-currentPressure);
                break;

            case EncounterTypes.Intellectual:
                playerState.ModifyConcentratin(-currentPressure);
                break;

            case EncounterTypes.Social:
                playerState.ModifyConfidence(-currentPressure);
                break;
        }
    }

    public int CalculatePressureResourceDamage(EncounterInfo encounterInfo, PlayerStatusResources resourceType, int pressureValue)
    {
        switch (encounterInfo.EncounterType)
        {
            case EncounterTypes.Physical:
                if (resourceType == PlayerStatusResources.Health)
                    return (int)-pressureValue / 2;
                break;

            case EncounterTypes.Intellectual:
                if (resourceType == PlayerStatusResources.Concentration)
                    return (int)-pressureValue / 2;
                break;

            case EncounterTypes.Social:
                if (resourceType == PlayerStatusResources.Confidence)
                    return (int)-pressureValue / 2;
                break;
        }

        return 0;
    }

    public void ApplyResourceChanges(PlayerState playerState, int healthChange, int focusChange, int confidenceChange)
    {
        if (healthChange != 0)
            playerState.ModifyHealth(healthChange);

        if (focusChange != 0)
            playerState.ModifyConcentratin(focusChange);

        if (confidenceChange != 0)
            playerState.ModifyConfidence(confidenceChange);
    }

    internal void ApplyResourceChanges(string key, int value)
    {
        throw new NotImplementedException();
    }
}