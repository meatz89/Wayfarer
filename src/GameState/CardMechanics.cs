
// Card mechanics
public class CardMechanics
{
    public int SuccessChance { get; set; }
    public int ComfortReward { get; set; }
    public Dictionary<EmotionalState, int> StateModifiers { get; set; } = new();
}
