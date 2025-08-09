# Confrontation System Design
## Making Consequences Personal and Human

### Core Philosophy
When players fail to deliver letters, the consequences shouldn't just be mechanical (token loss, leverage gain). NPCs should react like real people whose trust has been broken. Confrontation scenes create memorable "walk of shame" moments that make players feel the human cost of their failures.

### System Architecture

#### 1. Detection Logic
Confrontations trigger when:
- Player visits an NPC with `ExpiredCount > LastConfrontationCount`
- NPC is not in `Closed` state (Closed NPCs refuse all interaction)
- This is the first visit since the failure occurred

```csharp
// In GameFacade.StartConversationAsync
private bool ShouldTriggerConfrontation(string npcId)
{
    var history = _gameWorld.GetPlayer().NPCLetterHistory.GetValueOrDefault(npcId);
    if (history == null || history.ExpiredCount == 0) return false;
    
    var npc = _npcRepository.GetById(npcId);
    if (npc.EmotionalState == EmotionalState.Closed) return false;
    
    return history.ExpiredCount > npc.LastConfrontationCount;
}
```

#### 2. Confrontation Structure

##### Forced Opening
- NPC speaks FIRST - no player choice
- Immediate emotional impact
- References the specific failed letter

##### Free Attention
- Confrontations cost NO attention points
- The NPC WANTS this conversation
- Player can't escape by claiming "no attention"

##### Limited Choices (Max 3)
All choices acknowledge the failure:
1. **Apologetic**: "I'm sorry. I failed you."
2. **Defensive**: "I had impossible choices to make."
3. **Silent**: "..." (Accept their anger)

No "exit" option during initial confrontation beat.

#### 3. Emotional State Variants

##### ANXIOUS (1 failure)
**Opening**: Hurt and confused, seeking explanation
```
"You promised me this would be delivered. I trusted you... 
Was I wrong to do that? The merchant agreement is ruined now."
```

**Body Language**: "wringing hands, unable to meet your eyes"

**Recovery Path**: 
- Deliver next 2 letters on time
- Emotional state returns to Neutral
- Some trust restored but ceiling lowered

##### HOSTILE (2 failures)
**Opening**: Angry and accusatory
```
"You again? After what you cost me? That letter contained 
the deed to my father's shop. It's gone now. GONE!"
```

**Body Language**: "fists clenched, voice rising with each word"

**Recovery Path**:
- Cannot use HELP verb anymore
- Must complete special obligation quest
- Relationship permanently scarred

##### CLOSED (3+ failures)
**Opening**: Complete rejection
```
[Elena turns away the moment she sees you]
"No. Leave. There's nothing left to say."
```

**Body Language**: "back turned, refusing to acknowledge your presence"

**Recovery Path**:
- No direct recovery possible
- Must work through other NPCs
- Relationship effectively destroyed

### 4. Specific Consequence References

Confrontations MUST reference what was specifically lost:

##### By Stakes Type:
- **WEALTH**: "The trade route is lost. Three months of negotiations, wasted."
- **REPUTATION**: "My name is mud now. No one at court will receive me."
- **SAFETY**: "My brother needed that medicine. He's... he's getting worse."
- **SECRET**: "They know. Everyone knows now. I have to leave the city."

##### By Letter Content:
Pull actual letter properties to make it specific:
- Sender → Recipient relationship
- What was being communicated
- The deadline that was missed
- How long it's been expired

### 5. Recovery Mechanics

#### NOT Just Token Payment
Paying back tokens is mechanical and cheap. True recovery requires:

##### Consistent Actions Over Time
- Track "RedemptionProgress" per NPC
- Requires multiple successful deliveries
- Each success adds +1 progress
- Need 3-5 progress to restore one emotional level

##### Permanent Losses
Some things never come back:
- Certain conversation options locked forever
- Trust token ceiling permanently reduced
- Special quests no longer available
- Information they'll never share

##### Alternative Paths
When direct trust is broken, create new ways to help:
- Can't deliver letters? Help with local problems
- Can't negotiate? Provide information instead
- Can't investigate? Offer protection/escort

### 6. Implementation Details

#### NPC Model Changes
```csharp
class NPC {
    // Existing
    EmotionalState CurrentEmotionalState
    
    // New
    int LastConfrontationCount // Track confrontations shown
    int RedemptionProgress     // Progress toward recovery
    bool HasPermanentScar      // Some wounds don't heal
}
```

#### Confrontation Templates
```csharp
class ConfrontationTemplate {
    EmotionalState RequiredState
    string OpeningDialogue
    string BodyLanguage
    List<ConfrontationChoice> AvailableResponses
    string SpecificLossReference // Pull from actual letter
}
```

#### Integration Flow
1. `GameFacade.StartConversationAsync` checks for confrontation
2. If triggered, use `StartConfrontationAsync` instead
3. Load appropriate `ConfrontationTemplate` based on state
4. Force opening dialogue (no choice)
5. Present limited confrontation choices
6. Update `LastConfrontationCount` after scene
7. Return to normal conversation flow (if NPC willing)

### 7. Visual/Audio Cues

#### UI Changes During Confrontation
- Darker dialogue box border
- Subtle screen shake on accusations
- Red tint on harsh words
- Attention bar hidden (it's free)

#### Body Language Prominence
- Displayed ABOVE dialogue
- Larger font or different color
- Updates with each beat

### 8. Edge Cases

#### Multiple Expired Letters
- Reference the MOST IMPORTANT one
- Mention "and that's not all..." for multiple
- Leverage stacks for each failure

#### During Time-Critical Moments
- Confrontations are FAST (1-2 beats max)
- Can't be skipped but won't trap player
- Time still advances normally

#### Closed NPCs
- Won't even trigger conversation start
- Show "Elena refuses to speak with you"
- Must find alternative approaches

### 9. Testing Criteria

#### Emotional Authenticity
- [ ] Confrontations feel like real human reactions
- [ ] Specific losses are referenced, not generic
- [ ] Body language matches emotional state
- [ ] Recovery feels earned, not purchased

#### Mechanical Integration
- [ ] Triggers correctly on first visit after failure
- [ ] Doesn't repeat same confrontation
- [ ] Integrates with existing conversation flow
- [ ] Saves/loads correctly mid-confrontation

#### Player Impact
- [ ] Creates memorable "walk of shame" moments
- [ ] Makes consequences feel personal
- [ ] Motivates different player behavior
- [ ] Doesn't feel like punishment, but consequence

### 10. Example Confrontation Flow

**Scenario**: Player failed to deliver Elena's letter about her father's shop deed

**Detection**: 
- Elena has ExpiredCount: 1, LastConfrontationCount: 0
- Player enters Elena's location
- Confrontation triggers

**Forced Opening**:
```
[Elena looks up as you enter, her face crumbling from hope to despair]

"You... you didn't deliver it, did you? The deed to my father's 
shop. It had to be filed yesterday for the inheritance claim."

[shoulders sagging, voice barely a whisper]

"Twenty years he built that business. Gone. Because I trusted 
the wrong person."
```

**Player Choices**:
1. "I'm sorry. I had too many urgent letters. I failed you."
2. "The Baron's letter was going to get someone killed. I had to choose."
3. "..." [Accept her grief in silence]

**Resolution** (based on choice):
- Choice 1: "Sorry doesn't bring back twenty years of work."
- Choice 2: "There's always a more important letter, isn't there?"
- Choice 3: "Please leave. I need... I need to be alone."

**Aftermath**:
- Elena's state → ANXIOUS
- Leverage: +3
- Trust tokens: -3
- Permanent: Shop inheritance quest locked
- Recovery: Must deliver next 2 letters on time

### Conclusion

Confrontation scenes transform mechanical failures into human moments. They're not about punishing players but about making them feel the weight of their choices through authentic emotional reactions. Every failed delivery has a face, a name, and a specific loss attached to it.

The system succeeds when players remember not the -3 tokens, but Elena's shoulders sagging as she realizes her father's legacy is lost forever.