public class ActionFactory
{
    public ActionFactory(ActionRepository actionRepository)
    {
        ActionRepository = actionRepository;
    }

    public ActionRepository ActionRepository { get; }

    public ActionImplementation CreateActionFromTemplate(ActionTemplate template, EncounterTemplate encounterTemplate)
    {
        ActionImplementation actionImplementation = new ActionImplementation();

        actionImplementation.Name = template.Name;
        actionImplementation.Requirements = new List<Requirement>();
        actionImplementation.EnergyCosts = template.Energy ?? new();
        actionImplementation.Costs = template.Costs ?? new();
        actionImplementation.Rewards = template.Rewards ?? new();

        actionImplementation.Goal = template.Goal;
        actionImplementation.Complication = template.Complication;

        actionImplementation.BasicActionType = template.BasicActionType;
        actionImplementation.ActionType = template.ActionType;

        // Add energy costs
        int energyCost = GameRules.GetBaseEnergyCost(template.BasicActionType);

        actionImplementation.Requirements.Add(new EnergyRequirement(energyCost));
        actionImplementation.EnergyCosts.Add(new EnergyOutcome(-energyCost));

        if (encounterTemplate != null)
        {
            actionImplementation.EncounterTemplate = encounterTemplate;
        }

        return actionImplementation;
    }

}