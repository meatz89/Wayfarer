public class ActionFactory
{
    private readonly ActionRepository actionRepository;
    private readonly EncounterFactory encounterFactory;

    public ActionFactory(
        ActionRepository actionRepository,
        EncounterFactory encounterFactory)
    {
        this.actionRepository = actionRepository;
        this.encounterFactory = encounterFactory;
    }

    public ActionImplementation CreateActionFromTemplate(ActionDefinition template, string location, string locationSpot)
    {
        ActionImplementation actionImplementation = new ActionImplementation();

        // Transfer basic properties
        actionImplementation.Id = template.Id;
        actionImplementation.Name = template.Name;
        actionImplementation.Description = template.Description;
        actionImplementation.Difficulty = template.Difficulty;
        actionImplementation.LocationId = location;
        actionImplementation.LocationSpotId = locationSpot;

        // Handle movement actions
        if (!string.IsNullOrEmpty(template.MoveToLocation))
        {
            actionImplementation.DestinationLocation = template.MoveToLocation;
        }

        if (!string.IsNullOrEmpty(template.MoveToLocationSpot))
        {
            actionImplementation.DestinationLocationSpot = template.MoveToLocationSpot;
        }

        // Set encounter properties
        actionImplementation.Goal = template.Goal;
        actionImplementation.Complication = template.Complication;
        actionImplementation.EncounterType = template.EncounterType;

        // Convert SpotXp from float to int if needed
        actionImplementation.SpotXp = (int)template.SpotXp;

        // Set action type based on whether it's a one-time encounter
        actionImplementation.ActionType = template.IsOneTimeEncounter ? ActionTypes.Encounter : ActionTypes.Basic;

        // Create requirements, costs, and yields
        actionImplementation.Requirements = CreateRequirements(template);
        actionImplementation.Costs = CreateCosts(template);
        actionImplementation.Yields = CreateYields(template);

        // Create encounter template if needed
        ActionGenerationContext context = actionImplementation.GetActionGenerationContext();
        EncounterTemplate encounterTemplate = encounterFactory.GetDefaultEncounterTemplate();

        actionImplementation.Requirements.Add(new ActionPointRequirement(1));
        actionImplementation.Costs.Add(new ActionPointOutcome(-1));

        return actionImplementation;
    }

    private List<IRequirement> CreateRequirements(ActionDefinition template)
    {
        List<IRequirement> requirements = new();

        // Time window requirement
        if (template.TimeWindows != null && template.TimeWindows.Count > 0)
        {
            requirements.Add(new TimeWindowRequirement(template.TimeWindows));
        }

        // Resource requirements - use specific requirement types
        if (template.EnergyCost > 0)
        {
            requirements.Add(new EnergyRequirement(template.EnergyCost));
        }

        if (template.HealthCost > 0)
        {
            requirements.Add(new HealthRequirement(template.HealthCost));
        }

        if (template.ConcentrationCost > 0)
        {
            requirements.Add(new ConcentrationRequirement(template.ConcentrationCost));
        }

        if (template.ConfidenceCost > 0)
        {
            requirements.Add(new ConfidenceRequirement(template.ConfidenceCost));
        }

        if (template.CoinCost > 0)
        {
            requirements.Add(new CoinRequirement(template.CoinCost));
        }

        return requirements;
    }

    private List<Outcome> CreateCosts(ActionDefinition template)
    {
        // Only add costs that have a value greater than 0
        List<Outcome> costs = new();

        if (template.TimeWindowCost is not null && template.TimeWindowCost != string.Empty)
        {
            costs.Add(new TimeOutcome(template.TimeWindowCost));
        }

        if (template.EnergyCost > 0)
        {
            costs.Add(new EnergyOutcome(-template.EnergyCost));
        }

        if (template.HealthCost > 0)
        {
            costs.Add(new HealthOutcome(-template.HealthCost));
        }

        if (template.ConcentrationCost > 0)
        {
            costs.Add(new ConcentrationOutcome(-template.ConcentrationCost));
        }

        if (template.ConfidenceCost > 0)
        {
            costs.Add(new ConfidenceOutcome(-template.ConfidenceCost));
        }

        if (template.CoinCost > 0)
        {
            costs.Add(new CoinOutcome(-template.CoinCost));
        }

        return costs;
    }

    private List<Outcome> CreateYields(ActionDefinition template)
    {
        List<Outcome> yields = new();

        // Resource gains
        if (template.RestoresEnergy > 0)
        {
            yields.Add(new EnergyOutcome(template.RestoresEnergy));
        }

        if (template.RestoresHealth > 0)
        {
            yields.Add(new HealthOutcome(template.RestoresHealth));
        }

        if (template.RestoresConcentration > 0)
        {
            yields.Add(new ConcentrationOutcome(template.RestoresConcentration));
        }

        if (template.RestoresConfidence > 0)
        {
            yields.Add(new ConfidenceOutcome(template.RestoresConfidence));
        }

        if (template.CoinGain > 0)
        {
            yields.Add(new CoinOutcome(template.CoinGain));
        }

        // Relationship gains
        if (template.RelationshipChanges != null)
        {
            foreach (RelationshipGain relationshipGain in template.RelationshipChanges)
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