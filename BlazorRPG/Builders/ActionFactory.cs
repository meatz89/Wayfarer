public class ActionFactory
{
    public ActionFactory(ActionRepository actionRepository)
    {
        ActionRepository = actionRepository;
    }

    public ActionRepository ActionRepository { get; }
    
    public ActionImplementation CreateActionFromTemplate(ActionTemplate template)
    {
        ActionImplementation actionImplementation = new ActionImplementation();
        actionImplementation.ActionId = template.ActionId;
        actionImplementation.Name = template.Name;
        actionImplementation.Description = template.Description;
        actionImplementation.Goal = template.Goal;
        actionImplementation.Complication = template.Complication;
        actionImplementation.BasicActionType = template.BasicActionType;
        actionImplementation.TimeWindows = template.TimeWindows ?? new();
        actionImplementation.IsRepeatable = template.IsRepeatable;

        Random random = new Random();
        int r = random.Next(random.Next(0, 100));
        bool startsEncounter = template.EncounterChance > r;

        actionImplementation.EncounterChance = template.EncounterChance;
        actionImplementation.ActionType = startsEncounter ? ActionTypes.Encounter : ActionTypes.Basic;

        // Set time cost based on action type or template
        actionImplementation.TimeCostHours = template.TimeCostHours > 0 ? template.TimeCostHours : 1;

        // Set current and destination locations
        actionImplementation.CurrentLocation = template.LocationName;

        // If this is a movement action with a target spot
        if (!string.IsNullOrEmpty(template.MoveToLocation))
        {
            actionImplementation.DestinationLocation = template.MoveToLocation;
        }
        
        if (!string.IsNullOrEmpty(template.MoveToLocationSpot))
        {
            actionImplementation.DestinationLocationSpot = template.MoveToLocationSpot;
        }

        // Add energy costs
        actionImplementation.Requirements = template.Requirements ?? new();
        actionImplementation.EnergyCosts = template.Energy ?? new();
        actionImplementation.Costs = template.Costs ?? new();
        actionImplementation.Rewards = template.Rewards ?? new();

        actionImplementation.EncounterTemplate = ActionRepository.GetEncounterForAction(template);

        return actionImplementation;
    }

}