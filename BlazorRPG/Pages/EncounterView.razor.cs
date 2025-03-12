using BlazorRPG.Game.EncounterManager;
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
    [Parameter] public EncounterManager Encounter { get; set; }

    public UserEncounterChoiceOption hoveredChoice;
    public bool showTooltip;
    public double mouseX;
    public double mouseY;

    public bool IsLoading = true;

    public EncounterViewModel Model => GameManager.GetEncounterViewModel();

    protected override async Task OnInitializedAsync()
    {
        if(Encounter == null)
        {
            await GameManager.GenerateEncounter();
            IsLoading = false;
        }
    }

    public async Task HandleChoiceSelection(UserEncounterChoiceOption choice)
    {
        IsLoading = true;
        if (IsChoiceDisabled(choice))
        {
            return;
        }

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

    public string GetChoiceDescription(UserEncounterChoiceOption choice)
    {
        IChoice choice1 = choice.Choice;
        Dictionary<IChoice, string> choiceDescriptions = Model.EncounterResult.NarrativeResult.ChoiceDescriptions;
        string description = choiceDescriptions[choice1];
        return description;
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

        // Add activation information for inactive tags
        if (!Encounter.State.ActiveTags.Any(t => t.Name == tag.Name))
        {
            tooltip.AppendLine("\nActivation Condition:");
            tooltip.AppendLine(tag.GetActivationDescription());
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

    public string GetProjectedValue(ValueTypes changeType)
    {
        if (hoveredChoice == null) return "";

        int currentValue = GetCurrentValue(changeType);
        int projectedChange = GetProjectedChange(changeType);
        int projectedValue = currentValue + projectedChange;

        // Only show the change if it's not zero, and add class for styling
        if (projectedChange == 0)
        {
            return "";
        }
        else
        {
            string sign = projectedChange > 0 ? "+" : "";
            string projectedValueString = $"{sign}{projectedChange}";
            return projectedValueString;
        }
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

    public List<DetailedChange> GetValueChanges(IChoice choice)
    {
        // Use the stored CalculationResult
        //if (choice.CalculationResult == null) return new List<DetailedChange>();
        //return ConvertDetailedChanges(choice.CalculationResult);
        return new List<DetailedChange>();
    }

    public List<DetailedChange> ConvertDetailedChanges(ChoiceCalculationResult calculationResult)
    {
        List<DetailedChange> detailedChanges = new List<DetailedChange>();

        // Add modifications
        foreach (ValueModification change in calculationResult.ValueModifications)
        {
            if (change is MomentumModification evm)
            {
                AddDetailedChange(detailedChanges, ValueTypes.Momentum, change.Source, change.Amount);
            }
            if (change is PressureModification evp)
            {
                AddDetailedChange(detailedChanges, ValueTypes.Pressure, change.Source, change.Amount);
            }
            else if (change is EnergyCostReduction em)
            {
                AddDetailedChange(detailedChanges, ConvertEnergyTypeToChangeType(em.EnergyType), change.Source, em.Amount);
            }
        }

        detailedChanges = SortDetailedChanges(detailedChanges);

        return detailedChanges;
    }

    public int GetProjectedChange(ValueTypes changeType)
    {
        //if (hoveredChoice == null || hoveredChoice.Choice.CalculationResult == null) return 0;

        int projectedChange = 0;
        foreach (DetailedChange detailedChange in GetValueChanges(hoveredChoice.Choice))
        {
            if (detailedChange.ChangeType == changeType)
            {
                projectedChange += detailedChange.ChangeValues.TotalAmount;
            }
        }
        return projectedChange;
    }

    public void AddDetailedChange(List<DetailedChange> combined, ValueTypes changeType, string source, int amount)
    {
        bool found = false;
        foreach (DetailedChange dc in combined)
        {
            if (dc.ChangeType == changeType)
            {
                dc.ChangeValues.TotalAmount += amount;
                dc.ChangeValues.Sources.Add($"{source}: {(amount >= 0 ? "+" : "")}{amount}");
                found = true;
                break;
            }
        }

        if (!found)
        {
            combined.Add(new DetailedChange
            {
                ChangeType = changeType,
                ChangeValues = new ChangeValues
                {
                    TotalAmount = amount,
                    Sources = new List<string> { $"{source}: {(amount >= 0 ? "+" : "")}{amount}" }
                }
            });
        }
    }

    public List<CombinedValue> ConvertCombinedValues(Dictionary<ValueTypes, int> combinedValuesDict)
    {
        List<CombinedValue> combinedValuesList = new List<CombinedValue>();
        foreach (KeyValuePair<ValueTypes, int> kvp in combinedValuesDict)
        {
            combinedValuesList.Add(new CombinedValue { ChangeType = kvp.Key, Amount = kvp.Value });
        }
        return combinedValuesList;
    }

    public bool IsChoiceDisabled(UserEncounterChoiceOption choice)
    {
        return false;
        //// Use the ModifiedRequirements for the disabled check
        //return choice.Choice.CalculationResult.Requirements.Any(req =>
        //    !req.IsSatisfied(GameState));
    }

    public List<DetailedChange> SortDetailedChanges(List<DetailedChange> changes)
    {
        // Define the order of ChangeTypes
        List<ValueTypes> order = new List<ValueTypes>()
        {
            ValueTypes.Momentum,
            ValueTypes.Pressure,
            ValueTypes.PhysicalEnergy,
            ValueTypes.Concentration,
            ValueTypes.Reputation
        };

        return changes.OrderBy(dc => order.IndexOf(dc.ChangeType)).ToList();
    }

    public MarkupString GetValueTypeIcon(ValueTypes valueType)
    {
        return valueType switch
        {
            ValueTypes.Momentum => new MarkupString("<i class='value-icon outcome-icon'>⭐</i>"),
            ValueTypes.Pressure => new MarkupString("<i class='value-icon outcome-icon'>⭐</i>"),
            ValueTypes.PhysicalEnergy => new MarkupString("<i class='value-icon physical-icon'>💪</i>"),
            ValueTypes.Concentration => new MarkupString("<i class='value-icon focus-icon'>🎯</i>"),
            ValueTypes.Reputation => new MarkupString("<i class='value-icon social-icon'>👥</i>"),
            _ => new MarkupString("")
        };
    }

    public ValueTypes ConvertEnergyTypeToChangeType(EnergyTypes energyType)
    {
        return energyType switch
        {
            EnergyTypes.Physical => ValueTypes.PhysicalEnergy,
            EnergyTypes.Concentration => ValueTypes.Concentration,
            _ => throw new ArgumentException("Invalid EnergyType")
        };
    }

    // Get the pressure status text
    public string GetPressureStatusText()
    {
        if (Model.State.Pressure < 3)
            return "Normal";
        if (Model.State.Pressure < 6)
            return "High";
        if (Model.State.Pressure < 8)
            return "Critical";
        if (Model.State.Pressure < 10)
            return "Extreme";
        return "Failure Imminent";
    }

    // Check if a tag is disabled by pressure
    public bool IsTagDisabledByPressure(string tagName)
    {
        return Model.State.IsTagDisabled(tagName);
    }

    // Modify GetActiveTags to include the tag name in PropertyDisplay
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

        // Add disabled tags (they're not in ActiveTags but should still be shown)
        foreach (string tagName in Encounter.State.GetDisabledTagNames())
        {
            IEncounterTag tag = Encounter.State.Location.AvailableTags.FirstOrDefault(t => t.Name == tagName);
            if (tag != null)
            {
                string icon = GetTagIcon(tag);
                string tooltipText = GetTagTooltipText(tag) + "\n[DISABLED BY HIGH PRESSURE]";
                string cssClass = GetTagCssClass(tag) + " active disabled";

                properties.Add(new PropertyDisplay
                {
                    Text = tag.Name,
                    Icon = icon,
                    TooltipText = tooltipText,
                    CssClass = cssClass,
                    TagName = tag.Name
                });
            }
        }

        return properties;
    }
}
