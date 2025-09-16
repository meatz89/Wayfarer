# Package Loading System Refactor

## Problem Statement
The current two-phase loading system is overly complex and causes issues with cross-package dependencies. Specifically:
- Routes can't find spots from different packages, leaving location.Connections empty
- The travel system breaks because TravelManager.GetRoute() returns null
- The system tries to solve both static and dynamic loading with one complex solution

## Root Cause
The two-phase system (parse all, then apply all) doesn't properly handle cross-package references. When travel_package.json loads routes that reference spots from core_game_package.json, the lookup fails even though the spots exist.

## Solution: Separate Static and Dynamic Loading

### Static Content Loading (Game Start)
- Simple, sequential loading based on filename order
- No skeletons needed - we control the file organization
- Load packages in dependency order using numbered prefixes

### Dynamic Content Loading (Runtime AI)
- Keep skeleton system ONLY for AI-generated content
- Single package at a time with immediate completion
- Load → Create skeletons → AI completes → Load completion

## Implementation Details

### 1. File Reorganization
```
/Content/Core/
  01_foundation.json   (regions, districts, locations, spots)
  02_cards.json        (all card definitions)
  03_npcs.json         (NPCs and their decks)
  04_connections.json  (routes and relationships)
  05_gameplay.json     (items, letters, obligations)
```

### 2. Simplified PackageLoader
```csharp
public void LoadStaticPackages(string directoryPath)
{
    var packages = Directory.GetFiles(directoryPath, "*.json")
        .Where(f => !f.Contains("game_rules"))
        .OrderBy(f => Path.GetFileName(f))
        .ToList();

    foreach (var packageFile in packages)
    {
        LoadPackage(packageFile, allowSkeletons: false);
    }

    ValidateAndInitialize();
}

public List<string> LoadDynamicPackage(string json)
{
    LoadPackage(json, allowSkeletons: true);
    return _gameWorld.SkeletonRegistry.Keys.ToList();
}
```

### 3. Loading Order Within Package
Always load in dependency order:
1. Regions & Districts
2. Locations & Spots
3. Cards
4. NPCs
5. Routes (can now find spots)
6. Everything else

### 4. Key Benefits
- **Simplicity**: No complex dependency analysis
- **Performance**: Direct sequential loading
- **Clarity**: Clear separation of static vs dynamic
- **Maintainability**: Easy to understand and debug

## Migration Steps
1. Remove all two-phase loading code
2. Remove ParsedPackage class
3. Reorganize JSON files with numbered prefixes
4. Implement simple LoadStaticPackages method
5. Update LoadPackageContent to use allowSkeletons flag
6. Test travel system functionality