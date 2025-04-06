
public class ResourceManager
{
    public int CalculatePressureResourceDamage(EncounterInfo encounterInfo, PlayerStatusResources resourceType, int pressureValue)
    {
        switch (encounterInfo.Type)
        {
            case EncounterTypes.Physical:
                if (resourceType == PlayerStatusResources.Health)
                    return (int)-pressureValue;
                break;

            case EncounterTypes.Intellectual:
                if (resourceType == PlayerStatusResources.Concentration)
                    return (int)-pressureValue;
                break;

            case EncounterTypes.Social:
                if (resourceType == PlayerStatusResources.Confidence)
                    return (int)-pressureValue;
                break;
        }

        return 0;
    }

    public void ApplyResourceChanges(PlayerState playerState, int healthChange, int focusChange, int confidenceChange)
    {
        if (healthChange != 0)
            playerState.ModifyHealth(healthChange);

        if (focusChange != 0)
            playerState.ModifyConcentration(focusChange);

        if (confidenceChange != 0)
            playerState.ModifyConfidence(confidenceChange);
    }

    public void ApplyResourceChanges(string key, int value)
    {
        throw new NotImplementedException();
    }
}