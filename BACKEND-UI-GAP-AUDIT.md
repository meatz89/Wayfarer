# Backend Features Lacking UI Exposure - Audit Report

**Date**: 2025-07-30  
**Status**: Critical gaps found - ~30% of backend features inaccessible to players

## Executive Summary

A comprehensive audit of the Wayfarer codebase reveals significant gaps between implemented backend functionality and player-accessible UI. While the tutorial is production-ready, many post-tutorial game mechanics remain hidden from players due to missing UI elements.

## Methodology

1. Analyzed all Command classes in `/src/GameState/Commands/`
2. Reviewed CommandDiscoveryService implementation
3. Examined LocationActionsUIService mapping
4. Inspected UI components (NPCActionsView, MainGameplayView)
5. Cross-referenced with UI design principles documentation

## Critical Findings

### 1. Commands Without ANY UI Exposure

These backend commands are fully implemented but completely inaccessible to players:

#### **GatherResourcesCommand**
- **Purpose**: Allows resource gathering at FEATURE locations
- **Backend**: Fully implemented with item generation logic
- **Missing UI**: No "Gather Resources" button at FEATURE spots
- **Impact**: Players cannot collect materials, food, or medicine
- **Implementation Location**: CommandDiscoveryService lines 462-480

#### **BorrowMoneyCommand** 
- **Purpose**: Economic strategy through NPC loans
- **Backend**: Complete with token requirements and debt mechanics
- **Missing UI**: No borrowing interface in NPC conversations
- **Impact**: Players cannot manage cash flow through loans
- **Implementation Location**: CommandDiscoveryService lines 318-342

#### **BrowseCommand**
- **Purpose**: View available items and prices at markets
- **Backend**: Integrated with MarketManager
- **Missing UI**: No market browsing interface
- **Impact**: Players cannot discover items or plan purchases
- **Implementation Location**: CommandDiscoveryService lines 482-501

#### **Social Interaction Commands**
Multiple social commands lack UI:
- **KeepSecretCommand** (lines 269-289)
- **ShareLunchCommand** (lines 246-266)
- **PersonalErrandCommand** (lines 292-312)
- **EquipmentSocializeCommand** (lines 198-242)

### 2. Partially Exposed Features

#### **Route Discovery System**
- **Backend**: `DiscoverRouteCommand` and `RouteDiscoveryManager` implemented
- **UI Status**: Commands discovered but no clear UI for route learning
- **Missing**: Visual indication of discoverable routes, learning interface

#### **Standing Obligations**
- **Backend**: Full obligation system with queue modifications
- **UI Status**: Display-only in LetterQueueScreen
- **Missing**: Interaction mechanics, fulfillment interface

#### **Transport Methods**
- **Backend**: Multiple transport types supported
- **UI Status**: Hardcoded to "Walking" in GameFacade
- **Missing**: Transport selection UI, equipment requirements display

### 3. UI Mapping Gaps

#### **LocationActionsUIService Issues**
The service properly discovers commands but UI components don't render them:

```csharp
// Commands are discovered in CommandDiscoveryService:
if (spot.Type == LocationSpotTypes.FEATURE)
{
    // GatherResourcesCommand added to discovery result
}

// But NPCActionsView only shows pre-mapped action types:
private string GetActionVerb(ActionOptionViewModel action)
{
    // Only handles: Talk, Work, Socialize, Borrow Money, Learn Route
    // Missing: Gather, Browse, Keep Secret, Share Lunch, etc.
}
```

### 4. Command Categories Not Fully Represented

The `CommandCategory` enum defines categories but not all have UI representation:
- ✅ **Social**: Partially (Converse/Socialize shown, others missing)
- ✅ **Economic**: Partially (Work shown, Borrow/Browse missing)
- ✅ **Letter**: Fully represented
- ✅ **Rest**: Fully represented
- ✅ **Travel**: Fully represented
- ❌ **Special**: Limited representation

## Root Causes

### 1. **Incomplete Action Verb Mapping**
NPCActionsView.razor only recognizes specific action descriptions:
```csharp
if (desc.StartsWith("Talk to")) return "Talk";
if (desc.StartsWith("Work for")) return "Work";
// Missing mappings for Gather, Browse, etc.
```

### 2. **Location Actions Filtering**
Location-specific actions are discovered but filtered out:
```csharp
// Only shows actions without NPC names
if (string.IsNullOrEmpty(action.NPCName))
{
    locationActions.Add(action);
}
```

### 3. **Missing UI Components**
No dedicated UI components for:
- Resource gathering interface
- Market browsing screen
- Loan negotiation dialog
- Route discovery visualization

## Impact Analysis

### Player Experience Impact
- **Tutorial**: 100% complete and playable
- **Post-Tutorial**: Only ~70% of mechanics accessible
- **Economic Loop**: Broken - players can't gather resources or borrow money
- **Exploration**: Limited - no route discovery UI
- **Social Depth**: Reduced - advanced social commands hidden

### Game Balance Impact
- Players may struggle with cash flow (no loans)
- Resource economy non-functional (no gathering)
- Discovery mechanics underutilized
- Social token strategies limited

## Recommendations

### High Priority (Core Economic Loop)
1. **Add GatherResourcesCommand UI**
   - Add button at FEATURE locations
   - Show stamina cost and potential rewards
   - Display gathered items

2. **Implement BorrowMoneyCommand UI**
   - Add to NPC conversation options
   - Show token requirements
   - Display loan terms

3. **Create BrowseCommand Interface**
   - Dedicated market browsing view
   - Item filtering and sorting
   - Price comparison display

### Medium Priority (Gameplay Depth)
1. **Route Discovery Visualization**
   - Show discoverable routes
   - Display token requirements
   - Add learning progress

2. **Obligation Interaction UI**
   - Fulfillment mechanics
   - Negotiation interface
   - Consequence display

3. **Transport Selection**
   - Method selection dialog
   - Equipment requirements
   - Speed/cost tradeoffs

### Low Priority (Polish)
1. **Social Command Interfaces**
   - Secret keeping dialog
   - Lunch sharing UI
   - Personal errand system

2. **Bulk Operations**
   - Multi-item market transactions
   - Batch resource gathering

## Implementation Guide

### Quick Wins
1. **Update NPCActionsView.GetActionVerb()**
   ```csharp
   if (desc.StartsWith("Gather resources")) return "Gather";
   if (desc.StartsWith("Browse market")) return "Browse";
   if (desc.StartsWith("Borrow money from")) return "Borrow";
   ```

2. **Add Location Action Buttons**
   ```razor
   @if (spot.Type == LocationSpotTypes.FEATURE)
   {
       <button @onclick="GatherResources">🌿 Gather Resources</button>
   }
   ```

### Architectural Considerations
- All UI must use GameFacade pattern
- Maintain command discovery abstraction
- Follow existing UI design principles
- Ensure tutorial mode compatibility

## Testing Checklist

- [ ] Every CommandType has UI representation
- [ ] All discovered commands are actionable
- [ ] Command costs clearly displayed
- [ ] Error states properly communicated
- [ ] Tutorial restrictions respected

## Conclusion

While Wayfarer's backend is feature-complete, significant UI gaps prevent players from experiencing the full game. Implementing the missing UI elements, particularly for economic commands, should be the top priority to unlock the complete gameplay experience.