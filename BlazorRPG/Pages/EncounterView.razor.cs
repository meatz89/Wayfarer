using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

public partial class EncounterViewBase : ComponentBase
{
    [Inject] public IJSRuntime JSRuntime { get; set; } // Inject IJSRuntime
    [Parameter] public EventCallback<EncounterResult> OnEncounterCompleted { get; set; }
    [Parameter] public Encounter Encounter { get; set; }
    [Inject] public GameState GameState { get; set; }
    [Inject] public GameManager GameManager { get; set; }

    public UserEncounterChoiceOption hoveredChoice;
    public bool showTooltip;
    public double mouseX;
    public double mouseY;

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

    public string GetProjectedValue(ChangeTypes changeType)
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

    public int GetCurrentValue(ChangeTypes changeType)
    {
        Encounter currentEncounter = GameState.Actions.CurrentEncounter;
        EncounterStage encounterStage = currentEncounter.GetCurrentStage();
        EncounterStageContext encounterStageContext = encounterStage.EncounterStageContext;
        EncounterStageState currentValues = encounterStageContext.StageValues;
        return currentValues.Momentum;
    }

    public List<DetailedChange> GetValueChanges(EncounterChoice choice)
    {
        // Use the stored CalculationResult
        if (choice.CalculationResult == null) return new List<DetailedChange>();
        return ConvertDetailedChanges(choice.CalculationResult);
    }

    public List<DetailedChange> ConvertDetailedChanges(ChoiceCalculationResult calculationResult)
    {
        List<DetailedChange> detailedChanges = new List<DetailedChange>();

        // Add modifications
        foreach (ValueModification change in calculationResult.ValueModifications)
        {
            if (change is MomentumModification evm)
            {
                AddDetailedChange(detailedChanges, GetMomementumChangeType(), change.Source, change.Amount);
            }
            else if (change is EnergyCostReduction em)
            {
                AddDetailedChange(detailedChanges, ConvertEnergyTypeToChangeType(em.EnergyType), change.Source, em.Amount);
            }
        }

        detailedChanges = SortDetailedChanges(detailedChanges);

        return detailedChanges;
    }

    public int GetProjectedChange(ChangeTypes changeType)
    {
        if (hoveredChoice == null || hoveredChoice.EncounterChoice.CalculationResult == null) return 0;

        int projectedChange = 0;
        foreach (DetailedChange detailedChange in GetValueChanges(hoveredChoice.EncounterChoice))
        {
            if (detailedChange.ChangeType == changeType)
            {
                projectedChange += detailedChange.ChangeValues.TotalAmount;
            }
        }
        return projectedChange;
    }

    public void AddDetailedChange(List<DetailedChange> combined, ChangeTypes changeType, string source, int amount)
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

    public List<CombinedValue> ConvertCombinedValues(Dictionary<ChangeTypes, int> combinedValuesDict)
    {
        List<CombinedValue> combinedValuesList = new List<CombinedValue>();
        foreach (KeyValuePair<ChangeTypes, int> kvp in combinedValuesDict)
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
        // Use the ModifiedRequirements for the disabled check
        return choice.EncounterChoice.CalculationResult.Requirements.Any(req =>
            !req.IsSatisfied(GameState));
    }

    public List<DetailedChange> SortDetailedChanges(List<DetailedChange> changes)
    {
        // Define the order of ChangeTypes
        List<ChangeTypes> order = new List<ChangeTypes>()
        {
            ChangeTypes.Momentum,
            ChangeTypes.PhysicalEnergy,
            ChangeTypes.Concentration,
            ChangeTypes.Reputation
        };

        return changes.OrderBy(dc => order.IndexOf(dc.ChangeType)).ToList();
    }

    public MarkupString GetValueTypeIcon(ChangeTypes valueType)
    {
        return valueType switch
        {
            ChangeTypes.Momentum => new MarkupString("<i class='value-icon outcome-icon'>⭐</i>"),
            ChangeTypes.PhysicalEnergy => new MarkupString("<i class='value-icon physical-icon'>💪</i>"),
            ChangeTypes.Concentration => new MarkupString("<i class='value-icon focus-icon'>🎯</i>"),
            ChangeTypes.Reputation => new MarkupString("<i class='value-icon social-icon'>👥</i>"),
            _ => new MarkupString("")
        };
    }

    public ChangeTypes ConvertEnergyTypeToChangeType(EnergyTypes energyType)
    {
        return energyType switch
        {
            EnergyTypes.Physical => ChangeTypes.PhysicalEnergy,
            EnergyTypes.Concentration => ChangeTypes.Concentration,
            _ => throw new ArgumentException("Invalid EnergyType")
        };
    }

    public ChangeTypes GetMomementumChangeType()
    {
        return ChangeTypes.Momentum;
    }

}

public class Dimensions
{
    public int WindowHeight { get; set; }
    public int TooltipHeight { get; set; }
}