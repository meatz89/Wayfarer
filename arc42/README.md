# Wayfarer Architecture Documentation (arc42)

This directory contains architecture documentation following the [arc42 template](https://arc42.org).

## Arc42 Philosophy

Arc42 is a **cabinet, not a form**. We use it as organized drawers for architecture knowledge, not as a template to fill completely.

| Principle | Application |
|-----------|-------------|
| **"Dare to leave gaps"** | Empty sections are intentional. We document what matters. |
| **Concepts over implementation** | Describe patterns and WHY, not property names or formulas. |
| **5-15 elements per diagram** | If more elements needed, wrong abstraction level. |
| **Stakeholder-driven** | Document what developers need to understand the system. |

**What belongs here:** Architectural decisions, crosscutting patterns, trade-off rationale.

**What does NOT belong:** Code blocks, specific property names, concrete numbers, enum lists.

**Reference:** [arc42 FAQ](https://leanpub.com/arc42-faq/read), [arc42 in Practice](https://leanpub.com/arc42inpractice/read)

---

## Documentation Structure

| Section | Description |
|---------|-------------|
| [01 Introduction and Goals](01_introduction_and_goals.md) | Quality goals, stakeholders |
| [02 Constraints](02_constraints.md) | Technical, organizational constraints |
| [03 Context and Scope](03_context_and_scope.md) | System boundary |
| [04 Solution Strategy](04_solution_strategy.md) | Fundamental decisions |
| [05 Building Block View](05_building_block_view.md) | Static decomposition |
| [06 Runtime View](06_runtime_view.md) | Key runtime scenarios |
| [07 Deployment View](07_deployment_view.md) | Infrastructure |
| [08 Crosscutting Concepts](08_crosscutting_concepts.md) | Patterns spanning components |
| [09 Architecture Decisions](09_architecture_decisions.md) | ADRs with rationale |
| [10 Quality Requirements](10_quality_requirements.md) | Quality tree and scenarios |
| [11 Risks and Technical Debt](11_risks_technical_debt.md) | Known risks and debt |
| [12 Glossary](12_glossary.md) | Domain and technical terms |

## Quick Start

- **New to codebase:** Start with [01 Introduction](01_introduction_and_goals.md) then [04 Solution Strategy](04_solution_strategy.md)
- **Understanding structure:** See [05 Building Block View](05_building_block_view.md)
- **Looking up terms:** See [12 Glossary](12_glossary.md)
- **Why decisions were made:** See [09 Architecture Decisions](09_architecture_decisions.md)
- **Critical patterns:** See [08 Crosscutting Concepts](08_crosscutting_concepts.md) Section 8.8 (Dual-Tier Actions)

## Related Documentation

- [../gdd/](../gdd/) — Game design documentation (player experience, not technical)
- [../CLAUDE.md](../CLAUDE.md) — AI assistant coding standards and enforcement
- [../DUAL_TIER_ACTION_ARCHITECTURE.md](../DUAL_TIER_ACTION_ARCHITECTURE.md) — Detailed action architecture reference
