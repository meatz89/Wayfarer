public class AlwaysActiveCondition : ActivationCondition
{
    public override bool IsActive(EncounterTagSystem tagSystem) => true;
    public override string GetDescription() => "Always active";
}