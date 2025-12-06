using System.Reflection;
using Xunit;

namespace Wayfarer.Tests.Architecture;

/// <summary>
/// TDD tests for scene archetype naming refactoring.
///
/// PRINCIPLE: Archetype is a TYPE discriminator, not an ID.
/// - Property should be named "SceneArchetype" (not "SceneArchetypeId")
/// - Property should be non-nullable (all scenes have an archetype)
/// - Naming aligns with categorical property principles (§8.3, §8.10)
///
/// These tests define expected behavior BEFORE implementation (TDD red phase).
/// </summary>
public class SceneArchetypeNamingTests
{
    /// <summary>
    /// SceneTemplate should have property named "SceneArchetype" (not "SceneArchetypeId").
    /// SceneArchetype is a type discriminator selecting mechanical pattern, not an identifier.
    /// Named "SceneArchetype" (not just "Archetype") to avoid collision with SpawnPattern "Archetype".
    /// </summary>
    [Fact]
    public void SceneTemplate_HasSceneArchetypeProperty_NotSceneArchetypeId()
    {
        Type sceneTemplateType = typeof(SceneTemplate);

        // SHOULD have: SceneArchetype property
        PropertyInfo archetypeProp = sceneTemplateType.GetProperty("SceneArchetype");
        Assert.NotNull(archetypeProp);

        // SHOULD NOT have: SceneArchetypeId property (old naming with "Id" suffix)
        PropertyInfo oldProp = sceneTemplateType.GetProperty("SceneArchetypeId");
        Assert.Null(oldProp);
    }

    /// <summary>
    /// SceneArchetype property should be SceneArchetypeType enum (compile-time safe).
    /// </summary>
    [Fact]
    public void SceneTemplate_SceneArchetypeProperty_IsSceneArchetypeType()
    {
        Type sceneTemplateType = typeof(SceneTemplate);
        PropertyInfo archetypeProp = sceneTemplateType.GetProperty("SceneArchetype");

        Assert.NotNull(archetypeProp);
        Assert.Equal(typeof(SceneArchetypeType), archetypeProp.PropertyType);
    }

    /// <summary>
    /// SceneArchetype property should NOT be nullable.
    /// All scenes have an archetype - there is no "custom" scene without pattern.
    /// </summary>
    [Fact]
    public void SceneTemplate_SceneArchetypeProperty_IsNotNullable()
    {
        Type sceneTemplateType = typeof(SceneTemplate);
        PropertyInfo archetypeProp = sceneTemplateType.GetProperty("SceneArchetype");

        Assert.NotNull(archetypeProp);

        // Property type should be SceneArchetypeType, NOT SceneArchetypeType?
        Assert.Equal(typeof(SceneArchetypeType), archetypeProp.PropertyType);
        Assert.NotEqual(typeof(SceneArchetypeType?), archetypeProp.PropertyType);
    }

    /// <summary>
    /// SceneTemplateDTO should have property named "SceneArchetype" (matches JSON field).
    /// </summary>
    [Fact]
    public void SceneTemplateDTO_HasSceneArchetypeProperty()
    {
        Type dtoType = typeof(SceneTemplateDTO);

        // SHOULD have: SceneArchetype property
        PropertyInfo archetypeProp = dtoType.GetProperty("SceneArchetype");
        Assert.NotNull(archetypeProp);

        // SHOULD NOT have: SceneArchetypeId property
        PropertyInfo oldProp = dtoType.GetProperty("SceneArchetypeId");
        Assert.Null(oldProp);
    }

    /// <summary>
    /// Verify no "ArchetypeId" patterns exist in scene-related types.
    /// ID suffix implies identity reference, but archetype is a type discriminator.
    /// </summary>
    [Fact]
    public void SceneRelatedTypes_DoNotHave_ArchetypeIdProperties()
    {
        List<Type> sceneRelatedTypes = new List<Type>
        {
            typeof(SceneTemplate),
            typeof(SceneTemplateDTO),
            typeof(Scene)
        };

        List<string> violations = new List<string>();

        foreach (Type type in sceneRelatedTypes)
        {
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                // Check for any property ending with "ArchetypeId"
                if (prop.Name.EndsWith("ArchetypeId"))
                {
                    violations.Add($"{type.Name}.{prop.Name} uses 'Id' suffix but archetype is a type, not an identifier");
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// SceneArchetypeType enum should exist and contain all expected patterns.
    /// Enum is the compile-time safe way to represent archetype types.
    /// </summary>
    [Fact]
    public void SceneArchetypeType_Enum_ContainsExpectedPatterns()
    {
        // Encounter patterns
        Assert.True(Enum.IsDefined(typeof(SceneArchetypeType), SceneArchetypeType.InnLodging));
        Assert.True(Enum.IsDefined(typeof(SceneArchetypeType), SceneArchetypeType.DeliveryContract));
        Assert.True(Enum.IsDefined(typeof(SceneArchetypeType), SceneArchetypeType.RouteSegmentTravel));

        // Narrative patterns
        Assert.True(Enum.IsDefined(typeof(SceneArchetypeType), SceneArchetypeType.SeekAudience));
        Assert.True(Enum.IsDefined(typeof(SceneArchetypeType), SceneArchetypeType.InvestigateLocation));
        Assert.True(Enum.IsDefined(typeof(SceneArchetypeType), SceneArchetypeType.MoralCrossroads));
    }

    /// <summary>
    /// Valid SceneTemplate can be created with required SceneArchetype.
    /// </summary>
    [Fact]
    public void SceneTemplate_CanBeCreated_WithRequiredSceneArchetype()
    {
        SceneTemplate template = new SceneTemplate
        {
            Id = "test_scene",
            SceneArchetype = SceneArchetypeType.InnLodging,
            Archetype = SpawnPattern.Linear,
            SituationTemplates = new List<SituationTemplate>(),
            SpawnRules = new SituationSpawnRules { Pattern = SpawnPattern.Linear }
        };

        Assert.Equal(SceneArchetypeType.InnLodging, template.SceneArchetype);
    }
}
