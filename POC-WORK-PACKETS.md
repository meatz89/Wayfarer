# POC Implementation Work Packets

## Overview
This document defines precise work packets for implementing the Elena's Letter POC scenario. Each packet is self-contained, testable, and implements exactly ONE feature without legacy code or TODOs.

## Work Packet 1: Location Familiarity System

### Goal
Add familiarity tracking (0-3) to locations, enabling progressive discovery through investigation.

### Files to Modify
1. `/src/GameState/Location.cs`
   - Add `public int Familiarity { get; set; } = 0;`
   - Add `public int MaxFamiliarity { get; set; } = 3;`
   - Add `public int HighestObservationCompleted { get; set; } = 0;`

2. `/src/GameState/Player.cs`
   - Add `public Dictionary<string, int> LocationFamiliarity { get; set; } = new();`
   - Add method `GetLocationFamiliarity(string locationId)`
   - Add method `SetLocationFamiliarity(string locationId, int value)`

3. `/src/ViewModels/GameViewModels.cs`
   - Add Familiarity to LocationViewModel
   - Display as "Familiarity: 0/3" format

### Success Criteria
- Locations track familiarity 0-3
- Player tracks familiarity per location
- UI displays current familiarity

---

## Work Packet 2: Investigation Action

### Goal
Create Investigation action that costs 1 attention, takes 10 minutes, and increases familiarity based on spot properties.

### Files to Create
1. `/src/GameState/Actions/InvestigationAction.cs`
   ```csharp
   public class InvestigationAction
   {
       public int AttentionCost => 1;
       public int TimeMinutes => 10;
       
       public int GetFamiliarityGain(LocationSpot spot)
       {
           // Quiet: +2, Busy: +1, else: +1
       }
   }
   ```

### Files to Modify
1. `/src/Subsystems/Location/LocationFacade.cs`
   - Add `InvestigateLocation(string locationId, string spotId)` method
   - Check attention available
   - Apply familiarity gain based on spot
   - Advance time by 10 minutes
   - Cap at MaxFamiliarity

2. `/src/Pages/Components/LocationContent.razor`
   - Add Investigation button
   - Show cost (1 attention) and time (10 min)
   - Disable if attention unavailable or max familiarity reached

### Success Criteria
- Investigation costs 1 attention
- Takes 10 minutes game time
- Quiet spots give +2 familiarity
- Busy spots give +1 familiarity
- Capped at location's MaxFamiliarity

---

## Work Packet 3: Observation System Changes

### Goal
Make observations cost 0 attention, require familiarity levels, and track completion.

### Files to Modify
1. `/src/GameState/ObservationManager.cs`
   - Change attention cost to 0
   - Add familiarity check before allowing observation
   - Check prior observation requirements
   - Track highest observation completed

2. `/src/GameState/Location.cs`
   - Add `List<ObservationReward> ObservationRewards { get; set; }`
   - Each reward has: FamiliarityRequired, PriorObservationRequired, CardReward

3. `/src/Content/DTOs/ObservationRewardDTO.cs` (Create)
   ```csharp
   public class ObservationRewardDTO
   {
       public int FamiliarityRequired { get; set; }
       public int? PriorObservationRequired { get; set; }
       public string ObservationCardId { get; set; }
       public string TargetNpcId { get; set; }
   }
   ```

### Success Criteria
- Observations cost 0 attention
- First observation requires familiarity 1+
- Second requires familiarity 2+ AND first completed
- Third requires familiarity 3+ AND second completed

---

## Work Packet 4: NPC Observation Decks

### Goal
Add fourth deck to NPCs for observation cards received from location discoveries.

### Files to Modify
1. `/src/GameState/NPC.cs`
   - Add `public CardDeck ObservationDeck { get; set; } = new();`
   - Add `InitializeObservationDeck()` method

2. `/src/Content/NPCParser.cs`
   - Parse `hasObservationDeck` property
   - Initialize empty observation deck if true

3. `/src/GameState/ObservationCard.cs` (Create)
   ```csharp
   public class ObservationCard : ConversationCard
   {
       public string TargetNpcId { get; set; }
       public ObservationEffect Effect { get; set; }
   }
   ```

### Success Criteria
- NPCs have fourth deck for observations
- Elena and Marcus have observation decks
- Cards can be added to specific NPC's observation deck

---

## Work Packet 5: Observation Cards in Conversations

### Goal
Allow observation cards to be played during conversations as special SPEAK actions.

### Files to Modify
1. `/src/GameState/ConversationSession.cs`
   - Add `List<CardInstance> NPCObservationCards { get; set; }`
   - Load from NPC's observation deck at start
   - Track which have been played

2. `/src/Pages/Components/ConversationContent.razor`
   - Display observation cards separately from hand
   - Show as "Knowledge: [Card Name]" 
   - Allow playing for 0 focus cost
   - Remove after playing

3. `/src/Content/CardEffectProcessor.cs`
   - Add `AdvanceEmotionalState` effect type
   - Add `UnlockExchange` effect type
   - Reset flow to 0 on state advancement

### Success Criteria
- Observation cards appear in conversations
- Cost 0 focus to play
- Consumed permanently when played
- Can advance NPC connection state
- Can unlock hidden exchanges

---

## Work Packet 6: Token-Gated Exchanges

### Goal
Add token requirements to exchanges, preventing access without sufficient relationship.

### Files to Modify
1. `/src/GameState/ConversationCard.cs`
   - Add `public int MinimumTokensRequired { get; set; }`
   - Add `public ConnectionType? RequiredTokenType { get; set; }`

2. `/src/Subsystems/Conversation/ConversationFacade.cs`
   - Check token requirements in `CanSelectExchange()`
   - Return reason if insufficient tokens

3. `/src/Pages/Components/ExchangeContent.razor`
   - Show "Requires X [Type] tokens" on locked exchanges
   - Gray out unavailable exchanges
   - Show current tokens vs required

### Success Criteria
- Marcus caravan requires 2 Commerce tokens
- UI shows token requirements clearly
- Cannot select without sufficient tokens

---

## Work Packet 7: Work Action Scaling

### Goal
Make work output decrease based on hunger level.

### Files to Modify
1. `/src/GameState/Actions/WorkAction.cs`
   - Add hunger scaling: `coins = base - floor(hunger/25)`
   - Base amount varies by work type

2. `/src/Subsystems/Location/LocationFacade.cs`
   - Apply hunger penalty to work rewards
   - Show actual coins earned in result

### Success Criteria
- Work at 0-24 hunger: Full payment
- Work at 25-49 hunger: -1 coin
- Work at 50-74 hunger: -2 coins
- Work at 75-99 hunger: -3 coins
- Work at 100 hunger: -4 coins

---

## Work Packet 8: Content Package

### Goal
Create complete core_game_package.json with all POC content.

### Content Required
1. **NPCs**: Elena, Marcus, Lord Blackwood, Warehouse Recipient
2. **Locations**: Market Square, Copper Kettle, Noble District, Warehouse
3. **Spots**: All spots with time-based properties
4. **Routes**: Including checkpoint and caravan routes
5. **Cards**: Full starter deck (12 cards)
6. **Request Cards**: Elena's crisis letter
7. **Exchange Cards**: Marcus's trades
8. **Observation Cards**: Safe Passage, Merchant Route
9. **Investigation Rewards**: Two-stage observation progression
10. **Starting Conditions**: 10 attention, 0 coins, 50 hunger, Viktor's package

### Files to Modify
1. `/src/Content/Core/core_game_package.json`
   - Complete rewrite with all POC content
   - Remove all placeholder content
   - Ensure all IDs match exactly

### Success Criteria
- All content loads without skeletons
- Elena starts in Desperate state
- Marcus has gated caravan exchange
- Observations unlock in sequence
- Starting conditions match POC spec

---

## Work Packet 9: Integration Testing

### Goal
Verify complete POC flow works end-to-end.

### Test Scenarios
1. **Investigation Flow**
   - Start at Market Square
   - Investigate (costs 1 attention, +2 familiarity if morning/quiet)
   - Verify familiarity increases
   - Verify can't exceed max (3)

2. **Observation Flow**
   - At familiarity 1, observe for Safe Passage Knowledge
   - At familiarity 2, observe for Merchant Route
   - Verify cards go to correct NPC decks

3. **Elena Conversation**
   - Start conversation in Desperate state
   - Play Safe Passage Knowledge (0 focus)
   - Verify advances to Neutral state
   - Verify can reach request card

4. **Marcus Exchange**
   - Without tokens: Caravan not visible
   - With 2 Commerce tokens: Caravan visible
   - Play Merchant Route card: Unlocks exchange
   - Verify can purchase caravan transport

5. **Complete Scenario**
   - Full path from start to Noble District
   - Deliver Elena's letter before 5 PM
   - Verify all mechanics work together

### Success Criteria
- Can complete Elena scenario
- All mechanics function correctly
- No legacy code remains
- No TODOs in codebase
- Clean, production-ready implementation

---

## Implementation Order

1. **Work Packets 1-4**: Core mechanics (Location Familiarity, Investigation, Observation, NPC Decks)
2. **Work Packets 5-6**: Conversation integration (Observation cards, Token gates)
3. **Work Packet 7**: Work scaling
4. **Work Packet 8**: Content package
5. **Work Packet 9**: Integration testing

Each packet should be implemented completely before moving to the next. No partial implementations or TODOs allowed.