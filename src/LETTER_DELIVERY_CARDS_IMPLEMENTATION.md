# Letter Delivery Cards Implementation

## Summary
Implemented letter delivery cards that automatically appear in conversations when the player is carrying letters for that NPC.

## Changes Made

### 1. **ConversationCard.cs**
- Added `DeliveryObligationId` property to track which letter a card delivers

### 2. **ConversationSession.cs**
- Modified `StartConversation()` to check for letters with matching RecipientId
- Modified `StartCrisis()` to also check for deliverable letters
- Added `CreateLetterDeliveryCard()` method that:
  - Creates a card for each letter the player has for this NPC
  - Sets card weight to 0 (free to play)
  - Sets comfort reward based on letter importance:
    - CRITICAL: 10 comfort
    - HIGH: 7 comfort
    - MEDIUM: 5 comfort  
    - LOW: 3 comfort
  - Sets 100% success rate (delivery always succeeds)

### 3. **ConversationManager.cs**
- Enhanced `HandleSpecialCardEffectsAsync()` to handle letter delivery:
  - Gets obligation details before delivery (since it's removed from queue)
  - Calls `queueManager.DeliverObligation()` with the specific letter ID
  - Awards player the letter payment in coins
  - Awards tokens based on letter importance (1-3 tokens)
  - Shows success messages for delivery
  - Adds 5 bonus comfort to conversation

### 4. **CardContext.cs**
- Added `CustomText` and `LetterDetails` properties for delivery card display

## How It Works

1. When starting a conversation with an NPC, the system checks the player's obligation queue
2. For each letter where `RecipientId` matches the NPC's ID, a delivery card is created
3. These cards appear in the player's hand alongside regular conversation cards
4. When played successfully (always succeeds), the letter is delivered and removed from queue
5. Player receives payment and tokens, building relationship with the recipient

## Testing

Created test cases to verify:
- Single letter delivery card generation
- Multiple letters for same NPC
- No cards when no letters for that NPC
- Crisis conversations also get delivery cards
- Correct comfort rewards by importance

## Edge Cases Handled

- Multiple letters for the same NPC (all get cards)
- Letters in crisis conversations
- Proper cleanup when letter is delivered
- Payment and token rewards based on letter importance

## UI Integration Notes

The delivery cards will need UI support to:
- Display letter sender and payment info
- Show as distinct card type (maybe with envelope icon)
- Indicate they're free to play (0 weight)
- Show the comfort and token rewards

## Example Usage

If the player has a letter from Elena to Bertram in their queue:
1. Start conversation with Bertram
2. A "Deliver letter from Elena" card appears in hand
3. Play the card (costs 0 weight)
4. Letter is delivered, removed from queue
5. Player gets payment and tokens
6. Conversation gets +5 comfort bonus