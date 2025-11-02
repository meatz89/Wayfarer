/// <summary>
/// Context for Modal Scenes (Sir Brante forced moments).
/// Contains scene, current situation, and available choices.
/// Modal scenes take over full screen until player makes a choice.
/// </summary>
public class ModalSceneContext
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; }
    public Scene Scene { get; set; }
    public Situation CurrentSituation { get; set; }
    public string LocationId { get; set; }
    public string LocationName { get; set; }

    public ModalSceneContext()
    {
        IsValid = true;
        ErrorMessage = string.Empty;
    }
}
