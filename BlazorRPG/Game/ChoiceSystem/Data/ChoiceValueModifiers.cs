public class ChoiceValueModifiers
{
    public int EnergyCostModifier { get; set; } = 0;
    public int OutcomeModifier { get; set; } = 0;
    public int PressureGainModifier { get; set; } = 0;
    public int InsightGainModifier { get; set; } = 0;
    public int ResonanceGainModifier { get; set; } = 0;

    public Dictionary<string, int> ModifierDetails { get; set; } = new Dictionary<string, int>();

    public void AddModifierDetail(string source, int amount)
    {
        ModifierDetails.Add(source, amount);
    }
}