# Content Loading and Extension System

## Overview

Wayfarer uses a **package-based content loading system** that supports modular content, lazy loading with skeleton placeholders, and future AI-driven content generation. This system replaces the legacy phase-based initialization with a flexible, extensible architecture.

## Directory Structure

```
src/Content/
├── Core/                   # Core game packages (loaded at startup)
│   └── core_game_package.json
├── Generated/              # AI-generated content packages (future)
│   └── (empty - for AI-generated packages)
└── TestPackages/           # Test packages (not loaded in production)
    ├── test_01_npcs_with_missing_refs.json
    ├── test_02_locations_resolving_skeletons.json
    └── test_03_letters_with_missing_npcs.json
```

## Architecture Components

### 1. GameWorldInitializer

**Location**: `/src/Content/GameWorldInitializer.cs`

Static factory class that creates and initializes GameWorld instances:

```csharp
public static GameWorld CreateGameWorld()
{
    GameWorld gameWorld = new GameWorld();
    PackageLoader packageLoader = new PackageLoader(gameWorld);
    packageLoader.LoadPackagesFromDirectory("Content/Core");
    return gameWorld;
}
```

**Key Points**:
- Static to avoid circular dependencies during startup
- Loads only from `Content/Core` directory in production
- AI-generated content will be loaded from `Content/Generated`

### 2. PackageLoader

**Location**: `/src/Content/PackageLoader.cs`

Handles loading content packages with proper ordering and skeleton creation:

```csharp
public void LoadPackagesFromDirectory(string directoryPath)
{
    List<string> packageFiles = Directory.GetFiles(directoryPath, "*.json", SearchOption.AllDirectories)
        .OrderBy(f => 
        {
            if (f.Contains("core", StringComparison.OrdinalIgnoreCase)) return 0;
            if (f.Contains("base", StringComparison.OrdinalIgnoreCase)) return 1;
            if (f.Contains("expansion", StringComparison.OrdinalIgnoreCase)) return 2;
            if (f.Contains("generated", StringComparison.OrdinalIgnoreCase)) return 3;
            return 2;
        })
        .ThenBy(f => f)
        .ToList();
    LoadPackages(packageFiles);
}
```

**Load Order Priority**:
1. Core packages (priority 0)
2. Base content (priority 1)
3. Expansions (priority 2)
4. Generated content (priority 3)

### 3. Skeleton System

**Location**: `/src/Content/SkeletonGenerator.cs`

Creates mechanically complete but narratively generic placeholders for missing content:

```csharp
public static NPC GenerateSkeletonNPC(string id, string source)
{
    // Creates NPC with:
    // - Valid mechanical properties (stats, personality, profession)
    // - Generic narrative content ("Unnamed Merchant #3")
    // - IsSkeleton = true flag
    // - SkeletonSource tracking what created it
    // - Empty observation deck
}
```

**Skeleton Properties**:
- **Mechanically Complete**: Has all required game properties
- **Narratively Generic**: Uses placeholder names and descriptions
- **Deterministic**: Same ID always generates same skeleton (hash-based)
- **Replaceable**: Automatically replaced when real content loads

## Package Format

### Package Structure

```json
{
  "packageId": "unique_package_id",
  "metadata": {
    "name": "Package Name",
    "timestamp": "2025-09-02T10:00:00Z",
    "description": "Package description",
    "author": "Author name",
    "version": "1.0.0"
  },
  "startingConditions": {
    "coins": 10,
    "health": { "current": 100, "max": 100 },
    "hunger": { "current": 95, "max": 95 },
    "attention": 10
  },
  "content": {
    "cards": [...],
    "npcs": [...],
    "locations": [...],
    "spots": [...],
    "routes": [...],
    "letterTemplates": [...],
    "observations": [...],
    "investigationRewards": [...]
  }
}
```

### Content Types

#### Cards
```json
{
  "id": "trust_understanding",
  "type": "Normal",
  "connectionType": "Trust",
  "tokenType": "Trust",
  "persistence": "Persistent",
  "focus": 1,
  "baseFlow": 3,
  "displayName": "Mutual Understanding",
  "description": "Share a moment of genuine connection"
}
```

#### NPCs
```json
{
  "id": "elena",
  "name": "Elena",
  "profession": "Scribe",
  "locationId": "copper_kettle_tavern",
  "spotId": "corner_table",
  "personalityType": "DEVOTED",
  "currentState": "DESPERATE",
  "letterTokenTypes": ["Trust"],
  "hasObservationDeck": true
}
```

#### Locations
```json
{
  "id": "market_square",
  "name": "Market Square",
  "description": "Bustling marketplace",
  "tier": 1,
  "locationType": "Hub",
  "travelHubSpotId": "central_fountain",
  "baseFamiliarity": 0,
  "maxFamiliarity": 3
}
```

#### Location Spots
```json
{
  "id": "central_fountain",
  "locationId": "market_square",
  "name": "Central Fountain",
  "properties": {
    "morning": ["quiet"],
    "afternoon": ["busy"],
    "evening": ["closing"]
  },
  "canInvestigate": true
}
```

#### Investigation Rewards
```json
{
  "locationId": "market_square",
  "familiarityRequired": 1,
  "priorObservationRequired": null,
  "observationCard": {
    "id": "safe_passage_knowledge",
    "name": "Safe Passage Knowledge",
    "targetNpcId": "elena",
    "effect": "AdvanceToNeutralState"
  }
}
```

#### Observation Cards for NPC Decks
```json
{
  "id": "safe_passage_knowledge",
  "name": "Safe Passage Knowledge",
  "targetNpcId": "elena",
  "targetDeck": "observation",
  "persistence": "Persistent",
  "focus": 0,
  "effect": {
    "type": "ChangeConnectionState",
    "targetState": "Neutral"
  }
}
```

## Lazy Loading Process

### 1. Missing Reference Detection

When loading content, PackageLoader detects missing references:

```csharp
if (!gameWorld.Locations.Any(l => l.Id == dto.LocationId))
{
    // Location doesn't exist - create skeleton
    var skeleton = SkeletonGenerator.GenerateSkeletonLocation(
        dto.LocationId, 
        $"npc_{dto.Id}_reference");
    
    skeleton.Familiarity = 0;
    skeleton.MaxFamiliarity = 3;
    
    gameWorld.Locations.Add(skeleton);
    gameWorld.SkeletonRegistry[dto.LocationId] = "Location";
}
```

### 2. Skeleton Creation

Skeletons are created with valid game mechanics:

```csharp
var skeleton = new NPC
{
    ID = id,
    Name = $"Unnamed Merchant #{hash % 100}",
    PersonalityType = DeterministicPersonality(hash),
    Profession = DeterministicProfession(hash),
    ObservationDeck = new List<ConversationCard>(),
    IsSkeleton = true,
    SkeletonSource = source
};
```

### 3. Skeleton Resolution

When real content loads, skeletons are replaced:

```csharp
var existingSkeleton = gameWorld.NPCs
    .FirstOrDefault(n => n.ID == dto.Id && n.IsSkeleton);
if (existingSkeleton != null)
{
    // Preserve any observation cards accumulated
    var observationCards = existingSkeleton.ObservationDeck;
    gameWorld.NPCs.Remove(existingSkeleton);
    gameWorld.SkeletonRegistry.Remove(dto.Id);
    
    // Transfer observation cards to real NPC
    realNpc.ObservationDeck.AddRange(observationCards);
}
```

## Extension Points

### 1. Adding New Content Packages

Place new packages in appropriate directories:

- **Core Content**: `Content/Core/my_expansion.json`
- **AI-Generated**: `Content/Generated/ai_content_001.json`
- **Test Content**: `Content/TestPackages/test_scenario.json`

### 2. Custom Content Types

To add new content types:

1. Create DTO class:
```csharp
public class InvestigationRewardDTO
{
    public string LocationId { get; set; }
    public int FamiliarityRequired { get; set; }
    public int? PriorObservationRequired { get; set; }
    public ObservationCardDTO ObservationCard { get; set; }
}
```

2. Add to Package class:
```csharp
public class Package
{
    // ... existing properties
    public List<InvestigationRewardDTO> InvestigationRewards { get; set; }
}
```

3. Add loader method in PackageLoader:
```csharp
private void LoadInvestigationRewards(List<InvestigationRewardDTO> rewardDtos)
{
    foreach (var dto in rewardDtos)
    {
        // Parse and add to location's investigation rewards
        var location = gameWorld.Locations.FirstOrDefault(l => l.Id == dto.LocationId);
        if (location != null)
        {
            location.InvestigationRewards.Add(ParseReward(dto));
        }
    }
}
```

### 3. AI Content Generation

AI systems can generate packages and place them in `Content/Generated/`:

```csharp
// AI generates package
var package = AIContentGenerator.GeneratePackage(context);

// Ensure NPCs have observation decks
foreach (var npc in package.NPCs)
{
    npc.HasObservationDeck = true;
}

// Save to Generated directory
string path = Path.Combine("Content/Generated", $"ai_{timestamp}.json");
File.WriteAllText(path, JsonSerializer.Serialize(package));

// Load into game
packageLoader.LoadPackage(path);
```

## Skeleton Registry

GameWorld maintains a registry of all skeletons:

```csharp
public Dictionary<string, string> SkeletonRegistry { get; set; }

public List<string> GetSkeletonReport()
{
    return SkeletonRegistry.Select(kvp => $"{kvp.Value}: {kvp.Key}").ToList();
}
```

**Registry Format**:
- Key: Entity ID (e.g., "mysterious_tower")
- Value: Entity Type (e.g., "Location")

## Content Validation

### Parsers

Each content type has a dedicated parser with validation:

- `NPCParser.cs` - Validates and parses NPCs (includes observation deck)
- `LocationParser.cs` - Validates and parses locations (includes familiarity)
- `ConversationCardParser.cs` - Validates and parses cards
- `LetterTemplateParser.cs` - Validates and parses letter templates
- `InvestigationRewardParser.cs` - Validates and parses investigation rewards

### Enum Mappings

Parsers handle JSON string to enum conversions:

```csharp
private static ConnectionType ParseConnectionType(string connectionTypeStr)
{
    return connectionTypeStr switch
    {
        "Trust" => ConnectionType.Trust,
        "Commerce" => ConnectionType.Commerce,
        "Status" => ConnectionType.Status,
        "Shadow" => ConnectionType.Shadow,
        _ => throw new ArgumentException($"Unknown connection type: '{connectionTypeStr}'")
    };
}
```

### New Validations

Investigation rewards must validate:
- Location exists or create skeleton
- Familiarity requirement is valid (0-3)
- Prior observation requirement references valid observation level
- Target NPC exists or create skeleton with observation deck

## Testing

### Test Package Location

Test packages are stored in `Content/TestPackages/` and include:

1. **test_01_npcs_with_missing_refs.json** - NPCs referencing non-existent locations
2. **test_02_locations_resolving_skeletons.json** - Locations that resolve skeletons
3. **test_03_letters_with_missing_npcs.json** - Letters referencing non-existent NPCs
4. **test_04_investigations_and_observations.json** - Investigation rewards and observation cards

### Running Skeleton Tests

```bash
dotnet test --filter "FullyQualifiedName~SkeletonSystemTests"
```

Tests verify:
- Skeletons created for missing references
- Skeletons replaced when real content loads
- Deterministic skeleton generation
- Game playability with skeletons
- Multi-package accumulation
- Observation deck preservation during skeleton resolution
- Familiarity initialization on skeleton locations

## Best Practices

### 1. Package Design

- **Self-Contained**: Packages should work independently
- **No Hard Dependencies**: Use skeleton system for references
- **Incremental**: Add content gradually across packages
- **Versioned**: Include version in metadata
- **Observation Cards**: Specify target NPC and deck type

### 2. Content References

- **Allow Missing**: Let skeleton system handle missing references
- **Use Stable IDs**: Don't change entity IDs after publication
- **Document Dependencies**: Note expected references in metadata
- **Observation Targets**: Always specify which NPC receives observation cards

### 3. AI Content Generation

- **Generate Valid JSON**: Follow package schema exactly
- **Use Existing Enums**: Reference only valid enum values
- **Test Locally**: Validate packages before deployment
- **Track Skeletons**: Monitor skeleton registry for gaps
- **Include Observation Decks**: Ensure NPCs have observation deck property

## Migration from Legacy System

### Removed Components

- ❌ Phase-based initialization
- ❌ Individual JSON files (cards.json, npcs.json, etc.)
- ❌ ValidatedContentLoader
- ❌ GameConfigurationLoader
- ❌ CardDeckLoader
- ❌ TravelCardLoader

### New Components

- ✅ Package-based loading
- ✅ Skeleton system
- ✅ Unified PackageLoader
- ✅ Directory-based organization
- ✅ GameWorldInitializer
- ✅ Investigation reward system
- ✅ NPC observation decks

## Future Enhancements

### Planned Features

1. **Skeleton Quality Levels**: Basic → Enhanced → Full content
2. **Skeleton Persistence**: Save which skeletons player has seen
3. **Smart Generation**: Use context to generate better skeletons
4. **Hot Reloading**: Load new packages without restart
5. **Package Dependencies**: Explicit dependency declaration
6. **Content Versioning**: Handle package updates gracefully
7. **Observation Deck Merging**: Combine observation cards from multiple packages

### AI Integration Roadmap

1. **Phase 1**: AI generates packages in standard format
2. **Phase 2**: AI monitors skeleton registry and fills gaps
3. **Phase 3**: AI generates contextual content based on player actions
4. **Phase 4**: Real-time content adaptation and personalization

## Troubleshooting

### Common Issues

**Issue**: "Unknown X in JSON" errors
**Solution**: Check enum mappings in parsers, ensure JSON uses correct values

**Issue**: Too many skeletons created
**Solution**: Verify references exist, check for typos in IDs

**Issue**: Package not loading
**Solution**: Verify JSON syntax, check file location, review console logs

**Issue**: Duplicate content after loading
**Solution**: Ensure unique IDs, check skeleton replacement logic

**Issue**: Observation cards not appearing
**Solution**: Verify NPC has observation deck, check target NPC ID matches

**Issue**: Investigation not yielding cards
**Solution**: Check familiarity level, verify prior observation requirements

## API Reference

### Key Classes

- `GameWorld` - Central game state container
- `GameWorldInitializer` - Static factory for GameWorld creation
- `PackageLoader` - Loads content packages
- `SkeletonGenerator` - Creates placeholder content
- `Package` - Package data structure
- Various parsers - Convert DTOs to domain objects

### Key Methods

```csharp
// Initialize game world
GameWorld gameWorld = GameWorldInitializer.CreateGameWorld();

// Load specific package
packageLoader.LoadPackage("path/to/package.json");

// Load directory of packages
packageLoader.LoadPackagesFromDirectory("Content/Core");

// Check skeleton status
List<string> skeletons = gameWorld.GetSkeletonReport();

// Generate skeleton with observation deck
NPC skeleton = SkeletonGenerator.GenerateSkeletonNPC(id, source);
skeleton.ObservationDeck = new List<ConversationCard>();

// Add observation card to NPC
npc.ObservationDeck.Add(observationCard);
```

## Conclusion

The content loading and extension system provides a robust foundation for:
- **Modular Content**: Load packages independently
- **Graceful Degradation**: Game works with missing content
- **AI Integration**: Ready for procedural generation
- **Easy Extension**: Clear patterns for new content types
- **Testing Support**: Separate test content from production
- **Investigation System**: Location familiarity and observation rewards
- **NPC Knowledge**: Observation decks for discovered information

This architecture ensures Wayfarer can grow and adapt with new content while maintaining stability and playability.