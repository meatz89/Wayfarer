
/// <summary>
/// Result of finalizing a provisional scene into an active scene
/// Replaces value tuple (Scene, DependentResourceSpecs)
/// </summary>
public class SceneFinalizationResult
{
    public Scene Scene { get; init; }
    public DependentResourceSpecs DependentSpecs { get; init; }

    public SceneFinalizationResult(Scene scene, DependentResourceSpecs dependentSpecs)
    {
        Scene = scene;
        DependentSpecs = dependentSpecs;
    }
}
