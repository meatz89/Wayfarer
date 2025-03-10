using BlazorRPG.Game.EncounterManager;
using BlazorRPG.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using System.Collections.Generic;

public partial class EncounterViewBase : ComponentBase
{
    [Inject] public IJSRuntime JSRuntime { get; set; } // Inject IJSRuntime
    [Inject] public GameManager GameManager { get; set; }
    [Parameter] public EventCallback<EncounterResult> OnEncounterCompleted { get; set; }
    [Parameter] public Encounter Encounter { get; set; }

    public UserEncounterChoiceOption hoveredChoice;
    public bool showTooltip;
    public double mouseX;
    public double mouseY;

    public EncounterViewModel Model => GameManager.GetEncounterViewModel();

    public List<PropertyDisplay> GetLocationTags()
    {
        List<PropertyDisplay> properties = new List<PropertyDisplay>();

        return properties;
    }

    public List<PropertyDisplay> GetAvailableTags()
    {
        List<PropertyDisplay> properties = new List<PropertyDisplay>();

        return properties;
    }

    public List<PropertyDisplay> GetAvailableTagsPlayer()
    {
        List<PropertyDisplay> properties = new List<PropertyDisplay>();

        return properties;
    }

    public List<PropertyDisplay> GetActiveTags()
    {
        List<PropertyDisplay> properties = new List<PropertyDisplay>();

        return properties;
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

    public void HandleChoiceSelection(UserEncounterChoiceOption choice)
    {
        if (IsChoiceDisabled(choice))
        {
            return;
        }

        EncounterResult result = GameManager.ExecuteEncounterChoice(choice);
        OnEncounterCompleted.InvokeAsync(result);
        HideTooltip();
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

}
