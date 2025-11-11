# Arc42 Section 7: Deployment View

## 7.1 Infrastructure Requirements

### Runtime Requirements

**.NET 8 Runtime:**
- Required for ASP.NET Core execution
- Includes Common Language Runtime (CLR)
- Garbage collection and memory management

**Web Browser (Client-Side):**
- Modern browser with SignalR WebSocket support
- Minimum versions:
  - Chrome 90+ / Chromium-based browsers
  - Firefox 88+
  - Safari 14+
  - Microsoft Edge 90+
- JavaScript enabled (required for Blazor interactivity)
- WebSocket protocol support (required for SignalR)

**Server Resources:**
- **Memory:** In-memory GameWorld state (singleton pattern)
  - Typical session: 50-100 MB
  - Large procedural content: 200-300 MB
- **CPU:** Minimal (single-threaded request processing)
- **Disk:** JSON content packages (< 50 MB currently)
- **Network:** Persistent WebSocket connections (one per player)

---

## 7.2 Deployment Architecture

### Single-Process Application

**All Components Run in ASP.NET Core Process:**
- Blazor Server rendering engine
- SignalR hub for real-time communication
- Singleton services (GameWorld, Facades, Managers)
- Domain entities (in-memory collections)
- Static content serving (wwwroot)

### Service Lifetime Configuration

**Singleton Services (Application Lifetime):**
- **GameWorld** - Single source of truth, persists for application lifetime
- **Facades** - Stateless orchestrators (GameFacade, SocialFacade, MentalFacade, PhysicalFacade, etc.)
- **Managers** - Supporting services (TimeManager, ResourceManager, etc.)

**Scoped Services (Per-Connection):**
- **Blazor Components** - Per SignalR connection lifetime
- **UI Context Objects** - Scoped to component render cycles

**Render Mode: ServerPrerendered**
- Components render twice per page load:
  1. **Prerender Phase:** Static HTML generation on server
  2. **Interactive Phase:** SignalR connection + component re-render with interactivity
- **Consequence:** All initialization code must be idempotent (guard flags required)
- **Benefit:** Faster perceived initial load, better SEO potential

---

## 7.3 Build and Run Procedures

### Development Build

```bash
# Navigate to source directory
cd src

# Build application
dotnet build

# Output: ./bin/Debug/net8.0/
```

**Build Output:**
- Compiled assemblies (.dll)
- Runtime dependencies
- Static assets (wwwroot)
- Configuration files (appsettings.json)

### Running Application

```bash
# Navigate to source directory
cd src

# Run application (development mode)
dotnet run

# Application available at: http://localhost:5000
```

**Runtime Configuration:**
- Port: 5000 (default, configurable via ASPNETCORE_URLS)
- Environment: Development (uses appsettings.Development.json)
- Hot Reload: Enabled (file changes trigger recompile)

### Running Tests

```bash
# Navigate to source directory
cd src

# Run all tests
dotnet test

# Run specific test project
dotnet test Tests/UnitTests
```

### Production Build

```bash
# Navigate to source directory
cd src

# Build for production (Release configuration)
dotnet build --configuration Release

# Publish self-contained deployment
dotnet publish --configuration Release --output ./publish

# Output: ./publish/ (ready to deploy)
```

---

## 7.4 Runtime Configuration

### Blazor ServerPrerendered Mode

**Double Rendering Lifecycle:**

**Phase 1: Prerendering**
- Server generates static HTML
- Components execute lifecycle methods:
  - `OnInitializedAsync()`
  - `OnParametersSetAsync()`
  - `OnAfterRenderAsync(firstRender: true)`
- HTML sent to browser
- User sees static content immediately

**Phase 2: Interactive Rendering**
- Browser requests SignalR connection
- Server re-renders components with interactivity
- Components execute lifecycle methods AGAIN:
  - `OnInitializedAsync()` (second time)
  - `OnParametersSetAsync()` (second time)
  - `OnAfterRenderAsync(firstRender: false)`
- SignalR connection established
- Full interactivity available

**Idempotence Requirements:**
- ALL initialization code must check guard flags
- Example: `if (_gameWorld.IsGameStarted) return;`
- NO duplicate resource initialization
- NO duplicate message additions
- Event subscriptions managed carefully (unsubscribe before re-subscribing)

**Safe Patterns:**
- Singleton services persist across both render cycles
- GameWorld maintains state through both phases
- Read-only operations safe to run multiple times
- User-triggered actions only execute after interactive phase

### SignalR Configuration

**WebSocket Connection:**
- Protocol: WebSocket (falls back to Server-Sent Events, Long Polling if unavailable)
- Keep-alive: 15-second interval
- Disconnect timeout: 30 seconds

**Message Handling:**
- Component method invocations from client → server
- State change notifications from server → client
- Automatic reconnection on connection loss

---

## 7.5 Directory Structure

```
Wayfarer/
├── src/                           # Application source code
│   ├── Domain/                    # Core entities
│   │   ├── GameWorld.cs           # Single source of truth
│   │   ├── Scene.cs               # Narrative containers
│   │   ├── Situation.cs           # Choice moments
│   │   ├── Location.cs            # Spatial entities
│   │   ├── NPC.cs                 # Character entities
│   │   └── Route.cs               # Travel paths
│   │
│   ├── Subsystems/                # Domain services (Facades, Managers)
│   │   ├── Facades/               # Orchestration layer
│   │   │   ├── GameFacade.cs      # Primary orchestrator
│   │   │   ├── SocialFacade.cs    # Social challenge system
│   │   │   ├── MentalFacade.cs    # Mental challenge system
│   │   │   └── PhysicalFacade.cs  # Physical challenge system
│   │   └── Managers/              # Supporting services
│   │
│   ├── Content/                   # JSON parsing and catalogues
│   │   ├── Parsers/               # JSON → Domain entity conversion
│   │   ├── Catalogues/            # Categorical → Concrete translation
│   │   └── Generators/            # Procedural content generation
│   │
│   ├── Web/                       # Blazor UI layer
│   │   ├── Components/            # Razor components
│   │   │   ├── GameScreen.razor   # Authoritative parent
│   │   │   ├── LocationContent.razor
│   │   │   ├── ConversationContent.razor
│   │   │   └── MentalContent.razor
│   │   ├── ViewModels/            # UI data transfer objects
│   │   └── Program.cs             # Application entry point
│   │
│   └── wwwroot/                   # Static assets
│       ├── css/                   # Stylesheets
│       ├── images/                # Static images
│       └── js/                    # Client-side JavaScript (minimal)
│
└── Content/                       # JSON data packages
    └── Core/                      # Core game content
        ├── scenes/                # Scene definitions
        ├── locations/             # Location data
        ├── npcs/                  # NPC definitions
        ├── routes/                # Route data
        └── cards/                 # Card definitions
```

### Deployment Units

**Single Deployment Artifact:**
- Self-contained: Includes .NET runtime
- Framework-dependent: Requires .NET 8 runtime pre-installed
- Output: Directory with all assemblies + dependencies

**No Database Required:**
- All state in-memory (GameWorld singleton)
- Content loaded from JSON files at startup
- Save/load system: Planned but not implemented

**No External Services Required:**
- Optional: Ollama (local LLM) for AI narrative generation
- Fallback: Pre-authored templates if Ollama unavailable

---

## Diagrams

### Diagram 7.1: Deployment Architecture

```
┌────────────────────────────────────────────────────────────────┐
│                       User's Browser                           │
│                                                                 │
│  ┌──────────────┐          SignalR WebSocket                  │
│  │ Blazor Client│◄─────────────────────────────────────────┐  │
│  └──────────────┘                                           │  │
└──────────────────────────────────────────────────────────────┼──┘
                                                               │
┌───────────────────────────────────────────────────────────┐  │
│              ASP.NET Core Process (Server)                │  │
│                                                            │  │
│  ┌──────────────────────────────────────────────────────┐│  │
│  │        Blazor Server (Server-Side Rendering)         ││  │
│  │  ┌─────────────┐  ┌────────────────────┐            ││  │
│  │  │ Components  │  │ Prerender Engine    │            ││  │
│  │  └─────────────┘  └────────────────────┘            ││  │
│  │  ┌─────────────┐                                     ││  │
│  │  │SignalR Hub  │─────────────────────────────────────┼┼──┘
│  │  └─────────────┘                                     ││
│  └──────────────────────────────────────────────────────┘│
│                           ▼                               │
│  ┌──────────────────────────────────────────────────────┐│
│  │      Singleton Services (DI Container)               ││
│  │  ┌──────────┐  ┌─────────┐  ┌──────────┐           ││
│  │  │GameWorld │  │ Facades │  │ Managers │           ││
│  │  │(State)   │  │(Logic)  │  │(Support) │           ││
│  │  └──────────┘  └─────────┘  └──────────┘           ││
│  └──────────────────────────────────────────────────────┘│
│                           ▼                               │
│  ┌──────────────────────────────────────────────────────┐│
│  │         Domain Entities (In-Memory)                  ││
│  │     Scenes, Locations, NPCs, Routes, Cards           ││
│  └──────────────────────────────────────────────────────┘│
│                           ▲                               │
│                           │ (Load at startup)             │
│  ┌──────────────────────────────────────────────────────┐│
│  │             JSON Content Files                       ││
│  │   scenes.json, locations.json, npcs.json, etc.      ││
│  └──────────────────────────────────────────────────────┘│
└──────────────────────────────────────────────────────────┘
```

### Diagram 7.2: Component Rendering Lifecycle

```
┌─────────────────────────────┐
│   Browser Request           │
│   GET /                     │
└──────────┬──────────────────┘
           │
           ▼
┌──────────────────────────────┐
│  1. PRERENDER PHASE          │
│  - Server generates HTML     │
│  - OnInitializedAsync() runs │
│  - Components render         │
│  - Static HTML created       │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────┐
│  2. SEND HTML TO BROWSER     │
│  - Browser receives HTML     │
│  - User sees static content  │
│  - Blazor.js loads           │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────┐
│  3. SIGNALR CONNECTION       │
│  - Blazor requests WebSocket │
│  - SignalR hub establishes   │
│  - Connection confirmed      │
└──────────┬───────────────────┘
           │
           ▼
┌──────────────────────────────┐
│  4. INTERACTIVE RENDER       │
│  - OnInitializedAsync() runs │
│  -   AGAIN (idempotence!)    │
│  - Components re-render      │
│  - Interactive UI enabled    │
└──────────────────────────────┘

⚠️ CRITICAL: OnInitializedAsync()
   executes TWICE. Must guard with:
   if (_gameWorld.IsGameStarted)
       return;
```

---

## Related Documentation

- **02_constraints.md** - Blazor ServerPrerendered constraints, type system requirements
- **03_context_and_scope.md** - Technical context and system boundaries
- **05_building_block_view.md** - Component structure and service architecture
