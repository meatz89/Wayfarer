namespace Wayfarer.GameState;

public class LodgingService : PhysicalServiceType
{
    public override string Id => "lodging";

    public override string DisplayName => "Lodging";

    public override ChoiceReward GenerateRewards(int tier)
    {
        return new ChoiceReward
        {
            FullRecovery = true,
            TimeSegments = 6
        };
    }
}
