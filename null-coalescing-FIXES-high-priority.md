# SCORCHED EARTH NULL-COALESCING FIXES - HIGH-PRIORITY FILES

## FILE 1: MarketSubsystemManager.cs (13 violations)

### ANALYSIS

All 13 violations follow the same pattern:
- `item?.Name ?? itemId` (8 occurrences) - Null-conditional fallback to ID
- `_gameWorld.Venues ?? new List<Venue>()` (2 occurrences) - Entity property default
- `venue?.Name ?? venueId` (2 occurrences) - Null-conditional fallback to ID
- `_gameWorld.Venues?.FirstOrDefault(...)` (1 occurrence) - Nullable chain

### ROOT CAUSE

These violations stem from treating `item` and `venue` lookups as potentially null WITHOUT proper validation. The pattern is:
1. Look up entity
2. Use null-conditional to access property
3. Fall back to ID if null

This is **DEFENSIVE PROGRAMMING** that hides data integrity problems.

### FIXES

#### Lines 313, 339, 348, 349, 366, 383, 409, 418, 419, 436 (item?.Name ?? itemId)

**Current Pattern:**
```csharp
item?.Name ?? itemId
```

**Problem:** If `item` is null, using `itemId` as display name hides the fact that content is missing.

**Fix:**
```csharp
// At lookup site (find where item is assigned)
MarketItem item = _gameWorld.MarketItems.FirstOrDefault(i => i.Id == itemId);
if (item == null)
    throw new InvalidOperationException($"Market item '{itemId}' not found in GameWorld");

// Then use directly
item.Name
```

**Pattern Applied:** VIOLATION PATTERN 4 + Fail-Fast Validation

---

#### Lines 488, 535 (_gameWorld.Venues ?? new List<Venue>())

**Current Pattern:**
```csharp
List<Venue> locations = _gameWorld.Venues ?? new List<Venue>();
```

**Problem:** If `_gameWorld.Venues` is null, GameWorld is corrupted. Don't hide this with empty list.

**Check Inline Initialization:**
```bash
grep "Venues.*=" src/GameState/GameWorld.cs
```

**Expected:** `public List<Venue> Venues { get; set; } = new List<Venue>();`

**Fix:**
```csharp
List<Venue> locations = _gameWorld.Venues;
```

**Pattern Applied:** VIOLATION PATTERN 3 (Entity Property Defaults)

---

#### Lines 659 (venue?.Name ?? venueId)

**Current Pattern:**
```csharp
Venue venue = _gameWorld.Venues?.FirstOrDefault(l => l.Id == venueId);
return venue?.Name ?? venueId;
```

**Problem:** Chained nullable operators hide both potential failures:
1. `_gameWorld.Venues` could be null (VIOLATION PATTERN 3)
2. `venue` could be null (VIOLATION PATTERN 4)

**Fix:**
```csharp
Venue venue = _gameWorld.Venues.FirstOrDefault(l => l.Id == venueId);
if (venue == null)
    return venueId; // Acceptable: Display fallback for missing data

// OR fail fast if venue MUST exist:
if (venue == null)
    throw new InvalidOperationException($"Venue '{venueId}' not found");
return venue.Name;
```

**Decision Point:** Is missing venue a DATA ERROR or EXPECTED STATE?
- If DATA ERROR → Fail fast
- If EXPECTED STATE → Document why and use fallback explicitly

**Pattern Applied:** VIOLATION PATTERN 3 + VIOLATION PATTERN 4

---

### COMPLETE FIX FOR MarketSubsystemManager.cs

**Step 1:** Find all `item` variable assignments
**Step 2:** Add null check immediately after lookup
**Step 3:** Replace all `item?.Name ?? itemId` with `item.Name`
**Step 4:** Verify `_gameWorld.Venues` has inline initialization
**Step 5:** Replace `_gameWorld.Venues ?? new List<Venue>()` with `_gameWorld.Venues`
**Step 6:** Fix `venue` lookup with fail-fast or documented fallback
**Step 7:** Build and verify

---

## FILE 2: SocialFacade.cs (13 violations)

### ANALYSIS REQUIRED

```bash
grep -n "??" src/Subsystems/Social/SocialFacade.cs
```

**Expected Patterns:**
- Deck/Hand/Discard property access (VIOLATION PATTERN 1)
- NPC property access with fallback (VIOLATION PATTERN 4)
- Config data with defaults (VIOLATION PATTERN 2)

### FIX STRATEGY

1. Identify each violation context
2. Check if property has inline initialization (likely Deck.Hand, Deck.Discard)
3. Apply VIOLATION PATTERN 1 for session/deck properties
4. Apply VIOLATION PATTERN 4 for NPC lookups
5. Build and verify

---

## FILE 3: PhysicalFacade.cs (8 violations)

### EXPECTED PATTERNS

Based on Mental/Physical challenge architecture:
- `_gameWorld.CurrentPhysicalSession.Deck?.Hand ?? new List<CardInstance>()` (VIOLATION PATTERN 1)
- `_gameWorld.CurrentPhysicalSession?.Deck.Discard ?? new List<CardInstance>()` (VIOLATION PATTERN 1)
- Config data with defaults (VIOLATION PATTERN 2)

### FIX STRATEGY

1. Grep for all violations
2. Verify Deck, Hand, Discard have inline initialization (Phase 4 contract)
3. Replace `Deck?.Hand ?? new List<>()` with `Deck.Hand`
4. Replace `Session?.Deck` with `Session.Deck`
5. Build and verify

---

## FILE 4: MentalFacade.cs (6 violations)

### EXPECTED PATTERNS

Same as PhysicalFacade.cs:
- Session/Deck property access (VIOLATION PATTERN 1)
- Config data with defaults (VIOLATION PATTERN 2)

### FIX STRATEGY

Same as PhysicalFacade.cs - mechanical replacement after verifying inline initialization.

---

## FILE 5: ExchangeFacade.cs (6 violations)

### EXPECTED PATTERNS

Based on Exchange subsystem:
- `entry?.ExchangeCards ?? new List<ExchangeCard>()` (VIOLATION PATTERN 3)
- Config data with defaults (VIOLATION PATTERN 2)

### FIX STRATEGY

1. Grep for all violations
2. Verify entity properties have inline initialization
3. Replace with direct property access
4. Add fail-fast validation for config data
5. Build and verify

---

## FILE 6: ResourceFacade.cs (6 violations)

### EXPECTED PATTERNS

Based on Resource subsystem:
- Resource property access with defaults (VIOLATION PATTERN 3)
- Config data with defaults (VIOLATION PATTERN 2)

### FIX STRATEGY

1. Grep for all violations
2. Classify each by pattern
3. Apply appropriate fix
4. Build and verify

---

## SYSTEMATIC EXECUTION FOR HIGH-PRIORITY FILES

### WORKFLOW

For EACH file above:

```bash
# 1. Grep violations with context
grep -n -C 3 "??" src/Subsystems/.../FILE.cs > violations.txt

# 2. Classify each violation (manual)
# Pattern 1: Session/Deck property access
# Pattern 2: Config data fallbacks
# Pattern 3: Entity property defaults
# Pattern 4: Null-conditional chaining

# 3. Verify inline initialization (if Pattern 3)
grep "PROPERTY_NAME.*=" src/GameState/Entity.cs

# 4. Apply fixes systematically

# 5. Build after each file
cd src && dotnet build

# 6. Commit
git add FILE.cs
git commit -m "Remove null-coalescing violations from FILE.cs"
```

---

## EXAMPLE: COMPLETE FIX FOR MarketSubsystemManager.cs

### Before (Lines 311-314):
```csharp
MarketHistoryEntry entry = new MarketHistoryEntry
{
    ItemId = itemId,
    ItemName = item?.Name ?? itemId,  // VIOLATION
    VenueId = venueId,
    Action = TradeAction.Buy,
```

### After:
```csharp
// Add validation at item lookup (find where item is assigned)
MarketItem item = _gameWorld.MarketItems.FirstOrDefault(i => i.Id == itemId);
if (item == null)
    throw new InvalidOperationException($"Market item '{itemId}' not found in GameWorld");

// Then use directly
MarketHistoryEntry entry = new MarketHistoryEntry
{
    ItemId = itemId,
    ItemName = item.Name,  // FIXED
    VenueId = venueId,
    Action = TradeAction.Buy,
```

### Before (Line 488):
```csharp
List<Venue> locations = _gameWorld.Venues ?? new List<Venue>();  // VIOLATION
```

### After:
```csharp
List<Venue> locations = _gameWorld.Venues;  // FIXED (inline initialization verified)
```

### Before (Lines 658-659):
```csharp
Venue venue = _gameWorld.Venues?.FirstOrDefault(l => l.Id == venueId);  // VIOLATION
return venue?.Name ?? venueId;  // VIOLATION
```

### After:
```csharp
Venue venue = _gameWorld.Venues.FirstOrDefault(l => l.Id == venueId);  // FIXED
if (venue == null)
    return venueId; // Documented fallback: Display ID if venue not found
return venue.Name;  // FIXED
```

---

## COMPLETION CRITERIA FOR HIGH-PRIORITY FILES

✅ MarketSubsystemManager.cs - 13 violations fixed, builds successfully
✅ SocialFacade.cs - 13 violations fixed, builds successfully
✅ PhysicalFacade.cs - 8 violations fixed, builds successfully
✅ MentalFacade.cs - 6 violations fixed, builds successfully
✅ ExchangeFacade.cs - 6 violations fixed, builds successfully
✅ ResourceFacade.cs - 6 violations fixed, builds successfully

**Total Impact:** 52 of 143 violations eliminated (36%)

After completing these 6 files, the remaining 91 violations will follow similar patterns and be straightforward to fix.

---

## CRITICAL REMINDERS

1. **VERIFY** inline initialization before applying VIOLATION PATTERN 3
2. **FAIL FAST** for config data - don't hide missing required fields
3. **BUILD** after each file to catch regressions early
4. **DOCUMENT** any intentional fallbacks (rare, but acceptable when justified)
5. **NEVER** remove constructor `ArgumentNullException` guards

This is SCORCHED EARTH refactoring - complete or don't start.
