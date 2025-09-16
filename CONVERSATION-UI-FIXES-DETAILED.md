# Conversation UI Critical Fixes - Detailed Implementation Plan

## Context
The conversation screen UI was updated to match the mockup, but there are 5 critical issues that need fixing:
1. Vertical scrolling is broken
2. Connection state is duplicated
3. Personality rules show flavor text instead of actual mechanics
4. Request goals show generic text instead of actual card data
5. Request cards appear immediately instead of being hidden until rapport threshold is met

## Files That Need Changes
- `/mnt/c/git/wayfarer/src/Pages/Components/ConversationContent.razor`
- `/mnt/c/git/wayfarer/src/Pages/Components/ConversationContent.razor.cs`
- `/mnt/c/git/wayfarer/src/wwwroot/css/conversation.css`
- `/mnt/c/git/wayfarer/src/Subsystems/Conversation/ConversationSession.cs`
- `/mnt/c/git/wayfarer/src/Subsystems/Conversation/CardDeckManager.cs`

## Issue 1: Fix Scrolling (PARTIALLY DONE)
**Status**: CSS updated, needs testing
**Fix Applied**: Added `max-height: 400px; overflow-y: auto;` to `.cards-section`

## Issue 2: Remove Duplicate Connection State (PARTIALLY DONE)
**Status**: Razor updated, needs CSS update
**What Was Done**:
- Removed old `.emotional-state` div with text-based state display
- Kept only the new flow state bar
- Added focus and draw info to `.current-state`

**Still Needs**:
```css
.current-state .state-rules {
    font-size: 11px;
    color: #7a6250;
    margin-top: 4px;
}
```

## Issue 3: Fix Personality Rules to Show Actual Mechanics

**Current Problem**: `GetPersonalityRuleShort()` returns generic text like "Devoted: Strong emotional bonds"

**Required Fix**: Use the ACTUAL mechanical rules from `PersonalityRuleEnforcer`

The actual rules are defined in `/mnt/c/git/wayfarer/src/Subsystems/Conversation/PersonalityRuleEnforcer.cs`:
- DEVOTED (PersonalityModifierType.RapportLossMultiplier): "Devoted: Negative rapport effects are doubled"
- MERCANTILE (PersonalityModifierType.HighestFocusBonus): "Mercantile: Your highest focus card gains +30% success"
- PROUD (PersonalityModifierType.AscendingFocusRequired): "Proud: Cards must be played in ascending focus order"
- CUNNING (PersonalityModifierType.RepeatFocusPenalty): "Cunning: Playing same focus as previous costs -2 rapport"
- STEADFAST (PersonalityModifierType.RapportChangeCap): "Steadfast: All rapport changes capped at ±2"

**Implementation in ConversationContent.razor.cs**:
```csharp
protected string GetPersonalityRuleShort()
{
    if (Session?.NPC == null) return "";

    // Get the actual mechanical rule from the personality type
    var personality = Session.NPC.PersonalityType;
    return personality switch
    {
        PersonalityType.DEVOTED => "Devoted: Rapport losses doubled",
        PersonalityType.MERCANTILE => "Mercantile: Highest focus +30% success",
        PersonalityType.PROUD => "Proud: Cards must ascend in focus",
        PersonalityType.CUNNING => "Cunning: Same focus as prev -2 rapport",
        PersonalityType.STEADFAST => "Steadfast: Rapport changes capped ±2",
        _ => $"{personality}: Special rules apply"
    };
}
```

## Issue 4: Fix Request Goals to Show Actual Data

**Current Problem**: `GetRequestGoals()` returns hardcoded generic data:
- "Basic Progress" with "Small gain"
- "Good Progress" with "Medium gain"
- "Excellent Progress" with "Large gain"

**Required Fix**: Read from ACTUAL request cards in the NPC's request deck

**Data Structure**:
- NPCs have `Requests` property containing `NPCRequest` objects
- Each `NPCRequest` has `RequestCardIds` and `PromiseCardIds`
- Request cards have a `rapportThreshold` property in their template
- Cards are stored in `GameWorld.AllCardDefinitions`

**Implementation in ConversationContent.razor.cs**:
```csharp
protected List<RequestGoal> GetRequestGoals()
{
    var goals = new List<RequestGoal>();

    if (Context?.Npc?.Requests == null || !Context.Npc.Requests.Any())
        return goals;

    // Get the first active request (or current request if tracked)
    var request = Context.Npc.Requests.FirstOrDefault();
    if (request == null) return goals;

    // Get all request cards from this request
    var requestCards = request.GetRequestCards(GameFacade.GetGameWorld());

    // Group by rapport threshold and create goals
    var groupedCards = requestCards
        .Where(c => c.Template?.RapportThreshold != null)
        .GroupBy(c => c.Template.RapportThreshold.Value)
        .OrderBy(g => g.Key);

    foreach (var group in groupedCards)
    {
        var threshold = group.Key;
        var firstCard = group.First();

        // Determine reward based on threshold or card properties
        string reward = threshold switch
        {
            <= 5 => "1 Trust token",
            <= 10 => "2 Trust tokens",
            <= 15 => "3 Trust tokens + Observation",
            _ => "Special reward"
        };

        // Use actual card name or a descriptive name
        string goalName = threshold switch
        {
            <= 5 => "Basic Delivery",
            <= 10 => "Priority Delivery",
            <= 15 => "Immediate Action",
            _ => firstCard.Name
        };

        goals.Add(new RequestGoal
        {
            Threshold = threshold,
            Name = goalName,
            Reward = reward
        });
    }

    return goals;
}
```

## Issue 5: Implement Request Pile System

**Current Problem**: Request cards with rapport thresholds appear immediately in hand

**Required Fix**: Create a "Request Pile" that holds request cards until rapport threshold is met

### Step 1: Add RequestPile to ConversationSession
File: `/mnt/c/git/wayfarer/src/Subsystems/Conversation/ConversationSession.cs`
```csharp
public class ConversationSession
{
    // ... existing properties ...

    /// <summary>
    /// Request cards waiting for rapport threshold to be met
    /// </summary>
    public List<CardInstance> RequestPile { get; set; } = new List<CardInstance>();

    // ... rest of class ...
}
```

### Step 2: Modify CardDeckManager to Use Request Pile
File: `/mnt/c/git/wayfarer/src/Subsystems/Conversation/CardDeckManager.cs`

In `InitializeDeck()` method, separate request cards:
```csharp
// Instead of adding request cards to ActiveCards, add to RequestPile
if (card.CardType == CardType.Letter || card.CardType == CardType.Promise || card.CardType == CardType.BurdenGoal)
{
    session.RequestPile.Add(cardInstance);
}
else
{
    session.ActiveCards.Add(cardInstance);
}
```

### Step 3: Check Thresholds on LISTEN
File: `/mnt/c/git/wayfarer/src/Pages/Components/ConversationContent.razor.cs`

Add method to check and move cards:
```csharp
private void CheckRequestPileThresholds()
{
    if (Session?.RequestPile == null || Session.RapportManager == null) return;

    var currentRapport = Session.RapportManager.CurrentRapport;
    var cardsToMove = new List<CardInstance>();

    // Check each card in request pile
    foreach (var card in Session.RequestPile)
    {
        var threshold = GetRequestRapportThreshold(card);
        if (currentRapport >= threshold)
        {
            cardsToMove.Add(card);
        }
    }

    // Move qualifying cards to active hand
    foreach (var card in cardsToMove)
    {
        Session.RequestPile.Remove(card);
        Session.HandCards.Add(card);

        // Add message about card becoming available
        string message = $"{card.Template.Name} is now available (Rapport {currentRapport}/{GetRequestRapportThreshold(card)})";
        GameFacade.AddMessage(message, MessageType.Success);
    }
}
```

Call this in `ExecuteListen()`:
```csharp
protected async Task ExecuteListen()
{
    // ... existing listen logic ...

    // After drawing cards, check request pile
    CheckRequestPileThresholds();

    // ... rest of method ...
}
```

### Step 4: Update GetRequestGoals to Use Request Pile
```csharp
protected List<RequestGoal> GetRequestGoals()
{
    var goals = new List<RequestGoal>();

    if (Session?.RequestPile == null) return goals;

    // Group request pile cards by threshold
    var groupedCards = Session.RequestPile
        .GroupBy(c => GetRequestRapportThreshold(c))
        .OrderBy(g => g.Key);

    foreach (var group in groupedCards)
    {
        var threshold = group.Key;
        var card = group.First();

        string goalName = card.Template.Name ?? $"Rapport {threshold} Goal";
        string reward = DetermineRewardFromCard(card);

        goals.Add(new RequestGoal
        {
            Threshold = threshold,
            Name = goalName,
            Reward = reward
        });
    }

    return goals;
}
```

## Testing Requirements
1. Build the project and ensure no compilation errors
2. Start a conversation with an NPC that has request cards
3. Verify scrolling works in cards section
4. Verify only ONE connection state display (in flow bar)
5. Verify personality rule shows actual mechanics
6. Verify request goals show actual card names
7. Verify request cards don't appear until rapport threshold is met
8. Test LISTEN action moves cards from request pile when threshold reached

## Architecture Notes
- GameWorld is single source of truth for all card definitions
- ConversationSession manages runtime state
- CardDeckManager handles deck operations
- PersonalityRuleEnforcer contains the actual personality rules
- Request cards have rapport thresholds that gate their availability