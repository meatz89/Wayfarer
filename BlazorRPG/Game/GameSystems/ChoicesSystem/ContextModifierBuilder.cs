public class ContextModifierBuilder
{
    private readonly EncounterChoice _choice;
    private readonly EncounterActionContext _context;

    public ContextModifierBuilder(EncounterChoice choice, EncounterActionContext context)
    {
        _choice = choice;
        _context = context;
    }

    public ContextModifierBuilder ApplyTimeModifiers()
    {
        // Only modify energy outcomes based on time of day
        if (_choice.Cost is EnergyOutcome energyCost)
        {
            ModifyEnergyBasedOnTime(energyCost);
        }
        return this;
    }

    private void ModifyEnergyBasedOnTime(EnergyOutcome energyCost)
    {
        // Apply time-based modifiers to energy costs
        switch (_context.TimeSlot)
        {
            case TimeSlots.Morning when energyCost.EnergyType == EnergyTypes.Physical:
                energyCost.Amount--; // Physical actions cost less in morning
                break;
            case TimeSlots.Afternoon when energyCost.EnergyType == EnergyTypes.Focus:
                energyCost.Amount--; // Focus actions cost less in afternoon
                break;
            case TimeSlots.Evening when energyCost.EnergyType == EnergyTypes.Social:
                energyCost.Amount--; // Social actions cost less in evening
                break;
            case TimeSlots.Night:
                energyCost.Amount++; // All actions cost more at night
                break;
        }

        // Ensure energy cost never goes below 1
        energyCost.Amount = Math.Max(1, energyCost.Amount);
    }

    public ContextModifierBuilder ApplyLocationModifiers()
    {
        // Apply location-based value caps to encounter state changes
        switch (_context.LocationType)
        {
            case LocationTypes.Industrial:
                CapEncounterValue(nameof(_choice.EncounterStateChanges.Momentum), 10);
                break;
            case LocationTypes.Social:
                CapEncounterValue(nameof(_choice.EncounterStateChanges.Connection), 10);
                break;
            case LocationTypes.Commercial:
                CapEncounterValue(nameof(_choice.EncounterStateChanges.Advantage), 10);
                break;
            case LocationTypes.Nature:
                CapEncounterValue(nameof(_choice.EncounterStateChanges.Understanding), 10);
                break;
        }
        return this;
    }

    private void CapEncounterValue(string propertyName, int maxValue)
    {
        System.Reflection.PropertyInfo? property = typeof(EncounterStateValues).GetProperty(propertyName);
        if (property != null)
        {
            int currentValue = (int)property.GetValue(_choice.EncounterStateChanges);
            if (currentValue > maxValue)
            {
                property.SetValue(_choice.EncounterStateChanges, maxValue);
            }
        }
    }

    public EncounterChoice Build()
    {
        return _choice;
    }
}