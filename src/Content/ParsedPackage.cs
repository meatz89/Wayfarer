using System;
using System.Collections.Generic;

/// <summary>
/// Holds a parsed but not yet applied package.
/// This intermediate state allows for dependency analysis and optimal ordering
/// before applying content to the game world.
/// </summary>
public class ParsedPackage
{
    /// <summary>
    /// The original package object from JSON
    /// </summary>
    public Package Package { get; set; }

    /// <summary>
    /// Unique identifier for this package
    /// </summary>
    public string PackageId => Package?.PackageId;

    /// <summary>
    /// File path this was loaded from (if applicable)
    /// </summary>
    public string SourcePath { get; set; }

    /// <summary>
    /// When this package was parsed
    /// </summary>
    public DateTime ParsedAt { get; set; }

    /// <summary>
    /// Dependencies this package has on other entities
    /// Used to determine optimal loading order
    /// </summary>
    public PackageDependencies Dependencies { get; set; }

    /// <summary>
    /// Priority for loading (lower numbers load first)
    /// Core: 0, Base: 100, Expansion: 200, Generated: 300
    /// </summary>
    public int LoadPriority { get; set; }

    public ParsedPackage()
    {
        ParsedAt = DateTime.Now;
        Dependencies = new PackageDependencies();
    }

    /// <summary>
    /// Calculate load priority based on package naming conventions
    /// </summary>
    public void CalculateLoadPriority()
    {
        if (SourcePath == null)
        {
            LoadPriority = 999; // Unknown packages load last
            return;
        }

        string lowerPath = SourcePath.ToLowerInvariant();

        if (lowerPath.Contains("core"))
            LoadPriority = 0;
        else if (lowerPath.Contains("base"))
            LoadPriority = 100;
        else if (lowerPath.Contains("expansion"))
            LoadPriority = 200;
        else if (lowerPath.Contains("generated"))
            LoadPriority = 300;
        else
            LoadPriority = 150; // Default between base and expansion
    }
}

/// <summary>
/// Tracks dependencies between entities in a package
/// </summary>
public class PackageDependencies
{
    /// <summary>
    /// Location IDs referenced by NPCs in this package
    /// </summary>
    public List<string> RequiredLocationIds { get; set; } = new List<string>();

    /// <summary>
    /// Spot IDs referenced by NPCs in this package
    /// </summary>
    public List<string> RequiredSpotIds { get; set; } = new List<string>();

    /// <summary>
    /// Card IDs referenced by deck compositions in this package
    /// </summary>
    public List<string> RequiredCardIds { get; set; } = new List<string>();

    /// <summary>
    /// NPC IDs referenced by requests or exchanges in this package
    /// </summary>
    public List<string> RequiredNpcIds { get; set; } = new List<string>();

    /// <summary>
    /// Item IDs referenced by exchanges or obligations in this package
    /// </summary>
    public List<string> RequiredItemIds { get; set; } = new List<string>();

    /// <summary>
    /// Region IDs referenced by districts in this package
    /// </summary>
    public List<string> RequiredRegionIds { get; set; } = new List<string>();

    /// <summary>
    /// District IDs referenced by locations in this package
    /// </summary>
    public List<string> RequiredDistrictIds { get; set; } = new List<string>();

    /// <summary>
    /// Check if this package has any unmet dependencies given the current loaded entities
    /// </summary>
    public bool HasUnmetDependencies(List<string> loadedLocationIds,
                                     List<string> loadedSpotIds,
                                     List<string> loadedCardIds,
                                     List<string> loadedNpcIds,
                                     List<string> loadedItemIds,
                                     List<string> loadedRegionIds,
                                     List<string> loadedDistrictIds)
    {
        // Check if all required locations are loaded
        foreach (var locId in RequiredLocationIds)
        {
            if (!loadedLocationIds.Contains(locId))
                return true;
        }

        // Check if all required spots are loaded
        foreach (var spotId in RequiredSpotIds)
        {
            if (!loadedSpotIds.Contains(spotId))
                return true;
        }

        // Check if all required cards are loaded
        foreach (var cardId in RequiredCardIds)
        {
            if (!loadedCardIds.Contains(cardId))
                return true;
        }

        // Check if all required NPCs are loaded
        foreach (var npcId in RequiredNpcIds)
        {
            if (!loadedNpcIds.Contains(npcId))
                return true;
        }

        // Check if all required items are loaded
        foreach (var itemId in RequiredItemIds)
        {
            if (!loadedItemIds.Contains(itemId))
                return true;
        }

        // Check if all required regions are loaded
        foreach (var regionId in RequiredRegionIds)
        {
            if (!loadedRegionIds.Contains(regionId))
                return true;
        }

        // Check if all required districts are loaded
        foreach (var districtId in RequiredDistrictIds)
        {
            if (!loadedDistrictIds.Contains(districtId))
                return true;
        }

        return false; // All dependencies met
    }

    /// <summary>
    /// Calculate a dependency score (higher = more dependencies)
    /// </summary>
    public int GetDependencyScore()
    {
        return RequiredLocationIds.Count +
               RequiredSpotIds.Count +
               RequiredCardIds.Count +
               RequiredNpcIds.Count +
               RequiredItemIds.Count +
               RequiredRegionIds.Count +
               RequiredDistrictIds.Count;
    }
}