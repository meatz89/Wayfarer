using BlazorRPG.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Text;
public partial class EncounterViewBase : ComponentBase
{
    [Inject] public IJSRuntime JSRuntime { get; set; } // Inject IJSRuntime
    [Inject] public GameState GameState { get; set; }
    [Inject] public GameManager GameManager { get; set; }
    [Parameter] public EventCallback<EncounterResult> OnEncounterCompleted { get; set; }

    public UserEncounterChoiceOption hoveredChoice;
    public bool showTooltip;
    public double mouseX;
    public double mouseY;

    public bool IsLoading = true;


    // Add these methods to expose the enum values to the view
    public ApproachTags[] GetApproachTags() => Enum.GetValues<ApproachTags>().Where(x => x != ApproachTags.None).ToArray();
    public FocusTags[] GetFocusTags() => Enum.GetValues<FocusTags>().Where(x => true).ToArray();

    public bool IsChoiceDisabled(UserEncounterChoiceOption userEncounterChoiceOption) => userEncounterChoiceOption.Choice.IsBlocked;

    public EncounterViewModel GetModel()
    {
        return GameManager.GetEncounterViewModel();
    }

    protected override async Task OnInitializedAsync()
    {
        EncounterManager encounterManager = GetEncounter();
        if (encounterManager == null)
        {
            await GameManager.PrepareEncounter();
            IsLoading = false;
        }
    }

    public EncounterManager GetEncounter()
    {
        if (IsLoading)
            return null;

        EncounterManager encounterManager = GameState.Actions.GetCurrentEncounter();
        return encounterManager;
    }

    public async Task HandleChoiceSelection(UserEncounterChoiceOption choice)
    {
        hoveredChoice = null;
        showTooltip = false;
        IsLoading = true;

        EncounterResult result = await GameManager.ExecuteEncounterChoice(choice);

        await OnEncounterCompleted.InvokeAsync(result);
        HideTooltip();
        IsLoading = false;
    }

    public List<PropertyDisplay> GetLocationTags()
    {
        List<PropertyDisplay> properties = new List<PropertyDisplay>();

        return properties;
    }

    public List<UserEncounterChoiceOption> GetChoices()
    {
        List<UserEncounterChoiceOption> userEncounterChoiceOptions = GetModel().CurrentChoices;
        return userEncounterChoiceOptions;
    }

    public string GetChoiceName(UserEncounterChoiceOption choiceOption)
    {
        IChoice card = choiceOption.Choice;
        NarrativeResult narrativeResult = GetModel().EncounterResult.NarrativeResult;
        Dictionary<IChoice, ChoiceNarrative> choiceDescriptions = narrativeResult.ChoiceDescriptions;
        ChoiceNarrative choiceNarrative = null;

        if (choiceDescriptions != null && choiceDescriptions.ContainsKey(card))
            choiceNarrative = choiceDescriptions[card];

        string name = choiceOption.Description;
        if (choiceNarrative != null)
        {
            name = choiceNarrative.ShorthandName;
        }
        return name;
    }

    public List<PropertyDisplay> GetAvailableTags()
    {
        List<PropertyDisplay> properties = new List<PropertyDisplay>();

        if (GetEncounter().encounterState?.Location?.AvailableTags == null)
            return properties;

        // Get all available tags that aren't currently active
        foreach (IEncounterTag tag in GetEncounter().encounterState.Location.AvailableTags)
        {
            // Skip if the tag is already active
            if (GetEncounter().encounterState.ActiveTags.Any(t => t.Name == tag.Name))
                continue;

            string icon = GetTagIcon(tag);
            string tooltipText = GetTagTooltipText(tag);
            string cssClass = GetTagCssClass(tag);

            properties.Add(new PropertyDisplay
            {
                Text = tag.Name,
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
        tooltip.AppendLine(tag.Name);

        if (tag is NarrativeTag narrativeTag)
        {
            if (narrativeTag.BlockedFocus != null)
                tooltip.AppendLine($"Blocks {narrativeTag.BlockedFocus} focus");
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
        IEncounterTag tag = GetEncounter().encounterState.ActiveTags.FirstOrDefault(t => t.Name == tagName);
        if (tag is StrategicTag strategicTag)
        {
            return strategicTag.GetEffectDescription();
        }
        else if (tag is NarrativeTag narrativeTag && narrativeTag.BlockedFocus != null)
        {
            return $"Blocks {narrativeTag.BlockedFocus} focus";
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
        EncounterState state = GetModel().State;
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
            ValueTypes.Health => new MarkupString("<i class='value-icon physical-icon'>💪</i>"),
            ValueTypes.Concentration => new MarkupString("<i class='value-icon focus-icon'>🎯</i>"),
            ValueTypes.Confidence => new MarkupString("<i class='value-icon social-icon'>👥</i>"),
            _ => new MarkupString("")
        };
    }

    public List<PropertyDisplay> GetActiveTags()
    {
        List<PropertyDisplay> properties = new List<PropertyDisplay>();
        EncounterManager encounterManager = GetEncounter();

        if (encounterManager.encounterState?.ActiveTags == null)
            return properties;

        foreach (IEncounterTag tag in encounterManager.encounterState.ActiveTags)
        {
            string icon = GetTagIcon(tag);
            string tooltipText = GetTagTooltipText(tag);
            string cssClass = GetTagCssClass(tag) + " active";

            properties.Add(new PropertyDisplay
            {
                Text = tag.Name,
                Icon = icon,
                TooltipText = tooltipText,
                CssClass = cssClass,
                TagName = tag.Name // Add the tag name
            });
        }

        return properties;
    }
}
