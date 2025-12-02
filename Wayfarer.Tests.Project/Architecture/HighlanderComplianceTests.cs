using System.Reflection;
using Xunit;

namespace Wayfarer.Tests.Architecture;

/// <summary>
/// Tests for HIGHLANDER principle compliance (arc42 §8.1).
/// "There can be only one" - Every piece of game state has exactly one canonical storage location.
///
/// BEHAVIOR-ONLY TESTING (§8.21): These tests verify structural compliance,
/// not specific values or implementation details.
/// </summary>
public class HighlanderComplianceTests
{
    /// <summary>
    /// Verify that entities do not store both an object reference AND its ID.
    /// HIGHLANDER violation: storing both NPC and NPCId creates two sources of truth.
    /// Pattern: If property "Foo" exists of entity type, "FooId" should not exist.
    /// </summary>
    [Fact]
    public void Entities_ShouldNot_StoreBothObjectAndId()
    {
        Assembly assembly = typeof(GameWorld).Assembly;
        List<string> violations = new List<string>();

        // Entity types that should be referenced by object, not ID
        HashSet<Type> entityTypes = new HashSet<Type>
        {
            typeof(NPC), typeof(Location), typeof(Venue), typeof(RouteOption),
            typeof(Item), typeof(Scene), typeof(Situation), typeof(Obligation),
            typeof(Achievement), typeof(District), typeof(Region)
        };

        foreach (Type type in GetDomainTypes(assembly))
        {
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in properties)
            {
                // Check if this is an entity reference property
                if (entityTypes.Contains(prop.PropertyType))
                {
                    // Look for corresponding Id property
                    string idPropertyName = prop.Name + "Id";
                    PropertyInfo idProp = properties.FirstOrDefault(p =>
                        p.Name.Equals(idPropertyName, StringComparison.OrdinalIgnoreCase));

                    if (idProp != null && idProp.PropertyType == typeof(string))
                    {
                        violations.Add($"{type.Name}.{prop.Name} has both object reference AND {idProp.Name} (HIGHLANDER violation)");
                    }
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify that mutable state properties are not duplicated across types.
    /// Example: Player.CurrentLocationId should not exist if GameWorld tracks location via Player.CurrentPosition.
    /// </summary>
    [Fact]
    public void Player_ShouldNot_HaveRedundantLocationTracking()
    {
        Type playerType = typeof(Player);

        // Player should NOT have CurrentLocationId - location derived from CurrentPosition
        PropertyInfo locationIdProp = playerType.GetProperty("CurrentLocationId");
        Assert.Null(locationIdProp);

        // Player should NOT have CurrentLocation - derived from hex lookup
        PropertyInfo currentLocationProp = playerType.GetProperty("CurrentLocation");
        Assert.Null(currentLocationProp);

        // Player SHOULD have CurrentPosition (source of truth for location)
        PropertyInfo currentPositionProp = playerType.GetProperty("CurrentPosition");
        Assert.NotNull(currentPositionProp);
    }

    /// <summary>
    /// Verify GameWorld state collections use object references, not ID collections.
    /// HIGHLANDER: Single source of truth means entities, not ID strings.
    /// </summary>
    [Fact]
    public void GameWorld_StateCollections_ShouldContain_ObjectReferences()
    {
        Type gameWorldType = typeof(GameWorld);
        List<string> violations = new List<string>();

        PropertyInfo[] properties = gameWorldType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (PropertyInfo prop in properties)
        {
            // Check for List<string> properties that might be ID collections
            if (prop.PropertyType.IsGenericType &&
                prop.PropertyType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type elementType = prop.PropertyType.GetGenericArguments()[0];

                // Flag potential ID collections (List<string> with "Id" in name)
                if (elementType == typeof(string) && prop.Name.Contains("Id"))
                {
                    violations.Add($"GameWorld.{prop.Name} is List<string> - should be List<EntityType> for HIGHLANDER compliance");
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify that template state is separated from instance state.
    /// Templates (immutable archetypes) can have IDs.
    /// Instance state (mutable runtime) should not duplicate template data.
    /// </summary>
    [Fact]
    public void TemplateInstanceSeparation_ActiveStates_ReferenceTemplates()
    {
        // ActiveEmergencyState should reference EmergencySituation template
        Type activeEmergencyType = typeof(ActiveEmergencyState);
        PropertyInfo templateProp = activeEmergencyType.GetProperty("Template");
        Assert.NotNull(templateProp);
        Assert.Equal(typeof(EmergencySituation), templateProp.PropertyType);

        // ActiveEmergencyState should NOT duplicate template properties
        PropertyInfo nameProp = activeEmergencyType.GetProperty("Name");
        Assert.Null(nameProp);

        PropertyInfo descriptionProp = activeEmergencyType.GetProperty("Description");
        Assert.Null(descriptionProp);
    }

    /// <summary>
    /// Verify ObservationSceneState properly separates template from state.
    /// </summary>
    [Fact]
    public void TemplateInstanceSeparation_ObservationStates_ReferenceTemplates()
    {
        Type observationStateType = typeof(ObservationSceneState);
        PropertyInfo templateProp = observationStateType.GetProperty("Template");
        Assert.NotNull(templateProp);
        Assert.Equal(typeof(ObservationScene), templateProp.PropertyType);

        // Should NOT have Id property (template has it)
        PropertyInfo idProp = observationStateType.GetProperty("Id");
        Assert.Null(idProp);
    }

    /// <summary>
    /// Verify services store no state - all state in GameWorld.
    /// Service classes should not have state-tracking properties.
    /// </summary>
    [Fact]
    public void Services_ShouldNot_StoreGameState()
    {
        Assembly assembly = typeof(GameWorld).Assembly;
        List<string> violations = new List<string>();

        // Known state property names that services should not have
        HashSet<string> statePropertyNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "CurrentState", "ActiveSession", "PendingResult", "LastResult",
            "CachedValue", "StoredData", "SessionState"
        };

        foreach (Type type in GetServiceTypes(assembly))
        {
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);

            foreach (PropertyInfo prop in properties)
            {
                // Skip injected dependencies
                if (prop.PropertyType.IsInterface) continue;
                if (prop.Name.StartsWith("_")) continue;

                // Check for state-like properties
                bool looksLikeState = statePropertyNames.Any(s => prop.Name.Contains(s)) ||
                                     prop.Name.EndsWith("State") ||
                                     prop.Name.StartsWith("Current") ||
                                     prop.Name.StartsWith("Pending") ||
                                     prop.Name.StartsWith("Last");

                // Allow readonly/init-only properties (configuration, not state)
                if (looksLikeState && prop.CanWrite && !prop.SetMethod.IsPrivate)
                {
                    // Check if this is a GameWorld reference (allowed - that's where state lives)
                    if (prop.PropertyType != typeof(GameWorld))
                    {
                        violations.Add($"Service {type.Name}.{prop.Name} appears to store state (services should be stateless per HIGHLANDER)");
                    }
                }
            }
        }

        // Filter known exceptions (documented stateless patterns)
        violations = violations
            .Where(v => !v.Contains("TimeManager")) // TimeManager is special - it IS state
            .Where(v => !v.Contains("GameOrchestrator"))  // GameOrchestrator is coordinator, not service
            .ToList();

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify NPC stores tokens directly (not in separate collection).
    /// Tokens are NPC state, not Player state.
    /// </summary>
    [Fact]
    public void NPC_StoresTokensDirectly()
    {
        Type npcType = typeof(NPC);

        // NPC should have token properties
        PropertyInfo trustProp = npcType.GetProperty("Trust");
        Assert.NotNull(trustProp);
        Assert.Equal(typeof(int), trustProp.PropertyType);

        PropertyInfo diplomacyProp = npcType.GetProperty("Diplomacy");
        Assert.NotNull(diplomacyProp);
        Assert.Equal(typeof(int), diplomacyProp.PropertyType);

        PropertyInfo statusProp = npcType.GetProperty("Status");
        Assert.NotNull(statusProp);
        Assert.Equal(typeof(int), statusProp.PropertyType);

        PropertyInfo shadowProp = npcType.GetProperty("Shadow");
        Assert.NotNull(shadowProp);
        Assert.Equal(typeof(int), shadowProp.PropertyType);

        // NPC should have helper methods
        System.Reflection.MethodInfo getTokenCountMethod = npcType.GetMethod("GetTokenCount");
        Assert.NotNull(getTokenCountMethod);

        System.Reflection.MethodInfo setTokenCountMethod = npcType.GetMethod("SetTokenCount");
        Assert.NotNull(setTokenCountMethod);

        System.Reflection.MethodInfo getTotalTokensMethod = npcType.GetMethod("GetTotalTokens");
        Assert.NotNull(getTotalTokensMethod);
    }

    /// <summary>
    /// Verify Player does NOT store NPCTokens collection.
    /// Tokens are NPC state, not Player state.
    /// </summary>
    [Fact]
    public void Player_DoesNot_StoreNPCTokens()
    {
        Type playerType = typeof(Player);

        // Player should NOT have NPCTokens property
        PropertyInfo npcTokensProp = playerType.GetProperty("NPCTokens");
        Assert.Null(npcTokensProp);

        // Player should NOT have token helper methods (tokens managed on NPC)
        System.Reflection.MethodInfo getNPCTokenCountMethod = playerType.GetMethod("GetNPCTokenCount");
        Assert.Null(getNPCTokenCountMethod);

        System.Reflection.MethodInfo setNPCTokenCountMethod = playerType.GetMethod("SetNPCTokenCount");
        Assert.Null(setNPCTokenCountMethod);

        System.Reflection.MethodInfo getNPCTokenEntryMethod = playerType.GetMethod("GetNPCTokenEntry");
        Assert.Null(getNPCTokenEntryMethod);
    }

    // ========== HELPER METHODS ==========

    private List<Type> GetDomainTypes(Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => !t.Namespace?.Contains("Pages") ?? true)
            .Where(t => !t.Name.EndsWith("DTO"))
            .Where(t => !t.Name.EndsWith("Parser"))
            .Where(t => !t.Name.EndsWith("Service"))
            .Where(t => !t.Name.EndsWith("Manager"))
            .Where(t => !t.Name.EndsWith("Factory"))
            .ToList();
    }

    private List<Type> GetServiceTypes(Assembly assembly)
    {
        return assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.Name.EndsWith("Service") ||
                       t.Name.EndsWith("Activity") ||
                       t.Name.EndsWith("Executor"))
            .Where(t => t.Name != "TimeService") // TimeService intentionally stateful
            .ToList();
    }
}
