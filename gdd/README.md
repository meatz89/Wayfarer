# Wayfarer Game Design Document

## Document Philosophy

This GDD follows modern agile documentation principles:
- **Vision-first hierarchy**: Design pillars at the top, everything flows from them
- **Economy of documentation**: Say it once, say it clearly, link to details
- **Rationale over facts**: Explain WHY decisions create intended experiences
- **Searchable and scannable**: Clear headings, short sections, good cross-references

For technical architecture, see [arc42/](../arc42/) subdirectory.

---

## Quick Navigation

| Document | Purpose | Read When |
|----------|---------|-----------|
| [00_one_pager.md](00_one_pager.md) | Elevator pitch, pillars, audience | First read, stakeholder intro |
| [01_vision.md](01_vision.md) | Core experience, anti-goals, pillars explained | Understanding design philosophy |
| [02_world.md](02_world.md) | Setting, tone, spatial hierarchy | World-building, content creation |
| [03_core_loop.md](03_core_loop.md) | SHORT/MEDIUM/LONG loops, session structure | Gameplay flow understanding |
| [04_systems.md](04_systems.md) | Resources, stats, challenges overview | Systems interaction |
| [05_content.md](05_content.md) | A/B/C stories, archetypes, four-choice | Content creation |
| [06_balance.md](06_balance.md) | Balance principles, difficulty scaling | Tuning, balance decisions |
| [07_design_decisions.md](07_design_decisions.md) | Key DDRs with rationale | Understanding WHY choices were made |
| [08_glossary.md](08_glossary.md) | Essential term definitions | Quick reference |

---

## Reading Paths

### "I'm new to Wayfarer"
1. Start with [00_one_pager.md](00_one_pager.md) for overview
2. Read [01_vision.md](01_vision.md) for design philosophy
3. Skim [03_core_loop.md](03_core_loop.md) for gameplay understanding

### "I need to create content"
1. Read [05_content.md](05_content.md) for structure
2. Reference [08_glossary.md](08_glossary.md) for terms
3. See [arc42/08_crosscutting_concepts.md](../arc42/08_crosscutting_concepts.md) for archetype patterns

### "I need to balance a situation"
1. Read [06_balance.md](06_balance.md) for principles
2. See [06_balance.md §6.4](06_balance.md#64-the-sir-brante-rhythm-reference-model) for detailed methodology

### "I need to understand a design decision"
1. Check [07_design_decisions.md](07_design_decisions.md) for key DDRs
2. See [arc42/09_architecture_decisions.md](../arc42/09_architecture_decisions.md) for technical ADRs

---

## GDD vs Technical Documentation

**This GDD (gdd/ folder):**
- WHAT the game is and WHY it works
- Concise, scannable, vision-focused
- Start here for understanding

**Technical Documentation (arc42/ folder):**
- HOW systems are implemented technically
- Architecture patterns, decisions, constraints
- Go here for implementation details

---

## Cross-Reference to Technical Docs

| Design Concept | Technical Implementation |
|----------------|-------------------------|
| Scene-Situation-Choice | [arc42/05_building_block_view.md](../arc42/05_building_block_view.md) |
| Atmospheric Action Layer | [arc42/08_crosscutting_concepts.md](../arc42/08_crosscutting_concepts.md) §8.8 |
| Categorical Property Scaling | [arc42/08_crosscutting_concepts.md](../arc42/08_crosscutting_concepts.md) §8.2-8.3 |
| Entity Ownership | [arc42/08_crosscutting_concepts.md](../arc42/08_crosscutting_concepts.md) §8.9 |
