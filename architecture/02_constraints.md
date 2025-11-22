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
- Declarative collection queries (Where, Select, FirstOrDefault)
- Filtering entities by state or categorical properties
- Transforming entity collections

**2. Blazor Event Handlers (Frontend Only):**
- Click event handlers passing arguments to component methods
- UI-specific closures over component scope

**3. Framework Configuration (Rare Exceptions):**
- HTTP client timeout configuration
- Framework service builder configuration

#### FORBIDDEN Lambdas

**1. Backend Event Handlers:**
- Anonymous process exit handlers
- Anonymous lifecycle event subscriptions
- Must use named methods for debugging and testability

**2. Dependency Injection Registration:**
- Anonymous factory functions for service creation
- Must create instance explicitly then register

**3. Backend Logic:**
- Anonymous action delegates for business logic
- Must use named methods for stack traces and searchability

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
- Initialize all entity properties inline at declaration
- Empty collections initialized to empty List, strings to empty string
- Never null for collections or required properties

**Parser Rules:**
- Assign parsed values directly to entity properties
- NO null-coalescing operators
- Trust entity initialization contract

**Game Logic Rules:**
- Access properties directly without null checks
- Trust initialization (no defensive ?. operators)
- Let null references crash with clear stack traces

**Rationale:**
- Fails fast with clear stack traces (easier debugging)
- Single source of truth (entity initialization is contract)
- Less code (no redundant ?? operators)
- Forces fixing root cause (missing JSON data gets fixed, not hidden)

### No Backwards Compatibility

**Break things when refactoring:**
- Delete old methods entirely when renaming/refactoring
- NO Obsolete attributes preserving old signatures
- Force complete refactoring with compilation errors

**Rationale:** Active development phase. Clean breaks force complete refactoring. No technical debt accumulation.

### JSON Serialization Boundary

**Core Principle:** JSON field names MUST match C# property names exactly. Parsers MUST parse all JSON content into strongly-typed objects. The JSON-to-C# boundary is the serialization point - NO raw JSON allowed beyond parsers.

**Field Name Matching:**
- JSON field names match DTO property names exactly (case-sensitive)
- NO JsonPropertyName attributes for field renaming
- Template IDs acceptable in JSON and DTOs (immutable archetypes)

**Parse All JSON:**
- All JSON content parsed into DTO classes with explicit properties
- NO JsonElement passthrough to domain layer
- DTO structure documents JSON schema explicitly

**Parser Responsibility:**
- Parsers extract all data from DTOs into domain entities
- NO JsonElement properties in domain entities
- Domain entities work with typed objects only

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
- FORBIDDEN: Method named GetVenueById returning Location object
- CORRECT: Method named GetLocationById returning Location object

**Rationale:** Misleading names create cognitive load and bugs. Names must accurately reflect reality.

### Antipatterns (Strictly Forbidden)

#### ID Antipattern

**FORBIDDEN:**
- ❌ Encoding data in ID strings: action ID embedding destination location ID
- ❌ Parsing IDs to extract data: substring operations, split operations
- ❌ Using IDs for routing logic: string comparison for action dispatch

**CORRECT:**
- ✅ Use enums for routing: switch on ActionType enum
- ✅ Use strongly-typed object references: DestinationLocation object, NOT ID string
- ✅ Properties flow through entire data stack: JSON → DTO → Domain → ViewModel → Intent

**Entity Instance IDs DO NOT EXIST:**
- ❌ FORBIDDEN: NPC.Id, Location.Id, Route.Id, Scene.Id, Situation.Id
- ✅ ACCEPTABLE: SceneTemplate.Id, SituationTemplate.Id (immutable archetypes, not game state)
- ✅ CORRECT: Object references (`NPC.Location`), categorical properties (`PlacementFilter`), spatial coordinates (`Location.HexPosition`)

#### Generic Property Modification Antipattern

**FORBIDDEN:**
- String-based property routing (PropertyName string matched at runtime)
- String value parsing (NewValue string parsed to bool/int)
- Runtime string matching for property assignment

**CORRECT:**
- Explicit strongly-typed properties (LocationsToUnlock, LocationsToLock)
- Direct property access on object references
- Compiler-verified property assignment

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
- NO overload proliferation: GetSceneById, GetSceneByLocation, GetSceneByIdAndLocation
- Separate methods with clear names: GetSceneById, GetScenesAtLocation
- Method name clearly states what it does

**Rationale:** Method name should clearly state what it does. No input-based branching.

### Code Quality Constraints

**NO Exception Handling (unless explicitly requested):**
- Don't catch exceptions unless debugging specific issues
- Let exceptions bubble to surface bugs
- Exceptions indicate bugs that should be fixed

**NO Logging (unless explicitly requested):**
- No Log.Info/Debug/Error unless debugging specific issues
- Don't pollute code with logging

**Avoid Comments:**
- Self-documenting code preferred
- Exception: Complex algorithms, non-obvious business rules (rare)

**No Defaults Unless Strictly Necessary:**
- Don't return default instances when entity not found
- Throw exception on missing data
- Let it crash with clear error messages

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
