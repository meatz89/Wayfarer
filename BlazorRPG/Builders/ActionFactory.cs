public class ActionFactory
{
    public ActionFactory(ActionRepository actionRepository)
    {
        ActionRepository = actionRepository;
    }

    public ActionRepository ActionRepository { get; }
    public ActionImplementation CreateActionFromTemplate(SpotAction template, EncounterTemplate encounterTemplate = null)
    {
        ActionImplementation actionImplementation = new ActionImplementation();
        actionImplementation.ActionId = template.ActionId;
        actionImplementation.Name = template.Name;
        actionImplementation.Requirements = new List<Requirement>();
        actionImplementation.EnergyCosts = template.Energy ?? new();
        actionImplementation.Costs = template.Costs ?? new();
        actionImplementation.Rewards = template.Rewards ?? new();
        actionImplementation.Goal = template.Goal;
        actionImplementation.Complication = template.Complication;
        actionImplementation.BasicActionType = template.BasicActionType;
        actionImplementation.ActionType = template.ActionType;
        actionImplementation.TimeWindows = template.TimeWindows ?? new();

        // Set time cost based on action type or template
        actionImplementation.TimeCostHours = template.TimeCostHours > 0
            ? template.TimeCostHours
            : GameRules.GetBaseTimeCost(template.BasicActionType);

        // Set current and destination locations
        actionImplementation.CurrentLocation = template.LocationName;

        // If this is a movement action with a target spot
        if (!string.IsNullOrEmpty(template.LocationSpotTarget))
        {
            actionImplementation.DestinationLocation = template.LocationSpotTarget;
        }

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