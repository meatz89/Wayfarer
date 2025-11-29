# DDR-007 Documentation Violations Report

**Audit Date:** 2025-11-29
**Total Violations:** 3
**Scope:** All markdown files (*.md)

---

## Executive Summary

The documentation contains **3 violations** of DDR-007 in GDD documents. These contradict the explicitly stated principle that rejects multipliers in favor of absolute modifiers.

---

## Verified COMPLIANT Documents

### arc42/04_solution_strategy.md (Recently Fixed)

**Status:** COMPLIANT
**Line 27:** "Categories translate to absolute adjustments that stack predictably—no compounding multipliers that obscure final values."

This document was fixed in commit 8cfa9cc and now correctly describes the design intent.

---

## VIOLATIONS

### VIOLATION #1: gdd/05_content.md (Lines 109-113)

**Severity:** HIGH
**Principle Violated:** Absolute Modifiers

**Violating Text:**
```markdown
**NPCDemeanor:** Friendly (0.8x costs) → Neutral (1.0x) → Suspicious (1.2x) → Hostile (1.5x)

**Quality:** Poor (0.7x) → Standard (1.0x) → Fine (1.3x) → Exceptional (1.6x)

**PowerDynamic:** Subordinate (0.8x) → Equal (1.0x) → Superior (1.2x) → Authority (1.5x)
```

**Problem:** Uses multiplier notation (0.8x, 1.2x, 1.5x, etc.) to describe categorical property scaling. This directly contradicts DDR-007's statement: "Bonuses apply as flat additions, never multipliers."

**Recommendation:** Rewrite using absolute modifier language:
```markdown
**NPCDemeanor:** Friendly (-2 cost) → Neutral (base) → Suspicious (+2 cost) → Hostile (+5 cost)
```

---

### VIOLATION #2: gdd/05_content.md (Lines 119-120)

**Severity:** HIGH
**Principle Violated:** Absolute Modifiers

**Violating Text:**
```markdown
2. Catalog translates properties to multipliers
3. Archetype applies multipliers to base costs
```

**Problem:** Explicitly describes the system as using "multipliers" in the content pipeline.

**Recommendation:** Replace with:
```markdown
2. Catalog translates properties to absolute adjustments
3. Archetype applies adjustments to base costs
```

---

### VIOLATION #3: gdd/06_balance.md (Line 81)

**Severity:** MEDIUM
**Principle Violated:** Absolute Modifiers

**Violating Text:**
```markdown
These multiply base archetype costs, creating contextual difficulty...
```

**Problem:** Uses "multiply" verb to describe how categorical properties affect costs.

**Recommendation:** Replace with:
```markdown
These adjust base archetype costs, creating contextual difficulty...
```

---

## Documents Needing DDR-007 Cross-References

The following documents discuss numeric design but don't reference DDR-007:

| Document | Issue | Recommendation |
|----------|-------|----------------|
| gdd/01_vision.md | Perfect Information pillar doesn't mention DDR-007 | Add sentence linking to DDR-007 |
| gdd/06_balance.md | No "Numeric Foundation" section | Add subsection explaining DDR-007 role |
| arc42/04_solution_strategy.md | No explicit DDR-007 reference | Add one sentence after line 27 |
| arc42/08_crosscutting_concepts.md | Catalogue Pattern lacks numeric context | Add DDR-007 reference to §8.2 |

---

## Summary

| Document | Violations | Status |
|----------|------------|--------|
| gdd/05_content.md | 2 | NEEDS FIX |
| gdd/06_balance.md | 1 | NEEDS FIX |
| arc42/04_solution_strategy.md | 0 | COMPLIANT |
| gdd/07_design_decisions.md | 0 | COMPLIANT (DDR-007 source) |
| gdd/08_glossary.md | 0 | COMPLIANT |

**Action Required:** Fix 3 documentation violations and add 4 cross-references.
