using Wayfarer.Content.DTOs;

namespace Wayfarer.Content;

/// <summary>
/// ISOLATION BOUNDARY: Specs returned from SceneInstantiator to orchestrator
/// SceneInstantiator generates these specs (pure domain logic)
/// Orchestrator creates JSON files and loads via PackageLoader (infrastructure)
/// </summary>
public class DependentResourceSpecs
{
    public List<LocationDTO> Locations { get; set; } = new();
    public List<ItemDTO> Items { get; set; } = new();
    public string PackageId { get; set; }
    public string PackageJson { get; set; }
    public List<string> CreatedLocationIds { get; set; } = new();
    public List<string> CreatedItemIds { get; set; } = new();
    public List<string> ItemsToAddToInventory { get; set; } = new();

    public static DependentResourceSpecs Empty => new DependentResourceSpecs();

    public bool HasResources => Locations.Any() || Items.Any();
}
