public class ActionFactory
{
    public ActionFactory(ActionRepository actionRepository)
    {
        ActionRepository = actionRepository;
    }

    public ActionRepository ActionRepository { get; }

    public ActionImplementation CreateActionFromTemplate(ActionDefinition template)
    {
        ActionImplementation actionImplementation = new ActionImplementation();
        actionImplementation.Id = template.Id;
        actionImplementation.Description = template.Description;
        actionImplementation.Goal = template.Goal;
        actionImplementation.Complication = template.Complication;
        actionImplementation.EncounterType = template.EncounterType;
        actionImplementation.TimeWindows = template.TimeWindows ?? new();
        actionImplementation.IsRepeatable = template.IsRepeatable;
        actionImplementation.Difficulty = template.Difficulty;
        actionImplementation.Yields = template.Yields;
        actionImplementation.EnergyCost = template.EnergyCost;
        actionImplementation.TimeCost = template.TimeCost;
        actionImplementation.EncounterType = template.EncounterType;
        actionImplementation.Category = template.Category;

        Random random = new Random();
        int r = random.Next(random.Next(0, 100));
        bool startsEncounter = template.EncounterChance > r;

        actionImplementation.EncounterChance = template.EncounterChance;
        actionImplementation.ActionType = startsEncounter ? ActionTypes.Encounter : ActionTypes.Basic;

        // Set time cost based on action type or template
        actionImplementation.TimeCostHours = template.TimeCost > 0 ? template.TimeCost : 1;

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
        actionImplementation.Costs = template.Costs ?? new();
        actionImplementation.Rewards = template.Rewards ?? new();


        actionImplementation.EnergyCost = template.EnergyCost;
        actionImplementation.TimeCost = template.TimeCost;
        actionImplementation.EncounterType = template.EncounterType;
        actionImplementation.Category = template.Category;
        actionImplementation.Yields = template.Yields ?? new();

        actionImplementation.EncounterTemplate = ActionRepository.GetEncounterForAction(template);

        return actionImplementation;
    }

}