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
            ProcessOutcomes();
        }
    }

    public void ProcessOutcomes()
    {
        if (Result.encounterResults == EncounterResults.EncounterSuccess)
        {
            foreach (Outcome reward in Result.encounter.Context.ActionImplementation.SuccessOutcomes)
            {
                reward.Apply(GameState.Player);
            }
        }
        else if (Result.encounterResults == EncounterResults.EncounterFailure)
        {
            foreach (Outcome cost in Result.encounter.Context.ActionImplementation.FailureOutcomes)
            {
                cost.Apply(GameState.Player);
            }
        }

        foreach (Outcome cost in GetEnergyCosts())
        {
            cost.Apply(GameState.Player);
        }
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