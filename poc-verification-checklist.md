# Wayfarer POC Complete Verification Checklist

## Prerequisites
- Clean build with no errors: `cd /mnt/c/git/wayfarer/src && dotnet build`
- Launch game with Playwright
- Take screenshot of EVERY step to verify visual implementation matches design

## PHASE 1: Game Start Verification

### 1.1 Initial Load
- [ ] **SCREENSHOT**: Game loads to Market Square, Fountain spot
- [ ] **VERIFY RESOURCES BAR** shows exactly:
  - [ ] Coins: 10
  - [ ] Health: 75
  - [ ] Hunger: 60
  - [ ] Attention: 8/10
- [ ] **VERIFY TIME** shows: Tuesday, 9:00 AM (Morning)
- [ ] **VERIFY QUEUE DISPLAY** shows:
  - [ ] Position 1: Marcus Package (5hr deadline)
  - [ ] Position 2: Guard Report (8hr deadline)
  - [ ] Position 3: Empty (where Elena's letter will go)
  - [ ] Queue rule text: "Must complete Position 1 first!"

### 1.2 Location Display
- [ ] **VERIFY LOCATION HIERARCHY** displayed: Lower Wards → Market District → Central Square → Fountain
- [ ] **VERIFY SPOT TRAITS** shown: Crossroads, Public (-1 patience)
- [ ] **VERIFY ATMOSPHERE TEXT** appears (narrative description)
- [ ] **VERIFY OTHER SPOTS** visible: Merchant Row, Guard Post, North Alcove

## PHASE 2: Basic Navigation

### 2.1 Spot Movement (Instant, Free)
- [ ] **ACTION**: Click to move to Merchant Row
- [ ] **SCREENSHOT**: Confirm instant movement (no travel screen)
- [ ] **VERIFY**: Marcus visible at Merchant Row
- [ ] **VERIFY**: Spot traits updated (Commercial)
- [ ] **VERIFY**: No resource cost, no time advancement

### 2.2 Location Travel (From Crossroads)
- [ ] **ACTION**: Return to Fountain (Crossroads spot)
- [ ] **ACTION**: Click Travel action
- [ ] **SCREENSHOT**: Travel destination selection appears
- [ ] **VERIFY ROUTES**:
  - [ ] Copper Kettle: 15 min, free
  - [ ] Noble District Gate: BLOCKED (requires permit)
  - [ ] Temple District: 20 min, free
  - [ ] Warehouse District: 30 min, free

## PHASE 3: Deck-Driven Conversation Types

### 3.1 Marcus Conversation Options
- [ ] **ACTION**: At Merchant Row, click Marcus
- [ ] **SCREENSHOT**: Available conversation types
- [ ] **VERIFY OPTIONS BASED ON DECK**:
  - [ ] "Quick Exchange" (Marcus has exchange deck)
  - [ ] "Letter Offer" (Marcus has letter cards in deck)
  - [ ] "Deliver Letter" (only if Marcus has delivery card for player)
- [ ] **VERIFY**: NO "Burden Resolution" (Marcus has no burden cards)

### 3.2 Exchange System
- [ ] **ACTION**: Select "Quick Exchange" 
- [ ] **VERIFY**: Draws from exchange deck, NOT conversation deck
- [ ] **VERIFY**: NO request card (exchanges have no requests)
- [ ] **VERIFY**: Exchange options displayed as CARDS
- [ ] **VERIFY CARD**: "Buy Provisions" - 3 coins → Hunger = 0
- [ ] **VERIFY**: Success rate shown (75% with 2 Commerce tokens)
- [ ] **ACTION**: Play food exchange card
- [ ] **SCREENSHOT**: Confirm hunger reduced to 0
- [ ] **VERIFY**: Coins reduced to 7

## PHASE 4: Work Action

### 4.1 Work Execution  
- [ ] **ACTION**: Click Work action at Merchant Row
- [ ] **VERIFY COST**: Shows "2 attention → 8 coins"
- [ ] **ACTION**: Confirm work
- [ ] **SCREENSHOT**: After work completion
- [ ] **VERIFY**: Attention reduced to 6
- [ ] **VERIFY**: Coins increased to 15
- [ ] **VERIFY**: Time advanced to Midday (4 hours)

## PHASE 5: Observation System

### 5.1 Making Observations
- [ ] **ACTION**: Return to Fountain
- [ ] **VERIFY**: Observation available: "Guard Routes"
- [ ] **VERIFY COST**: 1 attention shown
- [ ] **ACTION**: Make observation
- [ ] **SCREENSHOT**: Observation card gained
- [ ] **VERIFY**: Card shows "Any→Guarded" state change
- [ ] **VERIFY**: Expiration timer shown (24hr)
- [ ] **VERIFY**: Attention reduced to 5

## PHASE 6: Travel to Elena

### 6.1 Travel and Wait
- [ ] **ACTION**: Travel to Copper Kettle (15 min)
- [ ] **ACTION**: Wait to Afternoon (2 PM for Elena availability)
- [ ] **ACTION**: Move to Corner Table spot
- [ ] **SCREENSHOT**: Elena at Corner Table

## PHASE 7: Elena Letter Offer Conversation

### 7.1 Conversation Type Selection
- [ ] **ACTION**: Click Elena
- [ ] **SCREENSHOT**: Available conversation types
- [ ] **VERIFY OPTIONS**:
  - [ ] "Letter Offer" (Elena has  Letter in deck)
  - [ ] "Burden Resolution" (Elena has 2 burden cards)
- [ ] **VERIFY**: Only conversation types for cards in deck shown

### 7.2 Burden Resolution Conversation
- [ ] **ACTION**: Select "Burden Resolution" conversation
- [ ] **VERIFY**: ONE burden resolution request card shuffled in
- [ ] **VERIFY**: NO letter cards accessible (wrong conversation type)
- [ ] **ACTION**: Exit conversation (test switching)

### 7.3 Letter Offer Conversation
- [ ] **ACTION**: Select "Letter Offer" conversation  
- [ ] **VERIFY**: ONE promise request card shuffled in Deck (letter)
- [ ] **VERIFY**: NO burden resolution cards (wrong conversation type)
- [ ] **SCREENSHOT**: Conversation screen loads
- [ ] **VERIFY HEADER**:
  - [ ] Elena - Devoted personality
  - [ ] DISCONNECTED state (deadline pressure)
  - [ ] Starting patience: 16 (15 base + 1 private spot)

### 7.4 Resource Displays
- [ ] **VERIFY FLOW**: Shows 5 (starting value)
- [ ] **VERIFY MOMENTUM**: Shows 0 (starting value)
- [ ] **VERIFY TOKENS**: Trust: 1 (+5% shown)
- [ ] **VERIFY**: Depth access shown: "Can access cards depth 0-5"

### 7.5 Turn 1: LISTEN Action
- [ ] **ACTION**: Click LISTEN
- [ ] **SCREENSHOT**: Cards drawn
- [ ] **VERIFY**: Cards drawn include Trust-type cards (Disconnected filter)
- [ ] **VERIFY**: At least 1 guaranteed state card
- [ ] **VERIFY**: Letter card appears (focus 0 in Disconnected)
- [ ] **VERIFY**: Patience reduced to 15

### 7.6 State Navigation
- [ ] **ACTION**: Play observation card "Guard Routes" (Any→Guarded)
- [ ] **VERIFY**: Success rate shown: 85%
- [ ] **VERIFY ON SUCCESS**:
  - [ ] State changes to GUARDED
  - [ ] Focus limit increases to 2
  - [ ] Momentum increases to +1

### 7.7 Building to Letter Depth
- [ ] **ACTION**: Play flow cards to reach depth 7
- [ ] **VERIFY**: Each success increases flow
- [ ] **VERIFY**: Depth access updates with flow

### 7.8 Letter Card Draw and Play
- [ ] **ACTION**: LISTEN when flow ≥ 7
- [ ] **VERIFY**: "Letter" request card drawn
- [ ] **VERIFY**: 3-turn urgency rule activated
- [ ] **ACTION**: Play letter card within 3 turns
- [ ] **VERIFY**: Negotiation success/failure
- [ ] **SCREENSHOT**: Terms negotiated

### 7.9 Automatic Queue Displacement
- [ ] **VERIFY ON POOR NEGOTIATION** (failure):
  - [ ] Letter forces position 1
  - [ ] Marcus Package automatically displaced to position 2
  - [ ] Guard Report automatically displaced to position 3
  - [ ] Token burn happens AUTOMATICALLY:
    - [ ] -2 Commerce tokens with Marcus (his package displaced)
    - [ ] -1 Shadow token with Guard (report displaced)
  - [ ] **VERIFY**: NO manual displacement UI needed

## PHASE 8: Delivery Card System

### 8.1 Letter Creates Delivery Card
- [ ] **VERIFY**: After accepting Elena's letter:
  - [ ] Delivery card added to LORD BLACKWOOD's deck
  - [ ] This enables "Deliver Letter" conversation with him

### 8.2 Queue Rules Enforcement
- [ ] **VERIFY**: If Elena's letter NOT in position 1:
  - [ ] Cannot deliver (blocked by queue rules)
  - [ ] Must complete position 1 first
- [ ] **ACTION**: Complete obligations in order

## PHASE 9: Promise Cards and Queue Modification

### 9.1 Promise Card Types
- [ ] **TEST**: In any conversation, encounter promise cards
- [ ] **VERIFY PROMISE TYPES**:
  - [ ] Meeting Promise (time-fixed appointment)
  - [ ] Escort Promise (transport obligation)
  - [ ] Investigation Promise (information gathering)

### 9.2 Promise Card Queue Effects
- [ ] **ACTION**: Play a promise card with urgency
- [ ] **VERIFY**: If promise demands position 1:
  - [ ] Queue automatically reorganizes
  - [ ] Displaced obligations burn tokens AUTOMATICALLY
  - [ ] Player sees token loss notification
- [ ] **VERIFY**: No manual queue management screen

## PHASE 10: Noble District Delivery

### 10.1 Checkpoint Access
- [ ] **ACTION**: Travel to Noble District Gate
- [ ] **VERIFY**: Checkpoint blocks without permit
- [ ] **OPTIONS**:
  - [ ] Pay 10 coin bribe
  - [ ] Use permit (if obtained from exchange)

### 10.2 Lord Blackwood Delivery
- [ ] **ACTION**: Travel to Lord Blackwood's Manor
- [ ] **ACTION**: Click Lord Blackwood
- [ ] **SCREENSHOT**: Conversation options
- [ ] **VERIFY**: "Deliver Letter" available (has delivery card)
- [ ] **ACTION**: Select "Deliver Letter" conversation
- [ ] **VERIFY**: Delivery request card shuffled in
- [ ] **ACTION**: Play delivery card
- [ ] **VERIFY**: Payment received
- [ ] **VERIFY**: Letter removed from queue

## PHASE 11: Deck Evolution

### 11.1 Successful Delivery Effects
- [ ] **VERIFY**: After delivering Elena's letter:
  - [ ] Trust flow cards added to Blackwood's deck
  - [ ] Elena's deck modified (relationship improved)
  - [ ] Delivery card removed from Blackwood's deck

### 11.2 Failed Delivery Effects  
- [ ] **TEST**: Let Elena's deadline expire
- [ ] **VERIFY**:
  - [ ] 2 burden cards added to Elena's deck
  - [ ] -2 Trust tokens with Elena
  - [ ] Next conversation with Elena shows "Burden Resolution" option

## PHASE 12: Multiple Request Prevention

### 12.1 One Request Per Conversation
- [ ] **TEST**: Start Letter Offer conversation
- [ ] **VERIFY**: Only ONE letter request card in deck
- [ ] **TEST**: If multiple letters available in NPC deck
- [ ] **VERIFY**: Player chooses WHICH letter conversation
- [ ] **VERIFY**: Only chosen letter's request card shuffled in

### 12.2 Request Card Exclusivity
- [ ] **VERIFY**: Cannot draw:
  - [ ] Burden request in Letter conversation
  - [ ] Letter request in Burden conversation
  - [ ] Multiple requests in same conversation

## PHASE 13: Failure States

### 13.1 Deadline Cascade
- [ ] **TEST**: Miss position 1 deadline while displaced
- [ ] **VERIFY**: Automatic token burns for displacement
- [ ] **VERIFY**: Failed obligation adds burdens
- [ ] **VERIFY**: Relationship permanently damaged

### 13.2 Resource Depletion
- [ ] **TEST**: Reach 100 hunger
- [ ] **VERIFY**: Starvation (-5 health per period)
- [ ] **TEST**: Health reaches 0
- [ ] **VERIFY**: Death state triggered

### 13.3 Conversation Failures
- [ ] **TEST**: Run out of patience
- [ ] **VERIFY**: Conversation ends, no request achieved
- [ ] **TEST**: Reach Hostile state
- [ ] **VERIFY**: Conversation ends next turn

## PHASE 14: Emergent Complexity

### 14.1 Multi-Letter Scenario
- [ ] **TEST**: Accept letters from multiple NPCs
- [ ] **VERIFY**: Queue fills up
- [ ] **VERIFY**: Each poor negotiation cascades displacements
- [ ] **VERIFY**: Token economy destroyed by cascading burns

### 14.2 Burden Accumulation
- [ ] **TEST**: Fail multiple deliveries
- [ ] **VERIFY**: Burden cards accumulate in decks
- [ ] **VERIFY**: Future conversations harder (burdens clog draws)
- [ ] **VERIFY**: Burden Resolution becomes necessary

## FINAL VERIFICATION

### Core Mechanics
- [ ] **Deck Determines Conversations**: Available conversation types match deck contents
- [ ] **One Request Per Conversation**: Each conversation type has exactly one request
- [ ] **Automatic Displacement**: Queue changes happen through card play, not UI
- [ ] **Perfect Information**: All effects visible before playing

### System Integration  
- [ ] **VERIFY**: All resources visible at ALL times
- [ ] **VERIFY**: No infinite loops or exploits
- [ ] **VERIFY**: All POC content accessible through normal play
- [ ] **VERIFY**: UI shows cards AS CARDS, not buttons

## COMPLETION CRITERIA

**This POC is ONLY complete when:**
1. ALL boxes checked with screenshots
2. Deck composition correctly drives conversation availability
3. Queue displacement happens automatically through card play
4. One request card per conversation enforced
5. All failure states cascade properly
6. No manual queue management UI exists

**DO NOT claim completion without screenshot verification of EVERY mechanic.**