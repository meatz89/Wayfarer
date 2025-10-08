
// Card mechanics
public class CardMechanics
{
    public int SuccessChance { get; set; }
    public int FlowReward { get; set; }
    public Dictionary<ConnectionState, int> StateModifiers { get; set; } = new();
}
