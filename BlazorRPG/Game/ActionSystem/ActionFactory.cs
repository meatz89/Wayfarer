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

        var context = actionImplementation.GetActionGenerationContext();
        EncounterTemplate encounterTemplate = GameContentBootstrapper.GetDefaultEncounterTemplate();

        return actionImplementation;
    }


    private List<Requirement> CreateRequirements(ActionDefinition template)
    {
        List<Requirement> requirements = new();

        if (template.TimeWindows != null && template.TimeWindows.Count > 0)
        {
            requirements.Add(new TimeRequirement(template.TimeWindows));
        }

        if (template.EnergyCost > 0)
        {
            requirements.Add(new EnergyRequirement(template.EnergyCost));
        }

        if (template.ConcentrationCost > 0)
        {
            requirements.Add(new ConcentrationRequirement(template.ConcentrationCost));
        }

        if (template.CoinCost > 0)
        {
            requirements.Add(new CoinRequirement(template.CoinCost));
        }

        if (template.ConfidenceCost > 0)
        {
            requirements.Add(new ConfidenceRequirement(template.ConfidenceCost));
        }

        return requirements;
    }

    private List<Outcome> CreateCosts(ActionDefinition template)
    {
        // Only add costs that have a value greater than 0
        List<Outcome> costs = new();

        if (template.TimeCost > 0)
        {
            costs.Add(new TimeOutcome(-template.TimeCost));
        }

        if (template.EnergyCost > 0)
        {
            costs.Add(new EnergyOutcome(-template.EnergyCost));
        }

        if (template.ConcentrationCost > 0)
        {
            costs.Add(new ConcentrationOutcome(-template.ConcentrationCost));
        }

        if (template.CoinCost > 0)
        {
            costs.Add(new CoinOutcome(-template.CoinCost));
        }

        if (template.ConfidenceCost > 0)
        {
            costs.Add(new ConfidenceOutcome(-template.ConfidenceCost));
        }

        return costs;
    }

    private List<Outcome> CreateYields(ActionDefinition template)
    {
        List<Outcome> yields = new();

        if (template.RestoresEnergy > 0)
        {
            yields.Add(new EnergyOutcome(template.RestoresEnergy));
        }

        if (template.CoinGain > 0)
        {
            yields.Add(new CoinOutcome(template.CoinGain));
        }

        if (template.RelationshipGains != null)
        {
            foreach (RelationshipGain relationshipGain in template.RelationshipGains)
            {
                if (relationshipGain.ChangeAmount > 0)
                {
                    yields.Add(new RelationshipOutcome(relationshipGain.CharacterName, relationshipGain.ChangeAmount));
                }
            }
        }

        return yields;
    }
}