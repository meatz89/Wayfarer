# DDR-007 Catalogue Compliance Report

**Audit Date:** 2025-11-29
**Catalogues Audited:** 18 classes
**Compliance Rate:** 14/18 (78%)
**Critical Violations:** 4 classes

---

## Executive Summary

The codebase has strong additive design in core game mechanics (cards, challenges, actions), but violates DDR-007 in **scaling systems** and **personality modifiers**.

---

## COMPLIANT CATALOGUES (14)

### Fully Additive Design

| Catalogue | Translation Pattern | Values | Status |
|-----------|---------------------|--------|--------|
| ConversationCatalog | Complexity → costs | 0, 2, 5, 8 | COMPLIANT |
| DeliveryJobCatalog | Segments → payment | 17-30 coins | COMPLIANT |
| LocationActionCatalog | Properties → availability | 0, 1, 8 | COMPLIANT |
| MentalCardEffectCatalog | Depth → effects | 2-13 ±1 | COMPLIANT |
| PhysicalCardEffectCatalog | Depth → effects | 2-13 ±1 | COMPLIANT |
| PlayerActionCatalog | Action → costs | 0, 2 | COMPLIANT |
| SituationArchetypeCatalog | Archetype → structure | 3, 5, 15 | COMPLIANT |
| SocialCardEffectCatalog | Stat × Depth → effects | 2-12 | COMPLIANT |
| StateClearConditionsCatalog | Category → flags | N/A | COMPLIANT |
| DependentResourceCatalog | Activity → filter | N/A | COMPLIANT |
| DifficultyModifier | Type → adjustment | -3, -2 | COMPLIANT |
| MarketPriceModifier | Location → override | Absolute | COMPLIANT |
| SkillModifier | Skill → adjustment | Integer | COMPLIANT |

### Exemplary: SocialCardEffectCatalog

This catalogue demonstrates DDR-007 compliance perfectly:

```
Depth 1-2: +2 Momentum
Depth 3-4: +5 Momentum
Depth 5-6: +8 Momentum
Depth 7-8: +12 Momentum
```

All values are explicit integers with tier-based progression. No percentages or multipliers.

---

## VIOLATIONS (4)

### VIOLATION #1: ObservationCatalog

**File:** `src/Content/Catalogs/ObservationCatalog.cs` (Lines 56-68)
**Severity:** HIGH

```csharp
public static int GetFindChanceModifier(ExaminationDepth depth)
{
    return depth switch
    {
        ExaminationDepth.Glance => 0,       // No bonus
        ExaminationDepth.Careful => 10,     // +10%
        ExaminationDepth.Exhaustive => 25,  // +25%
        ExaminationDepth.Insight => 35,     // +35%
        _ => 0
    };
}
```

**Problem:** Returns percentages (10%, 25%, 35%) which are multiplicative modifiers.

**Fix:** Replace with flat additive bonuses (+2, +5, +8).

---

### VIOLATION #2: EmergencyCatalog

**File:** `src/Content/Catalogs/EmergencyCatalog.cs` (Lines 44-71)
**Severity:** CRITICAL

```csharp
public static int GetCostScalingFactor(EmergencySeverity severity)
{
    return severity switch
    {
        EmergencySeverity.Minor => 10000,     // 1.0x
        EmergencySeverity.Moderate => 15000,  // 1.5x
        EmergencySeverity.Urgent => 20000,    // 2.0x
        EmergencySeverity.Critical => 30000,  // 3.0x
        _ => 10000
    };
}
```

**Problem:** Returns basis points (10000 = 1.0x) for multiplicative scaling.

**Fix:** Replace with flat cost adjustments (+0, +5, +10, +20 coins).

---

### VIOLATION #3: PersonalityModifier

**File:** `src/GameState/PersonalityModifier.cs` (Lines 20-56)
**Severity:** CRITICAL

```csharp
case PersonalityType.DEVOTED:
    modifier.Type = PersonalityModifierType.MomentumLossDoubled;
    modifier.Parameters["multiplier"] = 2;  // DOUBLE momentum losses
    break;
```

**Problem:** Uses `multiplier = 2` for momentum loss doubling.

**Fix:** Replace with additive penalty: `additionalLoss = 2`.

---

### VIOLATION #4: CardEffectFormula

**File:** `src/GameState/Cards/CardEffectFormula.cs` (Lines 41-43, 103-134)
**Severity:** HIGH

```csharp
public decimal ScalingMultiplier { get; set; } = 1.0m;

private int CalculateScaling(SocialSession session)
{
    int scaled = (int)(sourceValue * ScalingMultiplier);
    return scaled;
}
```

**Problem:** `EffectFormulaType.Scaling` uses multiplicative scaling.

**Note:** This appears to be defined but not currently used in active card definitions.

**Fix:** Remove or replace with tier-based additive bonuses.

---

## Violation Severity Ranking

| Severity | File | Issue |
|----------|------|-------|
| CRITICAL | EmergencyCatalog | Basis point cost/reward scaling |
| CRITICAL | PersonalityModifier | Momentum loss doubling (2x) |
| HIGH | ObservationCatalog | Percentage item find modifiers |
| HIGH | CardEffectFormula | Scaling formula with multiplier |

---

## Recommended Fix Priority

### Phase 1: Core Systems
1. **EmergencyCatalog** - Replace basis points with flat adjustments
2. **PersonalityModifier** - Replace multiplier with additive loss

### Phase 2: Challenge System
3. **ObservationCatalog** - Replace percentages with flat bonuses

### Phase 3: Preventive
4. **CardEffectFormula** - Remove or replace Scaling formula type

---

## Compliance Pattern Examples

**WRONG (Current - Multiplicative):**
```csharp
return severity switch
{
    Minor => 10000,    // 1.0x
    Moderate => 15000, // 1.5x
};
```

**CORRECT (DDR-007 Compliant):**
```csharp
return severity switch
{
    Minor => 0,    // No adjustment
    Moderate => 5, // +5 coins
};
```

---

## Summary

| Category | Count | Compliance |
|----------|-------|------------|
| Fully Compliant | 14 | 78% |
| Violations | 4 | 22% |

**Key Finding:** Card systems (Social, Mental, Physical) are exemplary. Scaling systems (Emergency, Observation) need remediation.
