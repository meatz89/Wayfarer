# Arc42 Section 3: Context and Scope

## 3.1 Business Context

### System Purpose

Wayfarer is a **single-player low-fantasy tactical RPG** providing resource management gameplay under pressure with impossible choices. Players navigate a procedurally-generated world as a traveler, making strategic decisions about how to spend scarce resources (time, energy, health, social capital, coins) across three interconnected gameplay loops.

### Core Value Proposition

**Strategic Depth Through Impossible Choices:** Players face resource-constrained decisions where all options are valid but insufficient resources force accepting one cost to avoid another. Optimization skill and strategic planning determine success. No "correct" path exists, only different costs revealing character through constraint.

**Infinite Content Without Resolution:** The main story (A-story) never ends. Like an endless road with no final destination, the game provides infinite procedurally-generated progression without arbitrary closure. Players choose WHEN to pursue main story, not IF.

### Gameplay Scope

The core gameplay consists of nested loops (short/medium/long) creating strategic tension through resource competition. Immediate encounters chain into delivery cycles, which accumulate into long-term progression. This structure forces trade-offs at multiple timescales, creating layered strategic depth.

**For detailed gameplay loop documentation**, see [design/02_core_gameplay_loops.md](../design/02_core_gameplay_loops.md).

### User Base

- **Primary Audience:** Single-player RPG enthusiasts seeking strategic decision-making
- **Playstyle:** Deliberate, planning-oriented players who enjoy resource optimization
- **Expected Session Length:** 30 minutes to 2+ hours
- **Target Platform:** Web browsers (desktop/laptop, modern browsers with WebSocket support)

---

## 3.2 External Interfaces

### Player Input

**Strategic Layer (Perfect Information):**
- Visual novel-style choice selection (button clicks)
- Scene/situation navigation
- Resource management decisions (view costs/rewards before commitment)
- Location navigation (within-venue instant, between-venue route selection)
- NPC interaction initiation

**Tactical Layer (Card-Based Challenges):**
- Card selection and play (Social/Mental/Physical challenges)
- Action pair execution (SPEAK/LISTEN, ACT/OBSERVE, EXECUTE/ASSESS)
- Challenge session management (pause investigations, commit to obstacles, engage conversations)

### Game Output

**Visual Display:**
- Narrative text (situation descriptions, choice action text, outcome narratives)
- Resource displays (coins, health, stamina, focus, resolve, time blocks, calendar day)
- Entity information (NPC names/demeanors, location descriptions, route properties)
- Perfect information overlays (exact costs before commitment: "Resolve -2, now 30, will have 28")

**State Feedback:**
- Resource state changes (immediate visual updates)
- Scene progression (situation transitions, scene completion)
- World state updates (location unlocks, NPC relationship changes)
- Calendar advancement (time block progression, day transitions)

### Content Pipeline

**Input:**
- JSON package files (scenes, NPCs, locations, routes, cards, SceneTemplates, archetypes)
- Categorical properties (NPCDemeanor, Quality, PowerDynamic enums)
- Archetype references (SceneArchetypeId, SituationArchetypeId)

**Processing:**
- Static parsers convert JSON → DTOs → Domain entities
- Catalogues translate categorical properties → concrete values (parse-time only)
- Markers resolve to generated entity GUIDs (spawn-time)

**Output:**
- Fully hydrated GameWorld state (in-memory domain entities)
- Scene/Situation/Action entity collections
- Validated entity references (all IDs resolve or throw)

---

## 3.3 Technical Context

### Runtime Environment

**Application Server:**
- .NET 8 Runtime / ASP.NET Core
- Blazor Server with ServerPrerendered mode
- In-process single-instance application

**Client Browser:**
- Modern browsers: Chrome 90+, Firefox 88+, Safari 14+, Edge 90+
- WebSocket support required (SignalR real-time communication)
- JavaScript enabled (Blazor client-side interactivity)

**Hosting Environment:**
- Self-hosted web server or cloud hosting (Azure/AWS/DigitalOcean)
- Persistent WebSocket connections required (Blazor Server constraint)
- HTTP/HTTPS on configurable port (default: localhost:5000)

### Architecture Pattern

**Domain-Driven Design:**
- Domain entities (Scene, Situation, GameWorld, Location, NPC, Route)
- Domain services (Facades: GameFacade, SocialFacade, MentalFacade, PhysicalFacade, etc.)
- No abstraction over-engineering (direct domain concepts, no generic layers)

**Facade Pattern:**
- GameFacade orchestrates all subsystems
- Specialized facades encapsulate business logic (SceneFacade, LocationFacade, ResourceFacade, etc.)
- All facades depend on GameWorld (single source of truth)

**Single Page Application:**
- GameScreen.razor as authoritative parent component
- Child components (LocationContent, ConversationContent, MentalContent, etc.)
- No page reloads (state-based UI transitions)

### State Management

**In-Memory Singleton:**
- GameWorld singleton persists for application lifetime
- All game state stored in GameWorld collections
- No external database (single-player, session-based)
- State resets on application restart (save/load system planned but not implemented)

**Service Lifetimes:**
- GameWorld: Singleton
- Facades: Singleton (stateless, operate on GameWorld)
- UI Components: Scoped (per SignalR connection)

### Technology Stack

- **Framework:** .NET 8 / ASP.NET Core / C# 12
- **UI Technology:** Blazor Server (Server-Side Rendering)
- **Real-Time Communication:** SignalR (WebSocket-based)
- **Content Format:** JSON (human-readable, version-controllable)
- **Architecture:** Domain-Driven Design, Facade Pattern, SPA

---

## 3.4 System Boundaries

### In Scope

**Core Gameplay Systems:**
- Strategic layer (Scene→Situation→Choice flow)
- Three parallel tactical systems (Social/Mental/Physical challenges)
- Resource management (coins, health, stamina, focus, resolve, time)
- Spatial navigation (hex-based world map, venue/location hierarchy, route travel)
- NPC interaction (conversations, bonds, requests, observations)

**Content Generation:**
- Procedural scene generation (archetype-based with categorical scaling)
- Parse-time catalogue translation (categorical → concrete values)
- AI narrative generation (Ollama integration for text enrichment)
- Marker resolution (template-generated dependent resources)

**Progression Systems:**
- Infinite A-story (infinite procedural main storyline)
- B-story and C-story content (authored side content)
- Obligation system (multi-phase investigations spawning scenes)
- Resource accumulation (stat increases, equipment, NPC bonds)

### Out of Scope

**Not Included:**
- Multiplayer (single-player only)
- Real-time combat (turn-based card challenges only)
- 3D graphics (text-based visual novel presentation)
- Voice acting (text-only narrative)
- Mobile platforms (desktop browser only)
- External persistence (no database, no cloud saves - planned but not implemented)
- Mod support (content authoring via JSON only)
- Internationalization (English only)

### External Dependencies

**AI Services (Optional):**
- Ollama (local LLM) for narrative text generation
- Operates as enhancement, not requirement
- Fallback: Pre-authored templates if AI unavailable

**No Other External Dependencies:**
- No database required
- No cloud services required
- No third-party APIs required
- Self-contained application

---

## Diagrams

### Diagram 3.1: Three-Tier Loop Hierarchy

```
┌────────────────────────────────────────────────────────────────┐
│                    SHORT LOOP (10-30s)                         │
│  Situation → View Choices → Select → Apply Costs → Next       │
└────────────────────────────────────────────────────────────────┘
                              ▼
┌────────────────────────────────────────────────────────────────┐
│                   MEDIUM LOOP (5-15m)                          │
│  Accept Delivery → Travel Route (chain of situations) →        │
│  Earn Coins → Survival Spending → Sleep                        │
└────────────────────────────────────────────────────────────────┘
                              ▼
┌────────────────────────────────────────────────────────────────┐
│                     LONG LOOP (hours)                          │
│  Repeat Deliveries → Accumulate → Equipment Upgrades →        │
│  NPC Bonds → New Routes → Procedural A-Story                   │
└────────────────────────────────────────────────────────────────┘
```

### Diagram 3.2: System Context

```
┌──────────────┐
│    Player    │
│  (Browser)   │
└──────┬───────┘
       │ SignalR WebSocket
       │ (Blazor UI interaction)
       ▼
┌──────────────────────────────────────────────────────────────┐
│                    Wayfarer System                            │
│                                                               │
│  ┌────────────┐     ┌─────────────┐     ┌────────────────┐ │
│  │  Blazor UI │────→│   Facades   │────→│   GameWorld    │ │
│  │ Components │←────│  (Services) │←────│ (State Store)  │ │
│  └────────────┘     └─────────────┘     └────────────────┘ │
│                           ▲                                  │
│                           │                                  │
│                     ┌─────┴──────┐                          │
│                     │   Parsers  │                          │
│                     └─────┬──────┘                          │
└───────────────────────────┼───────────────────────────────┘
                            │
                     ┌──────▼──────┐
                     │    JSON     │
                     │  Packages   │
                     │  (Content)  │
                     └─────────────┘
```

### Diagram 3.3: Entity Ownership Hierarchy

```
GameWorld (Single Source of Truth)
 ├─ Scenes (contain embedded Situations)
 │   └─ Situations (generate Actions at query-time)
 │       ├─ ChoiceTemplates (strategic choices)
 │       └─ SituationCards (tactical victory conditions)
 ├─ Locations (placement context, NOT owners)
 ├─ NPCs (placement context, NOT owners)
 ├─ Routes (placement context, NOT owners)
 └─ Actions (ephemeral, created on-demand, deleted after execution)
```

---

## Related Documentation

- **01_introduction_and_goals.md** - System purpose and quality goals
- **02_constraints.md** - Technical and organizational constraints
- **04_solution_strategy.md** - Fundamental architectural approaches
- **05_building_block_view.md** - Static structure and component decomposition
- **12_glossary.md** - Canonical term definitions
