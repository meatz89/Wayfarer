# Phase 3A Dependency Map - NarrativeService

## Method Chain Analysis

### GenerateTokenGainNarrative Flow

```
CALLERS (who needs NPC object?)
    ↓
NarrativeFacade.GenerateTokenGainNarrative(string npcId)
    ↓
NarrativeService.GenerateTokenGainNarrative(string npcId)
    ↓
_npcRepository.GetById(npcId) ← VIOLATION (string lookup)
    ↓
Uses npc.Name only
```

**Parallel Path:**
```
CALLERS (who needs NPC object?)
    ↓
EventNarrator.GenerateTokenGainNarrative(string npcId)
    ↓
_npcRepository.GetById(npcId) ← VIOLATION (string lookup)
```

### GenerateRelationshipMilestone Flow

```
CALLERS
    ↓
NarrativeFacade.GenerateRelationshipMilestone(string npcId, int totalTokens)
    ↓
NarrativeService.GenerateRelationshipMilestone(string npcId, int totalTokens)
    ↓
_npcRepository.GetById(npcId) ← VIOLATION
    ↓
Uses npc.Name only
```

**Parallel Path:**
```
CALLERS
    ↓
EventNarrator.GenerateRelationshipMilestone(string npcId, int totalTokens)
    ↓
_npcRepository.GetById(npcId) ← VIOLATION
```

### GenerateRelationshipDamageNarrative Flow

```
CALLERS
    ↓
NarrativeFacade.GenerateRelationshipDamageNarrative(string npcId, ...)
    ↓
NarrativeService.GenerateRelationshipDamageNarrative(string npcId, ...)
    ↓
_npcRepository.GetById(npcId) ← VIOLATION
    ↓
Uses npc.Name only
```

**Parallel Path:**
```
CALLERS
    ↓
EventNarrator.GenerateRelationshipDamageNarrative(string npcId, ...)
    ↓
_npcRepository.GetById(npcId) ← VIOLATION
```

## Refactoring Strategy

### Option 1: Change All Layers Together (HOLISTIC)
1. Change NarrativeService signatures: `string npcId` → `NPC npc`
2. Change EventNarrator signatures: `string npcId` → `NPC npc`
3. Change NarrativeFacade signatures: `string npcId` → `NPC npc`
4. Find and fix ALL callers of NarrativeFacade/EventNarrator
5. Callers must pass NPC objects instead of strings

**Pros**: Clean architecture, no GetById calls
**Cons**: Cascades to ALL callers (potentially many files)

### Option 2: Add Overloads (GRADUAL)
1. Keep existing `(string npcId)` methods
2. Add new `(NPC npc)` overloads
3. Old methods call GetById, then call new methods
4. Migrate callers gradually
5. Delete old methods when migration complete

**Pros**: No breaking changes, gradual migration
**Cons**: VIOLATES CLAUDE.md ("NO compatibility layers", "Complete refactoring only")

### Option 3: Identify Caller Patterns (STRATEGIC)
1. Find ALL callers first
2. Analyze what they have (NPC object or string ID?)
3. If callers have NPC objects: Just change signatures
4. If callers have string IDs: Must refactor callers too

**Pros**: Informed decision, minimal cascading changes
**Cons**: Requires deep analysis before action

## Decision

Following CLAUDE.md principles:
- "Delete first, fix after"
- "No compatibility layers or gradual migration (clean breaks)"
- "Complete refactoring only (no half-measures)"

**CHOOSE OPTION 1: Holistic refactoring**

## Next Steps

1. Find ALL callers of NarrativeFacade methods
2. Find ALL callers of EventNarrator methods
3. Analyze what data they have available
4. Change all layers together
5. Verify compilation

## Analysis Required

Need to search for:
- Who calls `NarrativeFacade.GenerateTokenGainNarrative`
- Who calls `NarrativeFacade.GenerateRelationshipMilestone`
- Who calls `NarrativeFacade.GenerateRelationshipDamageNarrative`
- Who calls `EventNarrator.GenerateTokenGainNarrative`
- Who calls `EventNarrator.GenerateRelationshipMilestone`
- Who calls `EventNarrator.GenerateRelationshipDamageNarrative`

Then for each caller:
- What do they currently have? (NPC object or NPC ID string?)
- Where do they get it from?
- Can we pass NPC object instead?
