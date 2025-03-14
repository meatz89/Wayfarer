
using BlazorRPG.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Text;

public partial class EncounterViewBase : ComponentBase
{
    [Inject] public IJSRuntime JSRuntime { get; set; } // Inject IJSRuntime
    [Inject] public GameManager GameManager { get; set; }
    [Parameter] public EventCallback<EncounterResult> OnEncounterCompleted { get; set; }
    public EncounterManager Encounter => GameManager.EncounterSystem.Encounter;

    public UserEncounterChoiceOption hoveredChoice;
    public bool showTooltip;
    public double mouseX;
    public double mouseY;

    public bool IsLoading = true;

    public bool IsChoiceDisabled(UserEncounterChoiceOption userEncounterChoiceOption) => false;
    public EncounterViewModel Model => GameManager.GetEncounterViewModel();

    // Add these methods to expose the enum values to the view
    public EncounterStateTags[] GetEncounterStateTags() => Enum.GetValues<EncounterStateTags>();
    public ApproachTags[] GetApproachTags() => Enum.GetValues<ApproachTags>();
    public FocusTags[] GetFocusTags() => Enum.GetValues<FocusTags>();

    protected override async Task OnInitializedAsync()
    {
        if (Encounter == null)
        {
            await GameManager.GenerateEncounter();
            IsLoading = false;
        }
    }
    public async Task HandleChoiceSelection(UserEncounterChoiceOption choice)
    {
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
        List<UserEncounterChoiceOption> userEncounterChoiceOptions = Model.CurrentChoices;
        return userEncounterChoiceOptions;
    }

    public string GetChoiceName(UserEncounterChoiceOption choice)
    {
        IChoice choice1 = choice.Choice;
        NarrativeResult narrativeResult = Model.EncounterResult.NarrativeResult;
        Dictionary<IChoice, ChoiceNarrative> choiceDescriptions = narrativeResult.ChoiceDescriptions;
        ChoiceNarrative choiceNarrative = null;

        if (choiceDescriptions != null && choiceDescriptions.ContainsKey(choice1))
            choiceNarrative = choiceDescriptions[choice1];

        string name = choice.Description;
        if (choiceNarrative != null)
        {
            name = choiceNarrative.ShorthandName;
        }
        return name;
    }

    public List<PropertyDisplay> GetAvailableTags()
    {
        List<PropertyDisplay> properties = new List<PropertyDisplay>();

        if (Encounter?.State?.Location?.AvailableTags == null)
            return properties;

        // Get all available tags that aren't currently active
        foreach (IEncounterTag tag in Encounter.State.Location.AvailableTags)
        {
            // Skip if the tag is already active
            if (Encounter.State.ActiveTags.Any(t => t.Name == tag.Name))
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
            if (narrativeTag.BlockedApproach.HasValue)
                tooltip.AppendLine($"Blocks {narrativeTag.BlockedApproach.Value} approaches");
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
        IEncounterTag tag = Encounter.State.ActiveTags.FirstOrDefault(t => t.Name == tagName);
        if (tag is StrategicTag strategicTag)
        {
            return strategicTag.GetEffectDescription();
        }
        else if (tag is NarrativeTag narrativeTag && narrativeTag.BlockedApproach.HasValue)
        {
            return $"Blocks {narrativeTag.BlockedApproach.Value} approaches";
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
        switch (changeType)
        {
            case ValueTypes.Momentum:
                return Model.State.Momentum;

            case ValueTypes.Pressure:
                return Model.State.Pressure;
        }
        return 0;
    }



    public MarkupString GetValueTypeIcon(ValueTypes valueType)
    {
        return valueType switch
        {
            ValueTypes.Momentum => new MarkupString("<i class='value-icon outcome-icon'>⭐</i>"),
            ValueTypes.Pressure => new MarkupString("<i class='value-icon outcome-icon'>⭐</i>"),
            ValueTypes.PhysicalEnergy => new MarkupString("<i class='value-icon physical-icon'>💪</i>"),
            ValueTypes.Focus => new MarkupString("<i class='value-icon focus-icon'>🎯</i>"),
            ValueTypes.Confidence => new MarkupString("<i class='value-icon social-icon'>👥</i>"),
            _ => new MarkupString("")
        };
    }

    public List<PropertyDisplay> GetActiveTags()
    {
        List<PropertyDisplay> properties = new List<PropertyDisplay>();

        if (Encounter?.State?.ActiveTags == null)
            return properties;

        foreach (IEncounterTag tag in Encounter.State.ActiveTags)
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
