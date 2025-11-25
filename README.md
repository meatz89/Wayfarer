# Wayfarer

Low-fantasy tactical RPG with narrative-driven strategic layer and three parallel tactical challenge systems (Social, Mental, Physical).

## Project Overview

Wayfarer combines visual novel-style narrative choices with tactical card-based challenges. Players navigate a low-fantasy world, engage with NPCs, pursue an infinite procedurally-generated main story (A-story), and resolve conflicts through strategic and tactical gameplay.

## Core Architecture

- **Strategic Layer:** Scene → Situation → Choice flow with perfect information
- **Tactical Layer:** Three parallel challenge systems (Social/Mental/Physical) with hidden complexity
- **Infinite A-Story:** Procedurally-generated main narrative spine that never ends
- **Procedural Content:** Catalogue-based generation with categorical property scaling
- **Blazor SPA:** Server-side rendering with real-time state management

## Documentation

Wayfarer uses a dual documentation structure separating technical architecture from game design:

### Technical Architecture (Arc42 Template)

Located in `arc42/` subdirectory as numbered markdown files (01-12):

- **[arc42/01_introduction_and_goals.md](arc42/01_introduction_and_goals.md)** - System overview, quality goals, stakeholders
- **[arc42/02_constraints.md](arc42/02_constraints.md)** - Technical, organizational, and convention constraints
- **[arc42/03_context_and_scope.md](arc42/03_context_and_scope.md)** - System boundaries and gameplay loops
- **[arc42/04_solution_strategy.md](arc42/04_solution_strategy.md)** - Core architectural decisions and patterns
- **[arc42/05_building_block_view.md](arc42/05_building_block_view.md)** - Component structure and entity relationships
- **[arc42/06_runtime_view.md](arc42/06_runtime_view.md)** - Dynamic behavior and execution flows
- **[arc42/07_deployment_view.md](arc42/07_deployment_view.md)** - Deployment architecture and infrastructure
- **[arc42/08_crosscutting_concepts.md](arc42/08_crosscutting_concepts.md)** - Patterns spanning multiple components
- **[arc42/09_architecture_decisions.md](arc42/09_architecture_decisions.md)** - ADRs documenting key decisions
- **[arc42/10_quality_requirements.md](arc42/10_quality_requirements.md)** - Quality scenarios and criteria
- **[arc42/12_glossary.md](arc42/12_glossary.md)** - Technical term definitions

**Start here for development:** Read 01, 03, 05, 08, and 12 for foundational understanding.

### Game Design Document (GDD)

Primary GDD in `gdd/` subdirectory (~1,200 lines, vision-focused):

- **[gdd/00_one_pager.md](gdd/00_one_pager.md)** - Elevator pitch, design pillars, audience
- **[gdd/01_vision.md](gdd/01_vision.md)** - Core experience, anti-goals, pillars explained
- **[gdd/02_world.md](gdd/02_world.md)** - Setting, tone, spatial hierarchy
- **[gdd/03_core_loop.md](gdd/03_core_loop.md)** - SHORT/MEDIUM/LONG loops, session structure
- **[gdd/04_systems.md](gdd/04_systems.md)** - Resources, stats, challenges overview
- **[gdd/05_content.md](gdd/05_content.md)** - A/B/C stories, archetypes, four-choice
- **[gdd/06_balance.md](gdd/06_balance.md)** - Balance principles, difficulty scaling
- **[gdd/07_design_decisions.md](gdd/07_design_decisions.md)** - Key DDRs with rationale
- **[gdd/08_glossary.md](gdd/08_glossary.md)** - Essential term definitions

**Start here for design understanding:** Read 00, 01, and 03 for foundational philosophy.

### Game Design Reference Documentation

Detailed reference in `design/` subdirectory (~15,000 lines, exhaustive detail):

- **[design/07_content_generation.md](design/07_content_generation.md)** - 21 archetypes, categorical scaling
- **[design/08_balance_philosophy.md](design/08_balance_philosophy.md)** - Complete balance methodology
- **[design/12_design_glossary.md](design/12_design_glossary.md)** - 67 term definitions

**Use reference docs for:** Detailed mechanics, archetype catalog, balance tuning.

### Additional Documentation

- **[CLAUDE.md](CLAUDE.md)** - Constitutional process philosophy for AI agents working on codebase
- **[design/README.md](design/README.md)** - Game design documentation structure guide

### Documentation Philosophy

**Separation Principle:**
- **Technical docs** answer HOW (implementation patterns, component structure, runtime behavior)
- **Design docs** answer WHAT/WHY (player experience goals, strategic depth, design rationale)
- Both required together for complete understanding

**Source of Truth Hierarchy:**
1. **Code** - Ultimate ground truth (what actually runs)
2. **Documentation** - Authoritative explanation (what it means, why it exists)
3. **CLAUDE.md** - Process philosophy (how to work, how to think)

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
- **UI:** Blazor Server (ServerPrerendered mode)
- **Language:** C# 12
- **Architecture:** Domain-Driven Design, Facade Pattern, SPA

## Project Structure

```
Wayfarer/
├── src/
│   ├── Domain/              # Core entities (GameWorld, Scene, Situation, etc.)
│   ├── Services/            # Domain services (SceneFacade, GameFacade, etc.)
│   ├── Parsers/             # JSON parsing and catalogues
│   ├── Components/          # Blazor UI components
│   └── wwwroot/             # CSS, assets
├── tests/                   # Test projects
├── arc42/                   # Arc42 technical documentation (01-12)
├── gdd/                     # Game Design Document (~1,200 lines)
├── design/                  # Game design reference documentation (~15,000 lines)
├── CLAUDE.md                # Constitutional process philosophy
└── README.md                # This file
```

## Design Principles (Tier 1 - Non-Negotiable)

1. **No Soft-Locks:** Always forward progress from every game state
2. **Single Source of Truth:** One owner per entity type (HIGHLANDER principle)
3. **Playability Over Compilation:** Inaccessible content is worse than crashes
4. **Perfect Information (Strategic Layer):** All costs, requirements, rewards visible before selection

See [gdd/01_vision.md](gdd/01_vision.md) for design pillars and philosophy.

## Architectural Highlights

### HIGHLANDER Pattern
"There can be only ONE." One concept, one representation. No redundant storage, no duplicate paths.

### Catalogue Pattern
Parse-time translation from categorical properties to concrete values. JSON has categories, runtime has concrete types.

### Three-Tier Timing Model
Templates (parse-time) → Scenes/Situations (spawn-time) → Actions (query-time). Lazy instantiation reduces memory.

### Requirement Inversion
Content exists from game start, requirements filter visibility. Perfect information enabled architecturally.

See arc42/08_crosscutting_concepts.md for complete pattern catalog.

## Contributing

**Required reading before contributing:**
1. **arc42/01_introduction_and_goals.md** - Understand system purpose and quality goals
2. **arc42/03_context_and_scope.md** - Understand system boundaries
3. **arc42/05_building_block_view.md** - Understand component structure
4. **arc42/08_crosscutting_concepts.md** - Understand architectural patterns
5. **arc42/12_glossary.md** - Learn technical terminology
6. **gdd/01_vision.md** - Understand design pillars and philosophy
7. **gdd/08_glossary.md** - Learn game design terminology
8. **CLAUDE.md** - Understand development process and mandatory protocols

**Coding Standards:**
- Follow arc42/02_constraints.md strictly (type restrictions, lambda rules, Blazor patterns)
- Use HIGHLANDER pattern (one concept, one representation)
- Propagate async throughout call stack
- Semantic honesty (method names match return types)
- No backwards compatibility (clean breaks when refactoring)

## License

[Add license information here]

## Contact

[Add contact information here]
