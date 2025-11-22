public class NarrativeOverlayViewModel
{
    public bool IsVisible { get; set; }
    public bool IsMinimized { get; set; }
    public string NarrativeTitle { get; set; }
    public int CurrentStep { get; set; }
    public int TotalSteps { get; set; }
    public string StepName { get; set; }
    public string StepDescription { get; set; }
    public string GuidanceText { get; set; }
    public List<string> AllowedActions { get; set; } = new List<string>();
}