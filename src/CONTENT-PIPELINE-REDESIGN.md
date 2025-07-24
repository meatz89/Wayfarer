# Content Pipeline Redesign for Multi-Dimensional Relationships

## Overview

The current content pipeline fails because it tries to validate and load content in isolation. With multi-dimensional NPC relationships (Trade, Trust, Noble, Common, Shadow), context-dependent interactions, and complex interdependencies, we need a graph-based, multi-phase loading system.

## Core Problems with Current System

1. **Sequential Loading**: Loads NPCs before routes, but routes affect NPC relationships
2. **Isolated Validation**: Validates each file independently, missing cross-references
3. **Single-Dimensional Thinking**: Treats NPCs as single entities, not multi-faceted relationships
4. **No Context Awareness**: Can't validate that "Martha (Trade 5)" and "Martha (Trust 3)" are the same person
5. **Rigid Type System**: DTOs don't capture the dynamic nature of token-based relationships

## New Architecture: Graph-Based Multi-Phase Loading

### Phase 1: Schema Discovery and Raw Loading
```csharp
public class ContentGraph
{
    // Raw content before validation
    private Dictionary<string, RawContent> _rawContent = new();
    
    // Entity registry with multi-key support
    private Dictionary<string, GraphNode> _nodes = new();
    
    // Relationship edges
    private List<GraphEdge> _edges = new();
}

public class GraphNode
{
    public string Id { get; set; }
    public string Type { get; set; } // "NPC", "Location", "Route", etc.
    public Dictionary<string, object> Properties { get; set; }
    public Dictionary<string, RelationshipContext> Contexts { get; set; } // Multi-dimensional
}

public class RelationshipContext
{
    public ConnectionType TokenType { get; set; }
    public int TokenCount { get; set; }
    public List<string> AvailableActions { get; set; }
    public List<string> PossibleLetterTypes { get; set; }
}
```

### Phase 2: Relationship Resolution
```csharp
public class RelationshipResolver
{
    public void ResolveMultiDimensionalRelationships(ContentGraph graph)
    {
        // For each NPC, create separate contexts for each token type
        foreach (var npcNode in graph.GetNodesOfType("NPC"))
        {
            var npcData = npcNode.Properties;
            
            // Create contexts for each possible token type
            foreach (ConnectionType tokenType in Enum.GetValues<ConnectionType>())
            {
                if (CanNPCHaveTokenType(npcData, tokenType))
                {
                    var context = new RelationshipContext
                    {
                        TokenType = tokenType,
                        TokenCount = 0, // Starting value
                        AvailableActions = DetermineActionsForContext(npcData, tokenType),
                        PossibleLetterTypes = DetermineLetterTypesForContext(npcData, tokenType)
                    };
                    
                    npcNode.Contexts[$"{tokenType}"] = context;
                }
            }
        }
    }
}
```

### Phase 3: Content Loading Pipeline

```csharp
public class MultiPhaseContentLoader
{
    private readonly ContentGraph _graph = new();
    private readonly List<IContentPhase> _phases = new();
    
    public MultiPhaseContentLoader()
    {
        // Order matters! Each phase builds on the previous
        _phases.Add(new RawLoadPhase());           // Load all JSON into memory
        _phases.Add(new EntityCreationPhase());     // Create basic entities
        _phases.Add(new RelationshipPhase());       // Resolve multi-dimensional relationships
        _phases.Add(new ValidationPhase());         // Validate with full context
        _phases.Add(new OptimizationPhase());       // Optimize for runtime
        _phases.Add(new MaterializationPhase());    // Create final game objects
    }
    
    public GameWorld LoadContent(string contentPath)
    {
        foreach (var phase in _phases)
        {
            var result = phase.Execute(_graph, contentPath);
            if (!result.Success)
            {
                LogPhaseErrors(phase, result);
                throw new ContentLoadException($"Phase {phase.Name} failed", result.Errors);
            }
        }
        
        return _graph.Materialize();
    }
}
```

## Phase Descriptions

### 1. Raw Load Phase
- Loads ALL JSON files into memory without parsing
- Creates file dependency map
- No validation, just reading

### 2. Entity Creation Phase
- Creates placeholder entities for all IDs found
- Builds initial graph structure
- Defers relationship resolution

### 3. Relationship Phase
- Resolves all cross-references
- Creates multi-dimensional relationship contexts
- Handles circular dependencies gracefully

### 4. Validation Phase
- Validates with full context available
- Can check complex rules like "Martha must have Trade context to offer dock letters"
- Reports errors with relationship context

### 5. Optimization Phase
- Pre-calculates common queries
- Indexes by multiple keys (NPC+TokenType combinations)
- Prepares for efficient runtime access

### 6. Materialization Phase
- Creates actual game objects
- Injects dependencies
- Produces final GameWorld

## New Content Structure

### Multi-Dimensional NPC Definition
```json
{
  "id": "martha_docker",
  "name": "Martha",
  "baseLocation": "millbrook",
  "baseSpot": "docks_main",
  "profession": "Docker",
  
  "tokenContexts": {
    "Trade": {
      "enabled": true,
      "startingRelationship": 1,
      "letterTypes": ["dock_manifest", "trade_delivery"],
      "actions": ["help_load_cargo", "negotiate_rates"],
      "obligations": {
        "5": {
          "id": "dock_priority",
          "type": "QueuePosition",
          "value": 5
        }
      }
    },
    "Trust": {
      "enabled": true,
      "startingRelationship": 0,
      "letterTypes": ["personal_request", "family_medicine"],
      "actions": ["ask_about_daughter", "share_meal"],
      "obligations": {
        "5": {
          "id": "family_guardian",
          "type": "MonthlyCheck",
          "value": "daughter_wellbeing"
        }
      }
    },
    "Common": {
      "enabled": true,
      "startingRelationship": 0,
      "letterTypes": ["casual_delivery"],
      "actions": ["share_lunch", "dock_gossip"]
    }
  }
}
```

### Context-Aware Letter Templates
```json
{
  "id": "martha_trade_urgent",
  "name": "Urgent Dock Manifest",
  "requiredContext": {
    "npcId": "martha_docker",
    "tokenType": "Trade",
    "minTokens": 2
  },
  "properties": {
    "urgency": "high",
    "payment": { "min": 8, "max": 12 },
    "deadline": { "min": 24, "max": 36 },
    "queuePosition": "calculated" // Based on Trade token count
  }
}
```

### Multi-Requirement Routes
```json
{
  "id": "scholars_evening_library",
  "name": "Scholar's Evening Access",
  "requirements": {
    "type": "MultiToken",
    "conditions": [
      {
        "npcId": "aldric_scholar",
        "tokenType": "Trust",
        "minTokens": 3
      },
      {
        "npcId": "aldric_scholar", 
        "tokenType": "Noble",
        "minTokens": 1
      }
    ]
  }
}
```

## Validation Rules for Multi-Dimensional System

### 1. Context Consistency
```csharp
public class ContextConsistencyValidator : IContentValidator
{
    public ValidationResult Validate(ContentGraph graph)
    {
        var errors = new List<ValidationError>();
        
        // Check that letter templates match declared contexts
        foreach (var letter in graph.GetNodesOfType("LetterTemplate"))
        {
            var requiredContext = letter.Properties["requiredContext"];
            var npcId = requiredContext["npcId"];
            var tokenType = requiredContext["tokenType"];
            
            var npc = graph.GetNode(npcId);
            if (!npc.Contexts.ContainsKey(tokenType))
            {
                errors.Add(new ValidationError(
                    $"Letter {letter.Id} requires {npcId} to have {tokenType} context, but NPC doesn't support it"
                ));
            }
        }
        
        return new ValidationResult(errors);
    }
}
```

### 2. Obligation Validation
```csharp
public class ObligationContextValidator : IContentValidator
{
    public ValidationResult Validate(ContentGraph graph)
    {
        // Ensure obligations are defined within appropriate token contexts
        // Check that obligation thresholds make sense
        // Verify obligation types match token types
    }
}
```

## Runtime Access Patterns

### Efficient Multi-Dimensional Queries
```csharp
public class MultiDimensionalNPCRepository
{
    private readonly Dictionary<(string npcId, ConnectionType tokenType), NPCContext> _contextCache;
    
    public NPCContext GetNPCContext(string npcId, ConnectionType tokenType)
    {
        return _contextCache.TryGetValue((npcId, tokenType), out var context) 
            ? context 
            : null;
    }
    
    public IEnumerable<NPCContext> GetAllContextsForNPC(string npcId)
    {
        return _contextCache
            .Where(kvp => kvp.Key.npcId == npcId)
            .Select(kvp => kvp.Value);
    }
    
    public IEnumerable<NPCContext> GetNPCsWithTokenType(ConnectionType tokenType, int minTokens = 0)
    {
        return _contextCache
            .Where(kvp => kvp.Key.tokenType == tokenType && kvp.Value.TokenCount >= minTokens)
            .Select(kvp => kvp.Value);
    }
}
```

## Migration Strategy

### Step 1: Parallel Implementation
- Keep existing system running
- Implement new loader alongside
- Use feature flag to switch

### Step 2: Adapter Layer
```csharp
public class LegacyToGraphAdapter
{
    public ContentGraph ConvertLegacyContent(
        List<Location> locations,
        List<NPC> npcs,
        List<Route> routes)
    {
        var graph = new ContentGraph();
        
        // Convert each NPC to multi-context node
        foreach (var npc in npcs)
        {
            var node = new GraphNode
            {
                Id = npc.ID,
                Type = "NPC",
                Properties = ConvertToProperties(npc),
                Contexts = InferContextsFromNPC(npc)
            };
            
            graph.AddNode(node);
        }
        
        return graph;
    }
}
```

### Step 3: Content Migration Tools
```csharp
public class ContentMigrationTool
{
    public void MigrateNPCFile(string oldPath, string newPath)
    {
        var oldNPCs = JsonSerializer.Deserialize<List<OldNPCDTO>>(
            File.ReadAllText(oldPath));
            
        var newNPCs = oldNPCs.Select(npc => new MultiDimensionalNPC
        {
            Id = npc.Id,
            Name = npc.Name,
            TokenContexts = InferTokenContexts(npc)
        });
        
        File.WriteAllText(newPath, 
            JsonSerializer.Serialize(newNPCs, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            }));
    }
}
```

## Benefits of New System

1. **Handles Complexity**: Multi-dimensional relationships are first-class concepts
2. **Validates Correctly**: Can validate complex cross-references and contexts
3. **Extensible**: Easy to add new token types or relationship dimensions
4. **Performance**: Pre-computed lookups for common queries
5. **Debuggable**: Clear phase separation makes issues easier to identify
6. **Future-Proof**: Can handle even more complex relationship systems

## Implementation Priority

1. **Core Graph Structure** (2 days)
   - ContentGraph class
   - GraphNode with multi-dimensional support
   - Basic phase infrastructure

2. **Relationship Resolution** (3 days)
   - Multi-context creation
   - Cross-reference resolution
   - Circular dependency handling

3. **Validation Framework** (2 days)
   - Context-aware validators
   - Error reporting with relationship context
   - Validation rule engine

4. **Migration Tools** (2 days)
   - Legacy format readers
   - Automated conversion
   - Validation of migrated content

5. **Runtime Integration** (2 days)
   - Repository updates
   - Query optimization
   - Performance testing

Total: ~11 days for full implementation

## Next Steps

1. Create proof-of-concept with single NPC (Martha with Trade/Trust contexts)
2. Validate approach handles all edge cases
3. Build out full implementation
4. Migrate existing content
5. Update all systems to use new pipeline