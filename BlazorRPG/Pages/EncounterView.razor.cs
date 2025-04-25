using BlazorRPG.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Text;
public partial class EncounterViewBase : ComponentBase
{
    [Inject] public IJSRuntime JSRuntime { get; set; }

    [Inject] public GameState GameState { get; set; }
    [Inject] public GameManager GameManager { get; set; }
    [Parameter] public EncounterManager EncounterManager { get; set; }
    [Parameter] public EventCallback<EncounterResult> OnEncounterCompleted { get; set; }
    private IJSObjectReference _tooltipModule;
    public PlayerState PlayerState
    {
        get
        {
            return GameState.PlayerState;
        }
    }

    public UserEncounterChoiceOption hoveredChoice;
    public bool showTooltip;
    public double tooltipX;
    public double tooltipY;

    public bool IsLoading = true;
    public ApproachTags[] GetApproachTags()
    {
        return Enum.GetValues<ApproachTags>().Where(x =>
    {
        return x != ApproachTags.None;
    }).ToArray();
    }

    public FocusTags[] GetFocusTags()
    {
        return Enum.GetValues<FocusTags>().Where(x =>
        {
            return true;
        }).ToArray();
    }

    public EncounterResult EncounterResult { get; private set; }
    public List<UserEncounterChoiceOption> CurrentChoices = new List<UserEncounterChoiceOption>();

    public bool IsChoiceDisabled(UserEncounterChoiceOption userEncounterChoiceOption)
    {
        return userEncounterChoiceOption.Choice.IsBlocked;
    }

    public EncounterViewModel Model;

    protected override async Task OnInitializedAsync()
    {
        Model = GetModel();
        GetChoices();

        if (EncounterManager != null && Model != null)
        {
            IsLoading = false;
        }
        else
        {
            IsLoading = true;
        }

        StateHasChanged();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _tooltipModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./js/tooltipInterop.js");
        }
    }

    public async Task ShowTooltip(UserEncounterChoiceOption choice, string elementId)
    {
        hoveredChoice = choice;
        showTooltip = true;

        if (_tooltipModule != null)
        {
            TooltipPosition position = await _tooltipModule.InvokeAsync<TooltipPosition>(
                "getTooltipPositionRelativeToElement", $"{elementId}");

            tooltipX = position.TooltipX;
            tooltipY = position.TooltipY;
        }

        StateHasChanged();
    }

    public void HideTooltip()
    {
        hoveredChoice = null;
        showTooltip = false;
    }

    private EncounterViewModel GetModel()
    {
        EncounterViewModel? encounterViewModel = GameManager.GetEncounterViewModel();

        if (encounterViewModel == null)
        {
            encounterViewModel = CreateGameOverModel();
        }

        return encounterViewModel;
    }

    private EncounterViewModel CreateGameOverModel()
    {
        EncounterViewModel encounterViewModel = new EncounterViewModel()
        {
            ChoiceSetName = "None",
            CurrentChoices = new List<UserEncounterChoiceOption>(),
            CurrentEncounter = null,
            State = EncounterState.Last,
            EncounterResult = new EncounterResult()
            {
                ActionImplementation = null,
                ActionResult = ActionResults.GameOver,
                EncounterEndMessage = "Game Over",
                NarrativeContext = null,
                PostEncounterEvolution = null,
                NarrativeResult = null
            }
        };

        return encounterViewModel;
    }

    public async Task HandleChoiceSelection(UserEncounterChoiceOption choice)
    {
        hoveredChoice = null;
        showTooltip = false;
        IsLoading = true;

        EncounterResult result = await GameManager.ExecuteEncounterChoice(choice);
        await CheckEncounterCompleted(result);

        Model = GetModel();
        GetChoices();

        HideTooltip();
        IsLoading = false;

        StateHasChanged();
    }

    private async Task CheckEncounterCompleted(EncounterResult result)
    {
        if (result.ActionResult == ActionResults.Ongoing)
        {
            EncounterResult = result;
        }
        else
        {
            await OnEncounterCompleted.InvokeAsync(result);
        }
    }

    public void GetChoices()
    {
        CurrentChoices = GameManager.GetChoices();
        StateHasChanged();
    }

    public string GetChoiceName(UserEncounterChoiceOption choiceOption)
    {
        CardDefinition card = choiceOption.Choice;
        NarrativeResult narrativeResult = Model.EncounterResult.NarrativeResult;
        Dictionary<CardDefinition, ChoiceNarrative> choiceDescriptions = narrativeResult?.ChoiceDescriptions;
        ChoiceNarrative choiceNarrative = null;

        if (choiceDescriptions != null && choiceDescriptions.ContainsKey(card))
            choiceNarrative = choiceDescriptions[card];

        string name = $"{card.GetName()}";
        if (choiceNarrative != null)
        {
            name = choiceNarrative.ShorthandName;
        }
        return name;
    }

    public List<PropertyDisplay> GetAvailableTags()
    {
        List<PropertyDisplay> properties = new List<PropertyDisplay>();

        if (EncounterManager.EncounterState?.EncounterInfo?.AllEncounterTags == null)
            return properties;

        // Get all available tags that aren't currently active
        foreach (IEncounterTag tag in EncounterManager.EncounterState.EncounterInfo.AllEncounterTags)
        {
            // Skip if the tag is already active
            if (EncounterManager.EncounterState.ActiveTags.Any(t =>
            {
                return t.NarrativeName == tag.NarrativeName;
            }))
                continue;

            string icon = GetTagIcon(tag);
            string tooltipText = GetTagTooltipText(tag);
            string cssClass = GetTagCssClass(tag);

            properties.Add(new PropertyDisplay
            {
                Text = tag.NarrativeName,
                Icon = icon,
                TooltipText = tooltipText,
                CssClass = cssClass,
                TagName = "",
            });
        }

        return properties;
    }

    private string GetTagIcon(IEncounterTag tag)
    {
        // Determine icon based on tag type
        if (tag is NarrativeTag)
            return "📜"; // Narrative tag icon
        else if (tag is StrategicTag)
            return "⚙️"; // Strategic tag icon

        return "🏷️"; // Default tag icon
    }

    private string GetTagCssClass(IEncounterTag tag)
    {
        // Determine CSS class based on tag type
        if (tag is NarrativeTag)
            return "narrative-tag";
        else if (tag is StrategicTag)
            return "strategic-tag";

        return "";
    }

    private string GetTagTooltipText(IEncounterTag tag)
    {
        StringBuilder tooltip = new StringBuilder();
        tooltip.AppendLine(tag.NarrativeName);

        if (tag is NarrativeTag narrativeTag)
        {
            if (narrativeTag.AffectedFocus != null)
                tooltip.AppendLine($"{narrativeTag.GetEffectDescription()}");
        }
        else if (tag is StrategicTag strategicTag)
        {
            tooltip.AppendLine(strategicTag.GetEffectDescription());
        }

        return tooltip.ToString();
    }

    public string GetTagEffectDescription(string tagName)
    {
        // Find the tag by name and use its description method
        IEncounterTag tag = EncounterManager.EncounterState.ActiveTags.FirstOrDefault(t =>
        {
            return t.NarrativeName == tagName;
        });
        if (tag is StrategicTag strategicTag)
        {
            return strategicTag.GetEffectDescription();
        }
        else if (tag is NarrativeTag narrativeTag)
        {
            return $"{narrativeTag.GetEffectDescription()}";
        }

        return "Affects encounter mechanics";
    }

    public int GetCurrentValue(ValueTypes changeType)
    {
        EncounterState state = Model.State;
        switch (changeType)
        {
            case ValueTypes.Momentum:
                return state.Momentum;

            case ValueTypes.Pressure:
                return state.Pressure;
        }
        return 0;
    }

    public MarkupString GetValueTypeIcon(ValueTypes valueType)
    {
        return valueType switch
        {
            ValueTypes.Momentum => new MarkupString("<i class='value-icon outcome-icon'>⭐</i>"),
            ValueTypes.Pressure => new MarkupString("<i class='value-icon outcome-icon'>⭐</i>"),
            ValueTypes.Health => new MarkupString("<i class='value-icon physical-icon'>⚡</i>"),
            ValueTypes.Concentration => new MarkupString("<i class='value-icon focus-icon'>🎯</i>"),
            ValueTypes.Confidence => new MarkupString("<i class='value-icon social-icon'>👥</i>"),
            _ => new MarkupString("")
        };
    }

    public List<PropertyDisplay> GetActiveTags()
    {
        List<PropertyDisplay> properties = new List<PropertyDisplay>();

        if (EncounterManager.EncounterState?.ActiveTags == null)
            return properties;

        foreach (IEncounterTag tag in EncounterManager.EncounterState.ActiveTags)
        {
            string icon = GetTagIcon(tag);
            string tooltipText = GetTagTooltipText(tag);
            string cssClass = GetTagCssClass(tag) + " active";

            properties.Add(new PropertyDisplay
            {
                Text = tag.NarrativeName,
                Icon = icon,
                TooltipText = tooltipText,
                CssClass = cssClass,
                TagName = tag.NarrativeName // Add the tag name
            });
        }

        return properties;
    }
}


public class TooltipPosition
{
    public double TooltipX { get; set; }
    public double TooltipY { get; set; }
}