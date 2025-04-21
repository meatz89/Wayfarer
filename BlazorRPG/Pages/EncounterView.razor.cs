using BlazorRPG.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Text;
public partial class EncounterViewBase : ComponentBase
{
    [Inject] public IJSRuntime JSRuntime { get; set; }
    [Inject] public GameState GameState { get; set; }
    [Inject] public GameManager GameManager { get; set; }
    [Parameter] public EncounterManager EncounterManager { get; set; }
    [Parameter] public EventCallback<EncounterResult> OnEncounterCompleted { get; set; }
    public PlayerState PlayerState => GameState.PlayerState;

    public UserEncounterChoiceOption hoveredChoice;
    public bool showTooltip;
    public double mouseX;
    public double mouseY;

    public bool IsLoading = true;
    public ApproachTags[] GetApproachTags() => Enum.GetValues<ApproachTags>().Where(x => x != ApproachTags.None).ToArray();
    public FocusTags[] GetFocusTags() => Enum.GetValues<FocusTags>().Where(x => true).ToArray();
    public EncounterResult EncounterResult { get; private set; }
    public List<UserEncounterChoiceOption> CurrentChoices = new List<UserEncounterChoiceOption>();

    public bool IsChoiceDisabled(UserEncounterChoiceOption userEncounterChoiceOption) => userEncounterChoiceOption.Choice.IsBlocked;
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

    public List<PropertyDisplay> GetLocationTags()
    {
        List<PropertyDisplay> properties = new List<PropertyDisplay>();

        return properties;
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
            if (EncounterManager.EncounterState.ActiveTags.Any(t => t.NarrativeName == tag.NarrativeName))
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
        IEncounterTag tag = EncounterManager.EncounterState.ActiveTags.FirstOrDefault(t => t.NarrativeName == tagName);
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

    public async Task ShowTooltip(UserEncounterChoiceOption choice, MouseEventArgs e)
    {
        hoveredChoice = choice;
        showTooltip = true;
        mouseX = e.ClientX + 10;
        mouseY = e.ClientY + 10;

        // Get dimensions using JavaScript interop
        Dimensions dimensions = await JSRuntime.InvokeAsync<Dimensions>("getDimensions");

        // Adjust mouseY if the tooltip would overflow
        if (mouseY + dimensions.TooltipHeight > dimensions.WindowHeight)
        {
            mouseY = e.ClientY - dimensions.TooltipHeight - 10; // Position above, with offset
        }
    }

    public void HideTooltip()
    {
        hoveredChoice = null;
        showTooltip = false;
    }

    public void OnMouseMove(MouseEventArgs e)
    {
        mouseX = e.ClientX + 10;
        mouseY = e.ClientY + 10;
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
