# CORRECT EMOTIONAL STATE RULES FROM POC

## Each State Has SPECIFIC Listen/Speak Rules!

### NEUTRAL
- **Listen**: Draw 2 from conversation deck
- **Speak**: Weight limit 3
- No letter deck checking, no crisis injection

### GUARDED  
- **Listen**: Draw 1 from conversation deck, state→Neutral
- **Speak**: Weight limit 2
- No letter deck checking

### OPEN
- **Listen**: Draw 3 from conversation deck, CHECK LETTER DECK for trust letters
- **Speak**: Weight limit 3
- ⚠️ Only state that checks for TRUST letters specifically!

### TENSE
- **Listen**: Draw 1 from conversation deck, state→Guarded
- **Speak**: Weight limit 1
- No letter deck checking

### EAGER
- **Listen**: Draw 3 from conversation deck
- **Speak**: Weight limit 3
- No letter deck checking

### OVERWHELMED
- **Listen**: Draw 1 from conversation deck, state→Neutral
- **Speak**: Maximum 1 card regardless of weight
- No letter deck checking

### CONNECTED
- **Listen**: Draw 3 from conversation deck, CHECK LETTER DECK for any letters
- **Speak**: Weight limit 4
- ⚠️ Checks for ALL letters regardless of type!

### DESPERATE
- **Listen**: Draw 2 from conversation deck + INJECT 1 from crisis deck, state→Hostile
- **Speak**: Weight limit 3, crisis cards cost 0 weight
- ⚠️ Injects from CRISIS deck, not conversation deck!

### HOSTILE
- **Listen**: Draw 1 from conversation deck + INJECT 2 from crisis deck, conversation ends
- **Speak**: Only crisis cards playable
- ⚠️ Conversation ends after Listen!

## NPC DECK ARCHITECTURE (Four Decks)

Each NPC maintains FOUR separate decks:

### 1. CONVERSATION DECK (20-25 cards)
- Contains: Comfort cards, State cards, Burden cards
- Used by: All emotional states for normal draws
- Depth values: 0-20

### 2. LETTER DECK (Variable size)
- Contains: Letter cards (create obligations)
- Checked during Listen in:
  - OPEN state (trust letters only)
  - CONNECTED state (all letters)
- Has eligibility requirements (tokens, state)

### 3. CRISIS DECK (Usually empty)
- Contains: Crisis cards only
- Injected in:
  - DESPERATE state (1 card on Listen)
  - HOSTILE state (2 cards on Listen)
- When non-empty, forces crisis conversation

### 4. EXCHANGE DECK (Mercantile NPCs only, 5-10 cards)
- Contains: Exchange cards only
- Used for: Quick Exchange (0 attention)
- Not used in standard conversations

## CURRENT IMPLEMENTATION ERRORS

1. **ExecuteListen() is generic** - Should have different behavior per state
2. **Letter deck checked always** - Should only check in OPEN/CONNECTED
3. **Crisis injection wrong** - Should draw from crisis deck, not generate
4. **Weight limits not per-state** - Each state has different weight limit
5. **State transitions missing** - GUARDED→NEUTRAL on Listen, etc.

## ELENA'S DESPERATE CONVERSATION (POC)

Elena starts DESPERATE because deadline < 2 hours:

1. **Listen in DESPERATE**:
   - Draw 2 from conversation deck
   - Inject 1 from crisis deck
   - State transitions to HOSTILE
   - NO letter deck check (not OPEN/CONNECTED)

2. **Speak in DESPERATE**:
   - Weight limit 3
   - Crisis cards cost 0 weight (normally weight 5)
   - Can play mix of regular and crisis cards

This is why Listen is a trap in DESPERATE - it forces HOSTILE state!