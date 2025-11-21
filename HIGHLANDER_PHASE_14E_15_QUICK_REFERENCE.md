# HIGHLANDER Phase 14E-15: Quick Reference - 16 Compilation Errors

## THREE ROOT CAUSES (Not 16 Independent Issues)

### ROOT CAUSE A: Entity .Id Property Deletion (8 errors)
**Problem:** Services accessing deleted .Id properties after HIGHLANDER Phase 2.1
**Fix Pattern:** Replace ID-based queries with Name/object reference queries

### ROOT CAUSE B: Property Misalignment (6 errors)
**Problem:** Properties renamed/deleted but usage sites not updated
**Fix Pattern:** Update all layers together (Entity → Service → UI)

### ROOT CAUSE C: Variable Scope Conflicts (2 errors)
**Problem:** Variable name collisions during refactoring
**Fix Pattern:** Rename conflicting variables

---

## ERRORS BY CONTRACT BOUNDARY

### QUERY-TIME (Facades/Services) - 10 Errors

| # | Error | File | Line | Fix Pattern |
|---|-------|------|------|-------------|
| 1 | MeetingObligation.Id | MeetingManager.cs | 54 | Use object reference or Name |
| 2 | Location.Id | SceneFacade.cs | 47 | Use object comparison |
| 3 | NPC.ID | SceneFacade.cs | 47 | Use object comparison |
| 4 | Scene.Id | SceneInstanceFacade.cs | 91 | Use TemplateId or object |
| 5 | Situation.Id | MentalFacade.cs | 67 | Query via scene.Situations |
| 6 | GameWorld.InitialLocationName | GameFacade.cs | 710 | Access via InitialPlayerConfig |
| 7 | RouteImprovement.RouteId | TravelTimeCalculator.cs | 72 | Use Route object |
| 8 | unlock.PathDTO | SituationCompletionHandler.cs | 276 | Use PathCard object |
| 9 | Variable "npc" conflict | SceneFacade.cs | 147 | Rename variable |
| 10 | GetLocation undefined | LocationManager.cs | 139 | Use LINQ query |

### UI-LAYER (Razor Components) - 3 Errors

| # | Error | File | Line | Fix Pattern |
|---|-------|------|------|-------------|
| 11 | MentalSession.ObligationId | MentalContent.razor | 12 | Display Obligation.Name |
| 12 | MentalSession.ObligationId | MentalFacade.cs | 88 | Store Obligation object |
| 13 | SocialChallengeContext.ConversationType | ConversationContent.razor.cs | 351 | Check RequestText directly |

### PARSE-TIME (DTOs in Domain) - 2 Errors

| # | Error | File | Line | Fix Pattern |
|---|-------|------|------|-------------|
| 14 | NPCData.NpcId | JsonNarrativeRepository.cs | 63 | Resolve to NPC object |
| 15 | Venue.Venue | LocationManager.cs | 232 | Use location.Venue |

### UNDEFINED METHOD - 1 Error

| # | Error | File | Line | Fix Pattern |
|---|-------|------|------|-------------|
| 16 | RouteImprovement.RouteId | TravelTimeCalculator.cs | 72 | Use Route object |

---

## FIX PATTERNS (Copy-Paste Ready)

### Pattern 1: Query Entity by Name (Not ID)

```csharp
// WRONG:
Location location = _gameWorld.Locations.FirstOrDefault(loc => loc.Id == locationId);

// CORRECT:
Location location = _gameWorld.Locations.FirstOrDefault(loc => loc.Name == locationName);
```

### Pattern 2: Use Object Reference (No Query)

```csharp
// WRONG:
string npcId = situation.NpcId;
NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);

// CORRECT:
NPC npc = situation.Npc; // Direct object reference
```

### Pattern 3: Query by TemplateId (Scenes)

```csharp
// WRONG:
Scene scene = _gameWorld.Scenes.FirstOrDefault(s => s.Id == sceneId);

// CORRECT:
Scene scene = _gameWorld.Scenes.FirstOrDefault(s => s.TemplateId == templateId);
```

### Pattern 4: Query Situations via Scene

```csharp
// WRONG:
Situation situation = _gameWorld.Situations.FirstOrDefault(sit => sit.Id == situationId);

// CORRECT:
Situation situation = _gameWorld.Scenes
    .SelectMany(s => s.Situations)
    .FirstOrDefault(sit => sit.TemplateId == templateId);
```

### Pattern 5: Access GameWorld Initial Config

```csharp
// WRONG:
string startingLocationName = _gameWorld.InitialLocationName; // Property doesn't exist

// CORRECT:
string startingLocationName = _gameWorld.InitialPlayerConfig?.StartingLocationName;
```

### Pattern 6: Store Object in Session (Not ID)

```csharp
// WRONG:
_gameWorld.CurrentMentalSession = new MentalSession
{
    ObligationId = engagement.Id, // Property doesn't exist
};

// CORRECT:
_gameWorld.CurrentMentalSession = new MentalSession
{
    Obligation = engagement, // Object reference
};
```

### Pattern 7: Display Name in UI (Not ID)

```razor
<!-- WRONG: -->
<span>Obligation: @Session.ObligationId</span>

<!-- CORRECT: -->
<span>Obligation: @Session.Obligation?.Name</span>
```

### Pattern 8: Check String Property (Not Enum)

```csharp
// WRONG:
if (Context?.ConversationType == "request") // Property doesn't exist

// CORRECT:
if (!string.IsNullOrEmpty(Context?.RequestText))
```

### Pattern 9: Resolve DTO to Entity at Parse-Time

```csharp
// WRONG (DTO in domain layer):
public class RouteSegmentUnlock
{
    public PathDTO PathDTO { get; set; }
}

// CORRECT (Entity in domain layer):
public class RouteSegmentUnlock
{
    public PathCard Path { get; set; }
}

// Parser resolves:
var unlock = new RouteSegmentUnlock
{
    Path = _pathCardRepository.GetByName(dto.PathId)
};
```

### Pattern 10: Replace Deleted Method with LINQ

```csharp
// WRONG:
Location location = GetLocation(locationId); // Method deleted

// CORRECT:
Location location = _gameWorld.Locations.FirstOrDefault(loc => loc.Name == locationName);
```

### Pattern 11: Rename Conflicting Variables

```csharp
// WRONG:
foreach (Scene scene in scenes)
{
    NPC npc = scene.CurrentSituation?.Npc;

    foreach (ChoiceTemplate choice in templates)
    {
        NPC npc = choice.TargetNpc; // ERROR: npc already declared
    }
}

// CORRECT:
foreach (Scene scene in scenes)
{
    NPC sceneNpc = scene.CurrentSituation?.Npc;

    foreach (ChoiceTemplate choice in templates)
    {
        NPC choiceNpc = choice.TargetNpc;
    }
}
```

---

## EXECUTION PHASES

### PHASE 14E: Query-Time Issues (10 errors)
**Files:** MeetingManager.cs, SceneFacade.cs, SceneInstanceFacade.cs, MentalFacade.cs, GameFacade.cs, TravelTimeCalculator.cs, SituationCompletionHandler.cs, LocationManager.cs

**Order:**
1. Fix entity queries (Patterns 1-4)
2. Fix property access (Patterns 5-6)
3. Rename variables (Pattern 11)
4. Replace deleted methods (Pattern 10)

### PHASE 15: UI-Layer Issues (3 errors)
**Files:** MentalContent.razor, MentalFacade.cs, ConversationContent.razor.cs

**Order:**
1. Update UI display (Pattern 7)
2. Update session initialization (Pattern 6)
3. Fix property checks (Pattern 8)

### PHASE 16: Parse-Time Issues (2 errors)
**Files:** SituationCompletionHandler.cs, JsonNarrativeRepository.cs

**Order:**
1. Change domain classes to store entities (Pattern 9)
2. Update parsers to resolve DTOs → entities
3. Update domain code to use entities

---

## VERIFICATION COMMANDS

```bash
# Check for remaining .Id access (should be ZERO)
grep -r "\.Id ==" src/GameState src/Services src/Subsystems --include="*.cs"

# Check for DTO usage in domain layer (should be ZERO)
grep -r "DTO" src/Services src/Subsystems --include="*.cs" | grep -v "// DTO"

# Build project (should succeed)
cd src && dotnet build

# Check for ID display in UI (should be ZERO)
grep -r "@.*\.Id" src/Pages --include="*.razor"
```

---

## KEY ARCHITECTURAL PRINCIPLES

1. **HIGHLANDER Pattern**: Domain entities have NO .Id properties
2. **Object References Only**: Relationships use direct object references
3. **Natural Keys**: Query by Name, TemplateId, or categorical properties
4. **Layer Separation**: DTOs exist ONLY in Content layer
5. **Parse-Time Resolution**: Parser translates DTOs → entities once

---

## DETAILED ANALYSIS

See `architecture/HIGHLANDER_PHASE_14E_15_ARCHITECTURAL_ANALYSIS.md` for:
- Complete architectural root cause analysis
- Contract boundary breakdowns
- Holistic refactoring strategies
- Architectural lessons learned
- Comprehensive verification checklist
