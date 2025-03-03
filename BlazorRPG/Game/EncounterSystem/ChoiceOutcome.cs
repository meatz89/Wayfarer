
/// <summary>
/// Outcome of a choice after all modifiers
/// </summary>
public class ChoiceOutcome
{
    public int Momentum { get; set; }
    public int Pressure { get; set; }

    public ChoiceOutcome(int momentum, int pressure)
    {
        Momentum = momentum;
        Pressure = pressure;
    }
}