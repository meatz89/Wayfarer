# DDR-007 JSON Content Violations Report

**Audit Date:** 2025-11-29
**Files Audited:** 21 JSON files
**Critical Violations:** 4 files

---

## Executive Summary

| Requirement | Status |
|-------------|--------|
| Mental Math Design (values under 30) | **FAIL** |
| Absolute Modifiers (no percentages) | **FAIL** |
| No Hidden Calculation | **FAIL** |

---

## CRITICAL VIOLATIONS

### 1. src/Content/Core/06_gameplay.json

**Violation Type:** Percentage-based modifiers + Large numeric values

| Field | Value | Problem |
|-------|-------|---------|
| `xpToNextLevel` | 100 | Far exceeds mental math threshold |
| `maxHunger` | 100 | Creates percentage-like 0-100 scale |
| `xpThresholds` | [10, 25, 50, 100] | Final value too large |
| `successBonus` | 5, 10, 15, 20 | **PERCENTAGE MODIFIERS** |

**Critical Example (Lines 91-93):**
```json
{
  "level": 2,
  "successBonus": 5,
  "description": "+5% success rate to all cards of this stat"
}
```

**Problem:** Fields named `successBonus` with descriptions containing "%" are explicit percentage modifiers, directly violating DDR-007's Absolute Modifiers principle.

---

### 2. src/Content/Core/19_achievements.json

**Violation Type:** Large numeric thresholds

| Achievement | Field | Value | Problem |
|-------------|-------|-------|---------|
| wealth_seeker | `coinsEarned` | 1000 | 125x starting coins (8) |
| merchant_prince | `coinsEarned` | 5000 | 625x starting coins |

**Problem:** Players cannot mentally calculate progress toward these goals. Starting with 8 coins, earning 1000 requires heavy calculation.

---

### 3. src/Content/Narratives/conversation_narratives.json

**Violation Type:** Implicit percentage scale

**Pattern:**
```json
{
  "rapportMin": -50,
  "rapportMax": 5
}
```

**Problem:** Rapport uses -50 to +50 range (100-point spread), functioning as an implicit percentage scale. This violates the spirit of Mental Math Design.

---

## ACCEPTABLE VALUES

The following content follows DDR-007:

| Category | Value Range | Status |
|----------|-------------|--------|
| Equipment costs | 5-30 coins | COMPLIANT |
| Challenge thresholds | 5-10 | COMPLIANT |
| Card depths | 1-8 | COMPLIANT |
| Focus/Stamina costs | 0-3 | COMPLIANT |
| Time costs | 1-2 | COMPLIANT |
| Stat requirements | 2-4 | COMPLIANT |

---

## Numeric Value Distribution

```
Equipment Costs:        5-30 coins       ACCEPTABLE
Character Starting:     8 coins          ACCEPTABLE
Experience Thresholds:  10-100 XP        CRITICAL (100 too large)
Stat Bonuses:          5, 10, 15, 20%    CRITICAL (percentage-based)
Rapport Scale:         -50 to +50        CRITICAL (implicit percentage)
Achievement Coins:     1000, 5000        CRITICAL (way too large)
```

---

## Recommendations

### Priority 1: CRITICAL

1. **06_gameplay.json - XP System**
   - Change `xpToNextLevel: 100` to `xpToNextLevel: 20`
   - Redesign `xpThresholds` to [5, 10, 15, 20]
   - **REMOVE percentage-based successBonus entirely**

2. **06_gameplay.json - Percentage Modifiers**
   - Replace `successBonus` with explicit stat unlocks or flat bonuses
   - Remove "%" from all descriptions

3. **19_achievements.json - Coin Thresholds**
   - Reduce `coinsEarned: 1000` to `coinsEarned: 80`
   - Reduce `coinsEarned: 5000` to `coinsEarned: 200`

4. **conversation_narratives.json - Rapport Scale**
   - Redesign from -50/+50 to categorical levels or -5/+5 range

---

## Summary

| File | Violations | Severity |
|------|------------|----------|
| 06_gameplay.json | Percentages, large XP | CRITICAL |
| 19_achievements.json | Large coin thresholds | CRITICAL |
| conversation_narratives.json | 100-point rapport scale | CRITICAL |
| Other files | None | COMPLIANT |

**Total: 3 files with critical DDR-007 violations in JSON content.**
