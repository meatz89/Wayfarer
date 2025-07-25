# Tutorial Implementation Approach

## Date: 2025-01-24

## Overview
This document outlines the comprehensive approach for implementing Wayfarer's tutorial system. The tutorial is designed as a 10-day narrative journey from poverty to patronage, teaching all core game mechanics through guided play.

## Core Problems Identified

### 1. Content Loading Architecture
**Problem**: All game content loads together with no separation between tutorial and main game.
**Impact**: Players see end-game content (patron letters, standing obligations) at game start.
**Solution**: Implement content separation via GameMode enum and conditional loading paths.

### 2. Missing Tutorial Integration
**Problem**: NarrativeManager exists but is completely disconnected from game systems.
**Evidence**: 
- LocationActionManager doesn't check narrative state
- No UI component to show tutorial guidance
- Tutorial doesn't auto-start on new game
**Solution**: Wire NarrativeManager into all relevant systems with event-driven progression.

### 3. Event-Driven vs Time-Based Progression
**Problem**: Tutorial shouldn't progress based on days but on player actions.
**Rationale**: Players learn at different paces; forcing day-based progression creates confusion.
**Solution**: Implement event listeners in NarrativeManager for all game actions.

## Architectural Decisions

### 1. Content Separation Strategy
```
/Content/Templates/          # Main game content
/Content/Templates/Tutorial/ # Tutorial-only content
```

**Implementation**:
- Add `GameMode` enum: `MainGame`, `Tutorial`
- Modify `InitializationContext` to include game mode
- Each initialization phase checks mode and loads appropriate content
- No mixing of tutorial and main game content

### 2. Event-Driven Narrative System
```csharp
public class NarrativeManager {
    // Listen to game events
    void OnLetterDelivered(Letter letter);
    void OnNPCConversation(string npcId);
    void OnLocationVisited(string locationId);
    void OnTokensEarned(string npcId, int amount);
    void OnLetterBurned(string letterId);
    
    // Progress based on events, not time
    void CheckProgressionTriggers();
}
```

### 3. Clean Integration Points
- **CommandDiscoveryService**: Filter actions through NarrativeManager
- **LocationRepository**: Hide locations based on narrative state
- **NPCRepository**: Control NPC availability via narrative flags
- **LetterGenerationService**: Generate tutorial-specific letters
- **UI Components**: Show/hide based on tutorial progress

## Implementation Plan

### Phase 1: Core Infrastructure (Days 1-3)
1. **GameMode System**
   - Create GameMode enum
   - Modify InitializationContext
   - Update all 6 initialization phases
   - Add mode selection at game start

2. **Content Directory Structure**
   - Create /Tutorial/ subdirectory
   - Move tutorial content to separate files
   - Ensure clean separation

3. **Fix Immediate Issues**
   - Debug why lower_ward_square spot is missing
   - Remove pre-existing letter from game start
   - Find source of patron obligation at startup

### Phase 2: Narrative System (Days 4-6)
1. **NarrativeManager Implementation**
   - Replace stub with real implementation
   - Add event listeners for all game actions
   - Implement progression checking
   - Create narrative state persistence

2. **Integration Points**
   - Wire into CommandDiscoveryService
   - Connect to conversation system
   - Add to save/load system
   - Create UI overlay component

3. **Tutorial Content Creation**
   - Define all narrative steps
   - Create tutorial NPCs
   - Design tutorial locations
   - Write tutorial-specific letters

### Phase 3: Polish and Testing (Days 7-8)
1. **UI/UX Polish**
   - Create NarrativeOverlay component
   - Add progress indicators
   - Implement UI element hiding
   - Add tutorial hints/guidance

2. **End-to-End Testing**
   - Test complete 10-day flow
   - Verify all branches work
   - Check save/load compatibility
   - Ensure no soft locks

## Key Technical Decisions

### 1. No Special Rules
Following the game's design philosophy, the tutorial uses existing systems:
- Same letter queue mechanics
- Same token system
- Same conversation system
- Just different content and progression

### 2. Content-Driven Design
Tutorial behavior emerges from content, not code:
- NPCs appear based on narrative flags
- Letters generate from tutorial templates
- Locations unlock through progression
- No hardcoded tutorial logic in business layer

### 3. Clean Separation
Tutorial mode is completely separate:
- Can test tutorial in isolation
- Main game unaffected by tutorial code
- Easy to maintain and modify
- Clear boundaries between modes

## Current State (2025-01-24)

### Completed
- Identified all integration gaps
- Designed comprehensive solution
- Created implementation plan
- Fixed action UI layout issues
- Created GameMode enum and modified InitializationContext
- Fixed lower_ward_square spot issue (spots weren't being added to Location.AvailableSpots)
- Fixed standing obligations validation (case-insensitive property matching)

### In Progress
- Investigating pre-existing letter in queue (no source found yet in initialization)
- Planning content separation for tutorial mode

### Key Fixes Applied
1. **Missing Spot Fix**: Added spots to Location.AvailableSpots in Phase2_LocationDependents
2. **Validation Fix**: Updated StandingObligationValidator to use case-insensitive property matching
3. **GameMode System**: Added GameMode enum and updated InitializationContext

### Mystery: Pre-existing Letter
- User reports letter in queue at game start
- Investigation shows:
  - LetterQueue initialized as empty array
  - No letter generation in initialization phases
  - Standing obligations loaded but not activated
  - No morning activities triggered at startup
  - Time set to 6:00 AM but no automatic letter generation
- Possible causes to investigate:
  - UI layer generating letters
  - Debug/test code
  - Client-side letter generation

### Remaining Work
- 6-10 days total implementation time
- Critical path: Content separation → Narrative implementation → Integration → Testing

## Next Session Starting Points

1. **Immediate Priority**: 
   - Implement GameMode enum in InitializationContext
   - Find why lower_ward_square spot doesn't appear
   - Remove patron obligation from game start

2. **Then Continue With**:
   - Update Phase1-6 for content separation
   - Create tutorial content files
   - Implement real NarrativeManager

## Technical Notes

### Why Not Use Existing Narrative?
The existing narrative definitions in NarrativeBuilder are good but:
- NarrativeManager is just a stub
- No event system implemented
- No integration with game systems
- No UI component for guidance

### Content Loading Fix
The key insight is that content loading happens in phases with dependencies:
1. Locations → Spots → NPCs → Routes → Letters
2. Each phase must respect GameMode
3. Tutorial content loads from separate directory
4. No cross-contamination between modes

### Event System Design
Tutorial progression based on player actions:
- Letter delivered → Check if tutorial letter → Progress
- NPC talked to → Check if first meeting → Progress
- Token earned → Check if first token → Show UI
- Location visited → Check if new area → Unlock content

This creates natural, player-driven progression without artificial day gates.