# Work Packets for Exhaust System Implementation

## Overview
Implementation of exhaust mechanics replacing "Final Word" with systemic properties-based approach.

## PACKET 1: Core Card Model with Properties List
**Agent**: systems-architect
**Objective**: Replace boolean flags with properties list system

### Tasks:
1. Create `CardProperty` enum in `/src/GameState/CardProperty.cs`
2. Update `ConversationCard` to use `List<CardProperty>` instead of booleans
3. Add helper properties for backwards compatibility
4. Ensure deep clone works with properties list

### Verification:
- Properties list replaces `IsFleeting` boolean
- Helper properties work correctly
- No TODOs or legacy code
- Build succeeds

### Files to modify:
- `/src/GameState/CardProperty.cs` (create)
- `/src/GameState/ConversationCard.cs`
- `/src/GameState/GoalCard.cs`
- `/src/GameState/ObservationCard.cs`

---

## PACKET 2: Three-Effect System Implementation
**Agent**: game-mechanics-designer
**Objective**: Implement separate success/failure/exhaust effects

### Tasks:
1. Create `CardEffect` class with Type, Value, and Data
2. Extend `CardEffectType` enum with all effect types
3. Replace single effect with three separate effects in ConversationCard
4. Remove all legacy effect properties

### Verification:
- Three distinct effect properties exist
- SetAtmosphere is an effect type (not separate property)
- No legacy effect fields remain
- Effects serialize properly to JSON

### Files to modify:
- `/src/GameState/CardEffect.cs` (create)
- `/src/GameState/CardEffectType.cs`
- `/src/GameState/ConversationCard.cs`

---

## PACKET 3: Exhaust Mechanics in HandManager
**Agent**: systems-architect
**Objective**: Implement property-based exhaust system

### Tasks:
1. Update `OnSpeakAction` to exhaust Fleeting cards
2. Implement `OnListenAction` to exhaust Opportunity cards
3. Create `ExecuteExhaustEffect` method
4. Track exhausted cards separately from discard
5. Handle EndConversation exhaust effect

### Verification:
- Fleeting cards exhaust only on SPEAK
- Opportunity cards exhaust only on LISTEN
- Cards with both properties exhaust on EITHER
- Exhaust effects execute before removal
- No "Final Word" checks remain

### Files to modify:
- `/src/Conversations/Managers/HandManager.cs`
- `/src/Subsystems/Conversation/ConversationFacade.cs`

---

## PACKET 4: Remove Final Word Legacy Code
**Agent**: general-purpose
**Objective**: Remove all HasFinalWord and Final Word references

### Tasks:
1. Remove `HasFinalWord` property from all card classes
2. Remove Final Word checks from HandManager
3. Remove Final Word UI indicators
4. Update any tests that reference Final Word
5. Search and remove all "FinalWord" or "Final Word" references

### Verification:
- Zero occurrences of "HasFinalWord" in codebase
- Zero occurrences of "FinalWord" (except in documentation)
- No broken references
- Build succeeds

### Files to search and clean:
- All `.cs` files
- All `.razor` files
- All `.json` files

---

## PACKET 5: JSON Content Migration
**Agent**: content-integrator
**Objective**: Update JSON schema and migrate existing content

### Tasks:
1. Update card JSON to use properties array
2. Add three-effect structure to cards
3. Convert goal cards to use Fleeting + Opportunity
4. Add exhaust effects to ~20% of non-persistent cards
5. Update CardDatabase to read new format

### Verification:
- All cards have properties array
- Goal cards have ["Fleeting", "Opportunity"]
- Each card has successEffect, failureEffect, exhaustEffect
- No old persistence format remains
- Content loads without errors

### Files to modify:
- `/src/Content/core_game_package.json`
- `/src/Content/CardDatabase.cs`
- `/src/Conversations/DeckInitializer.cs`

---

## PACKET 6: UI Property Display System
**Agent**: general-purpose
**Objective**: Display card properties transparently

### Tasks:
1. Create property badges component
2. Display all properties on cards
3. Add special styling for goal cards (both properties)
4. Implement three-effect display sections
5. Add tooltips explaining each property

### Verification:
- All properties show as badges
- Goal cards have critical warning
- Three effects display separately
- Tooltips explain mechanics
- CSS properly styles different properties

### Files to modify:
- `/src/Pages/Components/ConversationContent.razor`
- `/src/Pages/Components/ConversationContent.razor.cs`
- `/src/wwwroot/css/conversation.css`

---

## PACKET 7: Action Preview System
**Agent**: general-purpose
**Objective**: Show what will exhaust before actions

### Tasks:
1. Create SPEAK preview showing Fleeting exhausts
2. Create LISTEN preview showing Opportunity exhausts
3. Highlight critical exhausts (goals)
4. Show exhaust effects that will trigger
5. Add hover states to action buttons

### Verification:
- SPEAK preview shows Fleeting cards
- LISTEN preview shows Opportunity cards
- Goal exhausts show critical warning
- Exhaust effects display in preview
- Preview updates with hand changes

### Files to modify:
- `/src/Pages/Components/ConversationContent.razor`
- `/src/Pages/Components/ConversationContent.razor.cs`
- `/src/wwwroot/css/conversation.css`

---

## PACKET 8: Testing and Validation
**Agent**: change-validator
**Objective**: Verify complete implementation works

### Tasks:
1. Test Fleeting cards exhaust on SPEAK only
2. Test Opportunity cards exhaust on LISTEN only
3. Test goal cards (both properties) exhaust on either
4. Test exhaust effects trigger correctly
5. Test UI displays all properties
6. Verify no legacy code remains

### Verification:
- All test scenarios pass
- No HasFinalWord references
- No TODOs in code
- No compatibility layers
- Build and run succeeds
- Can play through Elena scenario

---

## Execution Order

### Phase 1: Core Backend (Packets 1-4)
1. PACKET 1: Core Card Model with Properties
2. PACKET 2: Three-Effect System
3. PACKET 3: Exhaust Mechanics
4. PACKET 4: Remove Legacy Code

### Phase 2: Content & UI (Packets 5-7)
5. PACKET 5: JSON Content Migration
6. PACKET 6: UI Property Display
7. PACKET 7: Action Preview System

### Phase 3: Validation (Packet 8)
8. PACKET 8: Testing and Validation

## Agent Instructions Template

Each agent should:
1. Read `/docs/refinements.md` for design spec
2. Read `/docs/card-design.md` for card system
3. Read `/mnt/c/git/wayfarer/EXHAUST-MECHANICS-IMPLEMENTATION.md` for implementation plan
4. Read `/mnt/c/git/wayfarer/CLAUDE.md` for coding standards
5. Complete ALL tasks in packet
6. Remove ALL legacy code
7. Add NO TODOs or compatibility layers
8. Ensure build succeeds
9. Report completion with verification checklist

## Success Criteria

✅ Properties list replaces all boolean flags
✅ Three-effect system fully implemented
✅ Exhaust mechanics work based on properties
✅ No HasFinalWord or Final Word remains
✅ JSON uses new format exclusively
✅ UI shows all properties transparently
✅ Action previews show exhaust warnings
✅ All tests pass
✅ Elena scenario playable