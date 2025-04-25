
public class ActionFactory
{
    public ActionFactory(ActionRepository actionRepository)
    {
        ActionRepository = actionRepository;
    }

    public ActionRepository ActionRepository { get; }

    public ActionImplementation CreateActionFromTemplate(ActionDefinition template, string location, string locationSpot)
    {
        ActionImplementation actionImplementation = new ActionImplementation();


        actionImplementation.Id = template.Id;
        actionImplementation.Name = template.Name;

        actionImplementation.Description = template.Description;
        actionImplementation.Difficulty = template.Difficulty;

        actionImplementation.LocationName = location;
        actionImplementation.LocationSpotName = locationSpot;

        // If this is a movement action with a target spot
        if (!string.IsNullOrEmpty(template.MoveToLocation))
        {
            actionImplementation.DestinationLocation = template.MoveToLocation;
        }

        if (!string.IsNullOrEmpty(template.MoveToLocationSpot))
        {
            actionImplementation.DestinationLocationSpot = template.MoveToLocationSpot;
        }

        actionImplementation.Goal = template.Goal;
        actionImplementation.Complication = template.Complication;
        actionImplementation.EncounterType = template.EncounterType;

        actionImplementation.EncounterType = template.EncounterType;
        actionImplementation.SpotXp = template.SpotXp;

        actionImplementation.ActionType = template.IsOneTimeEncounter ? ActionTypes.Encounter : ActionTypes.Basic;

        actionImplementation.Requirements = CreateRequirements(template);
        actionImplementation.Costs = CreateCosts(template);
        actionImplementation.Yields = CreateYields(template);

        actionImplementation.EncounterTemplate = actionImplementation.GetEncounterTemplate();

        return actionImplementation;
    }


    private List<Requirement> CreateRequirements(ActionDefinition template)
    {
        List<Requirement> requirements = new()
        {
            new TimeRequirement(template.TimeWindows),
            new EnergyRequirement(template.EnergyCost),
            new ConcentrationRequirement(template.ConcentrationCost),
            new CoinRequirement(template.CoinCost),
            new ConfidenceRequirement(template.ConfidenceCost),
        };
        return requirements;
    }

    private List<Outcome> CreateCosts(ActionDefinition template)
    {
        List<Outcome> costs = new()
        {
            new TimeOutcome(template.TimeCost),
            new EnergyOutcome(template.EnergyCost),
            new ConcentrationOutcome(template.ConcentrationCost),
            new CoinOutcome(template.CoinCost),
            new ConfidenceOutcome(template.ConfidenceCost),
        };
        return costs;
    }

    private List<Outcome> CreateYields(ActionDefinition template)
    {
        List<Outcome> yields = new()
        {
            new EnergyOutcome(template.RestoresEnergy),
            new CoinOutcome(template.CoinGain),
        };

        foreach (RelationshipChange relationshipGain in template.RelationshipGains)
        {
            yields.Add(new RelationshipOutcome(relationshipGain.CharacterName, relationshipGain.ChangeAmount));
        }

        return yields;
    }
}