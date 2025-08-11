# Transparent Mechanics UI Implementation

## Overview
Updated the UI components to show simplified game mechanics transparently, making formulas and calculations visible to players rather than hiding them behind narrative descriptions.

## Design Philosophy
- **Show the Math**: Players see "3 positions = 3 tokens" directly
- **Formula Transparency**: Emotional state = Stakes + Time is clearly displayed
- **Elegant Simplicity**: Simple rules presented beautifully, not as spreadsheets
- **Progressive Disclosure**: Complex math shown contextually when relevant

## Components Created

### 1. EmotionalStateDisplay.razor
- Shows NPC emotional state with transparent formula
- Displays: State = Stakes + Time Remaining
- Color-coded state badges (Anxious=Orange, Hostile=Dark Red, Closed=Gray, Neutral=Brown)
- Lists mechanical effects of each state

### 2. QueueManipulationPreview.razor
- Shows exact queue math: "Move from Position 5 → 2 = 3 tokens (3 positions)"
- Displays before/after token balances
- Clear cost calculations for all operations
- Visual confirmation dialogs with costs

### 3. Enhanced TokenDisplay.razor
- Shows token values with mechanical effects
- Formulas visible: Trust +3 = [+6h deadline bonus]
- Leverage warnings for negative tokens
- Color-coded positive/negative values

## UI Updates Made

### UnifiedChoice.razor
Enhanced to show transparent mechanics in choice previews:
- Token math displayed as "+3 Trust" or "-2 Commerce"
- Queue manipulation costs shown with position calculations
- State change previews with color coding
- Time calculations visible ("+4h deadline")

### ConversationScreen.razor
Integrated EmotionalStateDisplay component to replace hidden state tracking:
- Shows emotional state formula at top of conversation
- Derives state from stakes and time pressure
- Displays mechanical effects of current state

## CSS Enhancements (conversation.css)

### New Visual Elements
- `.token-math`: Displays token calculations with color coding
- `.formula`: Shows mathematical formulas in subtle backgrounds
- `.state-indicator`: Color-coded emotional state badges
- `.leverage-warning`: Red warnings for debt/leverage situations
- `.cost-calculation`: Formatted cost breakdowns

### Color Scheme
- Anxious: `#ffa500` (Orange)
- Hostile: `#8b0000` (Dark Red)  
- Closed: `#696969` (Gray)
- Neutral: `#6b5d4f` (Brown)
- Positive Effects: `#4caf50` (Green)
- Negative Effects: `#d32f2f` (Red)

## Mechanical Transparency Features

### 1. Emotional State Formula
```
Elena [ANXIOUS]
↳ Reputation at stake (urgent letter) + 3h remaining
```

### 2. Queue Math Display
```
Move Letter: Position 5 → Position 2
Cost Formula: 3 positions × 1 token/position = 3 Commerce tokens
Your Balance: 5 → 2
```

### 3. Token Mechanics
```
Trust: ● +3 [+6h deadline bonus]
Commerce: 🪙 x5 [Queue boost +2]
Status: ◆ 2 [No tier access]
⚠️ Leverage: Trust -2 [Letters at position 2]
```

### 4. Choice Previews
Before selecting any action, players see:
- Exact token costs and resulting balances
- State changes that will occur (Anxious → Hostile)
- Queue position changes with math
- Time costs in clear units

## Implementation Notes

### Challenges Addressed
1. **Missing Methods**: GameFacade didn't have all needed methods, so we derive data from existing ViewModels
2. **Enum Limitations**: EmotionalState enum only had 4 states (Neutral, Anxious, Hostile, Closed)
3. **ViewModel Properties**: LetterViewModel lacks Subject/PreviewText, using other properties to infer stakes

### Future Improvements
1. Add GetNpcEmotionalState() method to GameFacade for direct state access
2. Extend EmotionalState enum if more nuanced states needed
3. Add mechanical preview generation to choice factories
4. Create tooltip system for formula explanations

## Benefits of Transparency

### For Players
- Learn game rules through play, not manuals
- Make informed decisions with visible consequences
- Understand why things happen (not arbitrary)
- Feel in control of outcomes

### For Design
- Simpler rules are easier to balance
- Bugs become obvious when math is visible
- Players can optimize strategies
- Emergent gameplay from understood systems

## Testing Checklist
- [ ] Emotional state formulas display correctly
- [ ] Queue manipulation shows position math
- [ ] Token displays show mechanical effects
- [ ] Choice previews show all costs
- [ ] Color coding is consistent
- [ ] Formulas update in real-time
- [ ] Mobile responsive design maintained