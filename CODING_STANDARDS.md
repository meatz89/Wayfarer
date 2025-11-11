# Wayfarer Coding Standards

**Purpose:** Authoritative source for all coding conventions, naming standards, and code quality rules.

**Last Updated:** 2025-01

**Related Documentation:**
- **DESIGN_PHILOSOPHY.md:** Design principles (why we design this way)
- **ARCHITECTURAL_PATTERNS.md:** Architectural patterns (HIGHLANDER, Catalogue, etc.)
- **ARCHITECTURE.md:** System architecture

---

## Type System

### Allowed Types
- **`List<T>`** where T is entity or enum (ONLY collection type allowed)
- **Strongly-typed objects** (entity classes, enums)
- **`int`** for numbers (never float/double/decimal)

### FORBIDDEN Types
- ❌ **`Dictionary<K,V>`** - Use typed objects instead
- ❌ **`HashSet<T>`** - Use `List<T>` instead
- ❌ **`var`** - Always explicit types
- ❌ **`object`** - Always typed
- ❌ **`Func<>`** - No function types (except LINQ, see Lambda Rules)
- ❌ **`Action<>`** - No action types (except framework, see Lambda Rules)
- ❌ **`float/double/decimal`** - Use `int` only
- ❌ **Tuples** - Use explicit classes or structs

### Why These Restrictions

**`List<T>` only:**
- Enforces clear entity relationships
- No ambiguity about what collection contains
- Forces proper domain modeling

**No Dictionary:**
- String keys = magic strings = runtime errors
- No type safety
- Hides proper entity relationships
- Use strongly-typed objects instead

**No var:**
- Type must be obvious at declaration
- No inference games
- Readability over terseness

**int only:**
- Game values are discrete (health, coins, stat points)
- No floating-point precision issues
- Clear whole-number semantics

---

## Lambda Usage Rules

### ALLOWED Lambdas

**1. LINQ Queries:**
```csharp
// ✅ ALLOWED
var active = scenes.Where(s => s.State == SceneState.Active);
var ids = situations.Select(s => s.Id);
var first = npcs.FirstOrDefault(n => n.Id == targetId);
```

**2. Blazor Event Handlers:**
```csharp
// ✅ ALLOWED (Frontend only)
<button @onclick="() => HandleClick(arg)">Click</button>
<button @onclick="@(e => ProcessEvent(e))">Process</button>
```

**3. Framework Configuration:**
```csharp
// ✅ ALLOWED (Framework-specific, rare exceptions)
services.AddHttpClient<OllamaClient>(client => {
    client.Timeout = TimeSpan.FromSeconds(5);
});
```

### FORBIDDEN Lambdas

**1. Backend Event Handlers:**
```csharp
// ❌ FORBIDDEN
AppDomain.CurrentDomain.ProcessExit += (s, e) => {
    Log.CloseAndFlush();
};

// ✅ CORRECT - Named method
AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

private void OnProcessExit(object sender, EventArgs e) {
    Log.CloseAndFlush();
}
```

**2. Dependency Injection Registration:**
```csharp
// ❌ FORBIDDEN
services.AddSingleton<GameWorld>(_ => GameWorldInitializer.CreateGameWorld());

// ✅ CORRECT - Explicit
GameWorld gameWorld = GameWorldInitializer.CreateGameWorld();
builder.Services.AddSingleton(gameWorld);
```

**3. Backend Logic:**
```csharp
// ❌ FORBIDDEN
Action<Scene> processScene = (s) => { /* logic */ };

// ✅ CORRECT - Named method
private void ProcessScene(Scene scene) { /* logic */ }
```

### Why These Rules

**LINQ is expressive and safe:**
- Query syntax is readable
- Type-safe transformations
- Industry-standard pattern

**Backend lambdas are hidden complexity:**
- Hard to debug (no stack trace entry)
- Hard to test (anonymous)
- Hard to find (text search fails)
- Named methods solve all these problems

---

## Entity Initialization ("LET IT CRASH")

### REQUIRED Pattern

**Always initialize collections inline:**

```csharp
public class Scene {
    // ✅ CORRECT - Initialize inline
    public List<Situation> Situations { get; set; } = new List<Situation>();
    public string Title { get; set; } = "";
    public SomeType? OptionalProp { get; set; } = null;  // Nullable ONLY if truly optional
}
```

### Parser Rules

**Assign directly, never null-coalesce:**

```csharp
// ✅ CORRECT
entity.Situations = parsedSituations;

// ❌ FORBIDDEN
entity.Situations = parsedSituations ?? new List<Situation>();
```

**Why:** Collections initialize inline on entity. Parser trusts that contract. No redundant null-coalescing.

### Game Logic Rules

**Never null-coalesce when querying:**

```csharp
// ✅ CORRECT
var ids = scene.Situations.Select(s => s.Id);

// ❌ FORBIDDEN
var ids = scene.Situations?.Select(s => s.Id);
```

**ArgumentNullException ONLY for null constructor parameters:**

```csharp
public SceneFacade(GameWorld gameWorld) {
    // ✅ CORRECT - Guard constructor parameter
    if (gameWorld == null) throw new ArgumentNullException(nameof(gameWorld));
    _gameWorld = gameWorld;
}

// ❌ FORBIDDEN - Don't guard entity properties
if (scene.Situations == null) { /* defensive code */ }
```

### Why "Let It Crash"

**Benefits:**
1. **Fails fast** - Easier debugging, clear stack traces
2. **Single source of truth** - Entity initialization is the contract
3. **Less code** - No redundant `??` operators everywhere
4. **Forces fixing root cause** - Missing JSON data gets fixed, not papered over

**Philosophy:** If data is missing, crash with descriptive error. Don't hide problems with defaults.

---

## ID Antipattern (NO STRING ENCODING/PARSING)

### FORBIDDEN Patterns

**Never encode data in ID strings:**

```csharp
// ❌ FORBIDDEN
action.Id = $"move_to_{destinationId}";
scene.Id = $"investigate_{npcId}_{locationId}";
```

**Never parse IDs to extract data:**

```csharp
// ❌ FORBIDDEN
if (action.Id.StartsWith("move_to_")) {
    string destId = action.Id.Substring(8);
}

if (action.Id.Contains("_")) {
    string[] parts = action.Id.Split('_');
}
```

**Never use IDs for routing logic:**

```csharp
// ❌ FORBIDDEN
if (action.Id == "secure_room") { /* logic */ }
if (action.Id.StartsWith("negotiate_")) { /* logic */ }
```

### CORRECT Patterns

**Use enums for routing:**

```csharp
// ✅ CORRECT
switch (action.ActionType) {
    case ActionType.Navigate: /* logic */; break;
    case ActionType.Instant: /* logic */; break;
    case ActionType.StartChallenge: /* logic */; break;
}
```

**Use strongly-typed properties:**

```csharp
// ✅ CORRECT
public class NavigateAction {
    public string Id { get; set; }  // For uniqueness only
    public ActionType ActionType { get; set; } = ActionType.Navigate;
    public string DestinationLocationId { get; set; }  // Explicit property
}

// Usage
string dest = action.DestinationLocationId;  // Direct access, no parsing
```

**Properties flow through entire data stack:**

```
JSON → DTO → Parser → Domain Entity → ViewModel → Intent Handler
       DestinationLocationId property at every layer
```

### IDs Are Acceptable For

1. **Uniqueness** - Dictionary keys, UI rendering keys
2. **Debugging/Logging** - Display only, never logic
3. **Simple passthrough** - Domain → ViewModel, no logic

### IDs Are NEVER For

- ❌ Routing decisions (use ActionType enum)
- ❌ Conditional logic (use strongly-typed properties)
- ❌ Data extraction (add properties instead)

### Enforcement Checklist

- [ ] No `.Substring()`, `.Split()`, regex on IDs
- [ ] No `.StartsWith()`, `.Contains()`, `.EndsWith()` on IDs
- [ ] No `$"prefix_{data}"` patterns
- [ ] ActionType enum for routing, not ID matching
- [ ] Properties flow: Domain → ViewModel → Intent

---

## Generic Property Modification Antipattern

### FORBIDDEN Patterns

**No string-based property routing:**

```csharp
// ❌ FORBIDDEN
public class PropertyChange {
    public string PropertyName { get; set; }  // "IsLocked", "IsVisible"
    public string NewValue { get; set; }       // "true", "false"
}

if (change.PropertyName == "IsLocked") {
    location.IsLocked = bool.Parse(change.NewValue);
}
```

**Why forbidden:**
- Runtime string matching (error-prone)
- Runtime parsing (slow, error-prone)
- YAGNI violation (building generic system for hypothetical future properties)
- Trying to be "flexible" instead of explicit

### CORRECT Patterns

**Explicit strongly-typed properties:**

```csharp
// ✅ CORRECT
public class SceneReward {
    public List<string> LocationsToUnlock { get; set; } = new List<string>();
    public List<string> LocationsToLock { get; set; } = new List<string>();
    public List<string> LocationsToHide { get; set; } = new List<string>();
    public List<string> LocationsToReveal { get; set; } = new List<string>();
}

// Usage - Direct property modification
foreach (string locId in reward.LocationsToUnlock) {
    location.IsLocked = false;  // No string matching
}
```

**Add new properties when needed:**

When new state modification needed, add explicit property. Don't try to make existing system "flexible."

### Enforcement

- [ ] No string-based property routing fields
- [ ] No runtime parsing (`bool.Parse`, `int.Parse` from strings)
- [ ] No "extensible" generic modification systems
- [ ] Each property has one purpose

---

## Naming Conventions

### Entities

**PascalCase for entities:**
```csharp
public class Scene { }
public class Situation { }
public class GameWorld { }
```

### Properties

**PascalCase for properties:**
```csharp
public string Title { get; set; }
public int CurrentDay { get; set; }
public List<Situation> Situations { get; set; }
```

### Methods

**PascalCase for methods:**
```csharp
public void AdvanceToNextSituation() { }
public Scene GetActiveScene() { }
```

### Semantic Honesty

**Method names MUST match reality:**

```csharp
// ❌ FORBIDDEN - Name doesn't match return type
public Location GetVenueById(string id) {
    return location;  // Returns Location, not Venue
}

// ✅ CORRECT - Name matches return type
public Location GetLocationById(string id) {
    return location;
}
```

**Parameter types must match purpose:**

```csharp
// ❌ FORBIDDEN - Misleading name
public void ProcessVenue(Location location) { /* Venue in name, Location in signature */ }

// ✅ CORRECT - Honest naming
public void ProcessLocation(Location location) { }
```

### No Hungarian Notation

```csharp
// ❌ FORBIDDEN
string strTitle;
int iCount;
List<Scene> lstScenes;

// ✅ CORRECT
string title;
int count;
List<Scene> scenes;
```

---

## Code Structure

### Domain Services and Entities

**Allowed:**
- Domain entities (Scene, Situation, GameWorld)
- Domain services (SceneFacade, NavigationFacade)

**FORBIDDEN:**
- Helper classes
- Utility classes
- Manager classes (except specific facades)
- Static utility methods

**Why:** Everything should be a proper domain concept. "Helper" means unclear responsibility.

### No Extension Methods

```csharp
// ❌ FORBIDDEN
public static class StringExtensions {
    public static bool IsNullOrEmpty(this string str) { }
}

// ✅ CORRECT - Direct method or domain service
public bool IsValid(string input) { }
```

**Why:** Extension methods hide behavior. Explicit is better.

### One Method, One Purpose

**No overload proliferation:**

```csharp
// ❌ FORBIDDEN
GetSceneById(string id)
GetSceneByLocation(string locationId)
GetSceneByNPC(string npcId)
GetSceneByIdAndLocation(string id, string locationId)

// ✅ CORRECT - Separate methods with clear names
GetSceneById(string id)
GetScenesAtLocation(string locationId)
GetScenesWithNPC(string npcId)
```

**Why:** Method name should clearly state what it does. No input-based branching.

---

## Code Quality

### Exception Handling

**Default:** NO exception handling unless explicitly requested

```csharp
// ❌ FORBIDDEN (unless requested)
try {
    var scene = GetSceneById(id);
} catch (Exception ex) {
    Log.Error(ex);
    return null;
}

// ✅ CORRECT - Let exceptions bubble
var scene = GetSceneById(id);  // Throws if not found
```

**Why:** Let it crash. Exceptions indicate bugs that should be fixed, not papered over.

### Logging

**Default:** NO logging unless explicitly requested

```csharp
// ❌ FORBIDDEN (unless requested)
Log.Info("Entering AdvanceToNextSituation");
var next = scene.GetNextSituation();
Log.Debug($"Found situation: {next.Id}");

// ✅ CORRECT - No logging
var next = scene.GetNextSituation();
```

**Why:** Logging added on demand when debugging specific issues. Don't pollute code with logging.

### Comments

**Avoid comments:**

```csharp
// ❌ BAD - Comment explaining what code does
// Loop through all situations and find active one
foreach (var situation in scene.Situations) {
    if (situation.Status == SituationStatus.Active) { /* ... */ }
}

// ✅ GOOD - Self-documenting code
var activeSituation = scene.Situations.FirstOrDefault(s => s.Status == SituationStatus.Active);
```

**Exception:** Complex algorithms, non-obvious business rules (rare).

### No Defaults Unless Strictly Necessary

**Let it fail:**

```csharp
// ❌ FORBIDDEN
public Scene GetSceneById(string id) {
    var scene = _gameWorld.Scenes.FirstOrDefault(s => s.Id == id);
    return scene ?? new Scene();  // Default fallback
}

// ✅ CORRECT - Throw on missing
public Scene GetSceneById(string id) {
    var scene = _gameWorld.Scenes.FirstOrDefault(s => s.Id == id);
    if (scene == null) throw new InvalidOperationException($"Scene not found: {id}");
    return scene;
}
```

**Why:** Defaults hide missing data problems. Fail fast reveals bugs.

### No Backwards Compatibility

**Break things when refactoring:**

```csharp
// ❌ FORBIDDEN - Keeping old method for compatibility
[Obsolete]
public Scene GetGoalById(string id) => GetSceneById(id);

public Scene GetSceneById(string id) { /* ... */ }

// ✅ CORRECT - Delete old method entirely
public Scene GetSceneById(string id) { /* ... */ }
```

**Why:** We're in active development. Clean breaks force complete refactoring. No technical debt accumulation.

---

## Formatting

### No Regions

```csharp
// ❌ FORBIDDEN
#region Properties
public string Id { get; set; }
public string Title { get; set; }
#endregion

#region Methods
public void DoSomething() { }
#endregion

// ✅ CORRECT - No regions
public string Id { get; set; }
public string Title { get; set; }

public void DoSomething() { }
```

**Why:** Regions hide code. Visible organization is better.

### No Inline Styles

**In Blazor/HTML:**

```html
<!-- ❌ FORBIDDEN -->
<div style="color: red; font-size: 14px;">Content</div>

<!-- ✅ CORRECT -->
<div class="error-text">Content</div>
```

**Why:** CSS belongs in CSS files. Separation of concerns.

### Free-Flow Text Over Bullet Lists (Documentation)

**In documentation (where applicable):**

```markdown
<!-- Less Preferred -->
Benefits:
- Benefit 1
- Benefit 2
- Benefit 3

<!-- Preferred -->
Benefits flow naturally from the architecture. First, the system enables X which leads to Y. Second, the pattern enforces Z through explicit contracts...
```

**Why:** Narrative flow is more engaging than bullet lists. Use bullets for quick reference, prose for explanation.

---

## Never Label "Revised"/"Refined"

**In documentation or code comments:**

```markdown
<!-- ❌ FORBIDDEN -->
## Revised Architecture
## Refined Approach
## Updated Design

<!-- ✅ CORRECT -->
## Architecture
## Approach
## Design
```

**Why:** Documentation should represent current state. No need to signal it's "new" or "updated." Git history tracks changes.

---

## Enforcement

These standards are enforced through:

1. **Code Reviews** - Reviewers cite this document
2. **Pull Request Checklist** - Standards verification before merge
3. **Architecture Reviews** - Periodic codebase audits
4. **Onboarding** - New developers read this first

**When in doubt:** Follow existing code patterns in Scene.cs, GameWorld.cs, SceneFacade.cs (these are exemplary).

**Exceptions:** Must be explicitly documented with reason. No silent violations.

---

## Related Documentation

- **DESIGN_PHILOSOPHY.md:** Why we design this way (principles)
- **ARCHITECTURAL_PATTERNS.md:** Reusable architectural patterns
- **ARCHITECTURE.md:** System architecture
- **GLOSSARY.md:** Terminology definitions
