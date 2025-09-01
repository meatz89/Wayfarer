# Wayfarer Refined Design: Implementation Work Packets

## Overview
This document contains discrete, self-contained work packets for implementing the refined Wayfarer card-based conversation system. Each packet is designed to be completed independently with clear success criteria and no dependencies on future work.

## Critical Implementation Rules
1. **NO FALLBACKS**: Implement completely or not at all
2. **NO COMPATIBILITY LAYERS**: Delete old code immediately when replacing
3. **NO TODOS**: Every packet must produce production-ready code
4. **NO LEGACY CODE**: Remove all old implementations entirely
5. **COMPLETE VERIFICATION**: Each packet must be tested and verified before moving to next

---

## PACKET 1: Card Model Architecture
**Agent**: systems-architect
**Duration**: 4 hours
**Dependencies**: None

### Objectives
- Create clean single-effect card structure
- Implement strict card types with no multi-effects
- Add token type property to all cards
- Delete ALL legacy card implementations

### Implementation Tasks
1. Delete existing card models that don't conform to single-effect rule
2. Create new card type hierarchy:
   ```csharp
   public enum CardEffectType
   {
       FixedComfort,      // +1 to +5 or -1 to -3 comfort
       ScaledComfort,     // +X where X varies
       DrawCards,         // Draw 1 or 2 cards
       AddWeight,         // Add 1 or 2 to weight pool
       SetAtmosphere,     // Change atmosphere
       ObservationEffect, // Unique observation effects
       GoalEffect        // End conversation with outcome
   }
   
   public enum TokenType
   {
       Trust,
       Commerce,
       Status,
       Shadow
   }
   
   public class ConversationCard
   {
       public string Id { get; set; }
       public string Name { get; set; }
       public TokenType TokenType { get; set; }
       public int Weight { get; set; }
       public DifficultyTier Difficulty { get; set; }
       public bool IsFleeting { get; set; }
       public CardEffectType EffectType { get; set; }
       public int EffectValue { get; set; } // For fixed effects
       public string ScalingFormula { get; set; } // For scaled effects
       public AtmosphereType? AtmosphereChange { get; set; }
       public bool HasFinalWord { get; set; } // For goal cards
   }
   ```

3. Create observation card structure:
   ```csharp
   public class ObservationCard : ConversationCard
   {
       public ObservationEffectType UniqueEffect { get; set; }
       public int ExpirationHours { get; set; }
       
       // Constructor enforces observation rules
       public ObservationCard()
       {
           Weight = 1;
           Difficulty = DifficultyTier.VeryEasy; // 85%
           IsFleeting = false; // Always persistent
       }
   }
   ```

4. Create goal card structure with Final Word:
   ```csharp
   public class GoalCard : ConversationCard
   {
       public GoalType GoalType { get; set; }
       public ObligationType CreatesObligation { get; set; }
       public SuccessTerms SuccessTerms { get; set; }
       public FailureTerms FailureTerms { get; set; }
       
       public GoalCard()
       {
           Weight = 5; // or 6
           Difficulty = DifficultyTier.VeryHard; // 40%
           IsFleeting = true;
           HasFinalWord = true; // If discarded, conversation fails
       }
   }
   ```

### Verification Criteria
- [ ] No card can have multiple effects
- [ ] Every card has exactly ONE TokenType
- [ ] Observation cards always Weight 1, 85% success
- [ ] Goal cards always Weight 5-6, 40% success, fleeting
- [ ] No legacy card classes remain in codebase
- [ ] Compiler shows zero errors

### Agent Validation Command
```bash
dotnet build
grep -r "class.*Card" src/ --include="*.cs" | grep -v "ConversationCard\|ObservationCard\|GoalCard"
```

---

## PACKET 2: JSON Card Definition System
**Agent**: content-integrator
**Duration**: 6 hours
**Dependencies**: Packet 1 complete

### Objectives
- Define ALL cards in a single JSON file
- Create shared base deck for all NPCs
- Add NPC-specific special cards
- Ensure proper card distribution and balance

### Implementation Tasks
1. Create comprehensive cards.json structure:
   ```json
   {
     "baseDeck": {
       "fixedComfort": [
         {
           "id": "trust_comfort_1",
           "name": "I understand",
           "tokenType": "Trust",
           "weight": 1,
           "difficulty": "Easy",
           "effectType": "FixedComfort",
           "effectValue": 1,
           "persistent": true
         },
         {
           "id": "commerce_comfort_1",
           "name": "Fair deal",
           "tokenType": "Commerce",
           "weight": 1,
           "difficulty": "Easy",
           "effectType": "FixedComfort",
           "effectValue": 1,
           "persistent": true
         }
         // ... 40+ fixed comfort cards across all token types
       ],
       "scaledComfort": [
         {
           "id": "trust_scaled_1",
           "name": "Our trust runs deep",
           "tokenType": "Trust",
           "weight": 3,
           "difficulty": "Hard",
           "effectType": "ScaledComfort",
           "scalingFormula": "TrustTokens",
           "persistent": true
         },
         {
           "id": "commerce_scaled_1",
           "name": "Good business",
           "tokenType": "Commerce",
           "weight": 3,
           "difficulty": "Hard",
           "effectType": "ScaledComfort",
           "scalingFormula": "CommerceTokens",
           "persistent": true
         }
         // ... scaled cards for each token type
       ],
       "utility": [
         {
           "id": "draw_card_1",
           "name": "Let me think",
           "tokenType": "Trust",
           "weight": 1,
           "difficulty": "Medium",
           "effectType": "DrawCards",
           "effectValue": 1,
           "persistent": true
         }
         // ... draw and weight-add cards
       ],
       "setup": [
         {
           "id": "atmosphere_prepared",
           "name": "Careful approach",
           "tokenType": "Trust",
           "weight": 0,
           "difficulty": "Easy",
           "effectType": "SetAtmosphere",
           "atmosphereChange": "Prepared",
           "persistent": true
         }
         // ... atmosphere setup cards
       ],
       "dramatic": [
         {
           "id": "desperate_plea",
           "name": "Desperate plea",
           "tokenType": "Trust",
           "weight": 4,
           "difficulty": "Hard",
           "effectType": "FixedComfort",
           "effectValue": 4,
           "atmosphereChange": "Volatile",
           "persistent": false
         }
         // ... high-weight fleeting cards
       ]
     },
     "npcSpecialCards": {
       "elena": [
         {
           "id": "elena_crisis",
           "name": "This changes everything",
           "tokenType": "Trust",
           "weight": 5,
           "difficulty": "VeryHard",
           "effectType": "FixedComfort",
           "effectValue": 5,
           "atmosphereChange": "Final",
           "persistent": false,
           "unique": true
         }
       ],
       "marcus": [
         {
           "id": "marcus_deal",
           "name": "Deal of a lifetime",
           "tokenType": "Commerce",
           "weight": 5,
           "difficulty": "VeryHard",
           "effectType": "ScaledComfort",
           "scalingFormula": "CommerceTokens * 2",
           "persistent": false,
           "unique": true
         }
       ]
     }
   }
   ```

2. Create deck initialization system:
   ```csharp
   public class DeckInitializer
   {
       private CardDatabase cardDatabase;
       
       // Called ONCE at game initialization
       public void InitializeAllNPCDecks(GameWorld gameWorld)
       {
           var baseDeck = LoadBaseDeck();
           
           foreach (var npc in gameWorld.NPCs)
           {
               // Create a SEPARATE INSTANCE for each NPC
               var npcDeck = CreateDeckInstance(baseDeck, npc.Id, npc.Personality);
               npc.ConversationDeck = npcDeck;
               
               // Add NPC-specific special cards if any
               if (cardDatabase.NPCSpecialCards.ContainsKey(npc.Id))
               {
                   npc.ConversationDeck.AddRange(cardDatabase.NPCSpecialCards[npc.Id]);
               }
           }
       }
       
       private List<ConversationCard> CreateDeckInstance(List<ConversationCard> baseDeck, string npcId, NPCPersonality personality)
       {
           // Deep copy the base deck - each NPC gets their own instance
           var deck = new List<ConversationCard>();
           var primaryTokenType = GetPrimaryTokenType(personality);
           
           // Select cards favoring NPC's token type
           // 6 Fixed comfort (4-5 matching, 1-2 others)
           deck.AddRange(SelectAndCloneCards(baseDeck, CardEffectType.FixedComfort, primaryTokenType, 6));
           
           // 4 Scaled comfort (ALL matching token type)
           deck.AddRange(SelectAndCloneCards(baseDeck, CardEffectType.ScaledComfort, primaryTokenType, 4));
           
           // 2 Draw cards (matching token type)
           deck.AddRange(SelectAndCloneCards(baseDeck, CardEffectType.DrawCards, primaryTokenType, 2));
           
           // 2 Weight-add cards (matching token type)
           deck.AddRange(SelectAndCloneCards(baseDeck, CardEffectType.AddWeight, primaryTokenType, 2));
           
           // 3 Setup cards (mixed types)
           deck.AddRange(SelectAndCloneCards(baseDeck, CardEffectType.SetAtmosphere, null, 3));
           
           // 2 Dramatic cards (matching token type, fleeting)
           deck.AddRange(SelectAndCloneCards(baseDeck, CardEffectType.FixedComfort, primaryTokenType, 2, fleeting: true));
           
           return deck;
       }
       
       private List<ConversationCard> SelectAndCloneCards(List<ConversationCard> source, CardEffectType type, TokenType? preferredType, int count, bool fleeting = false)
       {
           var selected = new List<ConversationCard>();
           var candidates = source.Where(c => c.EffectType == type && c.IsFleeting == fleeting);
           
           if (preferredType.HasValue)
           {
               // 75% matching token type
               int matchingCount = (int)(count * 0.75f);
               selected.AddRange(candidates
                   .Where(c => c.TokenType == preferredType.Value)
                   .OrderBy(_ => Random.Next())
                   .Take(matchingCount)
                   .Select(c => c.DeepClone())); // Create independent copy
               
               // Fill rest with other types
               selected.AddRange(candidates
                   .Where(c => c.TokenType != preferredType.Value)
                   .OrderBy(_ => Random.Next())
                   .Take(count - selected.Count)
                   .Select(c => c.DeepClone()));
           }
           else
           {
               selected.AddRange(candidates
                   .OrderBy(_ => Random.Next())
                   .Take(count)
                   .Select(c => c.DeepClone()));
           }
           
           return selected;
       }
   }
   
   // Each NPC's deck can then evolve independently
   public class DeckEvolution
   {
       public void OnSuccessfulDelivery(NPC recipient, Letter letter)
       {
           // Add cards to THIS NPC's deck instance only
           recipient.ConversationDeck.Add(new ConversationCard
           {
               Id = $"delivered_{letter.Id}",
               Name = "Shared understanding",
               TokenType = letter.TokenType,
               // ... card properties based on delivery
           });
       }
   }
   ```

3. Load and validate JSON cards:
   ```csharp
   public class CardDatabase
   {
       public BaseDeck BaseDeck { get; set; }
       public Dictionary<string, List<ConversationCard>> NPCSpecialCards { get; set; }
       
       public static CardDatabase LoadFromJson(string jsonPath)
       {
           var json = File.ReadAllText(jsonPath);
           var database = JsonSerializer.Deserialize<CardDatabase>(json);
           
           // Validate all cards
           foreach (var card in database.GetAllCards())
           {
               ValidateCard(card);
           }
           
           return database;
       }
       
       private static void ValidateCard(ConversationCard card)
       {
           // Must have single effect
           if (card.HasMultipleEffects())
               throw new ValidationException($"Card {card.Id} has multiple effects");
           
           // Must have token type
           if (card.TokenType == null)
               throw new ValidationException($"Card {card.Id} missing token type");
           
           // Validate weight-effect correlation
           ValidateWeightEffectCorrelation(card);
       }
   }
   ```

### Verification Criteria
- [ ] All cards defined in single cards.json file
- [ ] Base deck contains 100+ cards across all types
- [ ] Each token type well represented in base deck
- [ ] NPC special cards are unique and thematic
- [ ] Deck builder creates balanced 20-card decks
- [ ] 75% of selected cards match NPC personality

### Agent Validation Test
```csharp
[Test]
public void ValidateJsonDeckBuilding()
{
    var database = CardDatabase.LoadFromJson("cards.json");
    var builder = new JsonDeckBuilder(database);
    
    var elenaDeck = builder.BuildNPCDeck("elena", NPCPersonality.Devoted);
    
    Assert.AreEqual(20, elenaDeck.Count);
    Assert.GreaterOrEqual(elenaDeck.Count(c => c.TokenType == TokenType.Trust), 15);
    Assert.IsTrue(elenaDeck.Any(c => c.Id.StartsWith("elena_")), "Should include Elena's special cards");
}
```

---

## PACKET 3: Single-Card SPEAK Enforcement
**Agent**: systems-architect
**Duration**: 4 hours
**Dependencies**: Packets 1-2 complete

### Objectives
- Modify UI to allow exactly ONE card selection
- Remove ALL multi-card selection logic
- Update conversation flow for single cards
- Delete legacy multi-select code

### Implementation Tasks
1. Modify ConversationContent.razor:
   ```razor
   @* Remove multi-select logic entirely *@
   <div class="card-hand">
       @foreach (var card in CurrentHand)
       {
           <div class="conversation-card @(SelectedCard?.Id == card.Id ? "selected" : "")"
                @onclick="() => SelectSingleCard(card)">
               <!-- Card display -->
           </div>
       }
   </div>
   
   <button @onclick="PlaySelectedCard" disabled="@(SelectedCard == null)">
       SPEAK (@SelectedCard?.Weight ?? 0 weight)
   </button>
   ```

2. Update selection logic:
   ```csharp
   private ConversationCard? SelectedCard { get; set; }
   
   private void SelectSingleCard(ConversationCard card)
   {
       // Only allow selection if weight available
       if (card.Weight <= CurrentWeightPool)
       {
           SelectedCard = card;
       }
   }
   
   private async Task PlaySelectedCard()
   {
       if (SelectedCard == null) return;
       
       await ConversationOrchestrator.PlaySingleCard(SelectedCard);
       
       // Remove fleeting cards after SPEAK
       RemoveFleetingCards();
       
       SelectedCard = null;
   }
   ```

3. Update ConversationOrchestrator:
   ```csharp
   public async Task PlaySingleCard(ConversationCard card)
   {
       // Deduct weight from pool
       CurrentWeightPool -= card.Weight;
       
       // Calculate success
       var successRate = CalculateSuccessRate(card);
       var success = Random.Next(100) < successRate;
       
       if (success)
       {
           await ApplyCardEffect(card);
           if (card.AtmosphereChange.HasValue)
               CurrentAtmosphere = card.AtmosphereChange.Value;
       }
       else
       {
           CurrentAtmosphere = AtmosphereType.Neutral;
       }
       
       // Check for goal card Final Word
       if (card is GoalCard goal && goal.HasFinalWord)
       {
           await EndConversation(success);
       }
   }
   ```

### Verification Criteria
- [ ] UI allows selection of exactly ONE card
- [ ] No multi-select checkboxes or controls remain
- [ ] SPEAK button shows weight cost of selected card
- [ ] Weight pool decreases by card weight on play
- [ ] Fleeting cards removed after ANY SPEAK action
- [ ] No legacy multi-card code remains

### Agent Validation
```bash
# Check for multi-select remnants
grep -r "SelectedCards\|MultiSelect\|multiple.*card" src/ --include="*.cs" --include="*.razor"
# Should return nothing
```

---

## PACKET 4: Weight Pool Persistence
**Agent**: systems-architect  
**Duration**: 3 hours
**Dependencies**: Packet 3 complete

### Objectives
- Ensure weight pool persists across SPEAK actions
- LISTEN refreshes to maximum capacity
- Implement Prepared atmosphere (+1 capacity)
- Delete any per-turn reset logic

### Implementation Tasks
1. Implement persistent weight pool:
   ```csharp
   public class WeightPoolManager
   {
       private int currentPool;
       private int maxCapacity;
       
       public void InitializeForState(EmotionalState state, AtmosphereType atmosphere)
       {
           maxCapacity = GetBaseCapacity(state);
           if (atmosphere == AtmosphereType.Prepared)
               maxCapacity += 1;
           
           currentPool = maxCapacity; // Start at max
       }
       
       public bool CanPlayCard(int weight) => weight <= currentPool;
       
       public void SpendWeight(int weight)
       {
           currentPool -= weight;
           // Pool persists - no reset here!
       }
       
       public void RefreshPool()
       {
           // ONLY called on LISTEN action
           currentPool = maxCapacity;
       }
       
       private int GetBaseCapacity(EmotionalState state)
       {
           return state switch
           {
               EmotionalState.Desperate => 3,
               EmotionalState.Tense => 4,
               EmotionalState.Neutral => 5,
               EmotionalState.Open => 5,
               EmotionalState.Connected => 6,
               _ => 5
           };
       }
   }
   ```

2. Update SPEAK to NOT refresh pool:
   ```csharp
   public async Task ExecuteSpeakAction(ConversationCard card)
   {
       weightPoolManager.SpendWeight(card.Weight);
       // NO pool refresh here - persists for next SPEAK
       await ProcessCardEffect(card);
   }
   ```

3. Update LISTEN to refresh pool:
   ```csharp
   public async Task ExecuteListenAction()
   {
       weightPoolManager.RefreshPool(); // Refresh to maximum
       DrawCardsForState(currentState);
       // Fleeting cards NOT removed on LISTEN
   }
   ```

### Verification Criteria
- [ ] Weight pool persists between SPEAK actions
- [ ] Multiple cards can be played until pool depleted
- [ ] LISTEN refreshes pool to current maximum
- [ ] Prepared atmosphere adds +1 to capacity
- [ ] No per-turn resets exist
- [ ] Can chain multiple low-weight cards

### Agent Validation Test
```csharp
[Test]
public void TestWeightPoolPersistence()
{
    // Start with 5 capacity
    manager.InitializeForState(EmotionalState.Neutral, AtmosphereType.Neutral);
    
    // Play 2-weight card
    manager.SpendWeight(2);
    Assert.AreEqual(3, manager.CurrentPool);
    
    // Play another 2-weight card WITHOUT refresh
    manager.SpendWeight(2);
    Assert.AreEqual(1, manager.CurrentPool);
    
    // LISTEN refreshes
    manager.RefreshPool();
    Assert.AreEqual(5, manager.CurrentPool);
}
```

---

## PACKET 5: Comfort Battery System
**Agent**: systems-architect
**Duration**: 3 hours
**Dependencies**: Packets 1-4 complete

### Objectives
- Enforce -3 to +3 range strictly
- Automatic state transitions at ±3
- Reset comfort to 0 after transition
- Desperate at -3 ends conversation

### Implementation Tasks
1. Implement comfort battery:
   ```csharp
   public class ComfortBatteryManager
   {
       private int currentComfort = 0;
       private EmotionalState currentState;
       
       public (bool stateChanged, EmotionalState newState) ApplyComfortChange(int change)
       {
           currentComfort += change;
           
           // Check for state transition
           if (currentComfort >= 3)
           {
               var newState = TransitionUp(currentState);
               currentComfort = 0; // Reset battery
               currentState = newState;
               return (true, newState);
           }
           else if (currentComfort <= -3)
           {
               if (currentState == EmotionalState.Desperate)
               {
                   // Conversation ends immediately
                   throw new ConversationEndedException("Desperate state at -3 comfort");
               }
               
               var newState = TransitionDown(currentState);
               currentComfort = 0; // Reset battery
               currentState = newState;
               return (true, newState);
           }
           
           return (false, currentState);
       }
       
       private EmotionalState TransitionUp(EmotionalState current)
       {
           return current switch
           {
               EmotionalState.Desperate => EmotionalState.Tense,
               EmotionalState.Tense => EmotionalState.Neutral,
               EmotionalState.Neutral => EmotionalState.Open,
               EmotionalState.Open => EmotionalState.Connected,
               EmotionalState.Connected => EmotionalState.Connected, // Max
               _ => current
           };
       }
       
       private EmotionalState TransitionDown(EmotionalState current)
       {
           return current switch
           {
               EmotionalState.Connected => EmotionalState.Open,
               EmotionalState.Open => EmotionalState.Neutral,
               EmotionalState.Neutral => EmotionalState.Tense,
               EmotionalState.Tense => EmotionalState.Desperate,
               _ => current
           };
       }
   }
   ```

2. Apply atmosphere modifiers:
   ```csharp
   private int ModifyComfortByAtmosphere(int baseChange, AtmosphereType atmosphere)
   {
       return atmosphere switch
       {
           AtmosphereType.Volatile => baseChange > 0 ? baseChange + 1 : baseChange - 1,
           AtmosphereType.Exposed => baseChange * 2,
           _ => baseChange
       };
   }
   ```

### Verification Criteria
- [ ] Comfort range strictly -3 to +3
- [ ] State transitions trigger at exactly ±3
- [ ] Comfort resets to 0 after transition
- [ ] Desperate at -3 ends conversation
- [ ] No comfort banking beyond ±3
- [ ] Atmosphere modifiers apply correctly

### Agent Validation Test
```csharp
[Test]
public void TestComfortBatteryTransitions()
{
    var battery = new ComfortBatteryManager(EmotionalState.Neutral);
    
    // +2 comfort - no transition
    var (changed1, state1) = battery.ApplyComfortChange(2);
    Assert.IsFalse(changed1);
    Assert.AreEqual(2, battery.CurrentComfort);
    
    // +1 more triggers transition
    var (changed2, state2) = battery.ApplyComfortChange(1);
    Assert.IsTrue(changed2);
    Assert.AreEqual(EmotionalState.Open, state2);
    Assert.AreEqual(0, battery.CurrentComfort); // Reset!
}
```

---

## PACKET 6: Atmosphere Persistence
**Agent**: systems-architect
**Duration**: 3 hours
**Dependencies**: Packets 1-5 complete

### Objectives
- Atmosphere persists until changed or failure
- LISTEN does NOT reset atmosphere
- Failure clears to Neutral
- Implement all atmosphere effects

### Implementation Tasks
1. Implement atmosphere manager:
   ```csharp
   public class AtmosphereManager
   {
       private AtmosphereType currentAtmosphere = AtmosphereType.Neutral;
       
       public void SetAtmosphere(AtmosphereType type)
       {
           currentAtmosphere = type;
           // Persists until explicitly changed
       }
       
       public void OnCardFailure()
       {
           currentAtmosphere = AtmosphereType.Neutral; // Only failure resets
       }
       
       public void OnListenAction()
       {
           // DO NOTHING - atmosphere persists through LISTEN
       }
       
       public int ModifyWeightCapacity(int baseCapacity)
       {
           if (currentAtmosphere == AtmosphereType.Prepared)
               return baseCapacity + 1;
           return baseCapacity;
       }
       
       public int ModifyCardDraw(int baseDraw)
       {
           return currentAtmosphere switch
           {
               AtmosphereType.Receptive => baseDraw + 1,
               AtmosphereType.Pressured => Math.Max(1, baseDraw - 1),
               _ => baseDraw
           };
       }
       
       public int ModifySuccessRate(int baseRate)
       {
           if (currentAtmosphere == AtmosphereType.Focused)
               return Math.Min(95, baseRate + 20);
           return baseRate;
       }
       
       public bool NextCardAutoSucceeds()
       {
           return currentAtmosphere == AtmosphereType.Informed;
       }
   }
   ```

2. Implement observation-only atmospheres:
   ```csharp
   public enum AtmosphereType
   {
       // Standard (30% of normal cards)
       Neutral, Prepared, Receptive, Focused, Patient, Volatile, Final,
       
       // Observation-only (unique effects)
       Informed,     // Next card cannot fail
       Exposed,      // Double comfort changes
       Synchronized, // Next effect happens twice
       Pressured     // -1 card on LISTEN
   }
   ```

### Verification Criteria
- [ ] Atmosphere persists across all actions
- [ ] Only card effects or failure change atmosphere
- [ ] LISTEN does NOT reset atmosphere
- [ ] All atmosphere effects implemented
- [ ] Observation atmospheres work correctly
- [ ] No legacy atmosphere reset code

### Agent Validation
```bash
# Check for LISTEN resetting atmosphere
grep -r "Listen.*Atmosphere.*Neutral\|ResetAtmosphere" src/ --include="*.cs"
# Should return nothing
```

---

## PACKET 7: Fleeting Card Removal
**Agent**: game-mechanics-designer
**Duration**: 2 hours
**Dependencies**: Packets 1-6 complete

### Objectives
- Fleeting cards removed after SPEAK (played or not)
- Persistent cards remain in hand
- Goal cards have Final Word property
- Create urgency without tracking

### Implementation Tasks
1. Implement fleeting removal:
   ```csharp
   public class HandManager
   {
       private List<ConversationCard> currentHand = new();
       
       public void OnSpeakAction(ConversationCard? playedCard)
       {
           // Remove ALL fleeting cards after SPEAK
           currentHand.RemoveAll(c => c.IsFleeting);
           
           // Check for unplayed goal with Final Word
           var unplayedGoal = currentHand
               .OfType<GoalCard>()
               .FirstOrDefault(g => g.HasFinalWord && g != playedCard);
               
           if (unplayedGoal != null)
           {
               throw new ConversationFailedException("Goal card not played - Final Word triggered");
           }
       }
       
       public void OnListenAction()
       {
           // Fleeting cards NOT removed on LISTEN
           // This preserves them for next SPEAK
       }
   }
   ```

2. Mark cards correctly:
   ```csharp
   private void AssignPersistence(ConversationCard card)
   {
       // 25% fleeting distribution
       if (card.Weight >= 4 || card is GoalCard)
       {
           card.IsFleeting = true; // High-impact cards fleeting
       }
       else if (Random.Next(100) < 25)
       {
           card.IsFleeting = true; // Random 25% of others
       }
       else
       {
           card.IsFleeting = false; // 75% persistent
       }
   }
   ```

### Verification Criteria
- [ ] Fleeting cards removed after SPEAK
- [ ] Persistent cards remain in hand
- [ ] Goal cards trigger Final Word if not played
- [ ] LISTEN preserves fleeting cards
- [ ] 25% fleeting distribution maintained
- [ ] Creates natural urgency

### Agent Validation Test
```csharp
[Test]
public void TestFleetingRemoval()
{
    var hand = new HandManager();
    hand.AddCard(new ConversationCard { IsFleeting = true });
    hand.AddCard(new ConversationCard { IsFleeting = false });
    
    hand.OnSpeakAction(null);
    
    Assert.AreEqual(1, hand.Count); // Only persistent remains
    Assert.IsFalse(hand.First().IsFleeting);
}
```

---

## PACKET 8: Token-Type Matching System
**Agent**: game-mechanics-designer
**Duration**: 4 hours
**Dependencies**: Packets 1-7 complete

### Objectives
- Tokens ONLY boost matching card types
- Trust tokens only help Trust cards
- Remove all generic token bonuses
- Implement district specialization

### Implementation Tasks
1. Implement token-type matching:
   ```csharp
   public class TokenBonusCalculator
   {
       public int CalculateBonus(ConversationCard card, NPCRelationship relationship)
       {
           // Get tokens that match card type
           int matchingTokens = card.TokenType switch
           {
               TokenType.Trust => relationship.TrustTokens,
               TokenType.Commerce => relationship.CommerceTokens,
               TokenType.Status => relationship.StatusTokens,
               TokenType.Shadow => relationship.ShadowTokens,
               _ => 0
           };
           
           // ONLY matching tokens provide bonus
           return matchingTokens * 5; // +5% per matching token
       }
       
       public int GetSuccessRate(ConversationCard card, NPCRelationship relationship)
       {
           int baseRate = GetBaseRateForDifficulty(card.Difficulty);
           int tokenBonus = CalculateBonus(card, relationship);
           
           // Clamp between 5% and 95%
           return Math.Clamp(baseRate + tokenBonus, 5, 95);
       }
   }
   ```

2. Update UI to show matching:
   ```razor
   <div class="card-success-info">
       Base: @GetBaseRate(card)%
       @if (GetMatchingTokens(card) > 0)
       {
           <span class="token-bonus">
               + @(GetMatchingTokens(card) * 5)% (@card.TokenType tokens)
           </span>
       }
       else
       {
           <span class="no-bonus">
               No @card.TokenType tokens
           </span>
       }
       = @GetTotalSuccessRate(card)%
   </div>
   ```

3. Remove generic bonuses:
   ```csharp
   // DELETE this old code:
   // int bonus = relationship.TotalTokens * 5;
   
   // REPLACE with:
   int bonus = GetMatchingTokenBonus(card.TokenType, relationship);
   ```

### Verification Criteria
- [ ] Only matching tokens provide bonuses
- [ ] Trust tokens don't help Commerce cards
- [ ] UI shows which tokens apply
- [ ] No generic token bonuses remain
- [ ] Token specialization enforced
- [ ] Districts require specific token types

### Agent Validation Test
```csharp
[Test]
public void TestTokenTypeMatching()
{
    var relationship = new NPCRelationship
    {
        TrustTokens = 3,
        CommerceTokens = 2
    };
    
    var trustCard = new ConversationCard { TokenType = TokenType.Trust };
    var commerceCard = new ConversationCard { TokenType = TokenType.Commerce };
    
    Assert.AreEqual(15, calculator.CalculateBonus(trustCard, relationship));    // 3 * 5
    Assert.AreEqual(10, calculator.CalculateBonus(commerceCard, relationship)); // 2 * 5
}
```

---

## PACKET 9: Observation Card System
**Agent**: game-mechanics-designer
**Duration**: 4 hours
**Dependencies**: Packets 1-8 complete

### Objectives
- Implement player observation deck (max 20)
- Create unique observation effects
- Add location-based observations
- Implement expiration system

### Implementation Tasks
1. Create observation deck:
   ```csharp
   public class PlayerObservationDeck
   {
       private List<ObservationCard> deck = new();
       private const int MAX_CARDS = 20;
       
       public bool TryAddObservation(ObservationCard card)
       {
           if (deck.Count >= MAX_CARDS)
               return false;
               
           card.ExpirationTime = DateTime.Now.AddHours(card.ExpirationHours);
           deck.Add(card);
           return true;
       }
       
       public void RemoveExpiredCards()
       {
           deck.RemoveAll(c => DateTime.Now > c.ExpirationTime);
       }
       
       public List<ObservationCard> GetPlayableCards()
       {
           RemoveExpiredCards();
           return deck.Where(c => c.Weight <= CurrentWeightPool).ToList();
       }
   }
   ```

2. Implement unique effects:
   ```csharp
   public enum ObservationEffectType
   {
       // Atmosphere setters
       SetInformed,     // Next card cannot fail
       SetExposed,      // Double comfort changes
       SetSynchronized, // Next effect twice
       SetPressured,    // -1 card on LISTEN
       
       // Cost bypasses
       FreePatience,    // Next action 0 patience
       FreeWeight,      // Next SPEAK 0 weight
       
       // Manipulations
       ResetComfort,    // Comfort = 0
       RefreshWeight    // Weight = max
   }
   
   public void ApplyObservationEffect(ObservationEffectType effect)
   {
       switch (effect)
       {
           case ObservationEffectType.SetInformed:
               atmosphereManager.SetAtmosphere(AtmosphereType.Informed);
               break;
           case ObservationEffectType.FreeWeight:
               nextSpeakFree = true;
               break;
           case ObservationEffectType.ResetComfort:
               comfortBattery.ResetToZero();
               break;
           // etc...
       }
   }
   ```

3. Create location observations:
   ```csharp
   public class LocationObservations
   {
       public ObservationCard? GetAvailableObservation(string location, TimePeriod period)
       {
           return (location, period) switch
           {
               ("MarketSquare", TimePeriod.Morning) => new ObservationCard
               {
                   Name = "Guard Routes",
                   UniqueEffect = ObservationEffectType.SetPressured,
                   ExpirationHours = 24
               },
               ("CopperKettle", _) when NPCPresent("Elena") => new ObservationCard
               {
                   Name = "Shared Hardship",
                   UniqueEffect = ObservationEffectType.SetInformed,
                   ExpirationHours = 48
               },
               _ => null
           };
       }
   }
   ```

### Verification Criteria
- [ ] Player deck limited to 20 cards
- [ ] All observation effects implemented
- [ ] Location observations available by time
- [ ] Cards expire after 24-48 hours
- [ ] Weight always 1, success 85%
- [ ] Unique effects not available on normal cards

### Agent Validation
```bash
# Verify observation cards have correct properties
grep -r "ObservationCard" src/ --include="*.cs" -A 5 | grep -E "Weight|Difficulty"
# Should show Weight = 1, Difficulty = VeryEasy
```

---

## PACKET 10: Goal Card System
**Agent**: game-mechanics-designer
**Duration**: 4 hours
**Dependencies**: Packets 1-9 complete

### Objectives
- Implement Final Word property
- Create goal types (Letter, Meeting, Resolution, Commerce)
- Goal selection based on conversation type
- Ensure 5-6 weight requirement

### Implementation Tasks
1. Implement goal deck structure:
   ```csharp
   public class NPCGoalDeck
   {
       private List<GoalCard> goals = new();
       
       public GoalCard? SelectGoalForConversationType(ConversationType type)
       {
           return type switch
           {
               ConversationType.Letter => goals.FirstOrDefault(g => g.GoalType == GoalType.Letter),
               ConversationType.Meeting => goals.FirstOrDefault(g => g.GoalType == GoalType.Meeting),
               ConversationType.Resolution => goals.FirstOrDefault(g => g.GoalType == GoalType.Resolution),
               ConversationType.Commerce => goals.FirstOrDefault(g => g.GoalType == GoalType.Commerce),
               _ => null
           };
       }
   }
   ```

2. Implement Final Word:
   ```csharp
   public class ConversationSession
   {
       public void OnSpeakAction(ConversationCard? playedCard)
       {
           // Remove fleeting cards
           var unplayedGoals = currentHand
               .OfType<GoalCard>()
               .Where(g => g.HasFinalWord && g != playedCard)
               .ToList();
           
           if (unplayedGoals.Any())
           {
               // Final Word triggers - conversation fails
               EndConversation(false, "Goal card not played");
               return;
           }
           
           // If goal was played
           if (playedCard is GoalCard goal)
           {
               var success = CalculateSuccess(goal);
               if (success)
               {
                   CreateObligation(goal.SuccessTerms);
               }
               else
               {
                   CreateObligation(goal.FailureTerms);
               }
               EndConversation(success, "Goal completed");
           }
       }
   }
   ```

3. Create goal types:
   ```csharp
   public GoalCard CreateLetterGoal(NPCPersonality personality)
   {
       return new GoalCard
       {
           Name = "Urgent Letter",
           GoalType = GoalType.Letter,
           TokenType = GetPrimaryTokenType(personality),
           Weight = 5,
           Difficulty = DifficultyTier.VeryHard, // 40%
           IsFleeting = true,
           HasFinalWord = true,
           SuccessTerms = new SuccessTerms
           {
               Deadline = 4,
               QueuePosition = QueuePosition.Flexible,
               Payment = 10
           },
           FailureTerms = new FailureTerms
           {
               Deadline = 1,
               QueuePosition = QueuePosition.First,
               Payment = 5
           }
       };
   }
   ```

### Verification Criteria
- [ ] Goal cards require 5-6 weight capacity
- [ ] Final Word ends conversation if not played
- [ ] Different goal types create different obligations
- [ ] Success/failure terms applied correctly
- [ ] Goals selected based on conversation type
- [ ] All goals have 40-50% base success

### Agent Validation Test
```csharp
[Test]
public void TestGoalFinalWord()
{
    var session = new ConversationSession();
    var goal = new GoalCard { HasFinalWord = true, IsFleeting = true };
    
    session.AddToHand(goal);
    session.OnSpeakAction(null); // Don't play goal
    
    Assert.IsTrue(session.HasEnded);
    Assert.IsFalse(session.WasSuccessful);
}
```

---

## PACKET 11: NPC Deck Specialization
**Agent**: content-integrator
**Duration**: 6 hours
**Dependencies**: Packets 1-10 complete

### Objectives
- Generate personality-specific decks
- 75% cards match NPC token type
- Create Elena, Marcus, Lord Blackwood, Guard Captain
- Implement exchange decks for merchants

### Implementation Tasks
1. Generate Elena's deck (Devoted):
   ```csharp
   public class ElenaConfiguration
   {
       public static NPCConfiguration Create()
       {
           return new NPCConfiguration
           {
               Name = "Elena",
               Personality = NPCPersonality.Devoted,
               BasePatience = 15,
               PrimaryTokenType = TokenType.Trust,
               
               ConversationDeck = new DeckGenerator().GenerateDeck(deck =>
               {
                   // 15+ Trust-type cards
                   deck.SetPrimaryType(TokenType.Trust, 0.75f);
                   deck.AddScaledComfort("Our trust runs deep", TokenType.Trust, ScaleWith.TrustTokens);
                   deck.AddScaledComfort("Lean on me", TokenType.Trust, ScaleWith.InverseComfort);
                   deck.AddDramaticCard("Desperate plea", TokenType.Trust, 4, fleeting: true);
               }),
               
               GoalDeck = new List<GoalCard>
               {
                   CreateLetterGoal("Crisis Refusal", TokenType.Trust, 5),
                   CreateLetterGoal("Formal Refusal", TokenType.Trust, 6),
                   CreateResolutionGoal("Clear the Air", TokenType.Trust)
               }
           };
       }
   }
   ```

2. Generate Marcus's deck (Mercantile):
   ```csharp
   public class MarcusConfiguration
   {
       public static NPCConfiguration Create()
       {
           return new NPCConfiguration
           {
               Name = "Marcus",
               Personality = NPCPersonality.Mercantile,
               BasePatience = 12,
               PrimaryTokenType = TokenType.Commerce,
               
               ExchangeDeck = new List<ExchangeCard>
               {
                   new ExchangeCard("Buy Provisions", 3, ResourceType.Coins, Effect.ResetHunger),
                   new ExchangeCard("Purchase Medicine", 5, ResourceType.Coins, Effect.Health20),
                   new ExchangeCard("Buy Access Permit", 15, ResourceType.Coins, Effect.NoblePermit),
                   new ExchangeCard("Accept Quick Job", 0, ResourceType.None, Effect.NewObligation)
               }
           };
       }
   }
   ```

3. Validate deck composition:
   ```csharp
   public void ValidateDeckComposition(List<ConversationCard> deck, TokenType primaryType)
   {
       var primaryCount = deck.Count(c => c.TokenType == primaryType);
       var percentage = (float)primaryCount / deck.Count;
       
       if (percentage < 0.70f)
           throw new InvalidDeckException($"Only {percentage:P} cards match primary type");
   }
   ```

### Verification Criteria
- [ ] Elena: 15+ Trust cards, 15 patience
- [ ] Marcus: 15+ Commerce cards, exchange deck
- [ ] Lord Blackwood: 15+ Status cards, 10 patience
- [ ] Guard Captain: balanced types, 13 patience
- [ ] All scaled cards match primary type
- [ ] Exchange decks for merchants only

### Agent Validation
```bash
# Check deck compositions
dotnet test --filter "TestName~DeckComposition"
```

---

## PACKET 12: UI Card Interface
**Agent**: general-purpose
**Duration**: 6 hours
**Dependencies**: Packets 1-11 complete

### Objectives
- Replace ALL button UI with cards
- Show complete card information
- Display token-type matching
- Perfect information principle

### Implementation Tasks
1. Create card display component:
   ```razor
   <div class="conversation-card @GetTokenTypeClass(card)">
       <div class="card-header">
           <span class="card-name">@card.Name</span>
           <span class="card-weight">@card.Weight</span>
       </div>
       
       <div class="card-body">
           <div class="token-type">
               <img src="@GetTokenIcon(card.TokenType)" />
               @card.TokenType
           </div>
           
           <div class="card-effect">
               @GetEffectDescription(card)
           </div>
           
           @if (card.AtmosphereChange != null)
           {
               <div class="atmosphere-change">
                   Sets: @card.AtmosphereChange
               </div>
           }
       </div>
       
       <div class="card-footer">
           <div class="success-rate">
               <span class="base-rate">@GetBaseRate(card)%</span>
               @if (GetMatchingTokens(card) > 0)
               {
                   <span class="bonus">+@(GetMatchingTokens(card) * 5)%</span>
               }
               <span class="total">=@GetTotalRate(card)%</span>
           </div>
           
           @if (card.IsFleeting)
           {
               <span class="fleeting-warning">⚡ Fleeting</span>
           }
       </div>
   </div>
   ```

2. Show resource bars:
   ```razor
   <div class="conversation-resources">
       <div class="weight-pool">
           Weight: @CurrentWeight / @MaxWeight
           <div class="weight-bar">
               @for (int i = 0; i < MaxWeight; i++)
               {
                   <span class="weight-pip @(i < CurrentWeight ? "filled" : "empty")"></span>
               }
           </div>
       </div>
       
       <div class="comfort-battery">
           Comfort: @CurrentComfort
           <div class="comfort-bar">
               @for (int i = -3; i <= 3; i++)
               {
                   <span class="comfort-pip @(GetComfortPipClass(i))"></span>
               }
           </div>
       </div>
       
       <div class="atmosphere-display">
           Atmosphere: @CurrentAtmosphere
           @if (CurrentAtmosphere != AtmosphereType.Neutral)
           {
               <span class="atmosphere-effect">@GetAtmosphereEffect()</span>
           }
       </div>
   </div>
   ```

3. Remove ALL button UI:
   ```csharp
   // DELETE all of this:
   // <button onclick="FriendlyChat">Friendly Chat</button>
   // <button onclick="Flatter">Flatter</button>
   
   // REPLACE with card-based interaction only
   ```

### Verification Criteria
- [ ] No buttons for game actions remain
- [ ] All interactions through cards
- [ ] Token matching clearly shown
- [ ] Success rates visible before play
- [ ] Weight costs displayed
- [ ] Perfect information achieved

### Agent Validation
```bash
# Check for button remnants
grep -r "<button" src/Pages/ --include="*.razor" | grep -v "SPEAK\|LISTEN"
# Should only show SPEAK/LISTEN buttons
```

---

## PACKET 13: Content JSON Migration
**Agent**: content-integrator
**Duration**: 4 hours
**Dependencies**: Packets 1-12 complete

### Objectives
- Migrate all card content to new format
- Delete legacy card definitions
- Create proper goal decks
- Validate against design rules

### Implementation Tasks
1. Create new card template format:
   ```json
   {
     "cards": {
       "trust_basic_1": {
         "name": "I understand",
         "tokenType": "Trust",
         "weight": 1,
         "difficulty": "Easy",
         "effectType": "FixedComfort",
         "effectValue": 1,
         "persistent": true
       },
       "trust_scaled_1": {
         "name": "Our trust runs deep",
         "tokenType": "Trust",
         "weight": 3,
         "difficulty": "Hard",
         "effectType": "ScaledComfort",
         "scalingFormula": "TrustTokens",
         "persistent": true
       }
     }
   }
   ```

2. Create observation templates:
   ```json
   {
     "observations": {
       "guard_routes": {
         "name": "Guard Routes",
         "location": "MarketSquare",
         "timePeriod": "Morning",
         "uniqueEffect": "SetPressured",
         "expirationHours": 24,
         "description": "You notice the guards' patrol patterns"
       }
     }
   }
   ```

3. Validate all content:
   ```csharp
   public class ContentValidator
   {
       public void ValidateCard(CardTemplate template)
       {
           // No multi-effects
           if (template.HasMultipleEffects())
               throw new ValidationException("Card has multiple effects");
               
           // Weight-effect correlation
           if (!IsValidWeightForEffect(template.Weight, template.EffectType))
               throw new ValidationException("Invalid weight-effect correlation");
               
           // Token type required
           if (string.IsNullOrEmpty(template.TokenType))
               throw new ValidationException("Card missing token type");
       }
   }
   ```

### Verification Criteria
- [ ] All cards follow single-effect rule
- [ ] Token types on every card
- [ ] Proper weight-effect correlations
- [ ] No legacy card formats remain
- [ ] Goal decks properly defined
- [ ] Observations have unique effects

### Agent Validation
```bash
# Validate JSON structure
jq '.cards | .[] | select(.effectType == null)' src/Content/cards.json
# Should return nothing (all cards have effect types)
```

---

## PACKET 14: Integration Testing
**Agent**: change-validator
**Duration**: 4 hours
**Dependencies**: ALL packets complete

### Objectives
- Test complete conversation flow
- Verify all mechanics work together
- Ensure no legacy code remains
- Validate Elena scenario

### Implementation Tasks
1. Create integration tests:
   ```csharp
   [Test]
   public async Task TestCompleteConversationFlow()
   {
       // Start conversation with Elena
       var session = await StartConversation("Elena", ConversationType.Letter);
       
       // Verify initial state
       Assert.AreEqual(EmotionalState.Desperate, session.NPCState);
       Assert.AreEqual(3, session.WeightCapacity);
       
       // Test single-card SPEAK
       var card = session.Hand.First(c => c.Weight <= 3);
       await session.PlayCard(card);
       
       // Verify weight persistence
       Assert.Less(session.CurrentWeight, 3);
       
       // Test LISTEN refresh
       await session.Listen();
       Assert.AreEqual(3, session.CurrentWeight);
       
       // Test goal card
       var goal = session.Hand.OfType<GoalCard>().FirstOrDefault();
       if (goal != null && session.CurrentWeight >= goal.Weight)
       {
           await session.PlayCard(goal);
           Assert.IsTrue(session.HasEnded);
       }
   }
   ```

2. Test token matching:
   ```csharp
   [Test]
   public void TestTokenTypeMatching()
   {
       var elena = GetNPC("Elena");
       elena.Relationship.TrustTokens = 3;
       elena.Relationship.CommerceTokens = 5;
       
       var trustCard = elena.Deck.First(c => c.TokenType == TokenType.Trust);
       var commerceCard = elena.Deck.First(c => c.TokenType == TokenType.Commerce);
       
       // Trust tokens help Trust cards
       Assert.AreEqual(15, GetTokenBonus(trustCard, elena));
       
       // Commerce tokens DON'T help Trust cards
       Assert.AreEqual(0, GetTokenBonus(commerceCard, elena));
   }
   ```

3. Clean build verification:
   ```bash
   #!/bin/bash
   # Clean build test
   dotnet clean
   dotnet build --no-incremental
   
   # Run all tests
   dotnet test
   
   # Check for legacy code
   echo "Checking for legacy code..."
   grep -r "MultiSelect\|CompatibilityMode\|LegacyCard" src/
   
   if [ $? -eq 0 ]; then
       echo "ERROR: Legacy code found!"
       exit 1
   fi
   
   echo "Build and tests successful!"
   ```

### Verification Criteria
- [ ] Complete Elena scenario playable
- [ ] All mechanics integrated correctly
- [ ] No compilation errors
- [ ] All tests pass
- [ ] No legacy code remains
- [ ] Performance acceptable

### Agent Validation
```bash
# Final validation
dotnet build && dotnet test && ./check-legacy.sh
```

---

## PACKET 15: Final Cleanup
**Agent**: general-purpose
**Duration**: 2 hours
**Dependencies**: Packet 14 complete

### Objectives
- Delete ALL legacy code
- Remove compatibility layers
- Clean up unused files
- Final documentation

### Implementation Tasks
1. Remove legacy files:
   ```bash
   # Delete old card systems
   rm -rf src/Legacy/
   rm -rf src/Compatibility/
   
   # Remove old UI components
   find src/Pages -name "*MultiSelect*" -delete
   find src/Pages -name "*Button*" -delete
   
   # Clean up unused models
   find src/Models -name "*Old*" -delete
   find src/Models -name "*Legacy*" -delete
   ```

2. Update documentation:
   ```markdown
   # Wayfarer Card System
   
   ## Core Mechanics
   - ONE card per SPEAK action
   - Weight pools persist until LISTEN
   - Comfort battery -3 to +3
   - Tokens only boost matching types
   - Atmosphere persists until changed
   
   ## No Legacy Code
   All old systems removed. No compatibility layers.
   ```

3. Final verification:
   ```csharp
   [Test]
   public void NoLegacyCode()
   {
       var files = Directory.GetFiles("src/", "*.cs", SearchOption.AllDirectories);
       foreach (var file in files)
       {
           var content = File.ReadAllText(file);
           Assert.IsFalse(content.Contains("Legacy"));
           Assert.IsFalse(content.Contains("Compatibility"));
           Assert.IsFalse(content.Contains("TODO"));
       }
   }
   ```

### Verification Criteria
- [ ] Zero legacy files remain
- [ ] No compatibility code
- [ ] No TODO comments
- [ ] Documentation updated
- [ ] Repository clean
- [ ] Ready for production

### Final Validation
```bash
# Absolute final check
find . -name "*.cs" -exec grep -l "TODO\|LEGACY\|COMPAT" {} \;
# Should return NOTHING
```

---

## Execution Order

1. **Phase 1 (Core)**: Packets 1-7 - Implement core mechanics
2. **Phase 2 (Features)**: Packets 8-10 - Add specialized features  
3. **Phase 3 (Content)**: Packets 11-13 - Generate content
4. **Phase 4 (Validation)**: Packets 14-15 - Test and cleanup

## Verification Protocol

After EACH packet:
1. Run compilation: `dotnet build`
2. Run packet-specific tests
3. Check for legacy code remnants
4. Verify no TODOs added
5. Confirm no compatibility layers

## Success Metrics

- ✅ All packets complete
- ✅ Zero compilation errors
- ✅ All tests passing
- ✅ No legacy code
- ✅ No compatibility layers
- ✅ No TODO comments
- ✅ Elena scenario fully playable
- ✅ Perfect adherence to refined design

This implementation plan provides complete transformation of Wayfarer into the refined card-based conversation system with no compromises, fallbacks, or legacy code.