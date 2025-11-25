# Wayfarer Architecture Documentation (arc42)

This directory contains architecture documentation following the [arc42 template](https://arc42.org).

## Documentation Structure

| Section | Status | Description |
|---------|--------|-------------|
| [01 Introduction and Goals](01_introduction_and_goals.md) | Complete | Quality goals, stakeholders |
| [02 Constraints](02_constraints.md) | Complete | Technical, organizational constraints |
| [03 Context and Scope](03_context_and_scope.md) | Complete | System boundary (Mermaid diagrams) |
| [04 Solution Strategy](04_solution_strategy.md) | Complete | Fundamental decisions (narrative) |
| [05 Building Block View](05_building_block_view.md) | Complete | Static decomposition (Level 1-2) |
| [06 Runtime View](06_runtime_view.md) | Complete | Key runtime scenarios (sequence diagrams) |
| [07 Deployment View](07_deployment_view.md) | Stub | Infrastructure (to be documented) |
| [08 Crosscutting Concepts](08_crosscutting_concepts.md) | Complete | 9 patterns including Dual-Tier Actions |
| [09 Architecture Decisions](09_architecture_decisions.md) | Complete | 10 ADRs with rationale |
| [10 Quality Requirements](10_quality_requirements.md) | Complete | Quality tree and scenarios |
| [11 Risks and Technical Debt](11_risks_technical_debt.md) | Complete | 5 risks, 4 debt items |
| [12 Glossary](12_glossary.md) | Complete | 50+ domain and technical terms |

## Quick Start

- **New to codebase:** Start with [01 Introduction](01_introduction_and_goals.md) then [04 Solution Strategy](04_solution_strategy.md)
- **Understanding structure:** See [05 Building Block View](05_building_block_view.md)
- **Looking up terms:** See [12 Glossary](12_glossary.md)
- **Why decisions were made:** See [09 Architecture Decisions](09_architecture_decisions.md)
- **Critical patterns:** See [08 Crosscutting Concepts](08_crosscutting_concepts.md) Section 8.8 (Dual-Tier Actions)

## Related Documentation

- [../design/](../design/) — Game design documentation (player experience, not technical)
- [../CLAUDE.md](../CLAUDE.md) — AI assistant coding standards and enforcement
- [../DUAL_TIER_ACTION_ARCHITECTURE.md](../DUAL_TIER_ACTION_ARCHITECTURE.md) — Detailed action architecture reference
