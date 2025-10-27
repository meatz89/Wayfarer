# Template Architecture Principle: Three-Level Design

## CRITICAL ARCHITECTURAL FOUNDATION

This document defines the **THREE-LEVEL ARCHITECTURE** for Situation Spawn Templates.

---

## The Three Levels (Never Mix These)

### LEVEL 1: PATTERN (Documentation Layer)

**What It Is:**
- Pure conceptual structures described in markdown documentation
- Authoring guidance for content creators
- NO JSON, NO CODE - just human-readable patterns

**Examples:**
- "Linear Progression" - Sequential story beats (A→B→C)
- "Hub-and-Spoke" - Central situation spawns multiple parallel options
- "Branching Consequences" - Success/failure lead to different futures
- "Discovery Chain" - Finding clues reveals new locations

**Purpose:**
- Provide reusable conceptual frameworks
- Document narrative structures that work well
- Guide decision-making when creating content
- Lives ONLY in `situation-spawn-patterns.md`

**Correct Example (from obstacle-templates.md):**
```markdown
### Template 1: The Gauntlet

**Pattern:** Single approach type, multiple preparations reduce cost

**Structure:**
- One approach goal available immediately at high property level
- Multiple preparation goals reduce same property
- Each preparation unlocks marginally better resolution
- Final state can reach property 0 (free resolution)

**Use Cases:**
- Physical barriers, natural obstacles, terrain challenges
```

---

### LEVEL 2: TEMPLATE (JSON Archetype Layer)

**What It Is:**
- Immutable archetype definitions with formulas and categorical filters
- Shared by ALL instances spawned from it
- Lives in `GameWorld.SituationTemplates` (List, NOT Dictionary)

**What JSON Templates MUST Contain:**
- ✅ Archetype enums (`"archetype": "Rescue"`)
- ✅ Requirement formulas (`"baseValue": "CurrentPlayerBond", "offset": 3`)
- ✅ Template-to-template spawn references (`"childTemplateId": "investigation_followup_template"`)
- ✅ Categorical entity filters (`"npcArchetype": "Innocent"`, NOT `"npcId": "elena"`)
- ✅ Narrative hints for AI generation (`"tone": "Urgent"`, `"theme": "Heroic sacrifice"`)

**What JSON Templates MUST NOT Contain:**
- ❌ Specific entity IDs (`"npcId": "elena"` - this is INSTANCE data)
- ❌ Fixed requirement thresholds (`"threshold": 5` - should be formula with offset)
- ❌ Runtime state (`"spawnedDay"`, `"completedDay"` - instance properties)

**Correct Template JSON Structure:**
```json
{
  "id": "rescue_plea_template",
  "archetype": "Rescue",
  "tier": 1,
  "interactionType": "NpcSocial",
  "npcFilters": {
    "archetype": "Innocent",
    "locationProximity": "Same"
  },
  "requirementFormula": {
    "orPaths": [
      {
        "numericRequirements": [
          {
            "type": "BondStrength",
            "baseValue": "CurrentPlayerBond",
            "offset": 3,
            "label": "Need trust to confide"
          }
        ]
      }
    ]
  },
  "successSpawns": [
    {
      "childTemplateId": "investigation_followup_template",
      "placementStrategy": "SameNpc",
      "requirementOffsets": {
        "bondStrengthOffset": 2
      }
    }
  ],
  "narrativeHints": {
    "tone": "Urgent, desperate",
    "theme": "Plea for help"
  }
}
```

---

### LEVEL 3: INSTANCE (Runtime Entity Layer)

**What It Is:**
- Concrete runtime entity living in `GameWorld.Situations` (List, NOT Dictionary)
- **Composition**: Instance REFERENCES template, does NOT clone it
- Has specific entity references populated by code at spawn time
- Has runtime state (spawned when, completed when, generated narrative)

**Composition Architecture (CRITICAL):**
```csharp
public class Situation
{
    // Composition: Reference shared immutable template (NOT cloned)
    public SituationTemplate Template { get; set; }

    // Runtime instance properties ONLY
    public string Id { get; set; }
    public NPC PlacementNpc { get; set; }  // Concrete entity (code selects)
    public Location PlacementLocation { get; set; }  // Concrete entity (code selects)
    public List<CalculatedRequirement> CalculatedRequirements { get; set; }  // Frozen from formulas
    public SituationStatus Status { get; set; }
    public int SpawnedDay { get; set; }
    public string GeneratedNarrative { get; set; }
    public Situation ParentSituation { get; set; }
}
```

**Access template properties via composition:**
```csharp
// ✅ CORRECT - Access template through instance reference
SituationArchetype archetype = instance.Template.Archetype;
List<TemplateSpawnRule> spawns = instance.Template.SuccessSpawns;
NarrativeHints hints = instance.Template.NarrativeHints;

// ❌ WRONG - Cloning template properties into instance
public class Situation
{
    public SituationArchetype Archetype { get; set; }  // Duplicated from template!
}
```

---

## Code Responsibilities at Spawn Time

**What CODE Does When Spawning Instance from Template:**

1. **Select Template:**
   ```csharp
   SituationTemplate template = gameWorld.SituationTemplates
       .FirstOrDefault(t => t.Id == "rescue_plea_template");
   ```

2. **Choose Entities Using Categorical Filters:**
   ```csharp
   // Template says: npcArchetype: "Innocent", locationProximity: "Same"
   NPC targetNpc = gameWorld.NPCs
       .Where(npc => npc.Archetype == NpcArchetype.Innocent)
       .Where(npc => npc.Location == currentLocation)
       .FirstOrDefault();
   ```

3. **Calculate Requirements from Formulas:**
   ```csharp
   // Template says: baseValue: "CurrentPlayerBond", offset: 3
   // Player current bond with targetNpc: 8
   // Calculated requirement: 11
   int currentBond = gameWorld.Player.GetBondWith(targetNpc);
   int threshold = currentBond + template.RequirementFormula.Offset; // 8 + 3 = 11
   ```

4. **Create Instance with Composition:**
   ```csharp
   Situation instance = new Situation
   {
       Template = template,  // Reference shared template (NOT cloned)
       Id = GenerateUniqueId(),
       PlacementNpc = targetNpc,  // Concrete entity selected by code
       PlacementLocation = targetLocation,  // Concrete entity selected by code
       CalculatedRequirements = calculatedReqs,  // Frozen values from formulas
       Status = SituationStatus.Dormant,
       SpawnedDay = currentDay
   };
   ```

5. **Add to GameWorld (List, NOT Dictionary):**
   ```csharp
   gameWorld.Situations.Add(instance);  // List.Add()
   ```

---

## Strong Typing Enforcement (NO DICTIONARIES, NO HASHSETS)

### FORBIDDEN:
```csharp
// ❌ WRONG - Dictionary lookup pattern
public Dictionary<string, SituationTemplate> SituationTemplates { get; set; }
SituationTemplate template = gameWorld.SituationTemplates["rescue_plea_template"];

// ❌ WRONG - HashSet
public HashSet<SituationTemplate> SituationTemplates { get; set; }
```

### REQUIRED:
```csharp
// ✅ CORRECT - List with LINQ queries
public List<SituationTemplate> SituationTemplates { get; set; } = new();
SituationTemplate template = gameWorld.SituationTemplates
    .FirstOrDefault(t => t.Id == "rescue_plea_template");
```

---

## Composition Over Cloning (NEVER CLONE TEMPLATES)

### WHY COMPOSITION:
1. **Single source of truth** - Template changes propagate to all instances
2. **Memory efficient** - Template data stored once, not duplicated per instance
3. **Type safety** - Access template properties through instance.Template reference
4. **Clear separation** - Runtime state vs design-time archetypes

### CORRECT Pattern:
```csharp
// Instance references template
Situation instance = new Situation { Template = template };

// Access template properties
string tone = instance.Template.NarrativeHints.Tone;
int tier = instance.Template.Tier;
```

### WRONG Pattern:
```csharp
// ❌ Cloning template properties into instance
Situation instance = new Situation
{
    Archetype = template.Archetype,  // Duplicated!
    Tier = template.Tier,  // Duplicated!
    NarrativeHints = template.NarrativeHints  // Duplicated!
};
```

---

## Summary Table

| Level | Lives In | Contains | Lookup Pattern | Purpose |
|-------|----------|----------|----------------|---------|
| **PATTERN** | Markdown docs | Conceptual structures, use cases | N/A (documentation only) | Guide content authoring |
| **TEMPLATE** | `List<SituationTemplate>` | Formulas, filters, template references | LINQ query: `.FirstOrDefault(t => t.Id == ...)` | Reusable archetypes |
| **INSTANCE** | `List<Situation>` | Concrete entities, runtime state, template reference | LINQ query: `.FirstOrDefault(s => s.Id == ...)` | Playable game content |

---

## Validation Checklist

Before any template implementation:

- [ ] Pattern documentation in markdown (NO JSON, NO entity IDs)
- [ ] Template JSON has categorical filters (NO specific entity IDs)
- [ ] Template JSON has formulas with offsets (NO fixed thresholds)
- [ ] Instance entity has Template reference (composition, NOT cloning)
- [ ] GameWorld uses `List<SituationTemplate>` (NOT Dictionary)
- [ ] GameWorld uses `List<Situation>` (NOT Dictionary)
- [ ] Code selects entities using LINQ queries (NOT dictionary lookups)
- [ ] Code calculates requirements at spawn time (formula + player state)
- [ ] Code populates instance with concrete entity references
- [ ] NO template properties duplicated in instance entity

---

## Reference Implementation

See existing codebase examples:
- **PATTERN documentation:** `obstacle-templates.md` (pure conceptual structures)
- **Strong typing:** `GameWorld.Locations` (List, NOT Dictionary)
- **Entity lookups:** LINQ queries throughout facade layer

This architecture ensures:
✅ Clear separation of concerns (pattern, template, instance)
✅ Strong typing enforcement (Lists, NO Dictionaries/HashSets)
✅ Memory efficiency (composition, NOT cloning)
✅ Type safety (compiler-enforced access patterns)
✅ Reusability (templates spawn many instances)
