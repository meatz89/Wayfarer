public class ChoiceValueModifiers
{
    public int OutcomeModifier { get; set; }
    public int PressureGainModifier { get; set; }
    public int InsightGainModifier { get; set; }
    public int ResonanceGainModifier { get; set; }
    public int EnergyCostModifier { get; set; }

    public Dictionary<string, int> ModifierDetails { get; private set; } = new Dictionary<string, int>();

    public void AddModifierDetail(string modifierName, int value)
    {
        ModifierDetails.Add(modifierName, value);
    }
}