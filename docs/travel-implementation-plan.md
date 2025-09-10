# Travel System Implementation Plan

## CORRECTED UNDERSTANDING

### Core Architecture
Each **route** consists of multiple **segments**. Each segment can be one of two types:

1. **FIXED PATH Segments** (Walking)
   - Present 2-3 predetermined path cards
   - Player chooses one path card to proceed
   - Cards persist face-down/face-up state across journeys

2. **EVENT Segments** (Caravan)
   - Randomly draw ONE event from the route's event pool
   - Each event contains 2-3 response cards (different ways to handle the situation)
   - Player chooses one response card to proceed
   - Same card mechanics as path cards

### Key Insight
"Encounters" are NOT a separate system - they're just event collections for caravan travel. Both walking and caravan use the SAME card selection and effect mechanics.

## Critical Issues Identified

### 1. Fundamental Architecture Misunderstanding
- Current system treats encounters as side-effects AFTER path selection
- Should be: encounters ARE the card choices in caravan segments
- RouteSegment only supports PathCardIds, no event type support

### 2. Missing Event Collection Structure
- Individual encounter cards exist but no grouping into events
- No way to represent "beggar event" with multiple response options
- No segment type differentiation (FixedPath vs Event)

### 3. Compilation Error
- TravelManager line 306 references non-existent `encounter.Effect` property
- Encounter cards restructured but code not updated

## Work Packets for Agent Implementation

### Work Packet 1: Core Data Structure Updates
**Agent**: general-purpose
**Prerequisites**: Read /mnt/c/git/wayfarer/docs/travel-system.md, CLAUDE.md
**Files to modify**:
- `/mnt/c/git/wayfarer/src/GameState/RouteSegment.cs` - Add SegmentType enum and EventCollectionId
- `/mnt/c/git/wayfarer/src/Content/DTOs/EventCollectionDTO.cs` - Create new DTO for event collections
- `/mnt/c/git/wayfarer/src/GameState/GameWorld.cs` - Add AllEventCollections dictionary

**Tasks**:
1. Add SegmentType enum (FixedPath, Event) to RouteSegment
2. Add EventCollectionId property to RouteSegment
3. Create EventCollectionDTO with Id, Name, NarrativeText, ResponseCards
4. Add Dictionary<string, EventCollectionDTO> to GameWorld

### Work Packet 2: Fix TravelManager Compilation and Logic
**Agent**: general-purpose  
**Prerequisites**: Read travel-system.md, understand segment types
**Files to modify**:
- `/mnt/c/git/wayfarer/src/GameState/TravelManager.cs`

**Tasks**:
1. Fix line 306 compilation error (remove encounter.Effect reference)
2. Add GetSegmentCards method that handles both segment types
3. Update SelectPathCard to work with unified card selection
4. Remove DrawEncounterCard method (obsolete)
5. Add event drawing logic for Event-type segments

### Work Packet 3: JSON Content Restructuring
**Agent**: general-purpose
**Prerequisites**: Read travel-system.md for content examples
**Files to modify**:
- `/mnt/c/git/wayfarer/src/Content/Core/core_game_package.json`

**Tasks**:
1. Convert encounterCards into eventCollections structure
2. Group related cards into events (e.g., beggar event with 2-3 response cards)
3. Update route segments to include type property
4. Add eventPool property to caravan routes
5. Ensure all path cards have startsRevealed property

### Work Packet 4: Parser Updates for Event Collections
**Agent**: general-purpose
**Prerequisites**: Understand parser architecture, no JsonElement pass-through
**Files to modify**:
- `/mnt/c/git/wayfarer/src/Services/GamePackageParser.cs` or similar

**Tasks**:
1. Add parsing for eventCollections section
2. Parse EventCollectionDTO with strongly typed ResponseCards
3. Populate GameWorld.AllEventCollections
4. Update route parsing to handle segment types
5. NO JsonElement pass-through - full deserialization

### Work Packet 5: Initialize Path Discovery System
**Agent**: general-purpose
**Prerequisites**: Read about face-down/face-up mechanics in travel-system.md
**Files to modify**:
- `/mnt/c/git/wayfarer/src/Services/GameWorldInitializer.cs` or similar

**Tasks**:
1. Initialize PathCardDiscoveries from startsRevealed property
2. NO ID checking - pure mechanical property driven
3. Set up event deck positions for routes with event pools
4. Initialize route discovery states

### Work Packet 6: UI Updates for Unified Card Selection
**Agent**: general-purpose
**Prerequisites**: Understand card-based UI principles from CLAUDE.md
**Files to modify**:
- `/mnt/c/git/wayfarer/src/Pages/Components/TravelContent.razor`
- `/mnt/c/git/wayfarer/src/Pages/Components/TravelPathContent.razor`

**Tasks**:
1. Display cards from GetSegmentCards (works for both types)
2. Show face-down cards with only name/stamina cost
3. Show face-up cards with full details
4. Ensure card selection uses SPEAK action (not buttons)
5. Display event narrative text when showing event response cards

### Work Packet 7: Testing and Validation
**Agent**: general-purpose
**Prerequisites**: Read all implementation changes
**Files to create**:
- Playwright test script

**Tasks**:
1. Test walking route with fixed path segments
2. Test caravan route with event segments
3. Verify face-down/face-up discovery persistence
4. Test event random drawing and response selection
5. Verify stamina management and travel completion

## Implementation Phases

### Phase 1: Fix Critical Breaks (IMMEDIATE)

#### 1.1 Fix Route References in core_game_package.json
```json
// Market↔Tavern (market_to_tavern/tavern_to_market)
CHANGE: ["market_crowds", "tavern_shortcut"]
TO: ["common_room", "back_alley"]

// Fix other broken references similarly
```

#### 1.2 Add Missing Path Cards
Must add these cards that routes reference:
- market_crowds
- tavern_shortcut  
- scholars_path
- night_passage
- smugglers_tunnel
- courier_express
- guard_patrol_route

Each card needs:
```json
{
  "id": "market_crowds",
  "name": "Through Market Crowds",
  "staminaCost": 1,
  "startsRevealed": false,  // MECHANIC-DRIVEN, NO ID CHECKS
  "hasEncounter": false,
  "isOneTime": false,
  "coinRequirement": 0,
  "permitRequirement": null,
  "travelTimeMinutes": 12,
  "hungerEffect": 0,
  "oneTimeReward": null,
  "narrativeText": "Navigate through the busy market crowds."
}
```

#### 1.3 Add Missing Encounter Cards
Must add these encounters that routes reference:
- helpful_stranger
- found_coins
- rain_shower
- guard_inspection

With MECHANICAL effects:
```json
{
  "id": "beggar",
  "name": "Beggar",
  "encounterType": "choice",
  "choices": [
    {
      "text": "Give coin",
      "coinCost": 1,
      "timeCost": 0
    },
    {
      "text": "Walk around",
      "coinCost": 0,
      "timeCost": 5
    }
  ]
}
```

### Phase 2: Implement Core Mechanics

#### 2.1 Encounter Choice System in TravelManager.cs
```csharp
public class EncounterChoice
{
    public string Text { get; set; }
    public int CoinCost { get; set; }
    public int TimeCost { get; set; }
    public int StaminaCost { get; set; }
    public bool RequiresPermit { get; set; }
    public bool ForceReturn { get; set; }
}

private void ApplyEncounterCard(EncounterCardDTO encounter)
{
    if (encounter.EncounterType == "immediate")
    {
        // Apply immediate effects
        ApplyEffects(encounter.CoinEffect, encounter.TimeEffect, encounter.StaminaEffect);
    }
    else if (encounter.EncounterType == "choice")
    {
        // Present valid choices to player
        var validChoices = encounter.Choices
            .Where(c => CanAffordChoice(c))
            .ToList();
            
        // For now: take first valid choice
        // Later: UI presents choices
        if (validChoices.Any())
        {
            var choice = validChoices.First();
            ApplyChoiceEffects(choice);
        }
    }
}
```

#### 2.2 Face-Down/Face-Up Discovery
```csharp
// In GameWorldInitializer
gameWorld.PathCardDiscoveries = new Dictionary<string, bool>();
foreach (var card in allPathCards)
{
    // Use mechanical property, NO ID CHECKING
    gameWorld.PathCardDiscoveries[card.Id] = card.StartsRevealed;
}
```

#### 2.3 Fix Stamina Derivation
```csharp
private int GetDerivedStamina(Player player)
{
    // Pure math based on health/hunger
    int baseStamina = 3;
    
    if (player.Health >= 80) baseStamina = 4;
    else if (player.Health <= 30) baseStamina = 1;
    
    if (player.Hunger >= 80) baseStamina -= 2;
    else if (player.Hunger >= 60) baseStamina -= 1;
    
    return Math.Max(0, baseStamina);
}
```

### Phase 3: System Integration

#### 3.1 Add Path Revelation to Conversation Cards
```json
{
  "id": "trade_route_advice",
  "type": "Normal",
  "effect": {
    "rapportChange": 1,
    "revealsPathCards": ["dock_workers_path", "loading_docks"]
  }
}
```

#### 3.2 Add Investigation Reveals
```json
{
  "id": "market_square",
  "investigationRewards": [
    {
      "familiarityLevel": 1,
      "revealsPathCards": ["merchant_avenue"]
    },
    {
      "familiarityLevel": 2,
      "revealsPathCards": ["main_gate"]
    }
  ]
}
```

#### 3.3 Connect to Obligation System
- Display deadline urgency during travel
- Add "courier" paths for urgent letters
- Encounters can damage/inspect letters

### Phase 4: Delete Legacy Code

#### 4.1 Remove TravelTimeMatrix
- Delete entire TravelTimeMatrix class
- Delete old GetTravelTime methods
- Remove equipment/terrain checks
- Keep ONLY path card system

### Phase 5: Testing & Validation

#### 5.1 Playwright Tests
- Test each route can be traveled
- Verify path discovery mechanics
- Test encounter choices
- Verify stamina management

#### 5.2 Integration Tests
- Conversation reveals paths
- Investigation reveals paths
- Travel affects letter delivery
- Resources cycle properly

## Success Criteria

1. **No Crashes**: All referenced cards exist
2. **Mechanical Effects**: Encounters have real consequences
3. **Discovery Works**: Face-down cards reveal permanently
4. **Choices Matter**: Multiple valid paths with trade-offs
5. **Systems Connect**: Travel integrates with conversations/obligations

## Implementation Order

1. **Fix route references** (prevents crashes)
2. **Add missing cards** (makes playable)
3. **Implement encounter mechanics** (adds gameplay)
4. **Add discovery system** (creates progression)
5. **Integrate with other systems** (creates depth)
6. **Delete legacy code** (removes confusion)
7. **Test everything** (ensures quality)

## Key Principles

- **NO ID MATCHING**: Everything driven by mechanical properties
- **NO SPECIAL CASES**: Use data, not code checks
- **FULL INTEGRATION**: Travel must connect to all systems
- **MEANINGFUL CHOICES**: Every path needs trade-offs
- **PROGRESSION**: Discovery creates long-term value