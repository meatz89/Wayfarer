public class ApproachCost
{
    public int EnergyCost { get; private set; }
    public int MoneyCost { get; private set; }
    public int ReputationImpact { get; private set; }
    public TimeSpan TimeCost { get; private set; }

    public ApproachCost(int energyCost, int moneyCost, int reputationImpact, TimeSpan timeCost)
    {
        EnergyCost = energyCost;
        MoneyCost = moneyCost;
        ReputationImpact = reputationImpact;
        TimeCost = timeCost;
    }
}