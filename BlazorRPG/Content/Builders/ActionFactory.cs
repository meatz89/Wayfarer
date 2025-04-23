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
        actionImplementation.TimeWindows = template.AvailableWindows ?? new();
        actionImplementation.IsRepeatable = template.IsRepeatable;
        actionImplementation.Difficulty = template.Difficulty;
        actionImplementation.Yields = template.Yields;
        actionImplementation.EncounterType = template.EncounterType;

        Random random = new Random();
        int r = random.Next(random.Next(0, 100));
        bool startsEncounter = template.EncounterChance > r;

        actionImplementation.EncounterChance = template.EncounterChance;
        actionImplementation.ActionType = startsEncounter ? ActionTypes.Encounter : ActionTypes.Basic;

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
        actionImplementation.EncounterType = template.EncounterType;

        actionImplementation.Requirements = template.Requirements ?? new();
        actionImplementation.Yields = template.Yields ?? new();
        actionImplementation.Costs = template.Costs ?? new();

        actionImplementation.EncounterTemplate = GetEncounterForAction(template);

        return actionImplementation;
    }

    public EncounterTemplate GetEncounterForAction(ActionDefinition actionTemplate)
    {
        ActionGenerationContext context = new ActionGenerationContext
        {
            ActionId = actionTemplate.Id,
            Goal = actionTemplate.Goal,
            Complication = actionTemplate.Complication,
            BasicActionType = actionTemplate.EncounterType.ToString(),
            SpotName = actionTemplate.LocationSpotName,
            LocationName = actionTemplate.LocationName,
        };

        EncounterTemplate encounterTemplate = WorldEncounterContent.GetDefaultTemplate();

        return encounterTemplate;
    }
}