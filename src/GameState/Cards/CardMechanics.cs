
// Card mechanics
public class CardMechanics
{
    public int SuccessChance { get; set; }
    public int FlowReward { get; set; }

    // DOMAIN COLLECTION PRINCIPLE: Explicit properties for fixed enum (ConnectionState)
    public int DisconnectedModifier { get; set; }
    public int GuardedModifier { get; set; }
    public int NeutralModifier { get; set; }
    public int ReceptiveModifier { get; set; }
    public int TrustingModifier { get; set; }

    public int GetStateModifier(ConnectionState state) => state switch
    {
        ConnectionState.DISCONNECTED => DisconnectedModifier,
        ConnectionState.GUARDED => GuardedModifier,
        ConnectionState.NEUTRAL => NeutralModifier,
        ConnectionState.RECEPTIVE => ReceptiveModifier,
        ConnectionState.TRUSTING => TrustingModifier,
        _ => 0
    };
}
