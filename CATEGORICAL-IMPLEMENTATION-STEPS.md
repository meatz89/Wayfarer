# Categorical Card System Implementation Steps

## Current Status
We are converting the card system from property-based to categorical, where card effects are determined by context (difficulty → magnitude) rather than hardcoded values.

## Step 1: Fix Atmosphere System ✅ IN PROGRESS
- [x] Update CategoricalEffectResolver to use magnitude for atmosphere
- [ ] Add GetAtmosphereFromMagnitude method
- [ ] Remove TargetAtmosphere property from ConversationCard
- [ ] Remove TargetAtmosphere property from CardInstance
- [ ] Remove targetAtmosphere parsing from ConversationCardParser

**Atmosphere Progression by Magnitude:**
- Magnitude 1: Patient (0 patience cost)
- Magnitude 2: Receptive (+1 card on LISTEN)
- Magnitude 3: Focused (+20% success all cards)
- Magnitude 4: Synchronized (next effect happens twice)

## Step 2: Convert Starter Deck (20 cards)
Replace existing cards in core_game_package.json with narratively meaningful cards:

1. **"Tell me what happened"** - Conversation/Trust/F2/Easy/Thought/Rapport/None/None
2. **"I've been there too"** - Conversation/Trust/F3/Medium/Impulse/Rapport/Backfire/None
3. **"You're right about that"** - Conversation/Trust/F1/Easy/Opening/Rapport/None/None
4. **"What would you do?"** - Conversation/Trust/F2/Easy/Thought/Threading/None/None
5. **"I need your advice"** - Conversation/Trust/F3/Medium/Impulse/Rapport/Backfire/None
6. **"That must have been difficult"** - Conversation/Trust/F1/VeryEasy/Thought/Rapport/None/None
7. **"Let me help you with that"** - Conversation/Trust/F2/Medium/Opening/Rapport/None/Threading
8. **"I trust your judgment"** - Conversation/Trust/F3/Medium/Impulse/Advancing/Overreach/None
9. **"What's your price?"** - Conversation/Commerce/F1/Easy/Thought/Threading/None/None
10. **"That's fair"** - Conversation/Commerce/F2/Easy/Opening/Rapport/None/None
11. **"Can we make a deal?"** - Conversation/Commerce/F3/Medium/Impulse/Atmospheric/Backfire/None
12. **"What if I throw in..."** - Conversation/Commerce/F2/Medium/Impulse/Threading/None/Focusing
13. **"You drive a hard bargain"** - Conversation/Commerce/F1/VeryEasy/Thought/Rapport/None/None
14. **"Let's shake on it"** - Conversation/Commerce/F3/Hard/Impulse/Advancing/Overreach/Regret
15. **"I know your secret"** - Conversation/Trust/F4/Hard/Impulse/Advancing/Overreach/Regret (THE CONFESSION)
16. **"You're not telling me everything"** - Conversation/Trust/F3/Hard/Impulse/Threading/Backfire/None
17. **"I'll make it worth your while"** - Conversation/Commerce/F4/Hard/Impulse/Advancing/Backfire/None
18. **"Between you and me..."** - Conversation/Trust/F2/Medium/Impulse/Atmospheric/None/Threading
19. **"I heard something interesting"** - Conversation/Trust/F1/Easy/Opening/Threading/None/None
20. **"This is important"** - Conversation/Trust/F2/Medium/Thought/Focusing/None/None

## Step 3: JSON Format Conversion
Convert from:
```json
{
  "id": "i_hear_you",
  "type": "Normal",
  "properties": ["Persistent"],
  "successEffect": { "type": "AddRapport", "value": "1" }
}
```

To:
```json
{
  "id": "tell_me_what_happened",
  "type": "Conversation",
  "personalityTypes": ["ALL"],
  "focus": 2,
  "persistence": "Thought",
  "successType": "Rapport",
  "failureType": "None",
  "exhaustType": "None",
  "connectionType": "Trust",
  "description": "Tell me what happened",
  "difficulty": "Easy",
  "dialogueFragment": "Tell me what happened. I want to understand."
}
```

## Step 4: Remove Legacy Code
- [ ] Delete all references to CardProperty enum
- [ ] Delete all references to CardEffect class
- [ ] Remove "properties" array parsing
- [ ] Remove effect object parsing
- [ ] Update tests to use categorical properties

## Step 5: UI Updates
- [ ] Update GetCategoryClass in NPCDeckViewer
- [ ] Display persistence badges (Thought/Impulse/Opening)
- [ ] Display success/failure/exhaust type badges
- [ ] Show magnitude based on difficulty

## Step 6: Testing
- [ ] Verify Thought cards survive LISTEN
- [ ] Verify Impulse cards removed after SPEAK
- [ ] Verify Opening cards removed after LISTEN
- [ ] Test all success effect types with proper magnitudes
- [ ] Test failure effects (especially Overreach)
- [ ] Test exhaust effects
- [ ] Verify atmosphere progression by magnitude

## Key Principles
- **NO HARDCODED VALUES**: Everything derives from difficulty → magnitude
- **NO BACKWARD COMPATIBILITY**: Clean break, delete legacy code
- **NARRATIVE PURPOSE**: Every card advances the story
- **VERISIMILITUDE**: Cards represent real conversational moves