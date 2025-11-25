# 8. Essential Glossary

## Why This Document Exists

This glossary defines essential game design terms for quick reference. For comprehensive definitions of all 67+ terms, see [design/12_design_glossary.md](../design/12_design_glossary.md).

---

## Core Concepts

### A-Story
The infinite main narrative spine. Phase 1 (A1-A10) is authored tutorial. Phase 2 (A11+) is procedurally generated continuation that never ends.

### Archetype
Reusable mechanical pattern defining situation structure (choice count, path types, cost formulas) independent of narrative content. Same archetype + different entity properties = infinite variations.

### Atmospheric Action Layer
Always-available core actions (Travel, Work, Rest, Move) forming persistent gameplay baseline. Scene-based actions layer on top, never replacing atmospheric scaffolding.

### Build
Player's character specialization emerging from stat allocation choices. Not a pre-selected class—identity emerges from resource investment decisions.

### Four-Choice Archetype
The pattern guaranteeing every A-story situation offers four path types: stat-gated (free for specialists), resource (costs coins/items), challenge (skill test), and fallback (always available).

---

## Gameplay Terms

### Impossible Choice
Core design principle. Every decision forces trade-offs between multiple valid alternatives. No optimal path exists—only the path you choose.

### Perfect Information
Design principle. All costs, requirements, and rewards visible before selection. Strategic layer has no hidden gotchas.

### Scene-Situation-Choice Flow
Narrative structure. Scene (container) → Situation (decision point) → Choice (four paths). All content follows this hierarchy.

### Strategic Layer
The WHAT and WHERE layer. Player sees all information. Decides WHETHER to attempt. Entities are persistent.

### Tactical Layer
The HOW layer. Card-based challenges. Hidden complexity (draw order). Tests execution skill. Sessions are temporary.

---

## Resources

### Universal Resources
Resources competing across all systems: Time (blocks per day), Coins (currency), Focus (Mental pool), Stamina (Physical pool), Resolve (Social pool), Health (survival threshold).

### Tactical Resources
Resources existing only within challenge sessions: Builder resources (Momentum/Progress/Breakthrough), Session resources (Initiative/Attention/Exertion), Threshold resources (Doubt/Exposure/Danger).

---

## The Five Stats

| Stat | Domain | Governs |
|------|--------|---------|
| **Insight** | Mental | Investigation, observation, puzzle-solving |
| **Cunning** | Mental | Deception, misdirection, reading situations |
| **Rapport** | Social | Building trust, friendly persuasion |
| **Diplomacy** | Social | Formal negotiation, authority appeal |
| **Authority** | Physical | Intimidation, command presence |

---

## Challenge Systems

### Mental Challenge
Card-based investigation. Build Progress through Leads and Details. Session resource: Attention. Threshold: Exposure.

### Physical Challenge
Card-based obstacle resolution. Overcome through Exertion. Session resource: Exertion. Threshold: Danger.

### Social Challenge
Card-based persuasion. Build Momentum through dialogue. Session resource: Initiative. Threshold: Doubt.

---

## Content Terms

### B-Story
Major side content. 3-8 scenes per storyline. Optional, player-initiated, substantial character arcs.

### C-Story
Minor side content. 1-2 scenes. World flavor, quick opportunities, organic encounters.

### Categorical Property
Entity attribute that scales archetype costs/rewards. Examples: NPCDemeanor, Quality, PowerDynamic. Enables contextual difficulty without explicit level gates.

### Frieren Principle
Design philosophy: The game never ends. The journey is the point, not arrival. Success measured by journey quality, not reaching destination.

---

## Cross-References

- **Complete Glossary**: See [design/12_design_glossary.md](../design/12_design_glossary.md) for all 67+ terms
- **Technical Terms**: See [arc42/12_glossary.md](../arc42/12_glossary.md) for implementation terminology
