public class ContextModifierBuilder
{
    private readonly NarrativeChoice _choice;
    private readonly NarrativeActionContext _context;

    public ContextModifierBuilder(NarrativeChoice choice, NarrativeActionContext context)
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
        // Apply location-based value caps to narrative state changes
        switch (_context.LocationType)
        {
            case LocationTypes.Industrial:
                CapNarrativeValue(nameof(_choice.NarrativeStateChanges.Momentum), 10);
                break;
            case LocationTypes.Social:
                CapNarrativeValue(nameof(_choice.NarrativeStateChanges.Connection), 10);
                break;
            case LocationTypes.Commercial:
                CapNarrativeValue(nameof(_choice.NarrativeStateChanges.Advantage), 10);
                break;
            case LocationTypes.Nature:
                CapNarrativeValue(nameof(_choice.NarrativeStateChanges.Understanding), 10);
                break;
        }
        return this;
    }

    private void CapNarrativeValue(string propertyName, int maxValue)
    {
        var property = typeof(NarrativeStateValues).GetProperty(propertyName);
        if (property != null)
        {
            int currentValue = (int)property.GetValue(_choice.NarrativeStateChanges);
            if (currentValue > maxValue)
            {
                property.SetValue(_choice.NarrativeStateChanges, maxValue);
            }
        }
    }

    public NarrativeChoice Build()
    {
        return _choice;
    }
}