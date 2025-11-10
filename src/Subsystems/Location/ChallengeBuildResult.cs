/// <summary>
/// Result of building challenges for location display
/// Replaces value tuple (List&lt;SituationCardViewModel&gt;, List&lt;SceneWithSituationsViewModel&gt;)
/// </summary>
public class ChallengeBuildResult
{
public List<SituationCardViewModel> AmbientSituations { get; init; }
public List<SceneWithSituationsViewModel> SceneGroups { get; init; }

public ChallengeBuildResult(List<SituationCardViewModel> ambientSituations, List<SceneWithSituationsViewModel> sceneGroups)
{
    AmbientSituations = ambientSituations;
    SceneGroups = sceneGroups;
}
}
