# Wayfarer Forward Progression System: Design Document

## 1. Introduction and Goals

Wayfarer combines a tag-based encounter system with a dynamically evolving world. While previous design documents established the core encounter mechanics and world evolution, this document focuses on ensuring coherent progression through the game world.

### Design Goals

1. **Forward Momentum**: Create incentives for players to continuously explore new areas
2. **Narrative Coherence**: Prevent encounter repetition while maintaining world continuity
3. **Balanced Resources**: Ensure resource management remains meaningful without becoming frustrating
4. **Hub-Based Development**: Create logical story development points that serve as narrative anchors
5. **Depth-Based Difficulty**: Scale challenges to match player development as they progress further
6. **Sustainable Evolution**: Guarantee the AI world evolution creates a playable, balanced experience

## 2. Node-Based World Structure

### 2.1 Location Network

Wayfarer's world consists of interconnected location nodes rather than a geographic map:

- **Locations**: Primary world units (towns, forests, ruins, etc.)
- **Location Spots**: Interaction points within locations (market stalls, clearings, altars, etc.)
- **Connections**: Logical travel paths between locations with associated costs
- **Depth**: Each location's distance from the starting point, measured in "hops"

### 2.2 Travel Mechanics

Travel between locations consumes resources:

- **Energy Cost**: Primary resource expended during travel, based on connection difficulty
- **Time Advancement**: Travel advances game time, affecting environmental properties
- **Risk Level**: Some connections expose players to random encounters
- **Connection Requirements**: Some paths require specific items or skills to traverse

### 2.3 Location Types

The world contains different types of locations:

- **Hubs**: Major locations with multiple location spots and essential services
- **Connective Locations**: Minor areas that create paths between hubs
- **Landmarks**: Special locations with unique encounters or significance
- **Hazard Zones**: Dangerous areas with higher risk/reward ratios

## 3. Forward Progression System

### 3.1 Depth-Based Structure

The world organizes progression based on depth from the starting point:

- **Initial Depth (0-1)**: Tutorial area with introductory encounters
- **Early Game (2-4)**: First major challenges and skill development
- **Mid Game (5-10)**: More complex encounters and strategic decisions
- **Late Game (11+)**: Advanced challenges requiring mastery

### 3.2 Difficulty Scaling

Encounter difficulty increases with depth:

- **Difficulty Level = Base + (Depth × Scaling Factor)**
- **Momentum Requirements**: Higher thresholds at greater depths
- **Pressure Generation**: Increased rate at greater depths
- **Strategic Complexity**: More challenging strategic tag combinations

### 3.3 Forward Incentives

Players are encouraged to move forward through:

- **Discovery Bonus**: XP and resources for discovering new locations
- **Diminishing Returns**: Reduced rewards for revisiting old areas
- **Quest Objectives**: Goals located at greater depths
- **Narrative Evolution**: Story developments that pull players forward

## 4. Hub-and-Spoke Design

### 4.1 Hub Structure

Every X depth levels, the world contains a hub location:

- **Hub Intervals**: Major hubs appear at depths 0, 3-5, 8-10, etc.
- **Essential Services**: Each hub contains spots for rest, trade, healing, and information
- **NPC Concentration**: Multiple characters with relationship development potential
- **Multiple Actions**: At least 5-8 different actions available
- **Story Beats**: Major narrative developments occur at hubs

### 4.2 Connective Paths

Between hubs, connective locations create progression paths:

- **Simple Locations**: Fewer spots and services than hubs
- **Forward Branching**: Each location connects to 1-3 locations at equal or greater depth
- **Resource Challenges**: Paths may test different resource types
- **Approach Variety**: Different paths favor different approach tags

### 4.3 Path Selection

Players choose their progression path through:

- **Connection Discovery**: Learning about new locations through encounters
- **Path Requirements**: Some connections require specific items or skills
- **Risk Assessment**: Evaluating danger levels of different routes
- **Resource Management**: Considering energy costs and available resources

## 5. Action Types and Repeatability

### 5.1 Encounter Actions

One-time narrative experiences:

- **Completion Tracking**: System marks encounters as completed after resolution
- **Visual Differentiation**: Completed encounters appear visually different
- **Narrative Continuity**: Future encounters may reference completed ones
- **World Impact**: Outcomes affect world evolution and character relationships

### 5.2 Basic Actions

Repeatable mechanical interactions:

- **Resource Management**: Actions focused on resource recovery or exchange
- **No Narrative Progression**: Don't advance main story elements
- **Consistent Effects**: Predictable outcomes each time
- **Examples**: Rest, Purchase Supplies, Sell Items, Cook Food, Repair Equipment

### 5.3 Transition Between Types

Some actions may change type based on context:

- **Initial Quest → Repeatable Service**: First meeting with a merchant becomes a simple trade action
- **One-Time Training → Practice**: Initial skill learning becomes a repeatable practice action
- **Story Event → Location Feature**: After a key event, a location may offer a repeatable action

## 6. Resource Management Integration

### 6.1 Essential Resources

Players must manage several critical resources:

- **Energy**: Required for travel and some actions
- **Health/Confidence/Concentration**: Sustains encounters
- **Money**: Required for goods and services
- **Time**: Advances with actions and travel
- **Supplies**: Consumables for resource recovery

### 6.2 Resource Management Loop

The world structure creates a sustainable resource cycle:

- **Expenditure**: Resources consumed during travel and encounters
- **Decision Point**: Player evaluates resource levels
- **Recovery Options**: Basic actions available at appropriate intervals
- **Cost-Benefit Analysis**: Trading coins or time for other resources
- **Forward Momentum**: Resource recovery occurs alongside progression

### 6.3 Resource Guarantees

World evolution ensures resource accessibility:

- **Maximum Travel Distance**: No more than X connections between rest locations
- **Emergency Options**: When resource levels are critical, new recovery options appear
- **Cost Scaling**: Resource costs increase with depth, maintaining tension
- **Service Distribution**: Different recovery options distributed across locations

## 7. Player Advancement Integration

### 7.1 Experience and Leveling

Player advancement correlates with world progression:

- **XP from Exploration**: Discovering new locations grants experience
- **Depth-Appropriate Challenges**: Encounters scale to challenge but not overwhelm
- **Skill Requirements**: Some paths require minimum levels or skills
- **Training Opportunities**: Hubs offer advancement opportunities

### 7.2 Card System Integration

The card-based progression system ties to world depth:

- **Card Availability**: Higher tier cards available at greater depths
- **Trainer Distribution**: Different approaches trainable at different hubs
- **Environmental Synergy**: Location properties match available card synergies
- **Progression Gates**: Some paths require specific cards to traverse

### 7.3 Relationship Development

Character relationships evolve alongside world progression:

- **NPC Movement**: Some characters appear at multiple depths
- **Relationship Consequences**: Past interactions affect future encounters
- **Faction Introduction**: New depths introduce new factions
- **Alliance Benefits**: Strong relationships unlock unique paths

## 8. World Evolution Requirements

### 8.1 Core Evolution Rules

Each completed encounter must generate:

- **Forward Progression**: At least one new location or spot at equal or greater depth
- **Alternative Path**: At least one new action offering a different approach
- **Resource Consideration**: If player resources are critical, guaranteed recovery option
- **Depth-Appropriate Challenge**: Difficulty scaled to current depth

### 8.2 Hub Generation Requirements

The world evolution must maintain hub structure:

- **Hub Detection**: System identifies when player is X connections from last hub
- **Hub Creation**: Forces generation of hub location with required services
- **Service Verification**: Ensures all essential services exist at each hub
- **Path Convergence**: Multiple paths lead to major hubs

### 8.3 AI Constraints

The world evolution AI operates under these constraints:

- **Narrative Coherence**: New elements must connect logically to existing ones
- **Resource Balance**: Cannot create excessive travel requirements without resources
- **Difficulty Curve**: Must scale challenges according to depth
- **Choice Preservation**: Must maintain multiple viable progression paths

## 9. Implementation Structure

### 9.1 World State Manager

```csharp
public class WorldStateManager
{
    // Track completed encounters to prevent repetition
    private HashSet<string> CompletedEncounterIds { get; } = new HashSet<string>();
    
    // Track location depths
    private Dictionary<string, int> LocationDepths { get; } = new Dictionary<string, int>();
    
    // Track discovered locations
    private HashSet<string> DiscoveredLocationIds { get; } = new HashSet<string>();
    
    public void MarkEncounterCompleted(string encounterId)
    {
        CompletedEncounterIds.Add(encounterId);
    }
    
    public bool IsEncounterCompleted(string encounterId)
    {
        return CompletedEncounterIds.Contains(encounterId);
    }
    
    public void SetLocationDepth(string locationId, int depth)
    {
        LocationDepths[locationId] = depth;
    }
    
    public int GetLocationDepth(string locationId)
    {
        return LocationDepths.TryGetValue(locationId, out int depth) ? depth : -1;
    }
    
    // Additional methods for managing the world state...
}
```

### 9.2 Progression Validator

```csharp
public class ProgressionValidator
{
    private readonly WorldStateManager _worldState;
    private readonly PlayerState _playerState;
    
    // Constants defining progression requirements
    private const int HUB_INTERVAL = 4; // Maximum depth between hubs
    private const int REST_INTERVAL = 2; // Maximum depth between rest locations
    
    public ValidationResult ValidateEvolution(WorldEvolutionResponse evolution)
    {
        int currentDepth = _worldState.GetLocationDepth(_playerState.CurrentLocationId);
        
        // Check for forward progression
        bool hasForwardProgression = CheckForwardProgression(evolution, currentDepth);
        
        // Check hub distribution
        bool hasProperHubDistribution = CheckHubDistribution(evolution, currentDepth);
        
        // Check resource access
        bool hasResourceAccess = CheckResourceAccess(evolution);
        
        return new ValidationResult
        {
            IsValid = hasForwardProgression && hasProperHubDistribution && hasResourceAccess,
            MissingElements = GetMissingElements(
                hasForwardProgression, 
                hasProperHubDistribution, 
                hasResourceAccess)
        };
    }
    
    // Implementation details for validation methods...
}
```

### 9.3 Evolution Enhancer

```csharp
public class EvolutionEnhancer
{
    private readonly WorldStateManager _worldState;
    private readonly PlayerState _playerState;
    private readonly ProgressionValidator _validator;
    
    public WorldEvolutionResponse EnhanceEvolution(WorldEvolutionResponse initialResponse)
    {
        // Validate initial response
        ValidationResult validation = _validator.ValidateEvolution(initialResponse);
        
        if (validation.IsValid)
        {
            return initialResponse;
        }
        
        // Fix missing elements
        WorldEvolutionResponse enhancedResponse = initialResponse;
        
        foreach (string missingElement in validation.MissingElements)
        {
            enhancedResponse = AddMissingElement(enhancedResponse, missingElement);
        }
        
        return enhancedResponse;
    }
    
    // Methods to add specific missing progression elements...
}
```

## 10. Example Progression Flow

### 10.1 Early Game Example

1. **Starting Village** (Depth 0, Hub)
   - Player completes initial encounters
   - Learns basic mechanics
   - Chooses one of several paths forward

2. **Forest Path** (Depth 1, Connective)
   - Encounters create branches to:
     - Hunter's Camp (Depth 2, Connective)
     - Abandoned Shrine (Depth 2, Landmark)

3. **Hunter's Camp** (Depth 2, Connective)
   - Limited resources available
   - Encounters point toward Crossroads

4. **Crossroads** (Depth 3, Hub)
   - Full services available
   - Multiple NPCs with quests
   - Branches to multiple Depth 4 locations

### 10.2 AI-Generated Path Examples

When a player completes "Negotiate with Bandits" at Depth 2:
1. AI detects player at Depth 2 with low Energy
2. AI identifies no hub within last 3 connections
3. AI generates:
   - Crossroads Town (Hub at Depth 3) with all essential services
   - Bandit Hideout (Challenge at Depth 3) for players seeking combat
   - Merchant Caravan (Resource at Depth 2) for immediate recovery

## 11. Conclusion

This forward progression system enhances Wayfarer's dynamically evolving world by ensuring:

1. Players naturally move forward through the world without repetition
2. Resources remain balanced throughout progression
3. Narrative and difficulty scale appropriately with advancement
4. AI-driven evolution maintains a coherent, engaging game structure
5. Hub locations provide anchor points for major story developments

By implementing these systems, Wayfarer creates a continuously unfolding adventure that feels personally tailored to each player's choices, yet always maintains narrative and mechanical coherence.