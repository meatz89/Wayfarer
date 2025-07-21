# Leverage System Implementation Guide

## Overview

The leverage system transforms Wayfarer's letter queue from a simple priority list into a dynamic representation of power relationships. Queue position emerges from the combination of social status (base position) and relationship dynamics (token balance), creating a mechanical representation of who has power over whom.

## Core Concept: Leverage Through Debt

When players need help but lack tokens, they go into "token debt" with NPCs. This debt creates leverage - the more you owe someone, the higher priority their letters receive in your queue. This mechanic naturally creates the narrative that debt gives others power over your life.

## Leverage Calculation Formula

### Base Positions by Social Status

```csharp
public static class LeverageConstants
{
    public const int PATRON_BASE_POSITION = 1;
    public const int NOBLE_BASE_POSITION = 3;
    public const int TRADE_BASE_POSITION = 5;
    public const int SHADOW_BASE_POSITION = 5;
    public const int COMMON_BASE_POSITION = 7;
    public const int TRUST_BASE_POSITION = 7;
}
```

### Token Balance Modifiers

The sender's leverage is modified by the player's token balance with them:

- **Negative tokens**: Position -1 per negative token (more leverage)
- **Zero tokens**: No modification
- **Positive tokens (1-3)**: No modification
- **High positive tokens (4+)**: Position +1 (less leverage, mutual respect)

### Leverage Calculation Method

```csharp
private int CalculateLeveragePosition(Letter letter)
{
    // Special case: Patron always gets position 1
    if (letter.IsFromPatron) 
        return LeverageConstants.PATRON_BASE_POSITION;
    
    // Get base position from social status
    int basePosition = GetBasePositionForTokenType(letter.TokenType);
    
    // Get token balance with the specific sender
    var senderId = GetNPCIdByName(letter.SenderName);
    var npcTokens = _connectionTokenManager.GetTokensWithNPC(senderId);
    var tokenBalance = npcTokens[letter.TokenType];
    
    // Apply token-based leverage modifier
    int leveragePosition = basePosition;
    
    if (tokenBalance < 0)
    {
        // Debt creates leverage - each negative token moves position up
        leveragePosition += tokenBalance; // Subtracts since negative
    }
    else if (tokenBalance >= 4)
    {
        // High positive relationship reduces leverage
        leveragePosition += 1;
    }
    
    // Apply relationship pattern modifiers
    leveragePosition = ApplyPatternModifiers(leveragePosition, senderId, tokenBalance);
    
    // Clamp to valid queue range
    return Math.Max(1, Math.Min(8, leveragePosition));
}

private int GetBasePositionForTokenType(ConnectionType tokenType)
{
    return tokenType switch
    {
        ConnectionType.Noble => LeverageConstants.NOBLE_BASE_POSITION,
        ConnectionType.Trade => LeverageConstants.TRADE_BASE_POSITION,
        ConnectionType.Shadow => LeverageConstants.SHADOW_BASE_POSITION,
        ConnectionType.Common => LeverageConstants.COMMON_BASE_POSITION,
        ConnectionType.Trust => LeverageConstants.TRUST_BASE_POSITION,
        _ => LeverageConstants.COMMON_BASE_POSITION
    };
}
```

### Pattern-Based Modifiers

Relationship patterns can modify leverage independent of token balance:

```csharp
private int ApplyPatternModifiers(int currentPosition, string npcId, int tokenBalance)
{
    var history = _gameWorld.GetPlayer().NPCLetterHistory.GetValueOrDefault(npcId);
    if (history == null) return currentPosition;
    
    // Repeated skipping creates leverage even without debt
    if (history.SkippedCount >= 2 && tokenBalance >= 0)
    {
        currentPosition -= 1; // More leverage due to pattern
    }
    
    // Consistent delivery reduces leverage
    if (history.ConsecutiveDeliveries >= 3 && tokenBalance >= 0)
    {
        currentPosition += 1; // Less leverage due to reliability
    }
    
    return currentPosition;
}
```

## Queue Entry with Displacement

When a letter's leverage position would place it in an occupied slot, it displaces existing letters downward.

### Displacement Logic

```csharp
public int AddLetterWithLeverage(Letter letter)
{
    var targetPosition = CalculateLeveragePosition(letter);
    var queue = _gameWorld.GetPlayer().LetterQueue;
    
    // If target position is empty, simple insertion
    if (queue[targetPosition - 1] == null)
    {
        return PlaceLetterAtPosition(letter, targetPosition);
    }
    
    // Target occupied - need displacement
    return DisplaceAndInsertLetter(letter, targetPosition);
}

private int DisplaceAndInsertLetter(Letter letter, int targetPosition)
{
    var queue = _gameWorld.GetPlayer().LetterQueue;
    
    // Announce the leverage-based displacement
    ShowLeverageDisplacement(letter, targetPosition);
    
    // Collect all letters from target position downward
    var lettersToDisplace = CollectLettersFromPosition(targetPosition);
    
    // Clear positions for reorganization
    ClearPositionsFrom(targetPosition);
    
    // Insert new letter at its leverage position
    queue[targetPosition - 1] = letter;
    letter.QueuePosition = targetPosition;
    
    // Reinsert displaced letters
    int nextAvailable = targetPosition;
    foreach (var displaced in lettersToDisplace)
    {
        nextAvailable++;
        if (nextAvailable <= 8)
        {
            queue[nextAvailable - 1] = displaced;
            displaced.QueuePosition = nextAvailable;
            NotifyLetterShifted(displaced, nextAvailable);
        }
        else
        {
            HandleQueueOverflow(displaced);
        }
    }
    
    return targetPosition;
}
```

### Queue Overflow Handling

When displacement pushes a letter beyond position 8:

```csharp
private void HandleQueueOverflow(Letter overflowLetter)
{
    _messageSystem.AddSystemMessage(
        $"💥 {overflowLetter.SenderName}'s letter FORCED OUT by leverage!",
        SystemMessageTypes.Danger
    );
    
    _messageSystem.AddSystemMessage(
        $"The weight of your obligations crushes {overflowLetter.SenderName}'s request.",
        SystemMessageTypes.Warning
    );
    
    // Record the forced discard (no token penalty - not player's choice)
    var senderId = GetNPCIdByName(overflowLetter.SenderName);
    RecordForcedDiscard(senderId);
}
```

## Token Debt Actions

### Patron Request System

```csharp
public class PatronRequestActions
{
    public ActionOption CreateRequestFundsAction()
    {
        var patronTokens = _tokenManager.GetTokensWithNPC("patron")[ConnectionType.Noble];
        
        return new ActionOption
        {
            Action = LocationAction.RequestPatronFunds,
            Name = "Write to patron requesting funds",
            Description = $"Receive 30 coins, -1 Patron leverage (current: {patronTokens})",
            HourCost = 1,
            StaminaCost = 0,
            CoinGain = 30,
            TokenChanges = new Dictionary<string, Dictionary<ConnectionType, int>>
            {
                ["patron"] = new() { [ConnectionType.Noble] = -1 }
            }
        };
    }
    
    public ActionOption CreateRequestEquipmentAction()
    {
        return new ActionOption
        {
            Action = LocationAction.RequestPatronEquipment,
            Name = "Request equipment from patron",
            Description = "Receive climbing gear, -2 Patron leverage",
            HourCost = 1,
            TokenChanges = new Dictionary<string, Dictionary<ConnectionType, int>>
            {
                ["patron"] = new() { [ConnectionType.Noble] = -2 }
            },
            GrantsItem = "climbing_gear"
        };
    }
}
```

### Emergency Assistance Actions

Context-sensitive actions that create debt:

```csharp
public List<ActionOption> GetEmergencyActions(NPC npc, Player player)
{
    var actions = new List<ActionOption>();
    
    // Borrow money when broke
    if (player.Coins < 10 && npc.Profession == "Merchant")
    {
        actions.Add(new ActionOption
        {
            Action = LocationAction.BorrowMoney,
            Name = $"Borrow money from {npc.Name}",
            Description = "20 coins now, -2 Trade leverage",
            TokenChanges = new Dictionary<string, Dictionary<ConnectionType, int>>
            {
                [npc.ID] = new() { [ConnectionType.Trade] = -2 }
            },
            CoinGain = 20,
            NPCId = npc.ID
        });
    }
    
    // Maintain route access through debt
    if (!HasSufficientTokensForRoute(route, npc) && npc.KnowsRoute(route))
    {
        actions.Add(new ActionOption
        {
            Action = LocationAction.PleedForAccess,
            Name = $"Plead with {npc.Name} for route access",
            Description = $"Maintain {route.Name} access, -1 leverage",
            TokenChanges = new Dictionary<string, Dictionary<ConnectionType, int>>
            {
                [npc.ID] = new() { [npc.LetterTokenTypes[0]] = -1 }
            },
            MaintainsAccess = route.ID
        });
    }
    
    return actions;
}
```

### Shadow Dealings

Dangerous opportunities that create leverage:

```csharp
private ActionOption CreateShadowDealingAction(NPC shadowNPC)
{
    return new ActionOption
    {
        Action = LocationAction.AcceptIllegalWork,
        Name = "Accept questionable job",
        Description = "30 coins, but they'll have leverage (-1 Shadow)",
        HourCost = 1,
        StaminaCost = 1,
        CoinGain = 30,
        TokenChanges = new Dictionary<string, Dictionary<ConnectionType, int>>
        {
            [shadowNPC.ID] = new() { [ConnectionType.Shadow] = -1 }
        },
        NPCId = shadowNPC.ID,
        Consequences = new[]
        {
            "This NPC will have dirt on you",
            "Future shadow letters will have high priority",
            "Refusing their letters may have consequences"
        }
    };
}
```

## UI Implementation

### Queue Display Enhancements

```csharp
public class QueuePositionDisplay
{
    public string GetLeverageIndicator(Letter letter)
    {
        var senderId = GetNPCIdByName(letter.SenderName);
        var tokens = _tokenManager.GetTokensWithNPC(senderId);
        var balance = tokens[letter.TokenType];
        
        // Visual indicators for leverage state
        return balance switch
        {
            <= -3 => "🔴🔴🔴", // Extreme leverage
            -2 => "🔴🔴",      // High leverage  
            -1 => "🔴",        // Some leverage
            0 => "",           // Neutral
            1 to 3 => "",      // Normal relationship
            >= 4 => "🟢",      // Mutual respect
        };
    }
    
    public string GetPositionExplanation(Letter letter, int actualPosition)
    {
        var basePosition = GetBasePositionForTokenType(letter.TokenType);
        
        if (actualPosition < basePosition)
        {
            return $"↑ Leverage moves from {basePosition} → {actualPosition}";
        }
        else if (actualPosition > basePosition)
        {
            return $"↓ Respect moves from {basePosition} → {actualPosition}";
        }
        
        return "";
    }
}
```

### Relationship Panel Enhancement

Show token debt clearly:

```csharp
public class RelationshipDisplay
{
    public string FormatTokenBalance(string npcId, ConnectionType tokenType)
    {
        var balance = GetTokenBalance(npcId, tokenType);
        
        if (balance < 0)
        {
            return $"{tokenType}: <span class='debt'>{balance}</span> (debt)";
        }
        else if (balance >= 4)
        {
            return $"{tokenType}: <span class='respect'>{balance}</span> ⭐";
        }
        else
        {
            return $"{tokenType}: {balance}";
        }
    }
}
```

## Standing Obligations for Leverage

New obligation effects that modify the leverage system:

```csharp
public enum ObligationEffect
{
    // Existing effects...
    
    // Leverage modifiers
    ShadowEqualsNoble,      // Shadow letters use Noble base position (3)
    MerchantRespect,        // Trade letters with 5+ tokens get additional +1 position
    CommonRevenge,          // Common letters from debt relationships use position 3
    PatronAbsolute,         // Patron letters push everything down (no displacement limit)
    DebtSpiral,             // All negative token positions get additional -1
}
```

### Obligation Implementation

```csharp
public int ApplyObligationModifiers(Letter letter, int calculatedPosition)
{
    var obligations = _obligationManager.GetActiveObligations();
    
    foreach (var obligation in obligations)
    {
        if (obligation.HasEffect(ObligationEffect.ShadowEqualsNoble) 
            && letter.TokenType == ConnectionType.Shadow)
        {
            // Shadow letters now use Noble base position
            calculatedPosition = Math.Min(calculatedPosition, 
                LeverageConstants.NOBLE_BASE_POSITION);
        }
        
        if (obligation.HasEffect(ObligationEffect.CommonRevenge)
            && letter.TokenType == ConnectionType.Common
            && HasDebtWith(letter.SenderName))
        {
            // Common folk with leverage get noble-level priority
            calculatedPosition = LeverageConstants.NOBLE_BASE_POSITION;
        }
    }
    
    return calculatedPosition;
}
```

## Integration Points

### LetterQueueManager Changes

1. Replace `AddLetterWithObligationEffects()` with `AddLetterWithLeverage()`
2. Add `CalculateLeveragePosition()` method
3. Implement `DisplaceAndInsertLetter()` for queue reorganization
4. Update `GetQueueDisplay()` to show leverage indicators

### ConnectionTokenManager Changes

1. Enhance debt feedback in `SpendTokensWithNPC()`
2. Add `GetLeverageLevel()` method for UI
3. Track debt spiral warnings
4. Implement pattern-based leverage tracking

### LocationActionManager Changes

1. Add patron request actions
2. Implement context-sensitive debt actions
3. Add shadow dealing opportunities
4. Show leverage consequences in action descriptions

## Testing Strategy

### Unit Tests

```csharp
[Test]
public void CalculateLeveragePosition_NobleWithDebt_GetsHigherPriority()
{
    // Arrange
    var letter = CreateNobleetter();
    SetTokenBalance(letter.SenderName, ConnectionType.Noble, -2);
    
    // Act
    var position = CalculateLeveragePosition(letter);
    
    // Assert
    Assert.AreEqual(1, position); // 3 base - 2 debt = 1
}

[Test]
public void QueueDisplacement_FullQueue_PushesLowestLeverageOut()
{
    // Arrange
    FillQueueCompletely();
    var highLeverageLetter = CreatePatronLetter();
    
    // Act
    var result = AddLetterWithLeverage(highLeverageLetter);
    
    // Assert
    Assert.IsTrue(result.DisplacedLetter != null);
    Assert.AreEqual(8, result.DisplacedLetter.PreviousPosition);
}
```

### Integration Tests

- Test full leverage calculation with obligations
- Verify queue displacement cascades
- Ensure token debt affects future letters
- Validate UI updates with leverage changes

## Migration Notes

### Save Game Compatibility

The leverage system is backward compatible:
- Existing letters maintain their positions until new letters arrive
- Token balances already support negative values
- New leverage calculation only applies to incoming letters

### Gradual Rollout

1. Phase 1: Implement leverage calculation (display only)
2. Phase 2: Enable displacement for new letters
3. Phase 3: Add debt-creating actions
4. Phase 4: Full UI integration with indicators

## Narrative Integration

### Leverage Messages

```csharp
public static class LeverageNarratives
{
    public static string GetDebtLeverageMessage(string npcName, int debtLevel)
    {
        return debtLevel switch
        {
            1 => $"{npcName} expects their favor to be remembered.",
            2 => $"Your debt to {npcName} weighs on your priorities.",
            3 => $"{npcName} has significant influence over your schedule.",
            _ => $"You are deeply indebted to {npcName}. They control your time."
        };
    }
    
    public static string GetDisplacementMessage(Letter displacer, Letter displaced)
    {
        if (displacer.TokenType == ConnectionType.Common && displaced.TokenType == ConnectionType.Noble)
        {
            return "Social hierarchy inverts as debt creates unexpected power.";
        }
        
        return "Leverage reshapes your obligations.";
    }
}
```

## Performance Considerations

- Leverage calculation is O(1) - simple lookups and arithmetic
- Displacement is O(n) worst case where n = queue size (max 8)
- Token balance lookups use existing dictionaries
- No additional storage required beyond existing systems

## Future Enhancements

1. **Leverage Decay**: Positive actions slowly reduce leverage over time
2. **Leverage Chains**: Debt with one NPC affects related NPCs
3. **Leverage Visualization**: Queue shows animated arrows for leverage effects
4. **Strategic Debt Tools**: More ways to deliberately take on debt for advantage

This implementation creates the full vision where debt becomes a mechanical force that reshapes priorities, making every request for help a strategic decision with lasting consequences.