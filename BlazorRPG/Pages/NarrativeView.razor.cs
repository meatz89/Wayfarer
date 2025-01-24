using Microsoft.AspNetCore.Components;

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

    public List<Outcome> GetActionOutcomes()
    {
        EncounterContext context = Result.encounter.Context;
        ActionImplementation actionImplementation = context.ActionImplementation;
        List<OutcomeCondition> outcomeConditions = actionImplementation.OutcomeConditions;
        if (outcomeConditions != null && outcomeConditions.Count > 0)
        {
            OutcomeCondition? outcome = outcomeConditions.FirstOrDefault(oc => oc.EncounterResults == Result.encounterResults);
            List<Outcome> outcomes = outcome.Outcomes;
            return outcomes;
        }

        return new();
    }

    public List<Outcome> GetEnergyCosts()
    {
        return Result.encounter.Context.ActionImplementation.EnergyCosts
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
            EnergyTypes.Focus => new MarkupString("<i class='value-icon focus-icon'>🎯</i>"),
            EnergyTypes.Social => new MarkupString("<i class='value-icon social-icon'>👥</i>"),
            _ => new MarkupString("")
        };
    }
}