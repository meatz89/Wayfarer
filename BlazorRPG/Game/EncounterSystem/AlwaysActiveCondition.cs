public class AlwaysActiveCondition : ActivationCondition
{
    public override bool IsActive(BaseTagSystem tagSystem) => true;
    public override string GetDescription() => "Always active";
}