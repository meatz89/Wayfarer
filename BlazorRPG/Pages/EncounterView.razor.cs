using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

public partial class EncounterViewBase : ComponentBase
{
    [Parameter] public EventCallback OnEncounterCompleted { get; set; }
    [Inject] public GameState GameState { get; set; }
    [Inject] public GameManager GameManager { get; set; }

    public UserEncounterChoiceOption hoveredChoice;
    public bool showTooltip;
    public double mouseX;
    public double mouseY;

    public void ShowTooltip(UserEncounterChoiceOption choice)
    {
        hoveredChoice = choice;
        showTooltip = true;
    }

    public void HideTooltip()
    {
        hoveredChoice = null;
        showTooltip = false;
    }

    public void HandleChoiceSelection(UserEncounterChoiceOption choice)
    {
        if (IsChoiceDisabled(choice))
        {
            return; // Don't execute if the choice is disabled
        }

        // Apply the choice to the encounter
        GameManager.ExecuteEncounterChoice(choice);

        // The GameManager should have already updated the encounter state,
        // so we can just trigger a re-render
        //StateHasChanged(); //Removed because we added it to OnInitialized
        OnEncounterCompleted.InvokeAsync();
    }

    public bool IsChoiceDisabled(UserEncounterChoiceOption choice)
    {
        return choice.EncounterChoice.Requirements.Any(req => !req.IsSatisfied(GameState.Player));
    }

    public void OnMouseMove(MouseEventArgs e)
    {
        mouseX = e.ClientX + 40;
        mouseY = e.ClientY + 40;
    }

    public bool IsRequirementMet(UserEncounterChoiceOption choice)
    {
        foreach (Requirement req in choice.EncounterChoice.Requirements)
        {
            if (!req.IsSatisfied(GameState.Player)) return false;
        }
        return true;
    }

    public EncounterStateValues CalculatePreviewState(EncounterStateValues currentState, List<ValueChange> valueChanges)
    {
        EncounterStateValues previewState = new EncounterStateValues(
            currentState.Outcome,
            currentState.Insight,
            currentState.Resonance,
            currentState.Pressure);

        foreach (ValueChange valueChange in valueChanges)
        {
            switch (valueChange.ValueType)
            {
                case ValueTypes.Outcome:
                    previewState.Outcome = Math.Clamp(previewState.Outcome + valueChange.Change, 0, 20);
                    break;
                case ValueTypes.Insight:
                    previewState.Insight = Math.Clamp(previewState.Insight + valueChange.Change, 0, 20);
                    break;
                case ValueTypes.Resonance:
                    previewState.Resonance = Math.Clamp(previewState.Resonance + valueChange.Change, 0, 20);
                    break;
                case ValueTypes.Pressure:
                    previewState.Pressure = Math.Clamp(previewState.Pressure + valueChange.Change, 0, 20);
                    break;
            }
        }

        return previewState;
    }

    // Method to determine the CSS class based on the change
    public string GetStateChangeClass(int currentValue, int previewValue)
    {
        if (previewValue > currentValue)
        {
            return "positive";
        }
        else if (previewValue < currentValue)
        {
            return "negative";
        }
        else
        {
            return "";
        }
    }

    // Updated method for requirement descriptions
    public MarkupString GetRequirementDescription(Requirement req, PlayerState player)
    {
        bool isMet = req.IsSatisfied(player);
        string iconHtml = isMet
            ? "<span class='green-checkmark'>✓</span>"
            : "<span class='red-x'>✗</span>";

        return new MarkupString($"{iconHtml} {req.GetDescription()}");
    }

    public List<string> CalculateEnergyCostPreview(EncounterChoice choice, PlayerState player, EncounterStateValues encounterStateValues)
    {
        List<string> preview = new();
        int previewPhysicalEnergy = player.PhysicalEnergy;
        int previewFocusEnergy = player.FocusEnergy;
        int previewSocialEnergy = player.SocialEnergy;
        int previewHealth = player.Health;

        foreach (Requirement req in choice.Requirements)
        {
            if (req is EnergyRequirement energyReq)
            {
                int cost = energyReq.Amount;

                // Pressure Modifier (Simulate the logic from ApplyEnergyCosts)
                if (encounterStateValues.Pressure >= 6)
                {
                    cost += 1;
                }

                switch (energyReq.EnergyType)
                {
                    case EnergyTypes.Physical:
                        previewPhysicalEnergy -= cost;
                        if (previewPhysicalEnergy < 0)
                        {
                            previewHealth += previewPhysicalEnergy; // Health penalty
                            previewPhysicalEnergy = 0;
                        }
                        preview.Add(
                            $"<span class='{(energyReq.Amount > 0 ? "negative" : "positive")}'>" +
                            $"{energyReq.EnergyType} Energy: ({player.PhysicalEnergy} -> {previewPhysicalEnergy})" +
                            $"</span>");
                        if (previewHealth < player.Health)
                        {
                            preview.Add(
                                $"<span class='negative'>" +
                                $"Health: ({player.Health} -> {previewHealth})" +
                                $"</span>");
                        }
                        break;

                    case EnergyTypes.Focus:
                        previewFocusEnergy -= cost;
                        if (previewFocusEnergy < 0)
                        {
                            previewFocusEnergy = 0; // Deplete energy in preview
                        }
                        preview.Add(
                            $"<span class='{(energyReq.Amount > 0 ? "negative" : "positive")}'>" +
                            $"{energyReq.EnergyType} Energy: ({player.FocusEnergy} -> {previewFocusEnergy})" +
                            $"</span>");
                        break;

                    case EnergyTypes.Social:
                        previewSocialEnergy -= cost;
                        if (previewSocialEnergy < 0)
                        {
                            previewSocialEnergy = 0; // Deplete energy in preview
                        }
                        preview.Add(
                            $"<span class='{(energyReq.Amount > 0 ? "negative" : "positive")}'>" +
                            $"{energyReq.EnergyType} Energy: ({player.SocialEnergy} -> {previewSocialEnergy})" +
                            $"</span>");
                        break;
                }
            }
        }

        return preview;
    }


    public int GetCurrentEnergy(PlayerState player, EnergyTypes type)
    {
        return type switch
        {
            EnergyTypes.Physical => player.PhysicalEnergy,
            EnergyTypes.Focus => player.FocusEnergy,
            EnergyTypes.Social => player.SocialEnergy,
            _ => 0
        };
    }

    public int GetMaxEnergy(PlayerState player, EnergyTypes type)
    {
        return type switch
        {
            EnergyTypes.Physical => player.MaxPhysicalEnergy,
            EnergyTypes.Focus => player.MaxFocusEnergy,
            EnergyTypes.Social => player.MaxSocialEnergy,
            _ => 0
        };
    }
}