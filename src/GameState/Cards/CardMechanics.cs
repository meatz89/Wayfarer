
// Card mechanics
public class CardMechanics
{
    public int SuccessChance { get; set; }
    public int FlowReward { get; set; }
    // DOMAIN COLLECTION PRINCIPLE: List<T> instead of Dictionary
    public List<ConnectionStateModifierEntry> StateModifiers { get; set; } = new List<ConnectionStateModifierEntry>();
}
