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

        actionImplementation.Name = template.Name;
        actionImplementation.Requirements = new List<Requirement>();
        actionImplementation.EnergyCosts = template.Energy ?? new();
        actionImplementation.Costs = template.Costs ?? new();
        actionImplementation.Rewards = template.Rewards ?? new();

        actionImplementation.Goal = template.Goal;
        actionImplementation.Complication = template.Complication;

        actionImplementation.BasicActionType = template.BasicActionType;
        actionImplementation.ActionType = template.ActionType;

        string encounterTemplateName = template.EncounterTemplateName;
        EncounterTemplate encounterTemplate = ActionRepository.GetEncounterTemplate(encounterTemplateName);
        if (encounterTemplate != null)
        {
            actionImplementation.EncounterTemplate = encounterTemplate;
        }

        // Add energy costs
        int energyCost = GameRules.GetBaseEnergyCost(template.BasicActionType);

        actionImplementation.Requirements.Add(new EnergyRequirement(energyCost));
        actionImplementation.EnergyCosts.Add(new EnergyOutcome(-energyCost));

        return actionImplementation;
    }

}