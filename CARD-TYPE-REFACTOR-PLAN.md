# Card Type System Refactoring Plan

## Problem Statement
The card system conflates TYPE (what kind of card) with PROPERTIES (mechanical behaviors). This creates confusion where:
- Exchange cards don't have CardProperty.Exchange set because the parser doesn't convert "type" to properties
- Properties enum contains type-related values (Exchange, GoalCard, Observable) that should be types
- Multiple overlapping type systems (CardType enum, Properties, computed properties)
- Game logic checks Properties for type instead of a dedicated type field

## Design Principles
1. **Separation of Concerns**: Card TYPE (what it is) vs PROPERTIES (how it behaves mechanically)
2. **Single Source of Truth**: One CardType field determines card type
3. **Parser Must Parse**: Convert JSON strings to proper enums, not pass through
4. **No Special Classes**: One ConversationCard class for all cards (delete RequestCard)

## Important Clarifications

### Burden vs BurdenGoal
- **Burden PROPERTY**: Cards that clog the deck/hand as negative consequences (mechanical behavior)
- **BurdenGoal TYPE**: Goal cards in the burden deck that enable "Make Amends" conversations
- These are completely different concepts that were conflated in the old system

### Promise vs Letter vs Delivery
- **Letter TYPE**: Goal cards that enable "Letter Offer" conversations. When played, creates BOTH an obligation AND a letter item in player's satchel
- **Promise TYPE**: Goal cards that create/modify obligations WITHOUT creating a letter item (pure obligation management)
- **Delivery**: Not a card type but a conversation option generated when player has both a letter item AND an obligation for the same recipient
- Key distinction: Letters create physical items + obligations, Promises only create obligations

## Implementation Work Packets

### Work Packet 1: Update Type System
**Files**: CardType.cs, CardProperty.cs
- Update CardType enum: Conversation, Exchange, Letter, Promise, BurdenGoal, Observation
- Remove type-related values from CardProperty enum (Exchange, GoalCard, Observable)
- Keep only mechanical properties: Persistent, Impulse, Opening, Skeleton, Burden, Unplayable
- Note: Burden PROPERTY (clogs deck) is different from BurdenGoal TYPE (enables Make Amends)
- Note: Letter TYPE creates items+obligations, Promise TYPE only creates obligations

### Work Packet 2: Update Card Model
**Files**: ConversationCard.cs, RequestCard.cs, GoalCard.cs
- Add `CardType CardType` property to ConversationCard
- Remove computed Type property that derives from Properties
- Remove Category property and helpers
- Delete RequestCard class (use CardType.Goal instead)
- Update any GoalCard references to use base ConversationCard

### Work Packet 3: Fix Parser
**Files**: ConversationCardParser.cs
- Parse dto.Type field and set card.CardType:
  - "Exchange" → CardType.Exchange
  - "Letter" → CardType.Letter
  - "Promise" → CardType.Promise
  - "BurdenGoal" → CardType.BurdenGoal
  - "Observation" → CardType.Observation
  - "Goal" → Determine based on effect (Letter if creates letter, Promise if not)
  - Default/null → CardType.Conversation
- Stop creating RequestCard instances
- Properties array only for mechanical behaviors

### Work Packet 4: Update Game Logic
**Files**: CardEffectProcessor.cs, ExchangeHandler.cs, SessionCardDeck.cs
- Change all `Properties.Contains(CardProperty.Exchange)` to `CardType == CardType.Exchange`
- Change all `Properties.Contains(CardProperty.GoalCard)` to `CardType == CardType.Goal`
- Change all `Properties.Contains(CardProperty.Observable)` to `CardType == CardType.Observation`
- Fix exchange card success calculation to check CardType

### Work Packet 5: Update UI Components
**Files**: ConversationContent.razor.cs, NPCDeckViewer.razor, CardDisplay.razor
- Update all UI components checking Properties for type
- Use CardType for styling and display logic
- Remove references to deleted properties

### Work Packet 6: Update Card Instances
**Files**: CardInstance.cs
- Update helper properties to use CardType
- Remove IsExchange property that checks Properties
- Add proper type checking methods if needed

## Success Criteria
1. Exchange cards have 100% success rate without crashes
2. All cards have explicit CardType set
3. No code checks Properties for card type
4. Parser converts JSON type to CardType enum
5. No RequestCard subclass exists
6. Game builds and runs without errors