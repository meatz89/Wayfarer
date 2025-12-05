# Wayfarer Standard Operating Procedures (SOPs)

This directory contains Standard Operating Procedures for repeatable processes in the Wayfarer project.

## SOP Philosophy

SOPs are **step-by-step guides for consistent execution**. Unlike arc42 (architecture) or GDD (design), SOPs focus on HOW to perform specific tasks reproducibly.

| Principle | Application |
|-----------|-------------|
| **User-focused** | Written for the person executing the procedure |
| **Actionable steps** | Each step produces a verifiable outcome |
| **Troubleshooting included** | Common failures documented with solutions |
| **Version controlled** | Revision history tracks what changed and why |

**What belongs here:** Repeatable processes, human-AI workflows, testing procedures, optimization loops.

**What does NOT belong:** One-time tasks, architectural decisions (arc42), design rationale (GDD).

---

## SOP Structure Standard

Each SOP follows this format:

| Section | Purpose |
|---------|---------|
| **Header** | Title, purpose, scope, roles, prerequisites |
| **Procedure** | Numbered steps with expected outcomes |
| **Verification** | How to confirm success |
| **Troubleshooting** | Common issues and solutions |
| **Metrics** | How to measure effectiveness |
| **Revision History** | What changed and when |

---

## Documentation Index

| SOP | Purpose | Use When |
|-----|---------|----------|
| [01 AI Narrative Optimization](01_ai_narrative_optimization.md) | Iterative prompt improvement for AI-generated narrative | Optimizing AI output quality |

---

## Quick Start

### "I need to optimize AI narrative quality"
1. Read [01 AI Narrative Optimization](01_ai_narrative_optimization.md)
2. Follow the iterative workflow (Steps 1-5)
3. Record results in revision history

### "I want to create a new SOP"
1. Copy the structure from an existing SOP
2. Fill in all sections (empty sections are NOT acceptable in SOPs, unlike arc42)
3. Number it sequentially (02_xxx.md)
4. Add to the Documentation Index above

---

## Related Documentation

| Location | Content |
|----------|---------|
| [../arc42/](../arc42/) | Architecture documentation (WHY/WHAT) |
| [../gdd/](../gdd/) | Game design documentation (player experience) |
| [../CLAUDE.md](../CLAUDE.md) | AI assistant coding standards |

---

## SOP Format Types

SOPs can use different formats depending on complexity:

| Format | Best For | Example |
|--------|----------|---------|
| **Step-by-step** | Linear processes that must follow sequence | Build and test procedures |
| **Hierarchical** | Complex processes with nested sub-steps | Multi-phase optimization |
| **Checklist** | Tasks that can be done in any order | Pre-flight verification |
| **Flowchart** | Decision-branching processes | Troubleshooting guides |

Most Wayfarer SOPs use **hierarchical** format for clear organization with expandable detail.

---

## References

- [A Basic Guide to Writing Effective SOPs](https://www.thefdagroup.com/blog/a-basic-guide-to-writing-effective-standard-operating-procedures-sops)
- [SOP Formats: Types, Use Cases and Essential Elements](https://scribehow.com/library/sop-format)
- [How to Write Standard Operating Procedures](https://www.smartsheet.com/content/standard-operating-procedures-manual)
