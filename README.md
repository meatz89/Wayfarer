# Wayfarer

Low-fantasy tactical RPG with narrative-driven strategic layer and three parallel tactical challenge systems (Social, Mental, Physical).

## Project Overview

Wayfarer combines visual novel-style narrative choices with tactical card-based challenges. Players navigate a low-fantasy world, engage with NPCs, pursue multi-phase investigations (Obligations), and resolve conflicts through strategic and tactical gameplay.

## Core Architecture

- **Strategic Layer:** Scene → Situation → Choice flow with perfect information
- **Tactical Layer:** Three parallel challenge systems (Social/Mental/Physical) with hidden complexity
- **Procedural Content:** Catalogue-based generation with categorical property scaling
- **Blazor SPA:** Server-side rendering with real-time state management

## Documentation

### Getting Started
- **[HOW_TO_PLAY_WAYFARER.md](HOW_TO_PLAY_WAYFARER.md)** - Gameplay guide and mechanics explanation
- **[ARCHITECTURE.md](ARCHITECTURE.md)** - System architecture overview (start here for development)

### Design & Philosophy
- **[DESIGN_PHILOSOPHY.md](DESIGN_PHILOSOPHY.md)** - 12 core design principles with conflict resolution
- **[GLOSSARY.md](GLOSSARY.md)** - Canonical term definitions (60+ terms)
- **[IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)** - Feature implementation status matrix

### Architectural Patterns
- **[ARCHITECTURAL_PATTERNS.md](ARCHITECTURAL_PATTERNS.md)** - Reusable patterns (HIGHLANDER, Catalogue, Three-Tier Timing, Let It Crash, Sentinel Values)
- **[CODING_STANDARDS.md](CODING_STANDARDS.md)** - Coding conventions and type system rules
- **[REQUIREMENT_INVERSION_PRINCIPLE.md](REQUIREMENT_INVERSION_PRINCIPLE.md)** - Pedagogical explanation of requirement pattern

### System-Specific Documentation
- **[SCENE_INSTANTIATION_ARCHITECTURE.md](SCENE_INSTANTIATION_ARCHITECTURE.md)** - Scene lifecycle, marker resolution, dependent resources
- **[HEX_TRAVEL_SYSTEM.md](HEX_TRAVEL_SYSTEM.md)** - Travel mechanics, routes, hex grid
- **[WAYFARER_CORE_GAME_LOOP.md](WAYFARER_CORE_GAME_LOOP.md)** - Core gameplay loop
- **[multi-scene-npc-interaction.md](multi-scene-npc-interaction.md)** - Multiple concurrent scenes per NPC pattern
- **[INFINITE_A_STORY_ARCHITECTURE.md](INFINITE_A_STORY_ARCHITECTURE.md)** - Procedural A-story generation (designed, not implemented)

### Procedural Content Generation
- **[PROCEDURAL_CONTENT_GENERATION.md](PROCEDURAL_CONTENT_GENERATION.md)** - Comprehensive technical specification (1889 lines)
- **[PROCEDURAL_CONTENT_QUICK_REFERENCE.md](PROCEDURAL_CONTENT_QUICK_REFERENCE.md)** - Fast lookup cheat sheet

### Technical
- **[PACKAGE_COHESION.md](PACKAGE_COHESION.md)** - JSON package organization principles
- **[BLAZOR-PRERENDERING.md](BLAZOR-PRERENDERING.md)** - Blazor prerendering patterns

### For AI Agents
- **[CLAUDE.md](CLAUDE.md)** - Claude AI agent instructions (optimized for code generation)

### Historical/Analysis
- **[ANALYSIS/](ANALYSIS/)** - Point-in-time diagnostic analysis documents

## Quick Start

### Build
```bash
cd src
dotnet build
```

### Run
```bash
cd src
dotnet run
```

Application runs on http://localhost:5000 by default.

### Test
```bash
cd src
dotnet test
```

## Technology Stack

- **Framework:** .NET 8 / ASP.NET Core
- **UI:** Blazor Server (SSR)
- **Language:** C# 12
- **Architecture:** Domain-Driven Design, Facade Pattern, SPA

## Project Structure

```
Wayfarer/
├── src/
│   ├── Domain/              # Core entities (GameWorld, Scene, Situation, etc.)
│   ├── Subsystems/          # Domain services (SceneFacade, GameFacade, etc.)
│   ├── Content/             # JSON parsing, catalogues, generators
│   ├── Web/                 # Blazor UI, view models
│   └── wwwroot/             # CSS, assets
├── Content/
│   └── Core/                # JSON packages (scenes, NPCs, locations, etc.)
└── docs/                    # Documentation (this folder)
```

## Design Principles (Tier 1)

1. **No Soft-Locks:** Always forward progress from every game state
2. **Single Source of Truth:** One owner per entity type (HIGHLANDER principle)
3. **Playability Over Compilation:** Inaccessible content is worse than crashes
4. **Perfect Information (Strategic Layer):** All costs, requirements, rewards visible before selection

See [DESIGN_PHILOSOPHY.md](DESIGN_PHILOSOPHY.md) for complete principle hierarchy.

## Contributing

- Read [ARCHITECTURE.md](ARCHITECTURE.md) first (required)
- Follow [CODING_STANDARDS.md](CODING_STANDARDS.md) strictly
- Review [ARCHITECTURAL_PATTERNS.md](ARCHITECTURAL_PATTERNS.md) for reusable patterns
- Check [GLOSSARY.md](GLOSSARY.md) for canonical term definitions
- Consult [DESIGN_PHILOSOPHY.md](DESIGN_PHILOSOPHY.md) for conflict resolution

## License

[Add license information here]

## Contact

[Add contact information here]
