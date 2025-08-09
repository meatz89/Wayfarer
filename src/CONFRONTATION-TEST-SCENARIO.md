# Confrontation System Test Scenario

## Setup: Creating a Failed Delivery

To test the confrontation system, you need to:

1. **Start the game** and accept letters from NPCs
2. **Let a letter expire** by not delivering it before the deadline
3. **Visit the NPC** whose letter expired

## Expected Confrontation Flow

### Initial Visit After Failure (Anxious State)

**Trigger**: Visit Elena after her first letter expires

**Expected Opening** (forced, no player choice):
```
[Elena looks up as you enter, her face crumbling from hope to despair]

"You promised me urgent trade agreement would reach the merchant guild. 
I trusted you... Was I wrong to do that? Everything depended on that delivery."

[wringing hands, eyes searching your face for answers]
```

**Player Choices** (all free, no attention cost):
1. "I'm sorry. I failed you. There's no excuse."
2. "I had impossible choices. Someone was going to suffer no matter what."
3. "..." [Accept their anger in silence]

**Expected Responses**:
- Choice 1: "Sorry doesn't bring back what was lost. But... perhaps there's still time to fix some of this."
- Choice 2: "There's always a more important letter, isn't there? I thought mine mattered too."
- Choice 3: "Your silence says enough. Please... just deliver the next one on time."

**Mechanical Effects**:
- Leverage: +2 (Elena gains power over you)
- Trust tokens: -3
- Redemption Progress: +1 (if apologetic or silent)
- Future letters from Elena will have higher queue priority

### Second Failure (Hostile State)

**Expected Opening**:
```
"You again? After what you've cost me? urgent trade agreement never reached the merchant guild. 
Do you have any idea what you've destroyed?"

[fists clenched, jaw tight with barely contained anger]
```

**Effects**:
- HELP verb locked with Elena
- Network effect: nearby NPCs lose trust
- Higher leverage and token penalties

### Third+ Failure (Closed State)

**Cannot Start Conversation**:
- Elena refuses to speak with you
- Must work through other NPCs to rebuild
- Some options permanently lost

## Testing the Recovery Mechanic

After a confrontation:
1. Successfully deliver Elena's next 2-3 letters on time
2. Redemption Progress increases with each success
3. Once threshold reached (3 for Anxious→Neutral), relationship improves slightly
4. Some scars remain permanent (reduced trust ceiling)

## Key Elements to Verify

### Emotional Authenticity
- [ ] Confrontation feels like a real person's reaction
- [ ] Specific letter details are referenced
- [ ] Body language matches emotional state
- [ ] No generic "quest failed" language

### Mechanical Integration
- [ ] Confrontation triggers on first visit after failure
- [ ] Free attention (no cost for confrontation)
- [ ] Limited choices (max 3, all acknowledge failure)
- [ ] Leverage and token changes apply correctly

### Visual/UI Elements
- [ ] Forced opening text appears without choice
- [ ] Body language displayed prominently
- [ ] Attention bar shows as "free" or full
- [ ] Darker tone/mood in UI

### Recovery Path
- [ ] Redemption progress tracked correctly
- [ ] Multiple successes required for improvement
- [ ] Some permanent consequences remain
- [ ] Alternative help methods available

## Console Commands for Testing

To quickly test confrontations without waiting for natural letter expiration:

```csharp
// In game console or debug mode:
// 1. Add letter history with expired count
player.NPCLetterHistory["elena_id"] = new LetterHistory { ExpiredCount = 1 };

// 2. Visit Elena to trigger confrontation
GameFacade.StartConversationAsync("elena_id");
```

## Success Criteria

The confrontation system succeeds when:
1. Players feel genuine guilt/shame for failures
2. NPCs feel like real people with justified anger
3. Recovery feels earned, not purchased
4. Consequences create memorable storytelling moments
5. The "walk of shame" motivates different player behavior

## Common Issues to Watch For

1. **Confrontation not triggering**: Check ExpiredCount > LastConfrontationCount
2. **Wrong emotional state**: Verify failure count mapping
3. **Generic responses**: Ensure specific letter details are pulled
4. **Too harsh/lenient**: Balance leverage and recovery thresholds
5. **Breaking normal conversations**: Ensure clean separation of systems