/// <summary>
/// Return type for SceneArchetypeCatalog methods
/// Contains generated multi-situation structure ready for embedding in SceneTemplate
/// Used exclusively at parse time - catalogues generate this, parser consumes it
/// </summary>
public class SceneArchetypeDefinition
{
    /// <summary>
    /// Generated SituationTemplates forming complete multi-situation arc
    /// Each SituationTemplate has ArchetypeId that SituationArchetypeCatalog will expand into ChoiceTemplates
    /// Example: service_with_location_access generates 4 SituationTemplates (negotiate, access, service, depart)
    /// </summary>
    public List<SituationTemplate> SituationTemplates { get; init; } = new List<SituationTemplate>();

    /// <summary>
    /// Generated spawn rules defining transitions between situations
    /// Pattern and transitions auto-generated based on archetype logic
    /// Example: Linear pattern with sequential transitions (situation1 → situation2 → situation3 → situation4)
    /// </summary>
    public SituationSpawnRules SpawnRules { get; init; }

    /// <summary>
    /// Dependent location specifications for self-contained scenes
    /// Catalogue generates specs, SceneInstantiator creates actual LocationDTOs at runtime
    /// Empty list = Scene uses pre-existing locations only (traditional pattern)
    /// </summary>
    public List<DependentLocationSpec> DependentLocations { get; init; } = new List<DependentLocationSpec>();

    /// <summary>
    /// Dependent item specifications for self-contained scenes
    /// Catalogue generates specs, SceneInstantiator creates actual ItemDTOs at runtime
    /// Empty list = Scene uses pre-existing items only (traditional pattern)
    /// </summary>
    public List<DependentItemSpec> DependentItems { get; init; } = new List<DependentItemSpec>();
}
