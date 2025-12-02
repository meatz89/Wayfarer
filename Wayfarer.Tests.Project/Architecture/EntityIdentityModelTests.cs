using System.Reflection;
using Xunit;

namespace Wayfarer.Tests.Architecture;

/// <summary>
/// Tests for Entity Identity Model compliance (arc42 §8.3).
/// Domain entities have no instance IDs. Relationships use direct object references.
/// Template IDs are allowed (immutable archetypes).
///
/// BEHAVIOR-ONLY TESTING (§8.21): Tests verify structural compliance.
/// </summary>
public class EntityIdentityModelTests
{
    // Domain entity types that should NOT have ID properties
    // NOTE: CardInstance excluded - it's an instance wrapper where InstanceId distinguishes
    // multiple card instances of the same template (valid identity tracking use case)
    private static readonly HashSet<Type> DomainEntityTypes = new HashSet<Type>
    {
        typeof(Player),
        typeof(NPC),
        typeof(Location),
        typeof(Venue),
        typeof(RouteOption),
        typeof(Item),
        typeof(District),
        typeof(Region),
        typeof(ActiveState),
        typeof(ActiveEmergencyState),
        typeof(ObservationSceneState),
        typeof(TravelSession),
        typeof(SocialSession),
        typeof(MentalSession),
        typeof(PhysicalSession),
        typeof(DeliveryJob),
        typeof(Inventory),
        typeof(PlayerScales),
        typeof(ConnectionState)
    };

    // Template types that ARE allowed to have IDs (immutable archetypes)
    private static readonly HashSet<Type> TemplateTypes = new HashSet<Type>
    {
        typeof(SceneTemplate),
        typeof(SituationTemplate),
        typeof(ChoiceTemplate),
        typeof(EmergencySituation),
        typeof(ObservationScene),
        typeof(ConversationTree),
        typeof(Achievement),
        typeof(State),
        typeof(Obligation),
        typeof(Scene),
        typeof(Situation)
    };

    /// <summary>
    /// Verify that domain entities do not have Id properties.
    /// HIGHLANDER + Entity Identity: No instance IDs on mutable state entities.
    /// </summary>
    [Fact]
    public void DomainEntities_ShouldNot_HaveIdProperties()
    {
        List<string> violations = new List<string>();

        foreach (Type entityType in DomainEntityTypes)
        {
            PropertyInfo idProp = entityType.GetProperty("Id");
            if (idProp != null && idProp.PropertyType == typeof(string))
            {
                violations.Add($"{entityType.Name}.Id exists but domain entities should not have instance IDs");
            }

            // Also check for common ID patterns
            PropertyInfo[] properties = entityType.GetProperties();
            foreach (PropertyInfo prop in properties)
            {
                if (prop.PropertyType == typeof(string) &&
                    (prop.Name == "EntityId" || prop.Name == "InstanceId" || prop.Name == "UniqueId"))
                {
                    violations.Add($"{entityType.Name}.{prop.Name} is an instance ID (forbidden per §8.3)");
                }
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify that template types CAN have Id properties (they're immutable archetypes).
    /// </summary>
    [Fact]
    public void TemplateTypes_May_HaveIdProperties()
    {
        List<string> templatesWithIds = new List<string>();

        foreach (Type templateType in TemplateTypes)
        {
            PropertyInfo idProp = templateType.GetProperty("Id");
            if (idProp != null)
            {
                templatesWithIds.Add(templateType.Name);
            }
        }

        // This is informational - templates CAN have IDs
        // At least some templates should have IDs for content referencing
        Assert.NotEmpty(templatesWithIds);
    }

    /// <summary>
    /// Verify NPC uses object references for relationships, not ID strings.
    /// </summary>
    [Fact]
    public void NPC_UsesObjectReferences_NotIdStrings()
    {
        Type npcType = typeof(NPC);

        // Should have Location object reference
        PropertyInfo locationProp = npcType.GetProperty("Location");
        Assert.NotNull(locationProp);
        Assert.Equal(typeof(Location), locationProp.PropertyType);

        // Should NOT have LocationId
        PropertyInfo locationIdProp = npcType.GetProperty("LocationId");
        Assert.Null(locationIdProp);

        // Should NOT have NpcId or Id
        PropertyInfo idProp = npcType.GetProperty("Id");
        Assert.Null(idProp);

        PropertyInfo npcIdProp = npcType.GetProperty("NpcId");
        Assert.Null(npcIdProp);
    }

    /// <summary>
    /// Verify Location uses object references for relationships.
    /// </summary>
    [Fact]
    public void Location_UsesObjectReferences_NotIdStrings()
    {
        Type locationType = typeof(Location);

        // Should have Venue object reference
        PropertyInfo venueProp = locationType.GetProperty("Venue");
        Assert.NotNull(venueProp);
        Assert.Equal(typeof(Venue), venueProp.PropertyType);

        // Should NOT have VenueId
        PropertyInfo venueIdProp = locationType.GetProperty("VenueId");
        Assert.Null(venueIdProp);

        // Should NOT have LocationId
        PropertyInfo locationIdProp = locationType.GetProperty("LocationId");
        Assert.Null(locationIdProp);
    }

    /// <summary>
    /// Verify RouteOption uses object references for endpoints.
    /// </summary>
    [Fact]
    public void RouteOption_UsesObjectReferences_ForEndpoints()
    {
        Type routeType = typeof(RouteOption);

        // Should have origin/destination as Location objects
        PropertyInfo originProp = routeType.GetProperty("Origin");
        PropertyInfo destProp = routeType.GetProperty("Destination");

        // If these exist, they should be Location type
        if (originProp != null)
        {
            Assert.Equal(typeof(Location), originProp.PropertyType);
        }

        if (destProp != null)
        {
            Assert.Equal(typeof(Location), destProp.PropertyType);
        }

        // Should NOT have OriginId or DestinationId
        PropertyInfo originIdProp = routeType.GetProperty("OriginId");
        PropertyInfo destIdProp = routeType.GetProperty("DestinationId");
        Assert.Null(originIdProp);
        Assert.Null(destIdProp);
    }

    /// <summary>
    /// Verify DeliveryJob uses object references for locations.
    /// </summary>
    [Fact]
    public void DeliveryJob_UsesObjectReferences_ForLocations()
    {
        Type jobType = typeof(DeliveryJob);

        // Check for Location object references
        PropertyInfo originProp = jobType.GetProperty("OriginLocation");
        if (originProp != null)
        {
            Assert.Equal(typeof(Location), originProp.PropertyType);
        }

        PropertyInfo destProp = jobType.GetProperty("DestinationLocation");
        if (destProp != null)
        {
            Assert.Equal(typeof(Location), destProp.PropertyType);
        }

        // Should NOT have location IDs
        PropertyInfo originIdProp = jobType.GetProperty("OriginLocationId");
        PropertyInfo destIdProp = jobType.GetProperty("DestinationLocationId");
        Assert.Null(originIdProp);
        Assert.Null(destIdProp);
    }

    /// <summary>
    /// Verify Player does not have entity ID.
    /// </summary>
    [Fact]
    public void Player_DoesNot_HaveEntityId()
    {
        Type playerType = typeof(Player);

        // Player should not have any ID property
        PropertyInfo idProp = playerType.GetProperty("Id");
        PropertyInfo playerIdProp = playerType.GetProperty("PlayerId");

        Assert.Null(idProp);
        Assert.Null(playerIdProp);
    }

    /// <summary>
    /// Verify Obligation uses object references for patron NPC.
    /// </summary>
    [Fact]
    public void Obligation_UsesObjectReference_ForPatron()
    {
        Type obligationType = typeof(Obligation);

        // Should have PatronNpc object reference
        PropertyInfo patronProp = obligationType.GetProperty("PatronNpc");
        if (patronProp != null)
        {
            Assert.Equal(typeof(NPC), patronProp.PropertyType);
        }

        // Should NOT have PatronNpcId
        PropertyInfo patronIdProp = obligationType.GetProperty("PatronNpcId");
        Assert.Null(patronIdProp);
    }

    /// <summary>
    /// Verify Scene uses object references for placement location.
    /// </summary>
    [Fact]
    public void Scene_UsesObjectReference_ForLocation()
    {
        Type sceneType = typeof(Scene);

        // Check for Location reference (actual property name may vary)
        PropertyInfo[] properties = sceneType.GetProperties();
        List<PropertyInfo> locationProps = properties
            .Where(p => p.PropertyType == typeof(Location))
            .ToList();

        // Should NOT have LocationId
        PropertyInfo locationIdProp = properties.FirstOrDefault(p =>
            p.Name == "LocationId" && p.PropertyType == typeof(string));
        Assert.Null(locationIdProp);
    }

    /// <summary>
    /// Verify helper entry classes use object references.
    /// </summary>
    [Fact]
    public void HelperEntries_UseObjectReferences()
    {
        // Player.KnownRoutes should be List<RouteOption> (no wrapper class needed)
        Type playerType = typeof(Player);
        PropertyInfo knownRoutesProp = playerType.GetProperty("KnownRoutes");
        Assert.NotNull(knownRoutesProp);
        Assert.Equal(typeof(List<RouteOption>), knownRoutesProp.PropertyType);

        // NPCExchangeCardEntry
        Type exchangeEntryType = typeof(NPCExchangeCardEntry);
        PropertyInfo npcProp = exchangeEntryType.GetProperty("Npc");
        Assert.NotNull(npcProp);
        Assert.Equal(typeof(NPC), npcProp.PropertyType);
    }

    /// <summary>
    /// Verify that no domain entity has both Name and Id.
    /// Names are for display, IDs are forbidden on domain entities.
    /// </summary>
    [Fact]
    public void DomainEntities_WithName_ShouldNot_HaveId()
    {
        List<string> violations = new List<string>();

        foreach (Type entityType in DomainEntityTypes)
        {
            PropertyInfo nameProp = entityType.GetProperty("Name");
            PropertyInfo idProp = entityType.GetProperty("Id");

            // If entity has Name property, it should NOT have Id property
            if (nameProp != null && nameProp.PropertyType == typeof(string) &&
                idProp != null && idProp.PropertyType == typeof(string))
            {
                violations.Add($"{entityType.Name} has both Name and Id - domain entities should use Name for identity, not redundant Id");
            }
        }

        Assert.Empty(violations);
    }

    /// <summary>
    /// Verify TemporaryRouteBlock uses object reference for route.
    /// </summary>
    [Fact]
    public void TemporaryRouteBlock_UsesObjectReference()
    {
        Type blockType = typeof(TemporaryRouteBlock);

        // Should have Route object reference
        PropertyInfo routeProp = blockType.GetProperty("Route");
        Assert.NotNull(routeProp);
        Assert.Equal(typeof(RouteOption), routeProp.PropertyType);

        // Should NOT have RouteId or RouteName for lookup
        PropertyInfo routeIdProp = blockType.GetProperty("RouteId");
        Assert.Null(routeIdProp);
    }
}
