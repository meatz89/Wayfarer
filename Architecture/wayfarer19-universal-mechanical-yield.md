# Universal Mechanical Yield System - Data Structures

After deep analysis, I've designed a system that focuses exclusively on quantifiable mechanical benefits. This eliminates narrative elements in favor of concrete gameplay impacts.

## Core Data Structures

### 1. ActionDefinition

```csharp
public class ActionDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int EnergyCost { get; set; }
    public int TimeCost { get; set; }
    public int EncounterChance { get; set; }
    public EncounterTypes EncounterType { get; set; }
    public List<YieldDefinition> Yields { get; set; } = new List<YieldDefinition>();
    public SkillRequirement SkillRequirement { get; set; }
    public ActionCategories Category { get; set; }
}

public enum ActionCategories
{
    GATHERING,    // Resource collection
    EXPLORATION,  // Finding new locations/spots
    INTERACTION,  // NPC relationships
    TRAVEL,       // Movement between locations
    MAINTENANCE   // Rest, repair, etc.
}
```

### 2. YieldDefinition

```csharp
public class YieldDefinition
{
    public YieldTypes Type { get; set; }
    public string TargetId { get; set; }  // Resource/Skill/Action/Card/Location ID
    public float BaseAmount { get; set; }
    public float SkillMultiplier { get; set; } = 1.0f;
    public string ScalingSkillId { get; set; }  // Skill that amplifies this yield
    public List<YieldCondition> Conditions { get; set; } = new List<YieldCondition>();
}

public enum YieldTypes
{
    RESOURCE,                // Direct resource gain (food, energy, etc.)
    SKILL_XP,                // Skill experience points
    LOCATION_ACCESS,         // Unlock new locations
    LOCATION_SPOT_ACCESS,    // Unlock new spots within locations
    ACTION_UNLOCK,           // Unlock new actions
    CARD_UNLOCK,             // Unlock new cards for encounters
    EFFICIENCY_BOOST,        // Improve yield rates for specific action types
    TRAVEL_DISCOUNT,         // Reduce travel costs to specific locations
    ENCOUNTER_CHANCE_REDUCTION, // Reduce chance of encounters
    NODE_DISCOVERY,          // Discover aspects of resource nodes
    NODE_REPLENISH           // Restore depleted resource nodes
}
```

### 3. YieldCondition

```csharp
public class YieldCondition
{
    public ConditionTypes Type { get; set; }
    public string TargetId { get; set; }
    public int RequiredValue { get; set; }
    public float Chance { get; set; } = 100.0f;  // Percent chance this yield occurs
}

public enum ConditionTypes
{
    SKILL_LEVEL,        // Requires specific skill level
    FIRST_DISCOVERY,    // Only applies on first discovery
    NODE_STATE,         // Depends on resource node state
    PLAYER_ENERGY,      // Depends on current energy level
    TIME_OF_DAY,        // Depends on game world time
    PROGRESSIVE_ACTION  // Depends on how many times action performed
}
```

### 4. ResourceNodeDefinition

```csharp
public class ResourceNodeDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ResourceNodeTypes Type { get; set; }
    public QualityTiers Quality { get; set; }
    public float MaxDepletion { get; set; } = 1.0f;
    public float ReplenishRate { get; set; } = 0.2f;
    public List<string> ActionIds { get; set; } = new List<string>();
    public List<NodeAspectDefinition> DiscoverableAspects { get; set; } = new List<NodeAspectDefinition>();
}

public enum ResourceNodeTypes
{
    FOOD,
    MEDICINAL,
    WATER,
    LANDMARK,
    OBSTACLE,
    PASSAGE,
    SETTLEMENT,
    TRADING
}

public enum QualityTiers
{
    NOVICE,
    APPRENTICE,
    ADEPT,
    MASTER
}
```

### 5. NodeAspectDefinition

```csharp
public class NodeAspectDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string SkillType { get; set; }
    public float SkillXPGain { get; set; }
    public List<YieldDefinition> Yields { get; set; } = new List<YieldDefinition>();
}
```

### 6. RegionDefinition

```csharp
public class RegionDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public QualityTiers ResourceQualityTier { get; set; }
    public int SkillCeiling { get; set; }
    public List<string> NodeIds { get; set; } = new List<string>();
    public Dictionary<string, int> TravelCosts { get; set; } = new Dictionary<string, int>();
}
```

## Example Content (JSON)

```json
{
  "actions": [
    {
      "id": "gather_berries",
      "name": "Gather Berries",
      "description": "Collect edible berries from the thicket",
      "energyCost": 15,
      "timeCost": 20,
      "encounterChance": 25,
      "encounterType": "PHYSICAL",
      "category": "GATHERING",
      "yields": [
        {
          "type": "RESOURCE",
          "targetId": "food",
          "baseAmount": 2,
          "skillMultiplier": 0.1,
          "scalingSkillId": "foraging"
        },
        {
          "type": "SKILL_XP",
          "targetId": "foraging",
          "baseAmount": 0.2,
          "conditions": [
            {
              "type": "PROGRESSIVE_ACTION",
              "targetId": "gather_berries",
              "requiredValue": 0
            }
          ]
        },
        {
          "type": "NODE_DISCOVERY",
          "targetId": "ripeness_patterns",
          "baseAmount": 1,
          "conditions": [
            {
              "type": "SKILL_LEVEL",
              "targetId": "perception",
              "requiredValue": 1,
              "chance": 70
            }
          ]
        }
      ],
      "skillRequirement": null
    },
    
    {
      "id": "scout_area",
      "name": "Scout Area",
      "description": "Explore surroundings to find new locations",
      "energyCost": 25,
      "timeCost": 45,
      "encounterChance": 40,
      "encounterType": "MIXED",
      "category": "EXPLORATION",
      "yields": [
        {
          "type": "LOCATION_SPOT_ACCESS",
          "targetId": "hidden_grove",
          "baseAmount": 1,
          "skillMultiplier": 0.0,
          "conditions": [
            {
              "type": "FIRST_DISCOVERY",
              "chance": 60
            }
          ]
        },
        {
          "type": "SKILL_XP",
          "targetId": "navigation",
          "baseAmount": 0.7,
          "conditions": [
            {
              "type": "FIRST_DISCOVERY",
              "chance": 100
            }
          ]
        },
        {
          "type": "TRAVEL_DISCOUNT",
          "targetId": "forest_edge",
          "baseAmount": 5,
          "conditions": [
            {
              "type": "SKILL_LEVEL",
              "targetId": "navigation",
              "requiredValue": 2
            }
          ]
        }
      ],
      "skillRequirement": null
    },
    
    {
      "id": "climb_tree",
      "name": "Climb Tree",
      "description": "Ascend the ancient tree to get a better view",
      "energyCost": 25,
      "timeCost": 20,
      "encounterChance": 40,
      "encounterType": "PHYSICAL",
      "category": "EXPLORATION",
      "yields": [
        {
          "type": "EFFICIENCY_BOOST",
          "targetId": "navigation",
          "baseAmount": 0.2,
          "skillMultiplier": 0.0
        },
        {
          "type": "ENCOUNTER_CHANCE_REDUCTION",
          "targetId": "travel",
          "baseAmount": 10,
          "skillMultiplier": 0.0,
          "conditions": [
            {
              "type": "SKILL_LEVEL",
              "targetId": "navigation",
              "requiredValue": 2
            }
          ]
        },
        {
          "type": "CARD_UNLOCK",
          "targetId": "terrain_advantage",
          "baseAmount": 1,
          "skillMultiplier": 0.0,
          "conditions": [
            {
              "type": "SKILL_LEVEL",
              "targetId": "perception",
              "requiredValue": 2
            }
          ]
        }
      ],
      "skillRequirement": null
    },
    
    {
      "id": "find_path_out",
      "name": "Find Path Out",
      "description": "Navigate the final path to exit the forest",
      "energyCost": 30,
      "timeCost": 40,
      "encounterChance": 100,
      "encounterType": "MIXED",
      "category": "TRAVEL",
      "yields": [
        {
          "type": "LOCATION_ACCESS",
          "targetId": "elmridge_valley",
          "baseAmount": 1,
          "skillMultiplier": 0.0
        },
        {
          "type": "SKILL_XP",
          "targetId": "navigation",
          "baseAmount": 1.0,
          "skillMultiplier": 0.0
        },
        {
          "type": "ACTION_UNLOCK",
          "targetId": "efficient_travel",
          "baseAmount": 1,
          "skillMultiplier": 0.0,
          "conditions": [
            {
              "type": "SKILL_LEVEL",
              "targetId": "navigation",
              "requiredValue": 2
            }
          ]
        }
      ],
      "skillRequirement": null
    }
  ],
  
  "resourceNodes": [
    {
      "id": "berry_thicket",
      "name": "Berry Thicket",
      "description": "A dense growth of berry bushes",
      "type": "FOOD",
      "quality": "NOVICE",
      "maxDepletion": 1.0,
      "replenishRate": 0.2,
      "actionIds": ["gather_berries", "identify_berries", "selective_harvesting"],
      "discoverableAspects": [
        {
          "id": "ripeness_patterns",
          "name": "Berry Ripeness Patterns",
          "description": "Understanding when berries are at optimal ripeness",
          "skillType": "FORAGING",
          "skillXPGain": 0.7,
          "yields": [
            {
              "type": "EFFICIENCY_BOOST",
              "targetId": "gather_berries",
              "baseAmount": 0.2
            }
          ]
        },
        {
          "id": "efficient_technique",
          "name": "Efficient Gathering Technique",
          "description": "A method to gather berries with minimal energy expenditure",
          "skillType": "FORAGING",
          "skillXPGain": 0.5,
          "yields": [
            {
              "type": "ACTION_UNLOCK",
              "targetId": "selective_harvesting",
              "baseAmount": 1
            }
          ]
        }
      ]
    },
    
    {
      "id": "ancient_tree",
      "name": "Ancient Tree",
      "description": "A massive tree that towers above the forest",
      "type": "LANDMARK",
      "quality": "NOVICE",
      "maxDepletion": 1.0,
      "replenishRate": 0.0,
      "actionIds": ["climb_tree", "study_surroundings", "mark_trail"],
      "discoverableAspects": [
        {
          "id": "height_advantage",
          "name": "Height Advantage",
          "description": "Using elevation to orient yourself in the forest",
          "skillType": "NAVIGATION",
          "skillXPGain": 0.7,
          "yields": [
            {
              "type": "ENCOUNTER_CHANCE_REDUCTION",
              "targetId": "travel",
              "baseAmount": 10
            }
          ]
        },
        {
          "id": "cardinal_directions",
          "name": "Cardinal Direction Determination",
          "description": "Using moss growth and branch patterns to find north",
          "skillType": "NAVIGATION",
          "skillXPGain": 0.5,
          "yields": [
            {
              "type": "TRAVEL_DISCOUNT",
              "targetId": "global",
              "baseAmount": 5
            }
          ]
        }
      ]
    }
  ],
  
  "regions": [
    {
      "id": "deepwood_forest",
      "name": "Deepwood Forest",
      "resourceQualityTier": "NOVICE",
      "skillCeiling": 3,
      "nodeIds": ["forest_entrance", "berry_thicket", "herb_patch", "stream_crossing", "ancient_tree", "forest_exit_path"],
      "travelCosts": {
        "elmridge_valley": 50
      }
    }
  ]
}
```

## Core Principles for Purely Mechanical Benefits

1. **Every Yield Has Quantifiable Impact**
   - All yields directly modify player stats, resources, or access
   - No yields that only provide information without mechanical benefit

2. **Discovery Always Unlocks Mechanics**
   - Discovering node aspects unlocks efficiency boosts or new actions
   - Exploring new areas unlocks travel discounts or encounter advantages

3. **Relationships Provide Concrete Benefits**
   - NPC relationships unlock new actions or cards
   - Social progress measured by mechanical advantages gained

4. **Progressive Scaling For All Effects**
   - Skills directly improve yield rates through multipliers
   - Higher skill levels unlock greater mechanical benefits

5. **Every System Component Maintains Replayability**
   - Each location spot has multiple discoverable aspects
   - Each aspect provides multiple mechanical benefits
   - Each benefit scales with skill progression

This system creates a comprehensive framework where every player action yields concrete mechanical benefits while maintaining elegant data structures that are easy to maintain and extend.