public class ActionTemplateBuilder
{
    private string name;
    private string description;
    private BasicActionTypes actionType;

    public List<Requirement> requirements = new();
    public List<Outcome> energy = new();
    public List<Outcome> costs = new();
    public List<Outcome> rewards = new();

    public bool IsEncounterAction = false;

    public ActionTemplateBuilder WithName(string name)
    {
        this.name = name;
        return this;
    }

    public ActionTemplateBuilder WithDescription(string description)
    {
        this.description = description;
        return this;
    }

    public ActionTemplateBuilder WithActionType(BasicActionTypes actionType)
    {
        this.actionType = actionType;
        return this;
    }

    public ActionTemplateBuilder StartsEncounter()
    {
        this.IsEncounterAction = true;
        return this;
    }


    public ActionTemplateBuilder ExpendsCoins(int cost)
    {
        if (cost < 0) return this;

        requirements.Add(new CoinsRequirement(cost));
        costs.Add(new CoinsOutcome(-cost));
        return this;
    }

    public ActionTemplate Build()
    {
        // Add validation to ensure required properties are set
        if (string.IsNullOrEmpty(name))
        {
            throw new InvalidOperationException("ActionTemplate must have a name.");
        }
        return new ActionTemplate(
            name,
            description,
            actionType,
            IsEncounterAction,
            requirements,
            energy,
            costs,
            rewards
        );
    }
}
