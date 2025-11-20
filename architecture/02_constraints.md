# Arc42 Section 2: Constraints

## 2.1 Technical Constraints

### Technology Stack

**Platform:**
- .NET 8 / ASP.NET Core
- C# 12

**UI Framework:**
- Blazor Server (Server-Side Rendering)
- ServerPrerendered mode (double-rendering lifecycle)

**Architecture Patterns:**
- Domain-Driven Design
- Facade Pattern
- Single Page Application (SPA)

### Type System Restrictions

The following type restrictions are **compiler-enforced architectural constraints** designed to prevent common design errors:

#### Allowed Types

- **`List<T>`** where T is entity or enum (ONLY collection type allowed)
- **Strongly-typed objects** (entity classes, enums)
- **`int`** for numbers (never float/double/decimal)

#### FORBIDDEN Types

- ❌ **`Dictionary<K,V>`** - String keys create magic strings and runtime errors. Use typed objects instead.
- ❌ **`HashSet<T>`** - Use `List<T>` for consistency
- ❌ **`var`** - Type must be obvious at declaration
- ❌ **`object`** - Always use typed references
- ❌ **`Func<>` / `Action<>`** - No function types except LINQ and framework (see Lambda Rules)
- ❌ **`float/double/decimal`** - Game values are discrete. Use `int` only.
- ❌ **Tuples** - Use explicit classes or structs

**Rationale:** Type restrictions enforce clear entity relationships, prevent ambiguity, force proper domain modeling, and eliminate magic strings.

### Lambda Expression Restrictions

#### ALLOWED Lambdas

**1. LINQ Queries:**
```csharp
// ✅ ALLOWED
var active = scenes.Where(s => s.State == SceneState.Active);
var ids = situations.Select(s => s.Id);
var first = npcs.FirstOrDefault(n => n.Id == targetId);
```

**2. Blazor Event Handlers (Frontend Only):**
```csharp
// ✅ ALLOWED
<button @onclick="() => HandleClick(arg)">Click</button>
```

**3. Framework Configuration (Rare Exceptions):**
```csharp
// ✅ ALLOWED
services.AddHttpClient<OllamaClient>(client => {
    client.Timeout = TimeSpan.FromSeconds(5);
});
```

#### FORBIDDEN Lambdas

**1. Backend Event Handlers:**
```csharp
// ❌ FORBIDDEN
AppDomain.CurrentDomain.ProcessExit += (s, e) => { Log.CloseAndFlush(); };

// ✅ CORRECT - Named method
AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
```

**2. Dependency Injection Registration:**
```csharp
// ❌ FORBIDDEN
services.AddSingleton<GameWorld>(_ => GameWorldInitializer.CreateGameWorld());

// ✅ CORRECT
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

**Rationale:** Backend lambdas are hard to debug (no stack trace entry), hard to test (anonymous), and hard to find (text search fails). Named methods solve all these problems.

### Blazor ServerPrerendered Mode Constraints

**Double Rendering Lifecycle:**
All component lifecycle methods run **TWICE**:
1. First Render: Server-side prerendering (generates static HTML)
2. Second Render: After establishing interactive SignalR connection

**Idempotence Requirements:**

All initialization code MUST be idempotent:
- Check flags before mutating state (`IsGameStarted`)
- Never add duplicate messages (use flags)
- Protect resource initialization (prevent doubling coins/health)
- Manage event subscriptions carefully (avoid double subscription)

**Safe Patterns:**
- All services as Singletons (persist across renders)
- GameWorld maintains state across both render phases
- Read-only operations safe to run multiple times
- User actions only happen after interactive phase

**Why ServerPrerendered:**
- Faster initial page load (user sees content immediately)
- Better SEO (if ever needed)
- Reduced perceived latency

**Alternative Rejected:** `Server` mode would eliminate double rendering but creates worse UX with slower initial load.

---

## 2.2 Organizational Constraints

### Package Cohesion Rules (Atomic Loading)

The content loading system processes packages **atomically** - each package is fully processed before moving to the next. This creates mandatory cohesion constraints:

**Core Principle:** Entities that directly reference each other MUST be in the same package.

#### Mandatory Co-Location Requirements

**1. NPCRequests and Their Cards:**
- NPCRequests must be in same package as ALL cards they reference
- Includes: `requestCards[]`, `promiseCards[]`
- **Why:** NPCRequest initialization validates all referenced cards exist immediately

**2. NPCs and Their Initial Decks:**
- NPCs must be in same package as cards in initial decks
- Includes: `conversationDeck[]`, `requestDeck[]`, `exchangeDeck[]`, `observationDeck[]`, `burdenDeck[]`
- **Why:** NPC initialization populates decks immediately

**3. Letters and Their Card References:**
- Letters must be in same package as cards they create/reference
- **Why:** Letter processing creates cards immediately upon delivery

**4. Card Effects and Target Cards:**
- Cards with `UnlockCard`/`AddCardToDeck` effects must be in same package as target cards
- **Why:** Card effects validated during initialization

**5. Deck Compositions and Referenced Cards:**
- Deck composition templates must be in same package as all referenced cards
- **Why:** Deck compositions applied immediately during package loading

#### Permitted Separations

**Acceptable across packages:**
- NPCs and Locations (skeleton pattern creates placeholder locations)
- Observation cards and target NPCs (deferred addition when NPC loads)
- Location spots and parent locations (skeleton generation)

**Validation Enforcement:** Missing dependencies throw `PackageLoadException` at parse-time (fail-fast).

### Entity Initialization Philosophy ("Let It Crash")

**Required Pattern:**
```csharp
// ✅ CORRECT - Initialize inline
public List<Situation> Situations { get; set; } = new List<Situation>();
public string Title { get; set; } = "";
```

**Parser Rules:**
```csharp
// ✅ CORRECT - Assign directly
entity.Situations = parsedSituations;

// ❌ FORBIDDEN - No null-coalescing
entity.Situations = parsedSituations ?? new List<Situation>();
```

**Game Logic Rules:**
```csharp
// ✅ CORRECT - Trust initialization
var ids = scene.Situations.Select(s => s.Id);

// ❌ FORBIDDEN - No defensive null checks
var ids = scene.Situations?.Select(s => s.Id);
```

**Rationale:**
- Fails fast with clear stack traces (easier debugging)
- Single source of truth (entity initialization is contract)
- Less code (no redundant ?? operators)
- Forces fixing root cause (missing JSON data gets fixed, not hidden)

### No Backwards Compatibility

**Break things when refactoring:**
```csharp
// ❌ FORBIDDEN - Keeping old method for compatibility
[Obsolete]
public Scene GetGoalById(string id) => GetSceneById(id);

// ✅ CORRECT - Delete old method entirely
public Scene GetSceneById(string id) { /* ... */ }
```

**Rationale:** Active development phase. Clean breaks force complete refactoring. No technical debt accumulation.

### JSON Serialization Boundary

**Core Principle:** JSON field names MUST match C# property names exactly. Parsers MUST parse all JSON content into strongly-typed objects. The JSON-to-C# boundary is the serialization point - NO raw JSON allowed beyond parsers.

**Field Name Matching:**
```csharp
// ✅ CORRECT - JSON field matches C# property
// JSON: { "currentSituationId": "situation_1" }
public string CurrentSituationId { get; set; }

// ❌ FORBIDDEN - JsonPropertyName attribute to rename
[JsonPropertyName("current_situation_id")]
public string CurrentSituationId { get; set; }
```

**Parse All JSON:**
```csharp
// ✅ CORRECT - Parse to strongly-typed object
public class LocationDTO {
    public string Id { get; set; }
    public string Name { get; set; }
    public HexPosition HexPosition { get; set; }
}

// ❌ FORBIDDEN - JsonElement passthrough
public class LocationDTO {
    public string Id { get; set; }
    public JsonElement Properties { get; set; }  // Deferred parsing
}
```

**Parser Responsibility:**
```csharp
// ✅ CORRECT - Parser extracts all data from JSON
var location = new Location {
    Id = dto.Id,
    Name = dto.Name,
    HexPosition = dto.HexPosition,  // Parsed by JSON deserializer
};

// ❌ FORBIDDEN - Domain entities with JsonElement
public class Location {
    public JsonElement RawData { get; set; }  // Runtime JSON parsing
}
```

**Rationale:**
- **Single Serialization Point**: JSON parsed once at boundary, never in domain logic
- **Type Safety**: Compiler catches JSON structure changes via DTO compilation errors
- **No Runtime JSON Parsing**: Domain entities work with typed objects only
- **Clear Contracts**: DTO structure documents JSON schema explicitly
- **Easier Refactoring**: Change DTO, compiler finds all affected code

**Enforcement:**
- No `JsonElement` properties in domain entities
- No `JsonPropertyName` attributes (field name IS property name)
- All JSON content parsed into DTO classes with explicit properties
- Parsers create domain entities from DTOs, not from raw JSON

---

## 2.3 Convention Constraints

### Semantic Honesty (Method Names MUST Match Reality)

**Required:**
- Method names match return types exactly
- Parameter types match parameter names
- Property names describe actual data

**Examples:**
```csharp
// ❌ FORBIDDEN - Name doesn't match return type
public Location GetVenueById(string id) { return location; }

// ✅ CORRECT - Name matches return type
public Location GetLocationById(string id) { return location; }
```

**Rationale:** Misleading names create cognitive load and bugs. Names must accurately reflect reality.

### Antipatterns (Strictly Forbidden)

#### ID Antipattern

**FORBIDDEN:**
- ❌ Encoding data in ID strings: `action.Id = $"move_to_{destinationId}"`
- ❌ Parsing IDs to extract data: `action.Id.Substring()`, `action.Id.Split('_')`
- ❌ Using IDs for routing logic: `if (action.Id == "secure_room")`

**CORRECT:**
- ✅ Use enums for routing: `switch (action.ActionType)`
- ✅ Use strongly-typed properties: `action.DestinationLocationId`
- ✅ Properties flow through entire data stack: JSON → DTO → Domain → ViewModel → Intent

**IDs Acceptable For:** Uniqueness (dictionary keys), debugging/logging, simple passthrough
**IDs NEVER For:** Routing decisions, conditional logic, data extraction

#### Generic Property Modification Antipattern

**FORBIDDEN:**
```csharp
// ❌ FORBIDDEN - String-based property routing
public class PropertyChange {
    public string PropertyName { get; set; }  // "IsLocked"
    public string NewValue { get; set; }       // "true"
}

if (change.PropertyName == "IsLocked") {
    location.IsLocked = bool.Parse(change.NewValue);
}
```

**CORRECT:**
```csharp
// ✅ CORRECT - Explicit strongly-typed properties
public class SceneReward {
    public List<string> LocationsToUnlock { get; set; } = new List<string>();
    public List<string> LocationsToLock { get; set; } = new List<string>();
}

foreach (string locId in reward.LocationsToUnlock) {
    location.IsLocked = false;  // Direct property access
}
```

**Rationale:** String matching is error-prone, slow, and violates YAGNI. Add explicit properties when needed.

### Code Structure Constraints

**Allowed:**
- Domain entities (Scene, Situation, GameWorld)
- Domain services (SceneFacade, NavigationFacade)

**FORBIDDEN:**
- Helper classes
- Utility classes
- Static utility methods
- Extension methods
- Manager classes (except specific facades)

**Rationale:** Everything should be a proper domain concept. "Helper" means unclear responsibility. Extension methods hide behavior.

### Method Design Constraints

**One Method, One Purpose:**
```csharp
// ❌ FORBIDDEN - Overload proliferation
GetSceneById(string id)
GetSceneByLocation(string locationId)
GetSceneByIdAndLocation(string id, string locationId)

// ✅ CORRECT - Separate methods with clear names
GetSceneById(string id)
GetScenesAtLocation(string locationId)
```

**Rationale:** Method name should clearly state what it does. No input-based branching.

### Code Quality Constraints

**NO Exception Handling (unless explicitly requested):**
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

**NO Logging (unless explicitly requested):**
- No Log.Info/Debug/Error unless debugging specific issues
- Don't pollute code with logging

**Avoid Comments:**
- Self-documenting code preferred
- Exception: Complex algorithms, non-obvious business rules (rare)

**No Defaults Unless Strictly Necessary:**
```csharp
// ❌ FORBIDDEN - Default fallback
return scene ?? new Scene();

// ✅ CORRECT - Throw on missing
if (scene == null) throw new InvalidOperationException($"Scene not found: {id}");
return scene;
```

**Rationale:** Let it crash. Exceptions indicate bugs that should be fixed, not papered over. Defaults hide missing data problems.

### Formatting Constraints

**FORBIDDEN:**
- ❌ Regions (#region/#endregion)
- ❌ Inline styles in HTML/Blazor
- ❌ Hungarian notation (strTitle, iCount, lstScenes)
- ❌ Labeling documentation as "Revised"/"Refined"/"Updated"

**REQUIRED:**
- PascalCase for entities, properties, methods
- CSS in CSS files (separation of concerns)
- Explicit type names (no var)

---

## Related Documentation

- **01_introduction_and_goals.md** - Quality goals and stakeholder concerns
- **08_crosscutting_concepts.md** - Architectural patterns and principles
- **10_quality_requirements.md** - Quality scenarios enforcing constraints
