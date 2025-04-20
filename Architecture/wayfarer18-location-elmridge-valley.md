# Extended Wayfarer World: Elmridge Valley

Below are concrete examples of locations, nodes, and actions for Elmridge Valley, which is immediately adjacent to the tutorial forest. This creates a small, interconnected world where all destinations are within walkable distance.

## Region Definition

```json
{
  "regions": [
    {
      "id": "elmridge_valley",
      "name": "Elmridge Valley",
      "resourceQualityTier": "APPRENTICE",
      "skillCeiling": 5,
      "nodeIds": ["crossroads", "elmridge_village", "farmlands", "river_crossing", "hillside_overlook", "abandoned_mine"],
      "travelCosts": {
        "deepwood_forest": 30,
        "northern_hills": 60
      }
    }
  ]
}
```

## Location Nodes

```json
{
  "resourceNodes": [
    {
      "id": "crossroads",
      "name": "Crossroads",
      "description": "A well-worn intersection where several paths meet",
      "type": "LANDMARK",
      "quality": "APPRENTICE",
      "maxDepletion": 1.0,
      "replenishRate": 0.0,
      "actionIds": ["read_signpost", "observe_travelers", "take_shortcut"],
      "discoverableAspects": [
        {
          "id": "worn_tracks",
          "name": "Worn Cart Tracks",
          "description": "Deep wheel ruts showing frequent travel to the village",
          "skillType": "NAVIGATION",
          "skillXPGain": 0.8,
          "yields": [
            {
              "type": "TRAVEL_DISCOUNT",
              "targetId": "elmridge_village",
              "baseAmount": 5
            }
          ]
        },
        {
          "id": "hidden_footpath",
          "name": "Hidden Footpath",
          "description": "A narrow trail leading to the river",
          "skillType": "NAVIGATION",
          "skillXPGain": 0.8,
          "yields": [
            {
              "type": "ACTION_UNLOCK",
              "targetId": "take_shortcut",
              "baseAmount": 1
            }
          ]
        }
      ]
    },

    {
      "id": "elmridge_village",
      "name": "Elmridge Village",
      "description": "A small settlement of wooden homes with thatched roofs",
      "type": "SETTLEMENT",
      "quality": "APPRENTICE",
      "maxDepletion": 1.0,
      "replenishRate": 0.0,
      "actionIds": ["visit_market", "speak_with_elder", "sleep_at_inn", "meet_locals"],
      "discoverableAspects": [
        {
          "id": "market_day",
          "name": "Market Day Schedule",
          "description": "The village market is busiest on the third day of each week",
          "skillType": "SOCIAL",
          "skillXPGain": 0.8,
          "yields": [
            {
              "type": "EFFICIENCY_BOOST",
              "targetId": "trading",
              "baseAmount": 0.15
            }
          ]
        },
        {
          "id": "crafting_knowledge",
          "name": "Village Crafting Knowledge",
          "description": "Techniques for creating practical items",
          "skillType": "CRAFTING",
          "skillXPGain": 0.8,
          "yields": [
            {
              "type": "ACTION_UNLOCK",
              "targetId": "craft_tools",
              "baseAmount": 1
            }
          ]
        }
      ]
    },

    {
      "id": "farmlands",
      "name": "Farmlands",
      "description": "Cultivated fields stretching along the valley floor",
      "type": "FOOD",
      "quality": "APPRENTICE",
      "maxDepletion": 1.0,
      "replenishRate": 0.3,
      "actionIds": ["harvest_crops", "help_farmers", "check_soil"],
      "discoverableAspects": [
        {
          "id": "crop_rotation",
          "name": "Crop Rotation Areas",
          "description": "Sections of field left fallow to restore soil",
          "skillType": "FORAGING",
          "skillXPGain": 0.8,
          "yields": [
            {
              "type": "EFFICIENCY_BOOST",
              "targetId": "harvest_crops",
              "baseAmount": 0.2
            }
          ]
        },
        {
          "id": "boundary_stones",
          "name": "Boundary Stones",
          "description": "Ancient markers denoting property lines",
          "skillType": "NAVIGATION",
          "skillXPGain": 0.6,
          "yields": [
            {
              "type": "LOCATION_SPOT_ACCESS",
              "targetId": "old_cellar",
              "baseAmount": 1
            }
          ]
        }
      ]
    },

    {
      "id": "river_crossing",
      "name": "River Crossing",
      "description": "A wooden bridge spanning the clear waters of Elm River",
      "type": "WATER",
      "quality": "APPRENTICE",
      "maxDepletion": 1.0,
      "replenishRate": 0.4,
      "actionIds": ["fish_river", "draw_water", "cross_bridge", "search_riverbank"],
      "discoverableAspects": [
        {
          "id": "deep_pools",
          "name": "Deep Fishing Pools",
          "description": "Deeper sections of river where larger fish gather",
          "skillType": "FORAGING",
          "skillXPGain": 0.8,
          "yields": [
            {
              "type": "EFFICIENCY_BOOST",
              "targetId": "fish_river",
              "baseAmount": 0.25
            }
          ]
        },
        {
          "id": "bridge_supports",
          "name": "Bridge Supports",
          "description": "Stone foundations supporting the wooden bridge",
          "skillType": "PERCEPTION",
          "skillXPGain": 0.6,
          "yields": [
            {
              "type": "ENCOUNTER_CHANCE_REDUCTION",
              "targetId": "cross_bridge",
              "baseAmount": 15
            }
          ]
        }
      ]
    }
  ]
}
```

## Actions

```json
{
  "actions": [
    {
      "id": "visit_market",
      "name": "Visit Market",
      "description": "Browse the local goods at the village market",
      "energyCost": 10,
      "timeCost": 20,
      "encounterChance": 30,
      "encounterType": "SOCIAL",
      "category": "INTERACTION",
      "yields": [
        {
          "type": "ACTION_UNLOCK",
          "targetId": "trade_goods",
          "baseAmount": 1,
          "skillMultiplier": 0.0
        },
        {
          "type": "SKILL_XP",
          "targetId": "social",
          "baseAmount": 0.4,
          "conditions": [
            {
              "type": "PROGRESSIVE_ACTION",
              "targetId": "visit_market",
              "requiredValue": 0
            }
          ]
        }
      ],
      "skillRequirement": null
    },
    
    {
      "id": "sleep_at_inn",
      "name": "Sleep at Inn",
      "description": "Pay for a room at the Elm & Barrel Inn",
      "energyCost": 5,
      "timeCost": 480,
      "encounterChance": 5,
      "encounterType": "SOCIAL",
      "category": "MAINTENANCE",
      "yields": [
        {
          "type": "RESOURCE",
          "targetId": "energy",
          "baseAmount": 80,
          "skillMultiplier": 0.0
        },
        {
          "type": "RESOURCE",
          "targetId": "health",
          "baseAmount": 20,
          "skillMultiplier": 0.0
        },
        {
          "type": "NODE_REPLENISH",
          "targetId": "global",
          "baseAmount": 0.3,
          "skillMultiplier": 0.0
        }
      ],
      "skillRequirement": null
    },
    
    {
      "id": "harvest_crops",
      "name": "Harvest Crops",
      "description": "Help gather ripe crops from the fields",
      "energyCost": 25,
      "timeCost": 45,
      "encounterChance": 20,
      "encounterType": "PHYSICAL",
      "category": "GATHERING",
      "yields": [
        {
          "type": "RESOURCE",
          "targetId": "food",
          "baseAmount": 3,
          "skillMultiplier": 0.2,
          "scalingSkillId": "foraging"
        },
        {
          "type": "SKILL_XP",
          "targetId": "foraging",
          "baseAmount": 0.5,
          "conditions": [
            {
              "type": "PROGRESSIVE_ACTION",
              "targetId": "harvest_crops",
              "requiredValue": 0
            }
          ]
        },
        {
          "type": "CARD_UNLOCK",
          "targetId": "harvester_strength",
          "baseAmount": 1,
          "skillMultiplier": 0.0,
          "conditions": [
            {
              "type": "SKILL_LEVEL",
              "targetId": "foraging",
              "requiredValue": 3
            }
          ]
        }
      ],
      "skillRequirement": null
    },
    
    {
      "id": "fish_river",
      "name": "Fish River",
      "description": "Cast a line into the clear waters of Elm River",
      "energyCost": 20,
      "timeCost": 60,
      "encounterChance": 35,
      "encounterType": "PHYSICAL",
      "category": "GATHERING",
      "yields": [
        {
          "type": "RESOURCE",
          "targetId": "food",
          "baseAmount": 4,
          "skillMultiplier": 0.15,
          "scalingSkillId": "foraging"
        },
        {
          "type": "SKILL_XP",
          "targetId": "foraging",
          "baseAmount": 0.6,
          "conditions": [
            {
              "type": "PROGRESSIVE_ACTION",
              "targetId": "fish_river",
              "requiredValue": 0
            }
          ]
        },
        {
          "type": "CARD_UNLOCK",
          "targetId": "patient_angler",
          "baseAmount": 1,
          "skillMultiplier": 0.0,
          "conditions": [
            {
              "type": "SKILL_LEVEL",
              "targetId": "foraging",
              "requiredValue": 4
            }
          ]
        }
      ],
      "skillRequirement": null
    },
    
    {
      "id": "take_shortcut",
      "name": "Take Shortcut",
      "description": "Use a hidden path to travel between locations more quickly",
      "energyCost": 15,
      "timeCost": 10,
      "encounterChance": 40,
      "encounterType": "PHYSICAL",
      "category": "TRAVEL",
      "yields": [
        {
          "type": "LOCATION_ACCESS",
          "targetId": "river_crossing",
          "baseAmount": 1,
          "skillMultiplier": 0.0
        },
        {
          "type": "TRAVEL_DISCOUNT",
          "targetId": "global",
          "baseAmount": 10,
          "skillMultiplier": 0.05,
          "scalingSkillId": "navigation"
        },
        {
          "type": "SKILL_XP",
          "targetId": "navigation",
          "baseAmount": 0.5,
          "conditions": [
            {
              "type": "PROGRESSIVE_ACTION",
              "targetId": "take_shortcut",
              "requiredValue": 0
            }
          ]
        }
      ],
      "skillRequirement": {
        "skill": "NAVIGATION",
        "level": 2
      }
    },
    
    {
      "id": "search_riverbank",
      "name": "Search Riverbank",
      "description": "Carefully examine the shoreline for useful items",
      "energyCost": 15,
      "timeCost": 25,
      "encounterChance": 30,
      "encounterType": "PHYSICAL",
      "category": "EXPLORATION",
      "yields": [
        {
          "type": "RESOURCE",
          "targetId": "crafting_materials",
          "baseAmount": 2,
          "skillMultiplier": 0.1,
          "scalingSkillId": "perception"
        },
        {
          "type": "LOCATION_SPOT_ACCESS",
          "targetId": "hidden_cove",
          "baseAmount": 1,
          "skillMultiplier": 0.0,
          "conditions": [
            {
              "type": "SKILL_LEVEL",
              "targetId": "perception",
              "requiredValue": 3,
              "chance": 70
            }
          ]
        },
        {
          "type": "SKILL_XP",
          "targetId": "perception",
          "baseAmount": 0.5,
          "conditions": [
            {
              "type": "PROGRESSIVE_ACTION",
              "targetId": "search_riverbank",
              "requiredValue": 0
            }
          ]
        }
      ],
      "skillRequirement": null
    }
  ]
}
```

## Location Relationships

To emphasize the small, interconnected world with walkable distances:

```json
{
  "locationConnections": [
    {
      "sourceId": "forest_exit_path",
      "targetId": "crossroads",
      "walkingTime": 8,
      "description": "A short path through thinning trees leading to open ground",
      "visibility": "VISIBLE_FROM_SOURCE"
    },
    {
      "sourceId": "crossroads",
      "targetId": "elmridge_village",
      "walkingTime": 15,
      "description": "A well-worn dirt road leading to the village",
      "visibility": "VISIBLE_FROM_SOURCE"
    },
    {
      "sourceId": "crossroads",
      "targetId": "farmlands",
      "walkingTime": 10,
      "description": "A path along field edges toward cultivated land",
      "visibility": "VISIBLE_FROM_SOURCE"
    },
    {
      "sourceId": "crossroads",
      "targetId": "river_crossing",
      "walkingTime": 12,
      "description": "A forked path leading toward the sound of flowing water",
      "visibility": "AUDIBLE_FROM_SOURCE"
    },
    {
      "sourceId": "elmridge_village",
      "targetId": "farmlands",
      "walkingTime": 6,
      "description": "A short walk past the village's last houses",
      "visibility": "VISIBLE_FROM_SOURCE"
    },
    {
      "sourceId": "elmridge_village",
      "targetId": "river_crossing",
      "walkingTime": 7,
      "description": "A well-used path leading to the water's edge",
      "visibility": "VISIBLE_FROM_BOTH"
    }
  ]
}
```

## Concrete Mechanical Benefits

Every action and discovery provides concrete mechanical benefits:

1. **Resource Acquisition**
   - Food from harvesting crops and fishing
   - Crafting materials from riverbank searches
   - Energy and health restoration from inn services

2. **Efficiency Improvements**
   - Better fishing yields from discovering deep pools
   - Improved harvesting through crop rotation knowledge
   - Trading benefits from market day schedule knowledge

3. **Access Unlocks**
   - New locations discovered through exploration
   - Hidden spots like old cellars and hidden coves
   - Shortcuts that reduce travel costs

4. **Encounter Advantages**
   - Bridge inspection reduces crossing encounter chances
   - Navigation improvements reduce travel encounters
   - Skill-specific cards for combat advantage

5. **Travel Optimization**
   - Shortcuts reduce time between locations
   - Path knowledge reduces energy costs
   - Skill improvements make travel more efficient

This system creates a small, interconnected world where all discoveries yield tangible mechanical benefits rather than abstract knowledge, keeping the game focused on concrete progression elements.