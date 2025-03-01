public class EncounterStateCondition
{
    public int? MaxValue;
    public int? MinValue;

    public bool IsMet(EncounterStageState state)
    {
        if ((MaxValue.HasValue && state.Momentum > MaxValue) || (MinValue.HasValue && state.Momentum < MinValue)) return false;
        return true;
    }

    public override string ToString()
    {
        string explanation = string.Empty;

        if (MinValue.HasValue)
        {
            explanation += $"Momentum >{MinValue.Value}";
        }
        if (MaxValue.HasValue)
        {
            explanation += $"Momentum <{MaxValue.Value}";
        }

        return explanation;
    }
}