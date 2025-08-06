# 🎮 FINAL Systemic Implementation Plan for Wayfarer

## Executive Summary
After consulting all specialized agents, the consensus is clear:
- **Generate choices from queue state FIRST**, not from verbs
- **Use 4 hidden verbs (PLACATE, EXTRACT, DEFLECT, COMMIT)** for backend consistency only
- **Show emotional truth, hide mechanical numbers** 
- **Use templates (~400 pieces)** to manage content scope
- **Maintain GameFacade pattern** for clean architecture

## 🎯 The Correct Design (Agent Consensus)

### Core Philosophy
- **Chen**: "Choices emerge from queue state, not verb menus"
- **Alex**: "Templates beat unique content - 400 pieces, not 20,000"
- **Priya**: "Hide ALL mechanics - no spreadsheets in dialogue"
- **Jordan**: "Show emotional truth, not numbers"
- **Kai**: "VerbContextualizer is salvageable but needs templates"

## 📐 Architecture Overview

```
Queue State
    ↓
NPC Emotional State (DESPERATE/HOSTILE/CALCULATING/WITHDRAWN)
    ↓
Generate Contextual Choices (based on state combinations)
    ↓
Map to Hidden Verbs (for resolution consistency)
    ↓
Present as Narrative (no numbers visible)
    ↓
Resolve Mechanically (update queue/tokens)
```

## 🔧 Implementation Components

### 1. ChoiceGenerator (NEW) - Generate from State, Not Verbs

```csharp
public class ChoiceGenerator
{
    private readonly LetterQueueManager _queueManager;
    private readonly NPCEmotionalStateCalculator _stateCalculator;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly VerbTemplateRepository _templates;
    private readonly GameWorld _gameWorld;
    
    public List<ConversationChoice> GenerateChoices(
        NPC npc,
        AttentionManager attention)
    {
        var choices = new List<ConversationChoice>();
        var player = _gameWorld.GetPlayer();
        
        // 1. ANALYZE QUEUE STATE FIRST
        var npcLetters = _queueManager.GetActiveLetters()
            .Where(l => l.SenderId == npc.ID || l.RecipientId == npc.ID)
            .OrderBy(l => l.DeadlineInDays)
            .ToList();
            
        var mostUrgent = npcLetters.FirstOrDefault();
        var queuePressure = CalculateQueuePressure();
        
        // 2. CALCULATE EMOTIONAL CONTEXT
        var npcState = _stateCalculator.CalculateState(npc);
        var tokens = _tokenManager.GetTokensWithNPC(npc.ID);
        
        // 3. GENERATE CHOICES FROM SITUATION (not verbs!)
        
        // Always free exit
        choices.Add(CreateExitChoice(npcState));
        
        // Context-specific choices based on state
        if (npcState == NPCEmotionalState.DESPERATE && mostUrgent != null)
        {
            // Desperate + has letter = immediate help option
            choices.Add(new ConversationChoice
            {
                ChoiceID = Guid.NewGuid().ToString(),
                NarrativeText = GenerateDesperateHelpText(npc, mostUrgent),
                AttentionCost = 0, // Easier when desperate
                BaseVerb = BaseVerb.COMMIT, // Hidden mapping
                IsAvailable = true
            });
            
            // Can also extract info when desperate
            if (attention.CanAfford(1))
            {
                choices.Add(new ConversationChoice
                {
                    ChoiceID = Guid.NewGuid().ToString(),
                    NarrativeText = "Tell me everything about the situation...",
                    AttentionCost = 1,
                    BaseVerb = BaseVerb.EXTRACT,
                    IsAvailable = true
                });
            }
        }
        else if (npcState == NPCEmotionalState.HOSTILE)
        {
            // Hostile = placate or deflect only
            choices.Add(new ConversationChoice
            {
                ChoiceID = Guid.NewGuid().ToString(),
                NarrativeText = GeneratePlacatingText(npc, tokens),
                AttentionCost = 1,
                BaseVerb = BaseVerb.PLACATE,
                IsAvailable = attention.CanAfford(1)
            });
        }
        
        // Queue manipulation if letters exist
        if (mostUrgent != null && mostUrgent.QueuePosition > 1)
        {
            var reorderCost = CalculateReorderCost(mostUrgent, npcState);
            choices.Add(new ConversationChoice
            {
                ChoiceID = $"reorder_{mostUrgent.Id}",
                NarrativeText = GenerateReorderText(mostUrgent, npcState),
                AttentionCost = reorderCost.attention,
                BaseVerb = BaseVerb.COMMIT,
                IsAvailable = attention.CanAfford(reorderCost.attention),
                // Store metadata for resolution
                Metadata = new Dictionary<string, object>
                {
                    ["letterToReorder"] = mostUrgent.Id,
                    ["tokenCost"] = reorderCost.tokens
                }
            });
        }
        
        // Cap at 5 choices, prioritize by urgency
        return choices.Take(5).ToList();
    }
    
    private string GenerateDesperateHelpText(NPC npc, Letter letter)
    {
        // Use templates based on stakes
        var template = _templates.GetTemplate("desperate_help", letter.Stakes);
        return template.Fill(new
        {
            NpcName = npc.Name,
            StakesHint = letter.GetStakesHint(),
            Deadline = GetDeadlineText(letter.DeadlineInDays)
        });
    }
}
```

### 2. VerbTemplateRepository (NEW) - ~400 Template Pieces

```csharp
public class VerbTemplateRepository
{
    // Templates stored as JSON for easy editing
    private Dictionary<string, TemplateSet> _templates;
    
    public VerbTemplateRepository()
    {
        LoadTemplatesFromJson("Content/Templates/conversation_templates.json");
    }
    
    public NarrativeTemplate GetTemplate(string context, StakeType stakes)
    {
        var key = $"{context}_{stakes}";
        if (_templates.ContainsKey(key))
            return _templates[key].GetRandom(); // Or rotate
            
        // Fallback to generic
        return _templates[$"{context}_generic"].GetRandom();
    }
}

// Example template structure (JSON):
{
  "desperate_help_REPUTATION": [
    "I'll make sure {NpcName}'s letter about {StakesHint} is delivered {Deadline}",
    "Your reputation matters - I'll handle this immediately",
    "{NpcName}, I understand what's at stake. Consider it done."
  ],
  "hostile_placate_generic": [
    "Please, let me explain about the delay...",
    "I know I've failed you before, but hear me out...",
    "Wait - there's more to this situation than you know..."
  ]
}
```

### 3. ChoiceResolver (NEW) - Maps Verbs to Mechanical Effects

```csharp
public class ChoiceResolver
{
    private readonly LetterQueueManager _queueManager;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly ITimeManager _timeManager;
    private readonly MessageSystem _messageSystem;
    
    public ConversationOutcome ResolveChoice(
        ConversationChoice choice,
        NPC npc,
        GameWorld gameWorld)
    {
        var outcome = new ConversationOutcome();
        
        // Spend attention first
        var attention = gameWorld.GetCurrentConversation()?.AttentionManager;
        if (attention != null && choice.AttentionCost > 0)
        {
            attention.TrySpend(choice.AttentionCost);
        }
        
        // Resolve based on hidden verb
        switch (choice.BaseVerb)
        {
            case BaseVerb.COMMIT:
                if (choice.Metadata?.ContainsKey("letterToReorder") == true)
                {
                    // Queue reordering commitment
                    var letterId = choice.Metadata["letterToReorder"].ToString();
                    var tokenCost = (int)choice.Metadata["tokenCost"];
                    
                    _queueManager.ReorderLetter(letterId, 1);
                    _tokenManager.BurnTokens(npc.ID, ConnectionType.Status, tokenCost);
                    
                    outcome.NarrativeResult = GenerateCommitmentNarrative(npc, letterId);
                    outcome.EmotionalFeedback = "Relief washes over their face";
                }
                else
                {
                    // General commitment to help
                    _tokenManager.AddTokens(npc.ID, ConnectionType.Trust, 1);
                    outcome.NarrativeResult = "You've strengthened your bond";
                    outcome.EmotionalFeedback = "They seem to trust you more";
                }
                break;
                
            case BaseVerb.PLACATE:
                // Reduces hostility, costs time
                _timeManager.AdvanceTime(30); // 30 minutes
                if (npc.GetEmotionalState() == NPCEmotionalState.HOSTILE)
                {
                    // Temporarily reduce hostility
                    outcome.NarrativeResult = "The tension eases slightly";
                    outcome.EmotionalFeedback = "Their shoulders relax a fraction";
                }
                break;
                
            case BaseVerb.EXTRACT:
                // Gain information, reveal routes
                var info = GenerateInformation(npc);
                gameWorld.GetPlayer().AddKnownInformation(info);
                _timeManager.AdvanceTime(20);
                
                outcome.NarrativeResult = $"You learn: {info.Description}";
                outcome.EmotionalFeedback = "They share something valuable";
                break;
                
            case BaseVerb.DEFLECT:
                // Redirect pressure, may damage relationship
                _tokenManager.BurnTokens(npc.ID, ConnectionType.Trust, 1);
                outcome.NarrativeResult = "You redirect the conversation";
                outcome.EmotionalFeedback = "They notice your evasion";
                break;
        }
        
        // Add message for UI feedback (no numbers!)
        _messageSystem.AddMessage(new SystemMessage
        {
            Text = outcome.EmotionalFeedback,
            Type = MessageType.Conversation,
            Duration = 3000
        });
        
        return outcome;
    }
}
```

### 4. DeterministicConversationManager (REPLACE AI)

```csharp
public class DeterministicConversationManager : INarrativeProvider
{
    private readonly ChoiceGenerator _choiceGenerator;
    private readonly ChoiceResolver _choiceResolver;
    private readonly SystemicDialogueGenerator _dialogueGenerator;
    
    public async Task<string> GenerateIntroduction(
        SceneContext context, 
        ConversationState state)
    {
        // Generate NPC opening based on their emotional state
        var npc = context.TargetNPC;
        var npcState = context.NPCEmotionalState;
        var relevantLetter = context.RelevantLetters?.FirstOrDefault();
        
        return _dialogueGenerator.GenerateOpening(npc, npcState, relevantLetter);
    }
    
    public async Task<List<ConversationChoice>> GenerateChoices(
        SceneContext context,
        ConversationState state,
        List<ChoiceTemplate> templates) // Ignore AI templates
    {
        // Generate from state, not templates
        return _choiceGenerator.GenerateChoices(
            context.TargetNPC,
            context.AttentionManager);
    }
    
    public async Task<string> GenerateReaction(
        SceneContext context,
        ConversationState state,
        ConversationChoice selectedChoice,
        bool success)
    {
        // Resolve the choice mechanically
        var outcome = _choiceResolver.ResolveChoice(
            selectedChoice,
            context.TargetNPC,
            context.GameWorld);
            
        return outcome.NarrativeResult;
    }
}
```

### 5. Emotional Transparency System (NO NUMBERS!)

```csharp
public class EmotionalFeedbackGenerator
{
    public string GenerateFeedback(ConversationOutcome outcome, NPC npc)
    {
        // NEVER show: "+2 Trust, -1 Commerce"
        // ALWAYS show emotional truth
        
        if (outcome.TokensGained?.Any() == true)
        {
            var tokenType = outcome.TokensGained.First().Key;
            return tokenType switch
            {
                ConnectionType.Trust => $"{npc.Name}'s eyes soften with gratitude",
                ConnectionType.Commerce => $"{npc.Name} nods approvingly at your professionalism",
                ConnectionType.Status => $"{npc.Name} acknowledges your standing",
                ConnectionType.Shadow => $"A knowing look passes between you",
                _ => $"{npc.Name} seems pleased"
            };
        }
        
        if (outcome.TokensBurned?.Any() == true)
        {
            return $"You sense {npc.Name}'s disappointment";
        }
        
        return outcome.EmotionalFeedback;
    }
}
```

## 📊 Content Scope (Alex's Template Approach)

### Total Content: ~400 Pieces
- **50 base templates** with variation points
- **80 stakes modifiers** (20 per stake type)
- **80 context flavors** (20 per token type)
- **100 pressure indicators** (20 per pressure level)
- **40 personality modifiers** (10 per archetype)
- **50 emotional feedback phrases**

### Template Example
```json
{
  "template": "{GREETING}, {STAKES_OPENER} {DEADLINE_PRESSURE}",
  "variations": {
    "GREETING": ["*catches your eye*", "*leans forward*", "*voice drops*"],
    "STAKES_OPENER": ["about {StakesHint}", "concerning {StakesHint}", "regarding {StakesHint}"],
    "DEADLINE_PRESSURE": ["needs immediate attention", "can't wait", "time is running out"]
  }
}
```

## 🎨 UI Presentation (Priya & Jordan's Vision)

### What Players See
```
┌─────────────────────────────────────┐
│ Elena leans forward, fingers         │
│ worrying her shawl                    │
│                                      │
│ "The letter contains Lord Aldwin's   │
│ marriage proposal. My refusal."      │
│                                      │
│ Her hand reaches toward yours,       │
│ trembling slightly                    │
│                                      │
│ > Take her trembling hand in comfort │
│ > "I'll handle your letter first"    │
│ > Ask about Lord Aldwin quietly      │
└─────────────────────────────────────┘
```

### What Players DON'T See
- ❌ "+2 Trust, -1 Commerce"
- ❌ "Burns 3 Status tokens"
- ❌ "Queue position: 8 → 1"
- ❌ "Attention cost: ◆◆"

### What Players Learn Through Play
- Taking someone's hand when desperate strengthens bonds
- Prioritizing letters has social costs
- Information flows freely from desperate people
- Time spent listening matters

## 🔨 Implementation Order

### Phase 1: Core Systems (Week 1)
1. **ChoiceGenerator** - 16 hours
2. **VerbTemplateRepository** - 8 hours
3. **ChoiceResolver** - 8 hours
4. **DeterministicConversationManager** - 8 hours

### Phase 2: Content Creation (Week 2)
1. **Base templates** - 20 hours
2. **Modular components** - 20 hours

### Phase 3: Integration (Week 3)
1. **Wire through GameFacade** - 8 hours
2. **Update ConversationScreen** - 4 hours
3. **Testing & polish** - 28 hours

**Total: 120 hours (3 weeks)**

## ✅ Success Criteria

1. **Choices emerge from queue state** - Not verb menus
2. **No numbers visible to players** - Emotional feedback only
3. **Templates create variety** - 400 pieces → thousands of combinations
4. **Hidden verbs stay hidden** - Backend consistency only
5. **Conversation feels human** - Not mechanical

## 🚫 What We're NOT Building

- ❌ Unique dialogue for every NPC (use 4 archetypes)
- ❌ Mechanical preview system (discover through play)
- ❌ AI-generated content (deterministic templates)
- ❌ Visible verb selection (choices emerge from context)
- ❌ Number-based feedback (emotional truth only)

## 🎯 The Final Test

When a player finishes a conversation, they should think:
- "I just betrayed Elena's trust to help Marcus"
- NOT "I just optimized my token distribution"

The queue creates the pressure. The choices emerge from that pressure. The verbs ensure consistency. But the player experiences only the human story.

**This is how we create systemic narrative that feels human.**