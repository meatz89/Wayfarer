# Minimal POC Implementation Plan: Letter Queue Core

**Goal**: Get a working letter queue system with minimal content to validate core mechanics. Focus on "systems in place, content minimal, ignore edge cases."

**Timeline**: 3 weeks to working POC

---

## **WEEK 1: Core Queue System**
*Just enough to see letters in a queue*

### Day 1-2: Letter and Queue Basics
```csharp
// Minimal Letter Entity
public class Letter {
    public string Id { get; set; }
    public string SenderName { get; set; }  // Just a string for now
    public string RecipientName { get; set; }
    public int Deadline { get; set; }
    public int Payment { get; set; }
    public ConnectionType TokenType { get; set; }
}

// Basic Queue
public class LetterQueue {
    private Letter[] slots = new Letter[8];
    
    public void AddLetter(Letter letter, int position) { 
        slots[position - 1] = letter;
    }
    
    public Letter GetLetterAt(int position) {
        return slots[position - 1];
    }
    
    public void RemoveLetterAt(int position) {
        slots[position - 1] = null;
        // Don't worry about shifting yet
    }
}
```

### Day 3: Connection Tokens
```csharp
public enum ConnectionType { Trust, Trade, Noble, Common, Shadow }

// Add to Player class
public Dictionary<ConnectionType, int> Tokens { get; set; } = new();
```

### Day 4-5: Minimal Queue UI
```razor
@* Super basic queue display *@
<div class="letter-queue">
    @for (int i = 1; i <= 8; i++) {
        <div class="queue-slot">
            <span>[@i]</span>
            @if (GetLetter(i) != null) {
                <div>@GetLetter(i).SenderName → @GetLetter(i).RecipientName</div>
                <div>@GetLetter(i).Deadline days | @GetLetter(i).Payment coins</div>
            } else {
                <div>[Empty]</div>
            }
        </div>
    }
</div>

@* Token display *@
<div class="tokens">
    @foreach (var token in GetPlayerTokens()) {
        <span>@token.Key: @token.Value</span>
    }
</div>
```

---

## **WEEK 2: Minimal Content & Actions**
*Just enough to test the system*

### Day 6-7: Three Test NPCs
```json
// Minimal NPCs - one per existing location
{
  "npcs": [
    {
      "id": "elena_test",
      "name": "Elena",
      "location": "millbrook",
      "tokenType": "Trust"
    },
    {
      "id": "marcus_test", 
      "name": "Marcus",
      "location": "thornwood",
      "tokenType": "Trade"
    },
    {
      "id": "noble_test",
      "name": "Lord Smith",
      "location": "crossbridge", 
      "tokenType": "Noble"
    }
  ]
}
```

### Day 8: Basic Letter Templates
```json
// 5-10 simple templates
{
  "letterTemplates": [
    {
      "id": "personal_1",
      "description": "Personal letter from {sender}",
      "tokenType": "Trust",
      "deadlineRange": [3, 5],
      "paymentRange": [2, 4]
    },
    {
      "id": "trade_1",
      "description": "Trade goods for {sender}",
      "tokenType": "Trade", 
      "deadlineRange": [2, 4],
      "paymentRange": [5, 8]
    }
    // ... few more
  ]
}
```

### Day 9: Token Earning
```csharp
// In delivery method
public void DeliverLetter(Letter letter) {
    // Give 1 token of the letter's type
    player.Tokens[letter.TokenType]++;
    
    // Remove from queue
    queue.RemoveLetterAt(1);
    
    // Add payment
    player.Coins += letter.Payment;
}
```

### Day 10: One Queue Action (Skip)
```csharp
public bool TrySkipDeliver(int position) {
    var letter = queue.GetLetterAt(position);
    if (letter == null) return false;
    
    // Need 1 token per position skipped
    int cost = position - 1;
    if (player.Tokens[letter.TokenType] >= cost) {
        player.Tokens[letter.TokenType] -= cost;
        DeliverLetter(letter);
        return true;
    }
    return false;
}
```

---

## **WEEK 3: Basic Integration**
*Connect to existing systems minimally*

### Day 11: Time Integration
```csharp
// Add to daily update
public void ProcessDailyUpdate() {
    // Countdown all deadlines
    for (int i = 1; i <= 8; i++) {
        var letter = queue.GetLetterAt(i);
        if (letter != null) {
            letter.Deadline--;
            if (letter.Deadline <= 0) {
                // Just remove for now
                queue.RemoveLetterAt(i);
            }
        }
    }
}
```

### Day 12: Letter Generation
```csharp
// Generate 1-2 letters per day
public void GenerateDailyLetters() {
    int count = Random.Range(1, 3);
    for (int i = 0; i < count; i++) {
        var template = GetRandomTemplate();
        var letter = new Letter {
            Id = Guid.NewGuid().ToString(),
            SenderName = GetRandomNPC().Name,
            RecipientName = GetRandomNPC().Name,
            Deadline = Random.Range(template.DeadlineMin, template.DeadlineMax),
            Payment = Random.Range(template.PaymentMin, template.PaymentMax),
            TokenType = template.TokenType
        };
        
        // Add to first empty slot (ignore gravity for now)
        AddToFirstEmptySlot(letter);
    }
}
```

### Day 13: Delivery at Position 1
```csharp
// Only allow delivery from position 1
public bool CanDeliver(int position) {
    if (position != 1) return false;
    
    var letter = queue.GetLetterAt(1);
    if (letter == null) return false;
    
    // For POC, assume player is always at right location
    return true;
}
```

### Day 14-15: Minimal Relationship Screen
```razor
@page "/relationships"

<h2>Character Relationships</h2>

@foreach (var npc in GetTestNPCs()) {
    <div class="npc-card">
        <h3>@npc.Name (@npc.TokenType)</h3>
        <p>Location: @npc.Location</p>
        <p>Your @npc.TokenType tokens: @GetTokenCount(npc.TokenType)</p>
    </div>
}
```

---

## **MINIMAL POC VALIDATION**

### Core Mechanics Working
- [x] 8-slot queue displays letters
- [x] Letters have sender, recipient, deadline, payment
- [x] Can add letters to queue
- [x] Can only deliver from position 1
- [x] Delivering earns 1 token
- [x] Can skip positions by spending tokens
- [x] Deadlines countdown daily
- [x] Letters expire when deadline = 0

### What We're NOT Doing Yet
- ❌ Queue shifting when letters removed
- ❌ Connection gravity 
- ❌ Standing obligations
- ❌ Per-NPC token tracking
- ❌ Patron letters
- ❌ Complex letter generation
- ❌ Relationship memory
- ❌ Save system updates
- ❌ Removing old systems
- ❌ Polish and edge cases

---

## **SUCCESS CRITERIA FOR MINIMAL POC**

1. **Queue Visible**: Can see 8 slots with letters
2. **Order Enforced**: Must deliver from position 1 (or skip with tokens)
3. **Tokens Work**: Earn tokens from delivery, spend to skip
4. **Deadlines Matter**: Letters expire and disappear
5. **Basic Loop**: Accept letters → Manage queue → Deliver → Earn tokens

---

## **IMPLEMENTATION NOTES**

### Use Existing Infrastructure
- Hook into existing GameWorldManager for daily updates
- Use existing UI framework for new screens
- Leverage existing location/travel (don't modify yet)
- Keep existing save system (add new fields carefully)

### Minimal Content Needs
- 3 NPCs (one per location)
- 5-10 letter templates
- No special letters yet
- No narrative content
- Just functional mechanics

### Quick Hacks Allowed
- Hardcode some values
- Skip error handling
- Ignore edge cases
- Use placeholder UI
- Don't optimize yet

---

## **NEXT STEPS AFTER POC**

Once this minimal POC is working:
1. **Playtest** core loop for fun
2. **Validate** queue pressure exists
3. **Tune** token earning/spending
4. **Then** proceed with full implementation

This minimal approach gets us to a playable letter queue in 3 weeks, allowing us to validate the core concept before investing in the full 12-week transformation.