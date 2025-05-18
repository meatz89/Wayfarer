# Wayfarer Core Implementation Specification

## Commission Actions System

### Action Structure & Types

#### Repeatable Actions (Always Available)
```
Action: "Help at the Inn"
Location: The Dusty Flagon
Approaches:
- Physical Approach: "Manual Labor" (Physical card)
  * Reward: 2 Silver, +1 Reputation
- Intellectual Approach: "Accounting Assistance" (Intellectual card)
  * Reward: 1 Silver, +2 Reputation
- Social Approach: "Serve Customers" (Social card)
  * Reward: 2 Silver, +1 Reputation, +1 Insight Point
```

#### Commission Actions (One-Time)
```
Commission Action: "Repair Damaged Furniture"
Location: The Dusty Flagon
Progress Threshold: 10
Approaches:
- Physical Approach: "Direct Repairs" (Physical card)
  * Reward: 8 Silver, +5 Reputation
- Intellectual Approach: "Analytical Repairs" (Intellectual card)
  * Reward: 6 Silver, +7 Reputation, +1 Insight Point
- Social Approach: "Coordinate Helpers" (Social card)
  * Reward: 5 Silver, +4 Reputation, refreshes 1 Physical card
```

## Procedural Encounter Generation System

### Encounter Stage Generation
Each approach triggers a procedural encounter with 2-3 stages. The system generates:

1. **Stage Theme**: Based on action + approach + stage number
2. **Options**: 2-3 choices per stage, each checking different skills
3. **Progress Values**: Each option modifies progress toward completion

#### Generation Algorithm
```
For each Stage (1 to N):
  Set baseDifficulty = 1 + stage_number
  Set baseProgress = stage_number * 2
  
  Generate Option 1:
    Skill = Primary skill for approach (e.g., Strength for Physical)
    Difficulty = baseDifficulty + locationModifiers
    SuccessProgress = baseProgress + 2
    FailureProgress = 1
    
  Generate Option 2:
    Skill = Secondary skill for approach (e.g., Precision for Physical)
    Difficulty = baseDifficulty + locationModifiers
    SuccessProgress = baseProgress
    FailureProgress = -1
    
  Generate Option 3 (No-Check):
    SuccessProgress = baseProgress - 1
    No failure case
```

### Skill Check System

#### Skill Check Resolution
```
Check Success = (Player Skill Level + Card Bonus + LocationModifier) >= Check Difficulty
```

- **Player Skill Level**: 0-5 numerical value
- **Card Bonus**: +1 to +3 based on card level
- **Location Modifier**: -1, 0, or +1 based on location properties

#### Location Property Effects
```
Location Property: "Crowded"
Effect: -1 to all Intellectual checks

Location Property: "Well-lit"
Effect: +1 to all Precision checks

Location Property: "Formal"
Effect: +1 to all Charm checks, -1 to all Intimidation checks
```

## Example Encounter Generation

### Action: "Repair Damaged Furniture" - Physical Approach

**Stage 1: Initial Assessment**
```
The furniture is in worse shape than expected. How will you begin?

Option 1: Apply Strength (Strength Check, DC 2)
"You could force the broken pieces back together..."
Success: +4 progress
Failure: +1 progress

Option 2: Careful Examination (Precision Check, DC 2)
"You could take time to examine exactly how it broke..."
Success: +3 progress
Failure: -1 progress

Option 3: Basic Approach (No Check)
"You could start with the simplest fixes first..."
Result: +2 progress
```

**Stage 2: Completing Repairs**
```
With the initial work done, you need to finish the job.

Option 1: Reinforce Structure (Strength Check, DC 3)
"You could add extra bracing to make it stronger..."
Success: +6 progress
Failure: +1 progress

Option 2: Fine Detailing (Precision Check, DC 3)
"You could carefully align and secure each joint..."
Success: +5 progress
Failure: -1 progress

Option 3: Standard Completion (No Check)
"You could just finish it in a workmanlike fashion..."
Result: +3 progress
```

## Reward Structure

### Intermediate Resources
```
Basic Resources:
- Silver (currency)
- AP (time blocks)
- Cards (action capabilities)

Intermediate Resources:
- Reputation (with different factions)
- Insight Points (for chronicle advancement)
- Commission Points (progress toward completion)
- Favor (with specific NPCs, unlocks special actions)

Goal Resources:
- Position Offer (requires high Reputation + completed commission chain)
- Housing Contract (requires Silver + Reputation threshold)
- Special Items (requires combination of Favor + Silver)
```

## Implementation Priorities

1. **Commission Actions**: Fixed approaches with procedural encounters
2. **Encounter Generation**: Deterministic but varied based on context
3. **Progress System**: Clear threshold with multiple ways to succeed
4. **Reward Structure**: Tangible immediate and intermediate resources

This system creates meaningful decisions at multiple levels:
- Which commissions to accept based on approaches available
- Which approach to choose based on your cards and skills
- Which option to select within encounters based on risk/reward
- How to use intermediate resources toward final goals

All this while maintaining the deterministic, procedurally-generated nature of encounters that reflect the player's choices and circumstances.

# Wayfarer POC Implementation - JSON Content

Here's the complete minimal POC implementation in JSON format:

## Locations and Spots

```json
{
  "locations": [
    {
      "id": "dusty_flagon",
      "name": "The Dusty Flagon",
      "spots": [
        {
          "id": "common_room",
          "name": "Common Room",
          "properties": ["busy", "social"],
          "npcIds": ["bertram", "henrik"]
        },
        {
          "id": "guest_room",
          "name": "Guest Room",
          "properties": ["quiet", "private"],
          "npcIds": []
        },
        {
          "id": "kitchen",
          "name": "Kitchen",
          "properties": ["warm", "busy"],
          "npcIds": ["eliza"]
        }
      ]
    },
    {
      "id": "town_square",
      "name": "Town Square",
      "spots": [
        {
          "id": "notice_board",
          "name": "Notice Board",
          "properties": ["public", "busy"],
          "npcIds": []
        },
        {
          "id": "guard_post",
          "name": "Guard Post",
          "properties": ["official", "structured"],
          "npcIds": ["sergeant_mills"]
        }
      ]
    },
    {
      "id": "craftsmens_quarter",
      "name": "Craftsmen's Quarter",
      "spots": [
        {
          "id": "blacksmith",
          "name": "Blacksmith's Forge",
          "properties": ["hot", "noisy"],
          "npcIds": ["gareth"]
        },
        {
          "id": "carpenter",
          "name": "Carpenter's Workshop",
          "properties": ["dusty", "organized"],
          "npcIds": ["jonas"]
        }
      ]
    }
  ]
}
```

## NPCs

```json
{
  "npcs": [
    {
      "id": "bertram",
      "name": "Bertram",
      "role": "Innkeeper",
      "initialRelationship": 0,
      "commissionIds": ["repair_furniture", "organize_records"]
    },
    {
      "id": "eliza",
      "name": "Eliza",
      "role": "Cook",
      "initialRelationship": 0,
      "commissionIds": ["kitchen_assistance"]
    },
    {
      "id": "henrik",
      "name": "Henrik",
      "role": "Regular Patron",
      "initialRelationship": 0,
      "commissionIds": ["deliver_message"]
    },
    {
      "id": "sergeant_mills",
      "name": "Sergeant Mills",
      "role": "Guard Captain",
      "initialRelationship": 0,
      "commissionIds": ["guard_training"]
    },
    {
      "id": "gareth",
      "name": "Gareth",
      "role": "Blacksmith",
      "initialRelationship": 0,
      "commissionIds": ["forge_tools"]
    },
    {
      "id": "jonas",
      "name": "Jonas",
      "role": "Carpenter",
      "initialRelationship": 0,
      "commissionIds": ["furniture_delivery"]
    }
  ]
}
```

## Commission Actions

```json
{
  "commissionActions": [
    {
      "id": "repair_furniture",
      "name": "Repair Damaged Furniture",
      "description": "Fix several broken chairs and tables for the inn.",
      "locationId": "dusty_flagon",
      "spotId": "common_room",
      "requiredReputation": 0,
      "expiresInDays": 2,
      "progressThreshold": 10,
      "approaches": [
        {
          "type": "physical",
          "name": "Direct Repairs",
          "description": "Use your strength and handiness to fix the furniture.",
          "primarySkill": "strength",
          "secondarySkill": "precision",
          "reward": {
            "silver": 8,
            "reputation": 5,
            "insightPoints": 0
          }
        },
        {
          "type": "intellectual",
          "name": "Analytical Approach",
          "description": "Analyze the furniture design to make optimal repairs.",
          "primarySkill": "analysis",
          "secondarySkill": "knowledge",
          "reward": {
            "silver": 6,
            "reputation": 7,
            "insightPoints": 1
          }
        },
        {
          "type": "social",
          "name": "Coordinate Helpers",
          "description": "Direct others to help with the repairs.",
          "primarySkill": "persuasion",
          "secondarySkill": "charm",
          "reward": {
            "silver": 5,
            "reputation": 8,
            "insightPoints": 0,
            "refreshCards": ["physical"]
          }
        }
      ]
    },
    {
      "id": "kitchen_assistance",
      "name": "Kitchen Assistance",
      "description": "Help prepare meals during a busy evening.",
      "locationId": "dusty_flagon",
      "spotId": "kitchen",
      "requiredReputation": 0,
      "expiresInDays": 1,
      "progressThreshold": 8,
      "approaches": [
        {
          "type": "physical",
          "name": "Cook and Prep",
          "description": "Handle the physical cooking tasks.",
          "primarySkill": "endurance",
          "secondarySkill": "precision",
          "reward": {
            "silver": 7,
            "reputation": 4,
            "insightPoints": 0
          }
        },
        {
          "type": "intellectual",
          "name": "Recipe Management",
          "description": "Organize ingredients and optimize cooking methods.",
          "primarySkill": "planning",
          "secondarySkill": "observation",
          "reward": {
            "silver": 5,
            "reputation": 5,
            "insightPoints": 1
          }
        },
        {
          "type": "social",
          "name": "Serving and Presenting",
          "description": "Focus on serving and interacting with patrons.",
          "primarySkill": "charm",
          "secondarySkill": "observation",
          "reward": {
            "silver": 4,
            "reputation": 7,
            "insightPoints": 0,
            "refreshCards": ["social"]
          }
        }
      ]
    },
    {
      "id": "guard_training",
      "name": "Guard Training Session",
      "description": "Participate in town guard training exercises.",
      "locationId": "town_square",
      "spotId": "guard_post",
      "requiredReputation": 15,
      "expiresInDays": 3,
      "progressThreshold": 12,
      "approaches": [
        {
          "type": "physical",
          "name": "Combat Demonstration",
          "description": "Show your physical combat capabilities.",
          "primarySkill": "strength",
          "secondarySkill": "endurance",
          "reward": {
            "silver": 12,
            "reputation": 10,
            "insightPoints": 0
          }
        },
        {
          "type": "intellectual",
          "name": "Tactical Analysis",
          "description": "Analyze training methods and suggest improvements.",
          "primarySkill": "analysis",
          "secondarySkill": "planning",
          "reward": {
            "silver": 8,
            "reputation": 12,
            "insightPoints": 2
          }
        },
        {
          "type": "social",
          "name": "Motivational Training",
          "description": "Focus on morale and unit coordination.",
          "primarySkill": "persuasion",
          "secondarySkill": "intimidation",
          "reward": {
            "silver": 6,
            "reputation": 15,
            "insightPoints": 1
          }
        }
      ]
    },
    {
      "id": "forge_tools",
      "name": "Forge Specialized Tools",
      "description": "Create a set of tools for the blacksmith.",
      "locationId": "craftsmens_quarter",
      "spotId": "blacksmith",
      "requiredReputation": 25,
      "expiresInDays": 4,
      "progressThreshold": 15,
      "approaches": [
        {
          "type": "physical",
          "name": "Direct Forging",
          "description": "Put your strength behind the hammer.",
          "primarySkill": "strength",
          "secondarySkill": "endurance",
          "reward": {
            "silver": 15,
            "reputation": 12,
            "insightPoints": 1
          }
        },
        {
          "type": "intellectual",
          "name": "Metallurgical Innovation",
          "description": "Apply scientific principles to improve the forging process.",
          "primarySkill": "knowledge",
          "secondarySkill": "analysis",
          "reward": {
            "silver": 10,
            "reputation": 15,
            "insightPoints": 3
          }
        },
        {
          "type": "social",
          "name": "Collaborative Crafting",
          "description": "Direct the blacksmith's apprentices in coordinated work.",
          "primarySkill": "persuasion",
          "secondarySkill": "charm",
          "reward": {
            "silver": 8,
            "reputation": 18,
            "insightPoints": 1,
            "refreshCards": ["intellectual", "physical"]
          }
        }
      ]
    }
  ]
}
```

## Repeatable Actions

```json
{
  "repeatableActions": [
    {
      "id": "help_inn",
      "name": "Help at the Inn",
      "description": "Assist with various tasks around the inn.",
      "locationId": "dusty_flagon",
      "spotId": "common_room",
      "approaches": [
        {
          "type": "physical",
          "name": "Manual Labor",
          "description": "Help with cleaning, moving supplies, and maintenance.",
          "primarySkill": "strength",
          "secondarySkill": "endurance",
          "reward": {
            "silver": 3,
            "reputation": 1
          }
        },
        {
          "type": "intellectual",
          "name": "Accounting Assistance",
          "description": "Help Bertram with his books and records.",
          "primarySkill": "analysis",
          "secondarySkill": "knowledge",
          "reward": {
            "silver": 2,
            "reputation": 1,
            "insightPoints": 1
          }
        },
        {
          "type": "social",
          "name": "Serve Customers",
          "description": "Help with customer service and satisfaction.",
          "primarySkill": "charm",
          "secondarySkill": "persuasion",
          "reward": {
            "silver": 2,
            "reputation": 2
          }
        }
      ]
    },
    {
      "id": "assistant_blacksmith",
      "name": "Assist Blacksmith",
      "description": "Help around the forge.",
      "locationId": "craftsmens_quarter",
      "spotId": "blacksmith",
      "approaches": [
        {
          "type": "physical",
          "name": "Work the Bellows",
          "description": "Provide the manual labor needed at the forge.",
          "primarySkill": "endurance",
          "secondarySkill": "strength",
          "reward": {
            "silver": 4,
            "reputation": 1,
            "refreshCards": ["physical"]
          }
        },
        {
          "type": "intellectual",
          "name": "Inspect Materials",
          "description": "Sort and examine metals and materials.",
          "primarySkill": "observation",
          "secondarySkill": "knowledge",
          "reward": {
            "silver": 2,
            "reputation": 1,
            "insightPoints": 1
          }
        },
        {
          "type": "social",
          "name": "Handle Customers",
          "description": "Take orders and manage customer expectations.",
          "primarySkill": "persuasion",
          "secondarySkill": "charm",
          "reward": {
            "silver": 3,
            "reputation": 2
          }
        }
      ]
    },
    {
      "id": "town_patrol",
      "name": "Join Town Patrol",
      "description": "Participate in routine town patrol.",
      "locationId": "town_square",
      "spotId": "guard_post",
      "approaches": [
        {
          "type": "physical",
          "name": "Active Patrol",
          "description": "Focus on active security and intervention.",
          "primarySkill": "strength",
          "secondarySkill": "endurance",
          "reward": {
            "silver": 3,
            "reputation": 2
          }
        },
        {
          "type": "intellectual",
          "name": "Observant Watch",
          "description": "Keep a watchful eye for suspicious activity.",
          "primarySkill": "observation",
          "secondarySkill": "analysis",
          "reward": {
            "silver": 2,
            "reputation": 2,
            "insightPoints": 1
          }
        },
        {
          "type": "social",
          "name": "Community Policing",
          "description": "Focus on citizen interactions and addressing concerns.",
          "primarySkill": "charm",
          "secondarySkill": "persuasion",
          "reward": {
            "silver": 2,
            "reputation": 3
          }
        }
      ]
    }
  ]
}
```

## Cards

```json
{
  "cardTypes": [
    {
      "id": "strength",
      "name": "Strength",
      "type": "physical",
      "skill": "strength",
      "energyCost": 1,
      "levels": [
        {
          "level": 1,
          "bonus": 1
        },
        {
          "level": 2,
          "bonus": 2
        },
        {
          "level": 3,
          "bonus": 3
        }
      ]
    },
    {
      "id": "endurance",
      "name": "Endurance",
      "type": "physical",
      "skill": "endurance",
      "energyCost": 1,
      "levels": [
        {
          "level": 1,
          "bonus": 1
        },
        {
          "level": 2,
          "bonus": 2
        },
        {
          "level": 3,
          "bonus": 3
        }
      ]
    },
    {
      "id": "precision",
      "name": "Precision",
      "type": "physical",
      "skill": "precision",
      "energyCost": 1,
      "levels": [
        {
          "level": 1,
          "bonus": 1
        },
        {
          "level": 2,
          "bonus": 2
        },
        {
          "level": 3,
          "bonus": 3
        }
      ]
    },
    {
      "id": "agility",
      "name": "Agility",
      "type": "physical",
      "skill": "agility",
      "energyCost": 1,
      "levels": [
        {
          "level": 1,
          "bonus": 1
        },
        {
          "level": 2,
          "bonus": 2
        },
        {
          "level": 3,
          "bonus": 3
        }
      ]
    },
    {
      "id": "analysis",
      "name": "Analysis",
      "type": "intellectual",
      "skill": "analysis",
      "concentrationCost": 1,
      "levels": [
        {
          "level": 1,
          "bonus": 1
        },
        {
          "level": 2,
          "bonus": 2
        },
        {
          "level": 3,
          "bonus": 3
        }
      ]
    },
    {
      "id": "observation",
      "name": "Observation",
      "type": "intellectual",
      "skill": "observation",
      "concentrationCost": 1,
      "levels": [
        {
          "level": 1,
          "bonus": 1
        },
        {
          "level": 2,
          "bonus": 2
        },
        {
          "level": 3,
          "bonus": 3
        }
      ]
    },
    {
      "id": "knowledge",
      "name": "Knowledge",
      "type": "intellectual",
      "skill": "knowledge",
      "concentrationCost": 1,
      "levels": [
        {
          "level": 1,
          "bonus": 1
        },
        {
          "level": 2,
          "bonus": 2
        },
        {
          "level": 3,
          "bonus": 3
        }
      ]
    },
    {
      "id": "planning",
      "name": "Planning",
      "type": "intellectual",
      "skill": "planning",
      "concentrationCost": 1,
      "levels": [
        {
          "level": 1,
          "bonus": 1
        },
        {
          "level": 2,
          "bonus": 2
        },
        {
          "level": 3,
          "bonus": 3
        }
      ]
    },
    {
      "id": "charm",
      "name": "Charm",
      "type": "social",
      "skill": "charm",
      "concentrationCost": 0,
      "levels": [
        {
          "level": 1,
          "bonus": 1
        },
        {
          "level": 2,
          "bonus": 2
        },
        {
          "level": 3,
          "bonus": 3
        }
      ]
    },
    {
      "id": "persuasion",
      "name": "Persuasion",
      "type": "social",
      "skill": "persuasion",
      "concentrationCost": 0,
      "levels": [
        {
          "level": 1,
          "bonus": 1
        },
        {
          "level": 2,
          "bonus": 2
        },
        {
          "level": 3,
          "bonus": 3
        }
      ]
    },
    {
      "id": "deception",
      "name": "Deception",
      "type": "social",
      "skill": "deception",
      "concentrationCost": 0,
      "levels": [
        {
          "level": 1,
          "bonus": 1
        },
        {
          "level": 2,
          "bonus": 2
        },
        {
          "level": 3,
          "bonus": 3
        }
      ]
    },
    {
      "id": "intimidation",
      "name": "Intimidation",
      "type": "social",
      "skill": "intimidation",
      "concentrationCost": 0,
      "levels": [
        {
          "level": 1,
          "bonus": 1
        },
        {
          "level": 2,
          "bonus": 2
        },
        {
          "level": 3,
          "bonus": 3
        }
      ]
    }
  ]
}
```

## Location Properties

```json
{
  "locationProperties": [
    {
      "id": "busy",
      "skillModifiers": {
        "analysis": -1,
        "observation": -1
      },
      "rewardModifiers": {
        "silver": 2,
        "reputation": 0,
        "insightPoints": 0
      }
    },
    {
      "id": "quiet",
      "skillModifiers": {
        "analysis": 1,
        "knowledge": 1,
        "charm": -1
      },
      "rewardModifiers": {
        "silver": 0,
        "reputation": 0,
        "insightPoints": 1
      }
    },
    {
      "id": "official",
      "skillModifiers": {
        "persuasion": 1,
        "deception": -1
      },
      "rewardModifiers": {
        "silver": 0,
        "reputation": 2,
        "insightPoints": 0
      }
    },
    {
      "id": "social",
      "skillModifiers": {
        "charm": 1,
        "persuasion": 1,
        "analysis": -1
      },
      "rewardModifiers": {
        "silver": 0,
        "reputation": 1,
        "insightPoints": 0
      }
    },
    {
      "id": "noisy",
      "skillModifiers": {
        "observation": -1,
        "intimidation": 1
      },
      "rewardModifiers": {
        "silver": 0,
        "reputation": 0,
        "insightPoints": -1
      }
    },
    {
      "id": "hot",
      "skillModifiers": {
        "endurance": -1,
        "strength": 1
      },
      "rewardModifiers": {
        "silver": 1,
        "reputation": 0,
        "insightPoints": 0
      }
    },
    {
      "id": "organized",
      "skillModifiers": {
        "precision": 1,
        "planning": 1
      },
      "rewardModifiers": {
        "silver": 0,
        "reputation": 0,
        "insightPoints": 1
      }
    },
    {
      "id": "public",
      "skillModifiers": {
        "deception": -1,
        "persuasion": 1
      },
      "rewardModifiers": {
        "silver": 0,
        "reputation": 1,
        "insightPoints": 0
      }
    },
    {
      "id": "private",
      "skillModifiers": {
        "deception": 1,
        "observation": 1
      },
      "rewardModifiers": {
        "silver": 0,
        "reputation": 0,
        "insightPoints": 0
      }
    },
    {
      "id": "warm",
      "skillModifiers": {
        "endurance": 1
      },
      "rewardModifiers": {
        "silver": 0,
        "reputation": 0,
        "insightPoints": 0
      }
    }
  ]
}
```

## Procedural Encounter Generation Rules

```json
{
  "encounterGenerationRules": {
    "stageCount": {
      "min": 2,
      "max": 3
    },
    "baseProgressValues": {
      "primarySuccess": 4,
      "primaryFailure": 1,
      "secondarySuccess": 3,
      "secondaryFailure": -1,
      "safeOption": 2
    },
    "difficultyProgression": {
      "primarySkillStartingDC": 2,
      "secondarySkillStartingDC": 1,
      "dcIncreasePerStage": 1
    },
    "progressThresholds": {
      "tier1": 10,
      "tier2": 12,
      "tier3": 15
    }
  }
}
```

## Starting Conditions

```json
{
  "startingConditions": {
    "warrior": {
      "skills": {
        "strength": 2,
        "endurance": 2,
        "precision": 1,
        "agility": 1,
        "analysis": 0,
        "observation": 1,
        "knowledge": 0,
        "planning": 0,
        "charm": 0,
        "persuasion": 1,
        "deception": 0,
        "intimidation": 1
      },
      "resources": {
        "silver": 20,
        "reputation": 0,
        "insightPoints": 0,
        "energy": 8,
        "concentration": 6
      },
      "startingCards": [
        "strength",
        "strength", 
        "endurance",
        "observation",
        "persuasion"
      ]
    },
    "scholar": {
      "skills": {
        "strength": 0,
        "endurance": 1,
        "precision": 1,
        "agility": 0,
        "analysis": 2,
        "observation": 1,
        "knowledge": 2,
        "planning": 1,
        "charm": 1,
        "persuasion": 0,
        "deception": 0,
        "intimidation": 0
      },
      "resources": {
        "silver": 15,
        "reputation": 0,
        "insightPoints": 0,
        "energy": 6,
        "concentration": 9
      },
      "startingCards": [
        "endurance",
        "analysis",
        "analysis",
        "knowledge",
        "charm"
      ]
    },
    "courtier": {
      "skills": {
        "strength": 0,
        "endurance": 0,
        "precision": 1,
        "agility": 1,
        "analysis": 0,
        "observation": 1,
        "knowledge": 1,
        "planning": 1,
        "charm": 2,
        "persuasion": 2,
        "deception": 1,
        "intimidation": 0
      },
      "resources": {
        "silver": 25,
        "reputation": 0,
        "insightPoints": 0,
        "energy": 7,
        "concentration": 7
      },
      "startingCards": [
        "precision",
        "observation",
        "charm",
        "charm",
        "persuasion"
      ]
    }
  }
}
```

## Success/Failure Conditions

```json
{
  "gameConditions": {
    "successConditions": {
      "minReputation": 40,
      "lodgingOptions": ["private_room", "rented_cottage"],
      "positionRequirement": true,
      "timeLimit": 14
    },
    "failureConditions": {
      "reputationBelowZero": true,
      "noLodging": true,
      "timeExpired": true
    },
    "cardRefreshCosts": {
      "physicalCard": 3,
      "intellectualCard": 3,
      "socialCard": 2
    },
    "lodgingCosts": {
      "floorSpace": {
        "silver": 2,
        "cardsRefreshed": 1
      },
      "sharedRoom": {
        "silver": 5,
        "cardsRefreshed": 2
      },
      "privateRoom": {
        "silver": 8,
        "cardsRefreshed": "all"
      },
      "rentedCottage": {
        "initialCost": 100,
        "dailyCost": 2,
        "cardsRefreshed": "all",
        "countsForSuccess": true
      }
    }
  }
}
```

This minimal POC implementation provides everything needed to test the core mechanics of Wayfarer:

1. Three main locations with multiple spots
2. Six NPCs offering commissions
3. Four commission actions with multiple approaches
4. Three repeatable actions for consistent resources
5. Complete card definitions for all skills
6. Location properties that modify skills and rewards
7. Clear encounter generation rules
8. Starting conditions for three profession types
9. Success and failure conditions with appropriate thresholds

The structure is tight, focused, and follows the resource economy principles we've discussed, creating meaningful strategic decisions while maintaining a streamlined implementation.