using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;

public partial class NarrativeViewBase : ComponentBase
{
    [Inject] public GameState GameState { get; set; }
    [Inject] public GameManager GameManager { get; set; }
    [Parameter] public LocationNames LocationName { get; set; }
    [Parameter] public EventCallback OnNarrativeCompleted { get; set; }
    [Parameter] public EncounterResult Result { get; set; }
    [Parameter] public bool ShowResult { get; set; } = false;

    public string NarrativeText { get; set; }

    protected override void OnParametersSet()
    {
        if (!ShowResult)
        {
            NarrativeText = GameManager.GetLocationNarrative(LocationName);
        }
        else
        {
            NarrativeText = Result.EncounterEndMessage;
        }
    }

    public List<Outcome> GetActionOutcomesSuccess()
    {
        ActionImplementation actionImplementation = Result.Encounter.ActionImplementation;

        List<Outcome> outcomes = new List<Outcome>();
        outcomes.AddRange(actionImplementation.Costs.ToList());
        outcomes.AddRange(actionImplementation.Rewards.ToList());

        return outcomes;
    }

    public List<Outcome> GetActionOutcomesFailure()
    {
        ActionImplementation actionImplementation = Result.Encounter.ActionImplementation;

        List<Outcome> outcomes = new List<Outcome>();
        outcomes.AddRange(actionImplementation.Costs.ToList());
        outcomes.AddRange(actionImplementation.Rewards.ToList());

        return outcomes;
    }

    public List<Outcome> GetEnergyCosts()
    {
        return Result.Encounter.ActionImplementation.EnergyCosts
            .ToList();
    }

    public MarkupString GetOutcomeIcon(Outcome outcome)
    {
        if (outcome is EnergyOutcome energyOutcome)
        {
            return GetEnergyTypeIcon(energyOutcome.EnergyType);
        }

        return outcome switch
        {
            HealthOutcome => new MarkupString("<i class='value-icon health-icon'>❤️</i>"),
            ConcentrationOutcome => new MarkupString("<i class='value-icon concentration-icon'>🌀</i>"),
            ReputationOutcome => new MarkupString("<i class='value-icon reputation-icon'>👤</i>"),
            CoinsOutcome => new MarkupString("<i class='value-icon coins-icon'>💰</i>"),
            ResourceOutcome => new MarkupString("<i class='value-icon resource-icon'>📦</i>"),
            KnowledgeOutcome => new MarkupString("<i class='value-icon knowledge-icon'>📚</i>"),
            _ => new MarkupString("")
        };
    }

    public MarkupString GetEnergyTypeIcon(EnergyTypes energyType)
    {
        return energyType switch
        {
            EnergyTypes.Physical => new MarkupString("<i class='value-icon physical-icon'>💪</i>"),
            EnergyTypes.Concentration => new MarkupString("<i class='value-icon focus-icon'>🎯</i>"),
            _ => new MarkupString("")
        };
    }
}