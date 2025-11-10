/// <summary>
/// Context for Scene screens containing scene state and metadata.
/// Used for multi-situation scenes at locations or with NPCs.
/// All scenes auto-activate when player enters their required context.
/// </summary>
public class SceneContext
{
public bool IsValid { get; set; }
public string ErrorMessage { get; set; }
public Scene Scene { get; set; }
public Situation CurrentSituation { get; set; }
public string LocationId { get; set; }
public string LocationName { get; set; }

public SceneContext()
{
    IsValid = true;
    ErrorMessage = string.Empty;
}
}
