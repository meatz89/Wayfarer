# Refined Commission System: Multi-Approach Progress Model

After thorough analysis of your feedback, I've redesigned the commission system to create meaningful progression paths with variable completion requirements. This system creates strategic depth while maintaining elegant simplicity.

## Two Commission Types

### 1. Accumulative Commissions (Repeatable Approaches)
These commissions allow players to repeat the same approaches multiple times until reaching the progress threshold:

```
Commission: "Assist at the Inn"
Location: Dusty Flagon
Progress Threshold: 8
Expires: 3 days

Approaches (repeatable until completion):
- Physical: "Handle Maintenance" (Physical card)
  • Generates encounter stages focused on Strength/Endurance
  • Each successful encounter: 2-6 progress points

- Intellectual: "Organize Inventory" (Intellectual card)
  • Generates encounter stages focused on Analysis/Knowledge  
  • Each successful encounter: 2-6 progress points

- Social: "Attend to Guests" (Social card)
  • Generates encounter stages focused on Charm/Persuasion
  • Each successful encounter: 2-6 progress points

Reward (upon completion): 10 Silver, 8 Reputation
```

Key features:
- Player can use any approach multiple times
- Skill level directly affects how many actions needed
- Commission only completes when progress threshold is reached
- All approaches remain available until completion

### 2. Sequential Commissions (Procedurally Generated Steps)
These commissions generate new actions after each step completion:

```
Commission: "Investigate Merchant Theft"
Initial Location: Market
Progress Threshold: 15
Current Progress: 0
Expires: 5 days

Step 1: "Question Witnesses"
Approaches:
- Physical: "Intimidating Presence" (Physical card)
- Intellectual: "Analytical Questioning" (Intellectual card)
- Social: "Sympathetic Listening" (Social card)

Upon reaching 5 progress, generates Step 2:
Step 2: "Follow Leads" (Location determined by approach used)
New approaches generated based on previous choices...

Upon reaching 10 progress, generates Step 3:
Step 3: "Resolve Situation" (Final location)
Final approaches generated based on previous path...

Reward (upon completion): 15 Silver, 12 Reputation, 2 Insight Points
```

Key features:
- Each step provides multiple valid approaches
- Next step is procedurally generated based on previous choices
- Progress accumulates across steps toward total threshold
- Commission completes when total progress meets threshold

## Procedural Encounter Generation System

Both commission types use the same encounter generation system, creating consistent mechanics with different narrative structures:

### Stage Generation Algorithm
```
function generateEncounterStages(approach, locationProperties):
    # Determine stage count (2-3 based on commission complexity)
    stageCount = 2 + (commission.tier > 1 ? 1 : 0)
    
    stages = []
    for stageNum in range(1, stageCount+1):
        # Set base parameters
        baseDifficulty = stageNum + (commission.tier - 1)
        baseProgress = stageNum * 2
        
        # Generate options based on approach type
        options = []
        
        # Primary skill option (best reward, hardest check)
        primarySkill = getPrimarySkill(approach.type)
        primaryDC = baseDifficulty + getLocationModifier(primarySkill, locationProperties)
        options.append({
            "skill": primarySkill,
            "difficulty": primaryDC,
            "successProgress": baseProgress + 2,
            "failureProgress": 0
        })
        
        # Secondary skill option (medium reward, medium check)
        secondarySkill = getSecondarySkill(approach.type)
        secondaryDC = baseDifficulty - 1 + getLocationModifier(secondarySkill, locationProperties)
        options.append({
            "skill": secondarySkill,
            "difficulty": secondaryDC,
            "successProgress": baseProgress,
            "failureProgress": -1
        })
        
        # Safe option (no check, minimal progress)
        options.append({
            "skill": "none",
            "difficulty": 0,
            "successProgress": baseProgress - 2,
            "failureProgress": 0
        })
        
        stages.append({
            "stageNum": stageNum,
            "options": options
        })
    
    return stages
```

### Skill Check Resolution
```
function resolveSkillCheck(skill, difficulty, playerSkill, cardBonus, locationModifiers):
    effectiveSkill = playerSkill + cardBonus
    
    if skill in locationModifiers:
        effectiveSkill += locationModifiers[skill]
    
    return effectiveSkill >= difficulty
```

## Always-Available Basic Actions

In addition to commissions, certain locations offer basic actions that are always available:

```
Action: "Warm by Hearth" (Inn)
- Uses 1 Time Block
- No card required
- Effect: Refreshes 1 Physical card
- Available at any time

Action: "Browse Books" (Library)
- Uses 1 Time Block
- No card required
- Effect: Refreshes 1 Intellectual card
- Available at any time

Action: "Join Tavern Conversation" (Inn)
- Uses 1 Time Block
- No card required
- Effect: Refreshes 1 Social card
- Available at any time
```

## Commission Step Generation for Sequential Commissions

For Sequential Commissions, each step generates the next step procedurally:

```
function generateNextCommissionStep(commission, currentStep, approachUsed, successLevel):
    # Determine next logical location
    nextLocation = determineNextLocation(currentStep.location, approachUsed)
    
    # Generate appropriate step name and description
    stepInfo = generateStepNarrative(commission.theme, approachUsed, currentProgress)
    
    # Create approach options that follow logically
    approaches = generateApproaches(commission.theme, approachUsed)
    
    # Increase difficulty slightly
    difficulty = currentStep.difficulty + 0.5
    
    return {
        "name": stepInfo.name,
        "description": stepInfo.description,
        "location": nextLocation,
        "approaches": approaches,
        "difficulty": difficulty
    }
```

## Complete Example: Investigation Commission

Here's a complete example of how a sequential investigation commission would unfold:

```
Commission: "Mysterious Deliveries"
Progress Threshold: 15
Expires in: 5 days

STEP 1: "Observe Market Activity"
Location: Market District
Approaches:
- Physical: "Track Suspicious Movements" (Physical card)
  Selected by player, generates encounter stages:
  Stage 1: Options test Endurance/Agility/Safe
  Stage 2: Options test Strength/Precision/Safe
  Player earns 4 progress points.

SYSTEM GENERATES STEP 2 based on Physical approach used:
STEP 2: "Follow Suspicious Cart"
Location: Warehouse District
Approaches:
- Physical: "Athletic Pursuit" (Physical card)
- Intellectual: "Deduce Destination" (Intellectual card)
- Social: "Ask for Directions" (Social card)
  Player selects Intellectual approach, gains 5 more progress.

SYSTEM GENERATES STEP 3 based on Intellectual approach used:
STEP 3: "Examine Warehouse Records"
Location: Merchant Guild
Approaches:
- Physical: "Break Into Records Room" (Physical card)
- Intellectual: "Research Documentation" (Intellectual card)
- Social: "Convince Clerk" (Social card)
  Player selects Social approach, gains 6 more progress.

COMMISSION COMPLETE: Total 15 progress reached
Reward: 15 Silver, 10 Reputation, 2 Insight Points
```

This system creates genuine strategic depth by:
1. Making skill levels directly affect progress efficiency
2. Creating natural narrative flow from player choices
3. Offering multiple valid paths to completion
4. Integrating location properties with skill checks
5. Maintaining consistent mechanics with variable narrative structures

The commission only completes when the progress threshold is met, creating a satisfying sense of accomplishment while allowing flexibility in how the player reaches that goal.