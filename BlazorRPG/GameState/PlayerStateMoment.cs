public class PlayerStateMoment
{
    public int Food { get; }
    public int MedicinalHerbs { get; }
    public int Energy { get; }
    public int Health { get; }
    public int Concentration { get; }
    public int Confidence { get; }

    public PlayerStateMoment(PlayerState playerState)
    {
        Food = playerState.Food;
        MedicinalHerbs = playerState.MedicinalHerbs;
        Energy = playerState.Energy;
        Health = playerState.Health;
        Concentration = playerState.Concentration;
        Confidence = playerState.Confidence;
    }
}