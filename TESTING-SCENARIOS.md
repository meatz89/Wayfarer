# 🎯 WAYFARER: Full Mockup Implementation Plan


## Testing Scenarios

### Scenario 1: Elena's Disconnected Letter
1. Start new game
2. Navigate to Market Square
3. See "Elena (Disconnected)" at Copper Kettle
4. Move to Corner Table
5. Start Letter Conversation (1 attention)
6. Turn 1: Draw 1-2 disconnected cards
7. Turn 2: Play state change card
8. Turn 3: Request card appears, play it
9. Letter added to queue position 1

### Scenario 2: Marcus Exchange
1. At Market Square
2. Move to Merchant Row
3. See Marcus with queue marker
4. Start Quick Exchange (0 attention)
5. See 3 exchange cards
6. "Buy Provisions" highlighted
7. Accept → Hunger set to 0

## Current Status: Phase 3 - UI Components Implementation

### Remaining Work:
1. Fix NPC location display (show at spots with states)
2. Add "You are here" indicator
3. Implement Work and Travel actions
4. Fix conversation UI (turn counter, patience display)
5. Create proper exchange card display
6. Test complete Elena scenario