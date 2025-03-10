using BlazorRPG.Game.EncounterManager;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

public partial class EncounterChoiceTooltipBase : ComponentBase
{
    [Inject] public GameManager GameManager { get; set; }
    [Inject] public GameState GameState { get; set; }
    [Parameter] public Encounter encounter { get; set; }
    [Parameter] public UserEncounterChoiceOption hoveredChoice { get; set; }
    [Parameter] public double mouseX { get; set; }
    [Parameter] public double mouseY { get; set; }

    public ChoiceProjection Preview => GameManager.GetChoicePreview(hoveredChoice);

    public class CalculationComponent
    {
        public string Source { get; set; }
        public int Value { get; set; }
    }

    public List<CalculationComponent> GetMomentumBreakdown()
    {
        List<CalculationComponent> components = new List<CalculationComponent>();

        // Only momentum choices build momentum
        if (hoveredChoice.Choice.EffectType == EffectTypes.Momentum)
        {
            // Base momentum (2 for standard, 3 for special)
            int baseMomentum = hoveredChoice.Choice is SpecialChoice ? 3 : 2;
            components.Add(new CalculationComponent
            {
                Source = "Base choice",
                Value = baseMomentum
            });

            // Add tag bonuses from active strategic tags
            foreach (IEncounterTag tag in encounter.State.ActiveTags)
            {
                // Only strategic tags provide bonuses
                if (tag is StrategicTag)
                {
                    // Determine if this tag affects this choice
                    int tagBonus = GetTagBonusForChoice(tag, hoveredChoice.Choice);
                    if (tagBonus != 0)
                    {
                        components.Add(new CalculationComponent
                        {
                            Source = tag.Name,
                            Value = tagBonus
                        });
                    }
                }
            }

            // Location bonuses (if any)
            if (encounter.State.Location.FavoredApproaches.Contains(hoveredChoice.Choice.Approach))
            {
                components.Add(new CalculationComponent
                {
                    Source = "Location favors approach",
                    Value = 1
                });
            }
        }
        else if (hoveredChoice.Choice is EmergencyChoice)
        {
            // Emergency choices build 1 momentum
            components.Add(new CalculationComponent
            {
                Source = "Emergency choice",
                Value = 1
            });
        }

        return components;
    }

    public List<CalculationComponent> GetPressureBreakdown()
    {
        List<CalculationComponent> components = new List<CalculationComponent>();

        // Pressure choices and emergency choices build pressure
        if (hoveredChoice.Choice.EffectType == EffectTypes.Pressure)
        {
            // Standard pressure choices build 2 pressure
            components.Add(new CalculationComponent
            {
                Source = "Pressure choice",
                Value = 2
            });
        }
        else if (hoveredChoice.Choice is EmergencyChoice)
        {
            // Emergency choices build 2 pressure
            components.Add(new CalculationComponent
            {
                Source = "Emergency choice",
                Value = 2
            });
        }

        // Add tag effects that reduce pressure
        foreach (IEncounterTag tag in encounter.State.ActiveTags)
        {
            // Only strategic tags provide bonuses
            if (tag is StrategicTag)
            {
                // Determine if this tag affects pressure for this choice
                int tagEffect = GetTagPressureEffectForChoice(tag, hoveredChoice.Choice);
                if (tagEffect != 0)
                {
                    components.Add(new CalculationComponent
                    {
                        Source = tag.Name,
                        Value = tagEffect
                    });
                }
            }
        }

        return components;
    }

    private int GetTagBonusForChoice(IEncounterTag tag, IChoice choice)
    {
        // Match tag effects to choices based on tag name and choice properties
        // This is a simplified approach - ideally you'd have more structured data
        if (tag.Name.Contains("Respect") && choice.Focus == FocusTags.Resource)
            return 1;
        else if (tag.Name.Contains("Wisdom") && choice.Focus == FocusTags.Information)
            return 1;
        else if (tag.Name.Contains("Network") && choice.Approach == ApproachTypes.Charm)
            return 1;
        else if (tag.Name.Contains("Distracted") && choice.Approach == ApproachTypes.Stealth)
            return 1;
        else if (tag.Name.Contains("Coordinated") && choice.Approach == ApproachTypes.Force)
            return 1;

        return 0;
    }

    private int GetTagPressureEffectForChoice(IEncounterTag tag, IChoice choice)
    {
        // Match tag pressure effects to choices based on tag name and choice properties
        if (tag.Name.Contains("Eye") && choice.Focus == FocusTags.Resource)
            return -1;
        else if (tag.Name.Contains("Network"))
            return -1; // End of turn effect, but we'll show it here for clarity
        else if (tag.Name.Contains("Superstitious"))
            return -1; // End of turn effect

        return 0;
    }

    public Dictionary<string, int> GetApproachTagChanges()
    {
        Dictionary<string, int> formattedChanges = new Dictionary<string, int>();

        if (Preview?.ApproachTagChanges == null)
            return formattedChanges;

        foreach (var tagChange in Preview.ApproachTagChanges)
        {
            formattedChanges[tagChange.Key.ToString()] = tagChange.Value;
        }

        return formattedChanges;
    }

    public Dictionary<string, int> GetFocusTagChanges()
    {
        Dictionary<string, int> formattedChanges = new Dictionary<string, int>();

        if (Preview?.FocusTagChanges == null)
            return formattedChanges;

        foreach (var tagChange in Preview.FocusTagChanges)
        {
            formattedChanges[tagChange.Key.ToString()] = tagChange.Value;
        }

        return formattedChanges;
    }

    public string GetTagEffectDescription(string tagName)
    {
        // Map tag names to their effects
        // This is a simplified approach - ideally you'd have more structured data
        if (tagName.Contains("Respect"))
            return "+1 momentum to Resource choices";
        else if (tagName.Contains("Eye"))
            return "-1 pressure from Resource choices";
        else if (tagName.Contains("Wisdom"))
            return "+1 momentum to Information choices";
        else if (tagName.Contains("Network"))
            return "-1 pressure at end of each turn";
        else if (tagName.Contains("Marketplace"))
            return "Blocks Concealment approaches";
        else if (tagName.Contains("Suspicion"))
            return "Blocks Charm approaches";
        else if (tagName.Contains("Guard"))
            return "Blocks Stealth approaches";

        return "Affects encounter mechanics";
    }
}