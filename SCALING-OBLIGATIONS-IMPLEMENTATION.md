# Dynamic Obligation Scaling System

## Overview

The dynamic obligation scaling system allows standing obligations to have effects that scale mathematically with token levels, creating more nuanced and emergent gameplay without special cases.

## Architecture

### 1. Core Enums

```csharp
public enum ScalingType
{
    None,       // No scaling (static effect)
    Linear,     // Linear scaling: effect = base + (tokens * scalingFactor)
    Stepped,    // Stepped scaling: effect changes at specific thresholds
    Threshold   // Threshold scaling: effect only applies above/below threshold
}

public enum ObligationEffect
{
    // ... existing effects ...
    DynamicLeverageModifier,   // Leverage scales with token level
    DynamicPaymentBonus,       // Payment bonus scales with token level
    DynamicDeadlineBonus       // Deadline bonus scales with token level
}
```

### 2. StandingObligation Properties

```csharp
// Dynamic Scaling Properties
public ScalingType ScalingType { get; set; } = ScalingType.None;
public float ScalingFactor { get; set; } = 1.0f;
public float BaseValue { get; set; } = 0f;
public float MinValue { get; set; } = 0f;
public float MaxValue { get; set; } = 100f;
public Dictionary<int, float> SteppedThresholds { get; set; }
```

### 3. Scaling Calculation Methods

- `CalculateDynamicValue(int tokenCount)` - Core scaling math
- `CalculateDynamicLeverage(Letter letter, int currentPosition, int tokenCount)`
- `CalculateDynamicPaymentBonus(Letter letter, int basePayment, int tokenCount)`
- `CalculateDynamicDeadlineBonus(Letter letter, int tokenCount)`

## Examples from scaling_obligations.json

### 1. Linear Scaling - Patron Debt Leverage
```json
{
    "ID": "patron_debt_leverage",
    "ScalingType": "Linear",
    "ScalingFactor": 0.5,
    "BaseValue": 0,
    "MinValue": 0,
    "MaxValue": 3
}
```
Each negative patron token improves letter position by 0.5, max improvement of 3 positions.

### 2. Stepped Scaling - Merchant Partnership
```json
{
    "ID": "merchant_partnership_bonus",
    "ScalingType": "Stepped",
    "SteppedThresholds": {
        "3": 2,
        "5": 5,
        "8": 10,
        "10": 15
    }
}
```
Payment bonuses increase at specific trade token thresholds.

### 3. Threshold Scaling - Common Folk Solidarity
```json
{
    "ID": "common_folk_solidarity",
    "ScalingType": "Threshold",
    "ActivationThreshold": 4,
    "BaseValue": 2
}
```
4+ common tokens grants +2 days deadline for their letters.

## Integration Points

### LetterQueueManager
- Calls `ApplyDeadlineBonuses()` which applies both static and dynamic deadline modifiers
- Uses `ApplyLeverageModifiers()` to calculate final letter position

### StandingObligationManager
- `ApplyLeverageModifiers()` - Applies all leverage effects including dynamic ones
- `CalculateTotalCoinBonus()` - Sums static and dynamic payment bonuses
- `ApplyDynamicDeadlineBonuses()` - Applies scaling deadline extensions

## Mathematical Clarity

The system maintains mathematical clarity:
- Linear: `effect = base + (tokens * factor)`
- Stepped: Uses highest threshold met
- Threshold: Binary on/off based on token count

All calculations are transparent in code with clear bounds (min/max values).

## Benefits

1. **No Special Cases**: Effects scale naturally with relationships
2. **Emergent Gameplay**: Players discover optimal token levels
3. **Clear Progression**: Mathematical scaling is predictable
4. **Extensible**: Easy to add new scaling effect types

## Testing

Unit tests in `ScalingObligationTests.cs` verify:
- Linear scaling with bounds
- Stepped thresholds
- Threshold activation
- Integration with letter system

All tests pass, confirming the implementation works correctly.