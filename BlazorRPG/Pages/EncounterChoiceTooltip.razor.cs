using Microsoft.AspNetCore.Components;

public partial class EncounterChoiceTooltipBase : ComponentBase
{
    [Inject] public GameManager GameManager { get; set; }
    [Inject] public GameState GameState { get; set; }
    [Parameter] public UserEncounterChoiceOption hoveredChoice { get; set; }
    [Parameter] public double tooltipX { get; set; }
    [Parameter] public double tooltipY { get; set; }

    protected string GetChoiceNarrative(UserEncounterChoiceOption choice)
    {
        if (choice.Choice is EncounterOption option && option.Skill != SkillTypes.None)
        {
            return choice.Choice.Description;
        }
        return choice.ChoiceDescription;
    }

    protected string GetSkillCheckInfo(UserEncounterChoiceOption choice)
    {
        if (choice.Choice is EncounterOption option && option.Skill != SkillTypes.None)
        {
            // Build skill check display
            var skillName = option.Skill.ToString();
            var difficulty = option.Difficulty;

            // Show location effects if any
            string locationEffect = option.LocationModifier switch
            {
                < 0 => $" (Easier due to location: -{Math.Abs(option.LocationModifier)})",
                > 0 => $" (Harder due to location: +{option.LocationModifier})",
                _ => ""
            };

            return $"Requires {skillName} check (Difficulty: {difficulty}{locationEffect})";
        }
        return "No skill check required";
    }

    protected string GetProgressInfo(UserEncounterChoiceOption choice)
    {
        if (choice.Choice is EncounterOption option)
        {
            string successText = option.SuccessProgress > 0
                ? $"Success: +{option.SuccessProgress} progress"
                : "Success: No progress";

            string failureText = option.FailureProgress != 0
                ? $"Failure: {(option.FailureProgress > 0 ? "+" : "")}{option.FailureProgress} progress"
                : "Failure: No progress";

            return $"{successText}\n{failureText}";
        }
        return "";
    }

    public string tooltipXpx
    {
        get
        {
            return $"{tooltipX}px";
        }
    }

    public string tooltipYpx
    {
        get
        {
            return $"{tooltipY}px";
        }
    }

    public ChoiceProjection Preview
    {
        get
        {
            return GameManager.GetChoicePreview(hoveredChoice);
        }
    }
}