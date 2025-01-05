public class ValueStateModifierBuilder
{
    private readonly NarrativeChoice _choice;
    private readonly NarrativeStateValues _values;

    public ValueStateModifierBuilder(NarrativeChoice choice, NarrativeStateValues values)
    {
        _choice = choice;
        _values = values;
    }

    public ValueStateModifierBuilder ApplyHighMomentum()
    {
        if (_values.Momentum >= 8)
        {
            _choice.CompletionPoints *= 2;
            _choice.NarrativeStateChanges.Tension += 1;

            // Modify cost if it's an energy outcome
            if (_choice.Cost is EnergyOutcome energyCost)
            {
                energyCost.Amount += 1;
            }
        }
        return this;
    }

    public ValueStateModifierBuilder ApplyHighAdvantage()
    {
        if (_values.Advantage >= 8)
        {
            // Clear additional requirements while preserving the main requirement
            _choice.AdditionalRequirements.Clear();
            _choice.NarrativeStateChanges.Understanding += 1;
        }
        return this;
    }

    public ValueStateModifierBuilder ApplyHighUnderstanding()
    {
        if (_values.Understanding >= 8)
        {
            _choice.UnlockedOptions.Add("HIDDEN_OPTION");
            _choice.NarrativeStateChanges.Connection += 1;
        }
        return this;
    }

    public ValueStateModifierBuilder ApplyHighConnection()
    {
        if (_values.Connection >= 8)
        {
            _choice.UnlockedOptions.Add("SPECIAL_REQUEST");
            _choice.NarrativeStateChanges.Advantage += 1;
        }
        return this;
    }

    public ValueStateModifierBuilder ApplyHighTension()
    {
        if (_values.Tension >= 8)
        {
            _choice.CompletionPoints = (int)(_choice.CompletionPoints * 1.5);
            _choice.NarrativeStateChanges.Momentum += 1;
        }
        return this;
    }

    public NarrativeChoice Build()
    {
        return _choice;
    }
}