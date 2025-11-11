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

Located in root directory as numbered markdown files (01-12):

- **[01_introduction_and_goals.md](01_introduction_and_goals.md)** - System overview, quality goals, stakeholders
- **[02_constraints.md](02_constraints.md)** - Technical, organizational, and convention constraints
- **[03_context_and_scope.md](03_context_and_scope.md)** - System boundaries and gameplay loops
- **[04_solution_strategy.md](04_solution_strategy.md)** - Core architectural decisions and patterns
- **[05_building_block_view.md](05_building_block_view.md)** - Component structure and entity relationships
- **[06_runtime_view.md](06_runtime_view.md)** - Dynamic behavior and execution flows
- **[07_deployment_view.md](07_deployment_view.md)** - Deployment architecture and infrastructure
- **[08_crosscutting_concepts.md](08_crosscutting_concepts.md)** - Patterns spanning multiple components
- **[09_architecture_decisions.md](09_architecture_decisions.md)** - ADRs documenting key decisions
- **[10_quality_requirements.md](10_quality_requirements.md)** - Quality scenarios and criteria
- **[11_risks_and_technical_debt.md](11_risks_and_technical_debt.md)** - Known risks and technical debt
- **[12_glossary.md](12_glossary.md)** - Technical term definitions

**Start here for development:** Read 01, 03, 05, 08, and 12 for foundational understanding.

### Game Design Documentation

Located in `design/` subdirectory as parallel numbered files (01-12):

- **[design/01_design_vision.md](design/01_design_vision.md)** - Core philosophy and design principles
- **[design/02_core_gameplay_loops.md](design/02_core_gameplay_loops.md)** - Three-tier loop structure (SHORT/MEDIUM/LONG)
- **[design/03_progression_systems.md](design/03_progression_systems.md)** - Stats, NPC bonds, resource layers
- **[design/04_challenge_mechanics.md](design/04_challenge_mechanics.md)** - Tactical systems (Mental/Physical/Social)
- **[design/05_resource_economy.md](design/05_resource_economy.md)** - Resource competition and trade-offs
- **[design/06_narrative_design.md](design/06_narrative_design.md)** - Infinite A-story architecture and integration
- **[design/07_content_generation.md](design/07_content_generation.md)** - Archetype systems and procedural generation
- **[design/08_balance_philosophy.md](design/08_balance_philosophy.md)** - Balance principles and tuning
- **[design/09_design_patterns.md](design/09_design_patterns.md)** - Game design patterns and anti-patterns
- **[design/10_tutorial_design.md](design/10_tutorial_design.md)** - Tutorial philosophy and mechanics teaching
- **[design/11_design_decisions.md](design/11_design_decisions.md)** - DDRs documenting design choices
- **[design/12_design_glossary.md](design/12_design_glossary.md)** - Game design term definitions

**Start here for design understanding:** Read design/01, design/02, design/06, and design/12.

### Additional Documentation

- **[CLAUDE.md](CLAUDE.md)** - Constitutional process philosophy for AI agents working on codebase
- **[IMPLEMENTATION_STATUS.md](IMPLEMENTATION_STATUS.md)** - Living status matrix tracking feature implementation
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
├── Content/
│   └── Core/                # JSON content packages
├── design/                  # Game design documentation (12 files)
├── 01-12.md                 # Arc42 technical documentation (12 files)
├── CLAUDE.md                # Constitutional process philosophy
├── IMPLEMENTATION_STATUS.md # Feature implementation matrix
└── README.md                # This file
```

## Design Principles (Tier 1 - Non-Negotiable)

1. **No Soft-Locks:** Always forward progress from every game state
2. **Single Source of Truth:** One owner per entity type (HIGHLANDER principle)
3. **Playability Over Compilation:** Inaccessible content is worse than crashes
4. **Perfect Information (Strategic Layer):** All costs, requirements, rewards visible before selection

See design/01_design_vision.md section 1.9 for complete three-tier principle hierarchy.

## Architectural Highlights

### HIGHLANDER Pattern
"There can be only ONE." One concept, one representation. No redundant storage, no duplicate paths.

### Catalogue Pattern
Parse-time translation from categorical properties to concrete values. JSON has categories, runtime has concrete types.

### Three-Tier Timing Model
Templates (parse-time) → Scenes/Situations (spawn-time) → Actions (query-time). Lazy instantiation reduces memory.

### Requirement Inversion
Content exists from game start, requirements filter visibility. Perfect information enabled architecturally.

See 08_crosscutting_concepts.md for complete pattern catalog.

## Contributing

**Required reading before contributing:**
1. **01_introduction_and_goals.md** - Understand system purpose and quality goals
2. **03_context_and_scope.md** - Understand system boundaries
3. **05_building_block_view.md** - Understand component structure
4. **08_crosscutting_concepts.md** - Understand architectural patterns
5. **12_glossary.md** - Learn technical terminology
6. **design/01_design_vision.md** - Understand design philosophy
7. **design/12_design_glossary.md** - Learn game design terminology
8. **CLAUDE.md** - Understand development process and mandatory protocols

**Coding Standards:**
- Follow 02_constraints.md strictly (type restrictions, lambda rules, Blazor patterns)
- Use HIGHLANDER pattern (one concept, one representation)
- Propagate async throughout call stack
- Semantic honesty (method names match return types)
- No backwards compatibility (clean breaks when refactoring)

## License

[Add license information here]

## Contact

[Add contact information here]
