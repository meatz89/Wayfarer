
public class LocationPropertyEffectCalculator
{
    public List<ValueChange> CalculatePropertyEffects(EncounterChoice choice, LocationProperties properties)
    {
        List<ValueChange> changes = new();


        return changes;
    }

    internal int CalculateEnergyCostModifier(EncounterChoice choice, LocationProperties locationProperties)
    {
        return 0;
    }

    internal List<Requirement> CalculateLocationRequirements(EncounterChoice choice, LocationProperties locationProperties)
    {
        return new List<Requirement>();
    }

    internal List<Outcome> CalculatePropertyCosts(EncounterChoice choice, LocationProperties locationProperties)
    {
        return new List<Outcome>();
    }

    internal List<Outcome> CalculatePropertyRewards(EncounterChoice choice, LocationProperties locationProperties)
    {
        return new List<Outcome>();
    }
}