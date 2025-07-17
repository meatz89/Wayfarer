# GAME ARCHITECTURE PRINCIPLES

This document defines the core architectural patterns and principles that must be maintained for system stability and design consistency.

## CORE ARCHITECTURAL PATTERNS

### Repository-Mediated Access (CRITICAL)
**ALL game state access MUST go through entity repositories.**

```csharp
// ❌ WRONG: Direct access
gameWorld.WorldState.Items.Add(item);

// ✅ CORRECT: Repository-mediated
itemRepository.AddItem(item);
```

### UI Access Patterns
- **Actions (State Changes)**: UI → GameWorldManager → Specific Manager
- **Queries (Reading State)**: UI → Repository → GameWorld.WorldState

### Stateless Repositories
Repositories MUST be stateless and only delegate to GameWorld.

## LETTER QUEUE SYSTEM ARCHITECTURE

### Core Components
- **LetterQueueManager**: 8-slot priority queue with position enforcement
- **Letter Entity**: Id, SenderId, RecipientId, TokenType, Deadline, Payment, QueuePosition
- **ConnectionTokenManager**: 5 token types (Trust/Trade/Noble/Common/Shadow) with per-NPC tracking
- **StandingObligationManager**: Permanent queue behavior modifiers

### Repository Pattern Implementation

```csharp
// LetterRepository - All letter data access
public class LetterRepository {
    private readonly GameWorld _gameWorld;
    
    public List<Letter> GetPlayerQueue() => _gameWorld.WorldState.Player.LetterQueue;
    public void AddLetterToQueue(Letter letter) { /* Position logic */ }
    public void RemoveLetterFromQueue(string letterId) { /* Shift positions */ }
}

// ConnectionTokenRepository - Token management
public class ConnectionTokenRepository {
    public Dictionary<ConnectionType, int> GetPlayerTokens();
    public Dictionary<string, Dictionary<ConnectionType, int>> GetTokensByNPC();
    public void AddTokens(ConnectionType type, int count, string npcId);
    public bool SpendTokens(ConnectionType type, int count);
}
```

## CONTENT/LOGIC SEPARATION (CRITICAL)

### Content ≠ Game Logic
**NEVER hardcode content IDs in business logic**

```csharp
// ❌ WRONG: Hardcoded content
if (itemId == "silver_sword") { damage = 10; }

// ✅ CORRECT: Use properties
if (item.Category == ItemCategory.Weapon) { damage = item.Damage; }
```

### Rules
1. **NO HARDCODED CONTENT IDS**: Never use specific names in switch statements
2. **USE PROPERTIES/CATEGORIES**: Business logic operates on entity properties
3. **CONTENT IS DATA**: Content should be configurable, not coded

## GAME INITIALIZATION PIPELINE

```
JSON Files → GameWorldSerializer → GameWorldInitializer → GameWorld → Repositories
```

### Key Content Files
- `letter_templates.json` - Letter generation templates
- `npcs.json` - NPCs with professions and relationships
- `actions.json` - Player actions and requirements
- `routes.json` - Travel routes with unlock conditions
- `locations.json` - Game locations with access requirements

## TESTING ARCHITECTURE

### Test Isolation
- **Each test class uses its own JSON content**
- **NEVER use production JSON in tests**
- **Create test-specific data**

### Test Patterns
```csharp
// ✅ CORRECT: Isolated test data
[Test]
public void TestLetterDelivery() {
    var testGameWorld = TestGameWorldFactory.CreateWithTestData();
    var letterQueueManager = new LetterQueueManager(testGameWorld, ...);
    // Test logic
}
```

## MESSAGE SYSTEM USAGE (CRITICAL)

**ALWAYS use MessageSystem for user feedback**

```csharp
// ❌ WRONG: Console output
Console.WriteLine("Letter delivered!");

// ✅ CORRECT: MessageSystem
MessageSystem.AddSystemMessage("Letter delivered!", SystemMessageTypes.Success);
```

### Message Types
- `SystemMessageTypes.Success` - Positive outcomes
- `SystemMessageTypes.Warning` - Cautions
- `SystemMessageTypes.Danger` - Failures
- `SystemMessageTypes.Info` - Neutral information

## CSS ARCHITECTURE PRINCIPLES

### Separate CSS Files
- **All CSS in dedicated .css files** - Never inline in Razor components
- **Use CSS variables** - Always use existing variables (--text-primary, --bg-panel)
- **Check existing styles first** - Always review existing CSS before creating new files
- **Maintain visual hierarchy** - Follow established font sizes and spacing

```css
/* ✅ CORRECT: Use CSS variables */
.letter-queue {
    background: var(--bg-panel);
    color: var(--text-primary);
}

/* ❌ WRONG: Hardcoded colors */
.letter-queue {
    background: #2a2a2a;
    color: #ffffff;
}
```

## CATEGORICAL DESIGN PRINCIPLE

### All Entities Need Categories
**Every entity must have unique types/categories for system interactions**

- **NPCs**: `TokenType`, `Profession`, `Schedule`
- **Letters**: `TokenType`, `Size`, `IsFromPatron`
- **Locations**: `AccessLevel`, `TokenRequirement`
- **Equipment**: `Category`, `EnablesAccess`

### Category-Based Rules
```csharp
// Game rules emerge from categorical interactions
if (npc.Profession == Professions.Merchant) {
    tokenType = ConnectionType.Trade;
}
```

## LEGACY CODE ELIMINATION

### Immediate Removal Principle
1. **DELETE THE ENTIRE FILE** - Don't comment out, remove completely
2. **REMOVE REFERENCES** - Clean up all dependencies
3. **NO PARTIAL PRESERVATION** - Don't try to salvage parts
4. **DOCUMENT REMOVAL** - Add removal principles to architecture docs

### Architectural Bug Discovery
**If you discover architectural bugs, STOP and fix them immediately**

Common violations:
- Duplicate state storage
- Inconsistent data access patterns
- Hardcoded content IDs in business logic
- Direct GameWorld access bypassing repositories

## GAME DESIGN PRINCIPLES

### Games Create Optimization Puzzles
**Players solve puzzles, systems don't solve for players**

```csharp
// ❌ WRONG: Automated system
public List<Item> GetProfitableItems() { /* Solves puzzle for player */ }

// ✅ CORRECT: Player discovers
public List<Item> GetAvailableItems() { /* Player finds opportunities */ }
```

### Emergent Complexity
- **Simple systems** (queue position, deadlines, tokens) interact to create strategic depth
- **Player agency** through meaningful choices with consequences
- **Discovery gameplay** through exploration and experimentation

## DEPENDENCY INJECTION PATTERNS

### Service Registration
```csharp
// Services must be registered for both app and tests
services.AddScoped<LetterQueueManager>();
services.AddScoped<ConnectionTokenManager>();
services.AddScoped<RouteUnlockManager>();
```

### Manager Dependencies
```csharp
// Managers depend on GameWorld and specific repositories
public LetterQueueManager(
    GameWorld gameWorld,
    LetterTemplateRepository letterTemplateRepository,
    NPCRepository npcRepository,
    MessageSystem messageSystem) { }
```

## TRANSFORMATION ARCHITECTURE

### System Interconnections
**All systems serve the letter queue priority system**

- **Travel System** → Queue delivery order enforcement
- **Inventory System** → Letter carrying capacity constraints
- **NPC System** → Connection token earning and spending
- **Equipment System** → Access to delivery locations
- **Time System** → Deadline pressure creation

### Queue-Centric Design
Every feature must answer: "How does this serve the letter queue optimization puzzle?"

---

**This document contains the essential architectural patterns. Follow these principles to maintain system consistency and avoid architectural violations.**