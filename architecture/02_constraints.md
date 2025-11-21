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
Lambda expressions used within LINQ query operations are permitted. This includes filtering collections based on predicate conditions, projecting collection elements to extract specific properties, and finding first matching elements based on criteria. These operations are declarative and enhance code readability.

**2. Blazor Event Handlers (Frontend Only):**
Inline lambda expressions in Blazor component markup for event handling are permitted. These are required by the Blazor framework for binding UI events to component methods with parameters.

**3. Framework Configuration (Rare Exceptions):**
Lambda expressions may be used for framework-level configuration where the framework API explicitly requires them. This includes configuring HTTP clients with timeout settings, middleware pipeline configuration, and other ASP.NET Core infrastructure setup where named methods would add unnecessary ceremony.

#### FORBIDDEN Lambdas

**1. Backend Event Handlers:**
Lambda expressions must not be used for subscribing to backend events such as application lifecycle events or domain events. Instead, use named methods that can be referenced in stack traces and unit tests. Anonymous lambdas hide behavior and make debugging significantly harder.

**2. Dependency Injection Registration:**
Lambda factories must not be used when registering services in the dependency injection container. Instead, instantiate the object explicitly before registration. This makes initialization order clear, enables debugging the initialization logic directly, and avoids hiding construction logic inside DI configuration.

**3. Backend Logic:**
Lambda expressions must not be used to define reusable backend logic patterns such as action delegates or function callbacks. All backend business logic must be implemented as named methods within appropriate service or entity classes. This ensures proper stack traces, testability, and code navigation.



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
Entity properties must be initialized inline at declaration time. Collection properties should be initialized to empty collections. String properties should be initialized to empty strings. This establishes the contract that these properties will never be null.

**Parser Rules:**
Parser code must assign parsed values directly to entity properties without null-coalescing operators. If the parser has successfully parsed a value, assign it. If parsing failed, let the exception bubble up rather than masking the failure with a default value.

**Game Logic Rules:**
Game logic code must trust that entity properties are properly initialized and never use defensive null-conditional operators. Access properties directly. If a property is null when it should not be, this represents a bug in entity initialization that should fail immediately rather than propagate silently.

**Rationale:**
- Fails fast with clear stack traces (easier debugging)
- Single source of truth (entity initialization is contract)
- Less code (no redundant ?? operators)
- Forces fixing root cause (missing JSON data gets fixed, not hidden)

### No Backwards Compatibility

**Break things when refactoring:**
When renaming or refactoring methods, do not preserve the old method signature with an obsolete attribute or compatibility wrapper. Delete the old method entirely and update all call sites. This forces complete refactoring and prevents technical debt accumulation.

**Rationale:** Active development phase. Clean breaks force complete refactoring. No technical debt accumulation.

### JSON Serialization Boundary

**Core Principle:** JSON field names MUST match C# property names exactly. Parsers MUST parse all JSON content into strongly-typed objects. The JSON-to-C# boundary is the serialization point - NO raw JSON allowed beyond parsers.

**Field Name Matching:**
JSON field names must match C# property names exactly, including casing. If a JSON field is named "currentSituationId", the corresponding C# property must be named "CurrentSituationId" (following C# PascalCase convention). Using the JsonPropertyName attribute to map differently-named fields is forbidden. If the JSON field naming must change, rename the JSON field itself.

**Parse All JSON:**
Data Transfer Object (DTO) classes must declare explicit strongly-typed properties for all JSON fields. DTOs must not contain JsonElement properties that defer parsing. The JSON deserializer should handle all structural parsing at the DTO boundary. Deferred parsing using JsonElement represents a failure to properly model the JSON structure.

**Parser Responsibility:**
Parser code creates domain entities from DTOs by extracting all required data from DTO properties. Domain entities must never contain JsonElement properties or any reference to JSON infrastructure. Runtime JSON parsing within domain logic is forbidden. The parser is the single serialization boundary - JSON exists only up to this point, strongly-typed objects exist beyond it.

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

**Example Violation:**
A method named "GetVenueById" that returns a Location object is forbidden. The method name claims to return a Venue but actually returns a Location. This semantic dishonesty creates cognitive load and bugs. The method must be renamed to "GetLocationById" to match its actual return type.

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
String-based property modification systems are forbidden. This pattern uses a PropertyChange class containing a string property name and a string new value, then performs runtime string matching to determine which property to modify. This requires parsing the value string at runtime and introduces silent failures when property names are mistyped.

**CORRECT:**
Use explicit strongly-typed properties for each modification type. Create a SceneReward class with properties like LocationsToUnlock and LocationsToLock, each holding a collection of identifiers. Iterate through the collection and modify the target property directly using normal property assignment. This provides compile-time safety and makes the modification intent explicit.

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
Avoid creating multiple overloaded methods with names like "GetSceneById", "GetSceneByLocation", and "GetSceneByIdAndLocation" that attempt to handle different query patterns. This overload proliferation obscures the actual purpose of each method. Instead, create separate methods with distinct names that clearly describe their purpose: one method to get a scene by identifier, a separate method to get all scenes at a specific location. Each method name should precisely describe its single responsibility.

**Rationale:** Method name should clearly state what it does. No input-based branching.

### Code Quality Constraints

**NO Exception Handling (unless explicitly requested):**
Do not wrap method calls in try-catch blocks that log errors and return null or default values. Let exceptions bubble up naturally. Exception handling hides failures and makes debugging harder by obscuring the root cause. If a scene lookup fails because the identifier doesn't exist, let the exception propagate immediately rather than catching it, logging it, and returning null.

**NO Logging (unless explicitly requested):**
- No Log.Info/Debug/Error unless debugging specific issues
- Don't pollute code with logging

**Avoid Comments:**
- Self-documenting code preferred
- Exception: Complex algorithms, non-obvious business rules (rare)

**No Defaults Unless Strictly Necessary:**
Do not use null-coalescing operators to provide default fallback values when data is missing. If a method returns null when it should return a valid object, this represents a bug that should fail immediately. Do not mask the failure by returning a new empty object. Instead, explicitly check for null and throw an exception with a clear error message describing what was not found.

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
