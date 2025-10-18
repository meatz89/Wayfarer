# SCORCHED EARTH NULL-COALESCING CLEANUP - BATTLE PLAN

## MISSION STATUS

**COMPLETED:**
- ✅ InvestigationActivity.cs (5 violations fixed, committed: 23153169)

**REMAINING:**
- 🔥 143 violations across 42 files
- 🎯 Target: ~50 constructor guards preserved, ~93 violations eliminated

---

## VIOLATION PATTERNS (FROM field-optionality-contract.md PHASE 5)

### VALID PATTERN - PRESERVE
```csharp
// Constructor ArgumentNullException guards - KEEP THESE
dependency ?? throw new ArgumentNullException(nameof(dependency))
```

### VIOLATION PATTERN 1: Session/Deck Property Access
```csharp
// BEFORE
return _gameWorld.CurrentMentalSession.Deck?.Hand.ToList() ?? new List<CardInstance>();
// AFTER
return _gameWorld.CurrentMentalSession.Deck.Hand.ToList();
```

**WHY:** Phase 4 established that Deck, Hand, etc. have inline initialization. Null-coalescing is redundant defensive programming.

### VIOLATION PATTERN 2: Config Data Fallbacks
```csharp
// BEFORE
player.Coins = config.Coins ?? 20;
// AFTER
if (!config.Coins.HasValue)
    throw new InvalidOperationException("Config missing required field 'coins'");
player.Coins = config.Coins.Value;
```

**WHY:** Config data without required fields indicates broken content. Fail fast, don't hide problems with defaults.

### VIOLATION PATTERN 3: Entity Property Defaults
```csharp
// BEFORE
List<ExchangeCard> cards = entry?.ExchangeCards ?? new List<ExchangeCard>();
// AFTER
List<ExchangeCard> cards = entry.ExchangeCards;
```

**WHY:** Entity properties with inline initialization (`= new List<T>()`) are never null. Null-coalescing is redundant.

### VIOLATION PATTERN 4: Null-Conditional Chaining
```csharp
// BEFORE
VenueId = currentSpot?.Id ?? ""
// AFTER
VenueId = currentSpot.Id
```

**WHY:** If currentSpot is required for operation, it should be validated. If it's null, fail fast, don't hide with empty string.

---

## SYSTEMATIC EXECUTION PLAN

### PHASE 1: SERVICES LAYER (6 files, ~13 violations)

#### DialogueGenerationService.cs (1 violation)
**Line 18:**
```csharp
// BEFORE
_templates = gameWorld.DialogueTemplates ?? new DialogueTemplates();

// AFTER
if (gameWorld.DialogueTemplates == null)
    throw new InvalidOperationException("GameWorld missing required DialogueTemplates");
_templates = gameWorld.DialogueTemplates;
```
**Pattern:** VIOLATION PATTERN 2 (Config Data Fallbacks)

---

#### GoalCompletionHandler.cs (4 violations)
**Need to grep and classify each violation**

---

#### DifficultyCalculationService.cs (2 violations)
**Need to grep and classify each violation**

---

#### ObstacleIntensityCalculator.cs (2 violations)
**Need to grep and classify each violation**

---

#### ObstacleGoalFilter.cs (1 violation)
**Need to grep and classify each violation**

---

#### InvestigationDiscoveryEvaluator.cs (1 violation)
**Need to grep and classify each violation**

---

### PHASE 2: EXCHANGE SUBSYSTEM (4 files, ~15 violations)

#### ExchangeFacade.cs (6 violations)
**Need to grep and classify each violation**

---

#### ExchangeOrchestrator.cs (4 violations)
**Need to grep and classify each violation**

---

#### ExchangeValidator.cs (2 violations)
**Need to grep and classify each violation**

---

#### ExchangeProcessor.cs (3 violations)
**Need to grep and classify each violation**

---

### PHASE 3: SOCIAL SUBSYSTEM (9 files, ~30 violations)

#### SocialFacade.cs (13 violations)
**CRITICAL: High violation count indicates systemic issue**
**Need to grep and classify each violation**

---

#### SocialNarrativeService.cs (4 violations)
**Need to grep and classify each violation**

---

#### ExchangeHandler.cs (4 violations)
**Need to grep and classify each violation**

---

#### SocialChallengeDeckBuilder.cs (2 violations)
**Need to grep and classify each violation**

---

#### SocialEffectResolver.cs (1 violation)
**Need to grep and classify each violation**

---

#### PersonalityRuleEnforcer.cs (1 violation)
**Need to grep and classify each violation**

---

#### AINarrativeProvider.cs (1 violation)
**Need to grep and classify each violation**

---

#### SocialNarrativeGenerator.cs (2 violations)
**Need to grep and classify each violation**

---

#### PromptBuilder.cs (5 violations)
**Need to grep and classify each violation**

---

#### JsonNarrativeProvider.cs (1 violation)
**Need to grep and classify each violation**

---

### PHASE 4: LOCATION SUBSYSTEM (6 files, ~16 violations)

#### LocationActionManager.cs (5 violations)
**Need to grep and classify each violation**

---

#### LocationFacade.cs (3 violations)
**Need to grep and classify each violation**

---

#### LocationManager.cs (3 violations)
**Need to grep and classify each violation**

---

#### LocationNarrativeGenerator.cs (2 violations)
**Need to grep and classify each violation**

---

#### MovementValidator.cs (1 violation)
**Need to grep and classify each violation**

---

#### NPCLocationTracker.cs (2 violations)
**Need to grep and classify each violation**

---

### PHASE 5: CHALLENGE SUBSYSTEMS (6 files, ~17 violations)

#### PhysicalFacade.cs (8 violations)
**CRITICAL: High violation count indicates systemic issue**
**Need to grep and classify each violation**

---

#### MentalFacade.cs (6 violations)
**Need to grep and classify each violation**

---

#### PhysicalEffectResolver.cs (1 violation)
**Need to grep and classify each violation**

---

#### MentalEffectResolver.cs (1 violation)
**Need to grep and classify each violation**

---

#### PhysicalNarrativeService.cs (1 violation)
**Need to grep and classify each violation**

---

#### MentalNarrativeService.cs (1 violation)
**Need to grep and classify each violation**

---

### PHASE 6: FACADE SUBSYSTEMS (11 files, ~52 violations)

#### MarketSubsystemManager.cs (13 violations)
**CRITICAL: Highest violation count - indicates systemic issue**
**Need to grep and classify each violation**

---

#### ArbitrageCalculator.cs (5 violations)
**Need to grep and classify each violation**

---

#### ResourceFacade.cs (6 violations)
**Need to grep and classify each violation**

---

#### TimeFacade.cs (3 violations)
**Need to grep and classify each violation**

---

#### TravelFacade.cs (1 violation)
**Need to grep and classify each violation**

---

#### EquipmentFacade.cs (2 violations)
**Need to grep and classify each violation**

---

#### ObstacleFacade.cs (1 violation)
**Need to grep and classify each violation**

---

#### CubeFacade.cs (1 violation)
**Need to grep and classify each violation**

---

#### PriceManager.cs (1 violation)
**Need to grep and classify each violation**

---

### CLEANUP: LEGACY FILES

#### SocialFacade.cs.bak (15 violations)
**DELETE THIS FILE - .bak files don't belong in version control**

---

## EXECUTION WORKFLOW

For EACH file:

1. **READ** the file completely
2. **GREP** for `??` with line numbers
3. **CLASSIFY** each violation by pattern (1, 2, 3, 4, or VALID)
4. **TRACE** entity property definitions to verify inline initialization
5. **FIX** all violations using appropriate pattern
6. **BUILD** after logical groups to verify no regressions
7. **COMMIT** with clear message documenting changes

---

## BUILD VERIFICATION CHECKPOINTS

**After Services layer:**
```bash
cd C:/Git/Wayfarer/src && dotnet build
```

**After Exchange subsystem:**
```bash
cd C:/Git/Wayfarer/src && dotnet build
```

**After Social subsystem:**
```bash
cd C:/Git/Wayfarer/src && dotnet build
```

**After Location subsystem:**
```bash
cd C:/Git/Wayfarer/src && dotnet build
```

**After Challenge subsystems:**
```bash
cd C:/Git/Wayfarer/src && dotnet build
```

**After Facade subsystems:**
```bash
cd C:/Git/Wayfarer/src && dotnet build
```

---

## FINAL VERIFICATION

```bash
# Count remaining violations (should be ~50 constructor guards)
cd C:/Git/Wayfarer && grep -r "??" src/Services src/Subsystems --include="*.cs" | grep -v "\.bak" | wc -l

# Verify constructor guards are only remaining violations
cd C:/Git/Wayfarer && grep -r "??" src/Services src/Subsystems --include="*.cs" | grep -v "\.bak" | grep -v "throw new ArgumentNullException"

# Expected: ZERO results (all violations eliminated, only constructor guards remain)
```

---

## HIGH-PRIORITY TARGETS

Based on violation counts, these files need IMMEDIATE attention:

1. **MarketSubsystemManager.cs** (13 violations) - Likely systemic Dictionary disease
2. **SocialFacade.cs** (13 violations) - Likely systemic Dictionary disease
3. **PhysicalFacade.cs** (8 violations) - Likely session/deck property access
4. **MentalFacade.cs** (6 violations) - Likely session/deck property access
5. **ExchangeFacade.cs** (6 violations) - Likely entity property defaults
6. **ResourceFacade.cs** (6 violations) - Likely entity property defaults

These 6 files contain **52 of 143 violations (36%)** - fixing them provides maximum impact.

---

## CRITICAL RULES

1. **NEVER** remove constructor `ArgumentNullException` guards
2. **ALWAYS** trust entity inline initialization (documented in Phase 4)
3. **ALWAYS** fail fast with descriptive errors for missing data
4. **BUILD** after each subsystem to verify no regressions
5. **COMPLETE** work - don't leave any violations unfixed

---

## COMPLETION CRITERIA

✅ All 143 violations processed
✅ Build succeeds with 0 errors, 0 warnings
✅ Final grep shows only constructor guards (~50 occurrences)
✅ All changes committed with clear messages
✅ SocialFacade.cs.bak deleted

This is SCORCHED EARTH refactoring - no half measures, no excuses.
