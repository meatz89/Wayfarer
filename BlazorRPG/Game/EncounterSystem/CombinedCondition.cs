public class CombinedCondition : ActivationCondition
{
    public List<ActivationCondition> Conditions { get; }
    public bool RequireAll { get; }

    public CombinedCondition(List<ActivationCondition> conditions, bool requireAll = true)
    {
        Conditions = conditions;
        RequireAll = requireAll;
    }

    public override bool IsActive(BaseTagSystem tagSystem)
    {
        if (RequireAll)
        {
            return Conditions.All(c => c.IsActive(tagSystem));
        }
        else
        {
            return Conditions.Any(c => c.IsActive(tagSystem));
        }
    }

    public override string GetDescription()
    {
        if (RequireAll)
        {
            return string.Join(" AND ", Conditions.Select(c => c.GetDescription()));
        }
        else
        {
            return string.Join(" OR ", Conditions.Select(c => c.GetDescription()));
        }
    }
}
