# DDR-007 Code Violations Report

**Audit Date:** 2025-11-29
**Total Violations:** 68
**Scope:** All C# code in `src/` directory

---

## Executive Summary

| Principle | Violations | Severity |
|-----------|-----------|----------|
| **Absolute Modifiers** (multiplicative bonuses) | 42 | CRITICAL |
| **Deterministic Arithmetic** (Random class usage) | 8 | CRITICAL |
| **Mental Math Design** (float/double/large numbers) | 18 | MEDIUM |

---

## 1. ABSOLUTE MODIFIERS VIOLATIONS (CRITICAL)

### 1.1 Decimal Multipliers in SituationArchetypeCatalog

**File:** `src/Content/Catalogs/SituationArchetypeCatalog.cs`

| Lines | Code Pattern | Multiplier |
|-------|--------------|------------|
| 793 | `(int)(archetype.StatThreshold * 0.6)` | 0.6x (Dominant) |
| 795 | `(int)(archetype.StatThreshold * 1.4)` | 1.4x (Submissive) |
| 802 | `(int)(archetype.CoinCost * 0.6)` | 0.6x (Basic) |
| 804 | `(int)(archetype.CoinCost * 1.6)` | 1.6x (Premium) |
| 805 | `(int)(archetype.CoinCost * 2.4)` | 2.4x (Luxury) |
| 812 | `(int)(scaledStatThreshold * 1.4)` | 1.4x (Hostile) |
| 816 | `(int)(scaledStatThreshold * 0.6)` | 0.6x (Friendly) |

**Impact:** Core cost scaling uses multiplicative modifiers that compound unpredictably.

### 1.2 Basis Points System (10000 = 1.0x)

**Affected Files:**
- `src/GameState/StandingObligation.cs` - ScalingFactorBasisPoints
- `src/Subsystems/Token/TokenEffectProcessor.cs` - Modifier multiplication
- `src/Subsystems/Market/PriceManager.cs` - Supply/Demand/Location modifiers
- `src/Subsystems/Token/RelationshipTracker.cs` - Decay calculation
- `src/GameState/TokenMechanicsManager.cs` - Modifier chaining

**Pattern:**
```csharp
// Multiply modifiers (e.g., 15000 * 12000 / 10000 = 18000 for 1.5x * 1.2x = 1.8x)
activeModifiers[modifier.Key] = activeModifiers[modifier.Key] * modifier.Value / 10000;
```

**Impact:** Entire subsystem built on multiplicative modifier architecture.

### 1.3 Percentage-Based Discounts

**File:** `src/Subsystems/Market/PriceManager.cs:454-460`
```csharp
int discountPercent = 100;
if (quantity >= 10) discountPercent = 90;  // 10% discount
else if (quantity >= 5) discountPercent = 95;  // 5% discount
return singlePrice * quantity * discountPercent / 100;
```

### 1.4 Personality Multipliers

**File:** `src/Subsystems/Social/PersonalityRuleEnforcer.cs:80-87`
```csharp
case PersonalityModifierType.MomentumLossDoubled:
    modifiedChange = baseMomentumChange * multiplier;  // 2x multiplier
```

### 1.5 Other Multiplier Locations

| File | Line | Multiplier |
|------|------|------------|
| `MarketItem.cs` | 15 | `Price * 0.7` (70% sell price) |
| `HexRouteGenerator.cs` | 377 | `timeSegments * 0.3` |
| `HexMapContent.razor.cs` | 85-89 | `0.75`, `0.5` (UI layout) |

---

## 2. DETERMINISTIC ARITHMETIC VIOLATIONS (CRITICAL)

### Random Class Usage in Strategic Systems

| File | Line | Usage |
|------|------|-------|
| `DialogueGenerationService.cs` | 8 | `new Random()` for dialogue selection |
| `TravelManager.cs` | 8 | `new Random()` for travel events |
| `HexRouteGenerator.cs` | 284 | `new Random()` for route generation |
| `ObservationFacade.cs` | 25 | `new Random()` for observations |
| `SkeletonGenerator.cs` | 9 | `static Random` for content generation |
| `Pile.cs` | 66 | `new Random()` for deck shuffling |
| `ObservationTemplates.cs` | 12, 27 | `new Random()` for template selection |
| `NarrativeService.cs` | 13 | `new Random()` for narrative generation |

**Impact:** Strategic outcomes unpredictable, violates Perfect Information pillar.

---

## 3. MENTAL MATH DESIGN VIOLATIONS (MEDIUM)

### 3.1 Float/Double Usage for Percentages

| File | Line | Pattern |
|------|------|---------|
| `StreamingContentState.cs` | 7 | `float StreamProgress` |
| `MentalContent.razor.cs` | 101, 107, 309, 705 | `(double)` casts for percentage |
| `InventoryContent.razor.cs` | 43-46 | `double GetWeightPercent()` |
| `ResourceBar.razor.cs` | 16-21 | `double PercentageWidth` |
| `DiscoveryJournal.razor.cs` | 49-52, 136-142 | `double` percentages |
| `PhysicalContent.razor.cs` | 111-117, 345, 787 | `(double)` casts |
| `ObligationModels.cs` | 113 | `double UtilizationPercentage` |

### 3.2 Decimal ScalingMultiplier

**File:** `src/GameState/Cards/CardEffectFormula.cs:43`
```csharp
public decimal ScalingMultiplier { get; set; } = 1.0m;
```

### 3.3 Large Numeric Literals

| File | Value | Purpose |
|------|-------|---------|
| `StandingObligation.cs` | 1000000 | MaxValueBasisPoints (100.0x cap) |
| Various | 10000 | Basis point constant (60+ uses) |

---

## 4. RECOMMENDED FIXES

### Priority 0: Immediate (Core Systems)

1. **SituationArchetypeCatalog** - Replace all `* 0.6`, `* 1.4`, etc. with absolute adjustments:
   ```csharp
   // WRONG: (int)(archetype.StatThreshold * 0.6)
   // RIGHT: archetype.StatThreshold - 2
   ```

2. **Remove all Random instances** - Replace with deterministic systems or move to tactical layer only

3. **Replace basis points system** - Convert to explicit integer modifiers

### Priority 1: High (Subsystems)

4. **PriceManager** - Replace percentage discounts with flat reductions
5. **PersonalityRuleEnforcer** - Replace "doubled" with "+N additional"
6. **TokenEffectProcessor** - Replace multiplicative modifier chaining

### Priority 2: Medium (UI)

7. **Replace all float/double percentage calculations** with integer-only display logic

---

## 5. COMPLIANCE PATTERNS

**WRONG (Multiplicative):**
```csharp
var adjustedCost = (int)(baseCost * 0.6);
var scaledValue = baseValue * multiplier / 10000;
```

**CORRECT (Additive):**
```csharp
var adjustedCost = baseCost - 2;
var scaledValue = baseValue + 5;
```

---

## Summary

The codebase has **systematically encoded multipliers** via:
1. Basis points system (28+ files)
2. Decimal multipliers in catalogues
3. Random class in 8 strategic components
4. Float/double percentages in UI

**Total: 68 violations requiring remediation.**
