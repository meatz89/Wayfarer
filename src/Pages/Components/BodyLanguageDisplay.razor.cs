using Microsoft.AspNetCore.Components;

public class BodyLanguageDisplayBase : ComponentBase
{
    [Parameter] public string Description { get; set; }
    [Parameter] public string NpcName { get; set; }
    [Parameter] public NPCEmotionalState? EmotionalState { get; set; }
    [Parameter] public StakeType? Stakes { get; set; }
    
    [Inject] private NPCEmotionalStateCalculator StateCalculator { get; set; }
    
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        
        // Auto-generate body language if we have state and stakes but no description
        if (string.IsNullOrEmpty(Description) && 
            EmotionalState.HasValue && 
            Stakes.HasValue && 
            StateCalculator != null)
        {
            Description = StateCalculator.GenerateBodyLanguage(EmotionalState.Value, Stakes.Value);
        }
    }
}