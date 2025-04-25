public class AlwaysActiveCondition : ActivationCondition
{
    public override bool IsActive(EncounterTagSystem tagSystem)
    {
        return true;
    }

    public override string GetDescription()
    {
        return "Always active";
    }
}