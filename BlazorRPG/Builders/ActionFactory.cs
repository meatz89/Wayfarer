using System.Xml.Linq;

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

        actionImplementation.ActionType = template.ActionType;
        actionImplementation.Name = template.Name;
        actionImplementation.Requirements = new List<Requirement>();
        actionImplementation.EnergyCosts = template.Energy;
        actionImplementation.Costs = template.Costs;
        actionImplementation.Rewards = template.Rewards;

        actionImplementation.Goal = template.Goal;
        actionImplementation.Complication = template.Complication;

        actionImplementation.IsEncounterAction = template.IsEncounterAction;

        EncounterTemplate encounterTemplate = ActionRepository.GetEncounterTemplate(template.EncounterTemplateName);
        if (encounterTemplate != null)
        {
            actionImplementation.EncounterTemplate = encounterTemplate;
        }

        // Add energy costs
        EnergyTypes energyType = GameRules.GetEnergyTypeForAction(template.ActionType);
        int energyCost = GameRules.GetBaseEnergyCost(template.ActionType);

        actionImplementation.Requirements.Add(new EnergyRequirement(energyType, energyCost));
        actionImplementation.EnergyCosts.Add(new EnergyOutcome(energyType, -energyCost));

        return actionImplementation;
    }

}