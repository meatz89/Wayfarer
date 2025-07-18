# CLAUDE.md

**⚠️ MANDATORY: READ THE ENTIRE CLAUDE.MD FILE BEFORE WRITING TO IT ⚠️**

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

### **NEW SESSION STARTUP CHECKLIST**
**CRITICAL**: Every new session must follow this exact sequence:

1. ✅ **READ CLAUDE.MD FIRST** - Understand architectural patterns and game design principles
2. ✅ **READ SESSION-HANDOFF.MD** - Get current progress, discoveries, and immediate next steps
3. ✅ **READ POC-TARGET-DESIGN.md** - Understand the target POC structure and requirements
4. ✅ **READ GAME-ARCHITECTURE.md** - Acquire a deep understanding of the architecture guidelines and principles
5. ✅ **READ INTENDED-GAMEPLAY.md** - Acquire a deep understanding of what we want the game experience to feel like for the player
6. ✅ **READ LOGICAL-SYSTEM-INTERACTIONS.MD** - Critical design guidelines for system changes
6. ✅ **READ IMPLEMENTATION-PLAN.MD** - Our current roadmap to follow
7. ✅ **ONLY THEN begin working** - Never start coding without understanding current state

### **DOCUMENTATION MAINTENANCE RULES**

1. ✅ **ALWAYS update SESSION-HANDOFF.md** - Document all discoveries, progress, and next steps
2. ✅ **ONLY update claude.md** - When architectural patterns change or new principles are discovered
3. ✅ **NEVER add temporary status** - Session progress goes in SESSION-HANDOFF.md, not claude.md
4. ✅ **Document user feedback immediately** - Critical constraints and discoveries go in SESSION-HANDOFF.md
5. ✅ **Keep files focused** - Each file has a specific purpose and audience
6. ✅ **Reference related files** - Always point to where related information can be found
7. ✅ **CONTINUALLY UPDATE DOCS** - When you learn something new that is not already documented, immediately update the relevant documentation files (IMPLEMENTATION-PLAN.md, GAME-ARCHITECTURE.md, UI-DESIGN-IMPLEMENTATION-PRINCIPLES.md, etc.) to capture the new knowledge

### **CRITICAL PATH TO TARGET VISION**
The game's vision is best captured in INTENDED-GAMEPLAY.md ("Dude, Let Me Tell You About Wayfarer"). To achieve this experience, these features are CRITICAL and must be implemented in order:

1. **Connection Gravity System** - Without it, no strategic specialization (Emma's Shadow problem)
2. **Network Referral System** - Players need agency to actively seek letters when needed
3. **Patron Mystery & Resources** - The "agent or pawn?" tension central to the experience
4. **Physical Constraints** - Equipment vs letter capacity trade-offs that define strategic depth
5. **NPC Memory & Cooling** - Makes every skip/expire decision permanently meaningful
6. **Other Letter Carriers** - The social web complexity and patron conflicts

See IMPLEMENTATION-PLAN.md Section "🎯 PRIORITY IMPLEMENTATION ROADMAP" for the complete 6-7 week path to target vision.

## PROJECT: WAYFARER - Letters and Ledgers

**WAYFARER - Letters and Ledgers** is a medieval letter-carrier RPG built as a Blazor Server application (.NET 8.0). Inspired by Kingkiller Chronicles' impossible social obligations and 80 Days' journey mechanics, players manage a priority queue of letters while navigating deadline pressure, relationship management, and mysterious patron demands. The game features a deterministic queue system with connection token economy creating constant priority dilemmas.

## CORE GAME DESIGN PRINCIPLES

### The Triumvirate Core: Letter Queue, Connection Tokens, and Standing Obligations

The entire game emerges from three interconnected systems that form the core loop:

1. **Letter Queue** - Your 8-slot priority queue of visible obligations
2. **Connection Tokens** - Relationships as spendable currency and permanent reputation
3. **Standing Obligations** - Permanent character modifications from your choices

**See Core Design Philosophy section in `IMPLEMENTATION-PLAN.md`** for how these three systems interconnect.

### The Letter Queue: Your Life in 8 Slots

The game centers around a **priority queue of 8 letters** that represents your social obligations and promises. This queue creates the entire strategic framework:

1. **Queue Order is Sacred** - You must deliver letters in order (1→2→3...) or burn relationships
2. **Deadlines Create Pressure** - Each letter has a deadline creating impossible optimization puzzles
3. **Connection Gravity** - High reputation affects where letters enter the queue
4. **Every Acceptance Matters** - New letters enter at slot 8, affecting everything above them
5. **Standing Obligations Reshape Play** - Permanent modifiers that alter queue behavior forever

### Connection Token Economy

The game uses **5 types of connection tokens** as universal currency and reputation:

1. **Trust (Green)** - Personal bonds, friendships, romance
2. **Trade (Blue)** - Merchant relationships, commercial reputation  
3. **Noble (Purple)** - Aristocratic connections, court standing
4. **Common (Brown)** - Everyday folk, local knowledge
5. **Shadow (Black)** - Underworld contacts, forbidden knowledge

Tokens serve multiple purposes:
- **Currency**: Spend on queue manipulation, route unlocking, special actions
- **Reputation**: Lifetime totals determine connection gravity and opportunities
- **Relationships**: Per-NPC tracking shows individual bonds

**CRITICAL: Contextual Token Spending Principle** - ALL token spending must be contextually tied to specific NPCs:
- **Letter Queue Manipulation**: Tokens spent from the letter SENDER's relationship (calling in their favor)
- **Route Unlocking**: Tokens spent from the NPC who controls/knows that route
- **Special Actions**: Tokens spent from the NPC providing the service or access
- **Standing Obligations**: Tokens lost from the specific NPC relationships violated

This creates narrative coherence: you're never abstractly spending "Trade tokens" - you're specifically burning your relationship with Marcus the Merchant or Elena the Scribe. Every token transaction damages or leverages a specific relationship.

**CRITICAL: Contextual Token Type Selection** - The TYPE of token gained or spent must match the interaction context:
- **Route Discovery**: If learning a merchant bypass, spend Trade tokens. If learning a mountain path, spend Common tokens
- **Letter Delivery**: Noble recipient grants Noble tokens, even if delivered for a Common NPC
- **Services**: Market transactions use Trade tokens, personal favors use Trust tokens
- **The Context Determines Type**: It's not about which NPC, but what KIND of interaction is happening

Example: Elena (a Trust NPC) teaches you a merchant route - you spend Trade tokens with Elena because it's a commercial context. Later, she asks a personal favor - you gain Trust tokens with Elena because it's a personal context.

**CRITICAL: Route Discovery Through Relationships** - Routes are discovered through natural play and relationships:
- **NO USAGE COUNTERS** - Never implement "use route X times to unlock Y" mechanics
- **NPC KNOWLEDGE** - Routes discovered by building relationships with locals who know them
- **EQUIPMENT CONTEXT** - Having proper gear makes NPCs willing to share dangerous routes
- **LETTER CONNECTIONS** - Some routes revealed through letter deliveries and recipient conversations
- **OBLIGATION NETWORKS** - Standing obligations grant access to specialized network routes

Discovery must feel like gaining local knowledge through trust, not grinding arbitrary counters.

**CRITICAL: Progression Separation Principle** - Keep progression logic completely separate from content:
- **PROGRESSION FILES** - All unlocks, requirements, and discovery rules in dedicated JSON files under Progression/
- **CLEAN ENTITIES** - NPCs, routes, locations contain ONLY their intrinsic properties (name, description, stats)
- **NO MIXED CONCERNS** - Never add unlock conditions, requirements, or progression data to entity definitions
- **DEDICATED MANAGERS** - RouteDiscoveryManager, NetworkUnlockManager handle progression, not entity managers

This prevents content pollution and keeps systems maintainable. Content describes what exists; progression describes how to access it.

### System Interconnections: How Queue and Tokens Transform Everything

**Travel System ↔ Letter Queue**
- Route planning now serves queue order enforcement
- Equipment requirements affect ability to reach recipients in deadline order
- Transport methods determine how many letters you can carry
- Time blocks consumed by travel directly impact deadline management

**Inventory System ↔ Letter Physical Constraints**  
- Letters take inventory slots based on size (documents, packages, samples)
- Equipment competes with letter capacity for space
- Transport bonuses increase letter carrying capacity
- Size categories create delivery priority puzzles

**NPC System ↔ Connection Tokens**
- Each NPC has a token type (Trust/Trade/Noble/Common/Shadow)
- NPCs remember every skipped or delayed letter
- Token accumulation with NPCs unlocks exclusive letter offers
- NPC availability windows conflict with queue delivery order

**Time System ↔ Deadline Pressure**
- 5 time blocks per day vs multiple letter deadlines
- Morning queue review and letter acceptance
- Travel time creates mathematical impossibilities
- Rest requirements conflict with delivery urgency

**Location System ↔ Letter Destinations**
- Access requirements gate certain deliveries
- Social equipment needed for noble/guild areas
- Route discovery through token spending
- Location spots as delivery endpoints

**Equipment System ↔ Queue Management**
- Climbing gear enables shortcut routes for urgent deliveries
- Social equipment (Court Attire) required for noble letters
- Equipment takes slots that could hold letters
- Equipment condition affects travel options

**Contract System → Letter System** (Complete Transformation)
- Contracts become Letters with queue positions
- Multi-step contracts become related letter chains
- Contract rewards become token payments
- Contract categories become Standing Obligations

**The Core Loop**: Accept letters → Manage queue vs deadlines → Plan routes → Navigate with equipment → Deliver in order (or spend tokens) → Earn tokens → Face new obligations → Repeat with harder choices

### System Interconnection Details

For detailed analysis of how these systems transform and interconnect, see:
- **`LETTER-QUEUE-TRANSFORMATION-ANALYSIS.md`** - Section: "System Transformations" 
- **`LOGICAL-SYSTEM-INTERACTIONS.md`** - Complete queue-based interaction rules
- **`LETTER-QUEUE-INTEGRATION-PLAN.md`** - System-by-system transformation details

## HIGH-LEVEL ARCHITECTURE

### Core Design Philosophy: Queue-Driven Social Obligations

The game follows a **letter queue priority** principle where every social obligation becomes a tangible letter in your queue:

1. **Queue position determines everything** - Letters must be delivered in order or relationships burn
2. **Tokens represent social capital** - Spendable resources for queue manipulation in crisis
3. **Deadlines create mathematical impossibilities** - Queue order vs expiration dates
4. **Standing obligations reshape gameplay** - Permanent modifiers to queue behavior
5. **Connection gravity rewards specialization** - Token accumulation affects letter entry position

### Core Design Philosophy: Token-Based Crisis Management

The game follows a **connection token economy** principle where relationships become spendable resources:

1. **Five token types map to social spheres** - Trust/Trade/Noble/Common/Shadow
2. **Tokens enable queue manipulation** - Spend social capital to solve priority crises  
3. **Token costs create meaningful trade-offs** - Burning relationships for immediate needs
4. **Connection gravity emerges from accumulation** - 3+ tokens affect queue entry position
5. **Every token spent weakens relationships** - Crisis management has permanent cost

### Architectural Patterns

#### Repository-Mediated Access (CRITICAL)
**ALL game state access MUST go through entity repositories.**

```csharp
// ❌ WRONG: Direct access
gameWorld.WorldState.Items.Add(item);

// ✅ CORRECT: Repository-mediated
itemRepository.AddItem(item);
```

#### UI Access Patterns
- **For Actions (State Changes)**: UI → GameWorldManager → Specific Manager
- **For Queries (Reading State)**: UI → Repository → GameWorld.WorldState

#### Stateless Repositories
Repositories MUST be completely stateless and only delegate to GameWorld.

*See GAME-ARCHITECTURE.md for detailed enforcement rules and patterns.*

### Game Initialization Pipeline

```
JSON Files → GameWorldSerializer → GameWorldInitializer → GameWorld → Repositories
```

**Key JSON Content Files** (`src/Content/Templates/`):
- `locations.json` - Game locations with court hierarchies and favor access levels
- `location_spots.json` - Specific spots within locations with social access requirements
- `routes.json` - Travel routes with terrain categories and information gathering opportunities
- `items.json` - Correspondence, equipment, and social signaling items
- `contracts.json` - Patron missions and favor-based assignments
- `actions.json` - Player actions including favor trading and information gathering
- `npcs.json` - NPCs with patronage relationships, favor levels, and loyalty conflicts

### Project Structure

#### **Core Game Management**
- `src/GameState/GameWorldManager.cs` - Central coordinator, UI gateway
- `src/GameState/GameWorld.cs` - Single source of truth for game state
- `src/Content/GameWorldInitializer.cs` - JSON content loading and game initialization

#### **Letter Queue System** (Replaces Contract System)
- `src/GameState/LetterQueue.cs` - 8-slot priority queue with order enforcement
- `src/GameState/Letter.cs` - Letter entity with deadline, sender, position
- `src/GameState/LetterQueueManager.cs` - Queue manipulation and delivery logic
- `src/Content/LetterRepository.cs` - Stateless letter data access

#### **Connection Token System** (Replaces Reputation/Favor)
- `src/GameState/ConnectionToken.cs` - Token entity with type (Trust/Trade/Noble/Common/Shadow)
- `src/GameState/ConnectionTokenManager.cs` - Token earning, spending, gravity calculation
- `src/GameState/StandingObligation.cs` - Permanent queue behavior modifiers
- `src/Content/TokenRepository.cs` - Stateless token data access

#### **Repository Pattern**
- `src/Content/LocationRepository.cs` - Stateless location data access
- `src/Content/ActionRepository.cs` - Stateless action data access  
- `src/Content/ItemRepository.cs` - Stateless item data access
- `src/Content/NPCRepository.cs` - Stateless NPC data access

#### **Business Logic**
- `src/GameState/TravelManager.cs` - Travel logic serving queue delivery order
- `src/GameState/MarketManager.cs` - Trading items for letter delivery
- `src/GameState/QueueManipulationService.cs` - Token spending for queue actions
- `src/GameState/RestManager.cs` - Rest and recovery logic
- `src/GameState/DeadlineManager.cs` - Deadline tracking and expiration
- `src/GameState/ConnectionGravityService.cs` - Token accumulation effects

#### **Service Configuration**
- `src/ServiceConfiguration.cs` - Dependency injection setup

#### **UI Components**
- `src/Pages/MainGameplayView.razor` - Main game screen coordinator
- `src/Pages/LetterQueue.razor` - 8-slot queue display with priority order
- `src/Pages/TokenDisplay.razor` - Connection token inventory and spending interface
- `src/Pages/Market.razor` - Trading interface for items needed in letters
- `src/Pages/TravelSelection.razor` - Route planning for queue delivery order
- `src/Pages/LetterBoard.razor` - Morning letter selection and acceptance
- `src/Pages/DeliveryInterface.razor` - Letter delivery and token earning
- `src/Pages/QueueManipulation.razor` - Token spending for queue actions
- `src/Pages/StandingObligations.razor` - Active permanent modifiers display
- `src/Pages/RestUI.razor` - Rest and recovery interface

### Testing Architecture

- **Framework**: xUnit
- **Test Isolation**: Each test class uses its own JSON content
- **NEVER use production JSON in tests** - create test-specific data

*See GAME-ARCHITECTURE.md for detailed testing patterns and requirements.*

### Code Quality

The project uses multiple code analyzers:
- Microsoft.CodeAnalysis.Analyzers
- Roslynator.Analyzers
- SonarAnalyzer.CSharp

Analysis is configured in `wayfarer.ruleset` with enforcement during build.

### Key Design Principles

1. **Queue Order Creates Drama**: The requirement to deliver in order (or burn tokens) creates constant moral dilemmas.
2. **Tokens Are Relationships**: Every token spent represents actual social capital being burned for immediate needs.
3. **Deadlines Force Impossible Choices**: Mathematical impossibility of satisfying all deadlines creates strategic depth.
4. **Gravity Rewards Specialization**: Token accumulation affects queue entry, encouraging relationship focus.
5. **Standing Obligations Reshape Play**: Permanent modifiers create unique playthroughs based on choices.
6. **All Mechanics Visible**: Queue position, deadlines, token costs, and gravity effects must be clear in UI.

### Categorical Interconnection Requirements

**All entities must have unique types/categories** that interact with the queue and token systems:
- NPCs: `TokenType` (Trust/Trade/Noble/Common/Shadow), `LetterSender` (true/false), `DeliveryRecipient` (true/false)
- Letters: `Size` (Small/Medium/Large), `TokenType` (matches sender), `Deadline` (days), `QueuePosition` (1-8)
- Locations: `AccessLevel` (Public/Private/Restricted), `TokenRequirement` (which tokens needed for access)
- Equipment: `EnablesAccess` (Noble/Guild/Shadow areas), `EnablesRoutes` (mountain/forest paths)

**Game rules emerge from queue position and token interactions**:
- Queue Position + Deadline → Delivery pressure and crisis decisions
- Token Accumulation + Letter Type → Connection gravity queue entry boost
- Token Spending + Queue Position → Crisis management options
- Standing Obligations + Letter Type → Permanent queue behavior changes

**Core Design Rules:**
- **NEVER** allow free queue reordering - always require token cost or relationship burn
- **ALWAYS** make deadlines create real pressure through queue position conflicts  
- **ENSURE** token costs are meaningful - spending tokens burns actual relationships
- **VISIBLE** queue state - players must see position, deadlines, and token costs clearly

*See GAME-ARCHITECTURE.md for detailed categorical system implementations.*

### UI FEEDBACK AND MESSAGING
**ALWAYS use MessageSystem for user feedback** - Never use Console.WriteLine or similar for user feedback.
```csharp
// ❌ WRONG: Console output for user feedback
Console.WriteLine($"Delivered letter! Earned {payment} coins");

// ✅ CORRECT: MessageSystem for user feedback
MessageSystem.AddSystemMessage($"Delivered letter! Earned {payment} coins", SystemMessageTypes.Success);
```

The MessageSystem is properly displayed in the UI and provides consistent feedback across the game. Message types include:
- `SystemMessageTypes.Success` - For positive outcomes (deliveries, rewards)
- `SystemMessageTypes.Warning` - For cautions (low tokens, expiring letters)
- `SystemMessageTypes.Error` - For failures (insufficient tokens, missed deadlines)
- `SystemMessageTypes.Info` - For neutral information

### CODE WRITING PRINCIPLES

**TRANSFORMATION APPROACH**:
- **RENAME AND RECONTEXTUALIZE** - Don't wrap new functionality in old classes, rename them to reflect new purpose
- **DELETE LEGACY CODE ENTIRELY** - Remove old contract system, reputation system, favor system completely
- **NO COMPATIBILITY LAYERS** - Clean break from old mechanics to new queue/token system
- **FRESH TEST SUITE** - Delete old tests and write new ones for queue/token mechanics

**GENERAL PRINCIPLES**:
- Do not leave comments in code that are not TODOs or SERIOUSLY IMPORTANT
- After each change, run the tests to check for broken functionality. Never commit while tests are failing
- **ALWAYS write unit tests confirming errors before fixing them** - This ensures the bug is properly understood and the fix is validated
- You must run all tests and execute the game and do quick smoke tests before every commit
- **Never keep legacy code for compatibility**
- **NEVER use suffixes like "New", "Revised", "V2", etc.** - Replace old implementations completely and use the correct final name immediately. Delete old code, don't leave it behind.
- **CRITICAL: NEVER USE REFLECTION** - If you find ANY reflection usage in the codebase:
  1. **IMMEDIATELY create highest priority TODO** to remove the reflection
  2. **STOP all other work** - reflection makes code unmaintainable and breaks refactoring
  3. **Fix it properly** - make fields public, add proper accessors, or redesign the architecture
  4. **NO EXCEPTIONS** - There is never a valid reason to use reflection in production code
- **CRITICAL: LEGACY CODE ELIMINATION PRINCIPLE** - When files contain ONLY legacy functionality:
  1. **DELETE THE ENTIRE FILE** - Do not comment out, do not exclude from compilation
  2. **REMOVE COMPLETELY** - If a test file only tests removed systems, delete it entirely
  3. **NO PARTIAL PRESERVATION** - Do not try to salvage parts of legacy-only files
  4. **DOCUMENT THE PRINCIPLE** - Add removal principles to CLAUDE.md, not individual changes
  5. **NO DELETION COMMENTS** - Do not leave comments about what was deleted, just remove it cleanly
- **CRITICAL: IMMEDIATE LEGACY CODE ELIMINATION** - If you discover ANY legacy code, compilation errors, or deprecated patterns during development, you MUST immediately:
  1. **CREATE HIGH-PRIORITY TODO ITEM** to fix the legacy code
  2. **STOP current work** and fix the legacy code immediately
  3. **NEVER ignore or postpone** legacy code fixes
  4. **NEVER say "these are just dependency fixes"** - fix them now or create immediate todo items
- **CRITICAL: ARCHITECTURAL BUG DISCOVERY** - If you discover architectural bugs (e.g., duplicate state storage, inconsistent data access patterns), you MUST:
  1. **IMMEDIATELY create highest priority TODO** to fix the architectural issue
  2. **STOP all other work** - architectural bugs corrupt the entire system
  3. **NEVER work around architectural bugs** - fix them at the source
  4. **Document the fix in GAME-ARCHITECTURE.md** for future reference
- **CRITICAL: CONTENT/LOGIC SEPARATION** - NEVER hardcode content IDs (location names, item names, NPC names, etc.) into business logic:
  1. **CONTENT ≠ GAME LOGIC** - Content IDs must never control program flow
  2. **NO HARDCODED CONTENT IDS** - Never use specific location names, item names, etc. in switch statements or conditionals
  3. **USE PROPERTIES/CATEGORIES** - Business logic should operate on entity properties (LocationType, ItemCategory, etc.), not specific IDs
  4. **CONTENT IS DATA** - Content should be configurable data, not part of the code logic
  5. **VIOLATION IS CRITICAL** - Any hardcoded content ID in business logic is a critical architectural violation
- **CRITICAL: NEVER USE STRING-BASED CATEGORY MAPPING** - Categories must be properly defined in JSON and parsed into enums/classes:
  1. **FORBIDDEN**: Mapping categories based on string matching in item IDs (e.g., `itemId.Contains("hammer")`)
  2. **REQUIRED**: Categories must be explicit properties in JSON files
  3. **REQUIRED**: Parsers must map JSON category properties to proper enum values
  4. **NO EXCEPTIONS** - String-based inference of categories violates the categorical design principle
- **CRITICAL: NEVER LEAVE DEPRECATED CODE** - Remove deprecated fields, properties, and methods immediately:
  1. **FORBIDDEN**: Leaving deprecated fields/properties/methods with [Obsolete] attributes
  2. **FORBIDDEN**: Keeping old implementations "for backward compatibility"
  3. **REQUIRED**: Delete deprecated code immediately when refactoring
  4. **REQUIRED**: Update all references to use new implementations
  5. **NO EXCEPTIONS** - Deprecated code creates confusion and maintenance debt
- **NAMESPACE POLICY** - Special exception for Blazor components:
  1. **NO NAMESPACES in regular C# files** - Makes code easier to work with, no using statements needed
  2. **EXCEPTION: Blazor/Razor components MAY use namespaces** - Required for Blazor's component discovery
  3. **Blazor namespace pattern**: Use `Wayfarer.Pages` for pages, `Wayfarer.Pages.Components` for components
  4. **Update _Imports.razor** - Include necessary namespace imports for Blazor components only
- **CRITICAL: NO FALLBACKS FOR OLD DATA** - Fix data files instead of adding compatibility code:
  1. **FORBIDDEN**: Adding fallback logic to handle old JSON/data formats
  2. **FORBIDDEN**: Writing code like "fallback to old property if new one missing"
  3. **REQUIRED**: Update all JSON/data files to use new format immediately
  4. **REQUIRED**: Remove old properties from data files completely
  5. **NO EXCEPTIONS** - Fallback code is technical debt that will never be cleaned up

### NARRATIVE COMMUNICATION PRINCIPLE (Critical)
**All game mechanics must communicate to the player through visible UI and narrative context. Silent background mechanics violate player agency.**

**Core Rules:**
- ✅ **EVERY CHANGE IS VISIBLE** - If something changes, player sees it happen with narrative context
- ✅ **PLAYER TRIGGERS MECHANICS** - No automatic progressions without player action
- ✅ **CLEAR CAUSE AND EFFECT** - Players understand why things happen
- ❌ **NO SILENT MECHANICS** - Never change state without UI feedback
- ❌ **NO HIDDEN CALCULATIONS** - All math that affects gameplay must be transparent

**Example**: Instead of silently adding tokens, show "Elena smiles. 'Thank you!' (+1 Trust token with Elena)"

### GAME DESIGN PRINCIPLES (Critical for Games vs Apps)
**Games create interactive optimization puzzles for the player to solve, not automated systems that solve everything for them.**

- ✅ **GAMEPLAY IS IN THE PLAYER'S HEAD** - Fun comes from systems interacting in clever ways that create optimization challenges
- ✅ **DISCOVERY IS GAMEPLAY** - Players must explore, experiment, and learn to find profitable trades, efficient routes, optimal strategies
- ❌ **NO AUTOMATED CONVENIENCES** - Don't create `GetProfitableItems()` or `GetBestRoute()` methods that solve the puzzle for the player
- ❌ **NO GAMEPLAY SHORTCUTS** - No "Trading Opportunities" UI panels that tell players exactly what to do
- ✅ **EMERGENT COMPLEXITY** - Simple systems (pricing, time blocks, stamina) that interact to create deep strategic decisions
- ✅ **MEANINGFUL CHOICES** - Every decision should involve sacrificing something valuable (time vs money vs stamina)
- ✅ **PLAYER AGENCY** - Players discover patterns, build mental models, develop personal strategies through exploration

**Example**: Instead of showing "Buy herbs at town_square (4 coins) → Sell at dusty_flagon (5 coins) = 1 profit", let players discover this by visiting locations, checking prices, and building their own understanding of the market.

### UI DESIGN PRINCIPLES (Critical for Game vs App UX)
**UI should support discovery and decision-making, not replace player thinking or overwhelm with information.**

#### **CONTEXTUAL INFORMATION PRINCIPLES**
- ✅ **SHOW RELEVANT, NOT COMPREHENSIVE** - Display only information immediately relevant to player's current context
- ✅ **PROGRESSIVE DISCLOSURE** - Start with essential info, allow drilling down for details when needed
- ❌ **NO INFORMATION OVERLOAD** - Don't show all possible information at once
- ❌ **NO STRATEGIC CATEGORIZATION** - Don't artificially separate information into "strategic" vs "non-strategic"

#### **DECISION-FOCUSED DESIGN**
- ✅ **DECISION SUPPORT** - Present information that helps players make immediate decisions
- ✅ **CONTEXTUAL RELEVANCE** - Show information based on what the player is currently doing
- ❌ **NO OPTIMIZATION HINTS** - Don't tell players what the "best" choice is
- ❌ **NO AUTOMATED ANALYSIS** - Don't provide "Investment Opportunities" or "Trade Indicators"

#### **SPATIAL EFFICIENCY**
- ✅ **EFFICIENT SPACE USE** - Every pixel should serve a purpose
- ✅ **VISUAL HIERARCHY** - Use icons, colors, and layout to convey information quickly
- ❌ **NO VERBOSE TEXT** - Don't use 15+ lines of text when 3-4 lines suffice
- ❌ **NO REDUNDANT SECTIONS** - Don't repeat the same information in multiple places

#### **FORBIDDEN UI PATTERNS**
- ❌ **"Strategic Market Analysis" sections** - Violates NO AUTOMATED CONVENIENCES principle
- ❌ **"Equipment Investment Opportunities"** - Tells players what to buy, removing discovery
- ❌ **"Trade Opportunity Indicators"** - Automated system solving optimization puzzles
- ❌ **"Profitable Items" lists** - Removes the challenge of finding profit opportunities
- ❌ **"Best Route" recommendations** - Eliminates route planning gameplay
- ❌ **Verbose NPC schedules** - Information overload that doesn't help decisions

#### **REQUIRED UI PATTERNS**
- ✅ **Basic availability indicators** - Simple 🟢/🔴 status without detailed explanations
- ✅ **Item categories for filtering** - Help players find what they're looking for
- ✅ **Current status information** - What's happening right now
- ✅ **Essential action information** - What the player can do immediately
- ✅ **Click-to-expand details** - Full information available when specifically requested

#### **CSS ARCHITECTURE PRINCIPLES**
See `UI-DESIGN-IMPLEMENTATION-PRINCIPLES.md` for complete CSS architecture and validation requirements.

### FRONTEND PERFORMANCE PRINCIPLES
- **NEVER use caching in frontend components** - Components should be stateless and reactive
- **Reduce queries by optimizing when objects actually change** - Focus on state change detection, not caching
- **Log at state changes, not at queries** - Debug messages should track mutations, not reads
- **Use proper reactive patterns** - Let Blazor's change detection handle rendering optimization


*See GAME-ARCHITECTURE.md for detailed initialization flow and system dependencies.*

# Development Principles: Emergent Design

## Core Principle: Emergent Mechanics
**Never hardcode restrictions or bonuses.** All gameplay constraints must emerge from mathematical interactions between simple atomic systems (time, stamina, coins, etc.). Players should experience strategic pressure through resource scarcity and efficiency trade-offs, not arbitrary limitations. If you're tempted to write `if (condition) { player.CanNotDoX = true; }` or add magic bonuses, instead create mathematical relationships that naturally discourage or encourage the behavior through consequences.

## Design Framework: Experience vs Mechanics vs Agency
Always distinguish between three layers:

**Player Experience**: What the player feels ("I can't afford to trade during rush deliveries")  
**Hardcoded Mechanics**: What the code actually enforces (tight deadlines, travel time, resource costs)  
**Player Agency**: What choices remain available (can still trade, but will miss deadline and lose payment)

**Goal**: Complex strategic experiences should arise from simple mathematical rules interacting, not from programmed restrictions. Players should always retain choice, but face natural consequences that guide rational decision-making. The game should feel designed and intentional while being mathematically inevitable.

## Game Feature Simplicity Principle
**Games are not compliance systems.** When implementing consequences or tracking player behavior:

- ✅ **IMMEDIATE CONSEQUENCES** - One-time penalties that happen right when the player acts (lose tokens, miss payment)
- ✅ **NATURAL CONSEQUENCES** - Consequences that emerge from existing systems (expired letters lose tokens)
- ✅ **VISIBLE WARNINGS** - Show danger coming, not complex tracking after (highlight expiring letters)
- ❌ **NO VIOLATION TRACKING** - Don't track how many times a player broke a rule
- ❌ **NO ESCALATING PENALTIES** - Don't make consequences worse over time
- ❌ **NO REPUTATION SYSTEMS** - Token counts ARE the reputation system

**Example**: When a letter expires, the player loses 2 tokens with that NPC. That's it. No violation counter, no reputation hit, no permanent record. The lost tokens ARE the consequence, and they naturally affect future gameplay through connection gravity.

**The Core Truth**: If you're building complex tracking systems, you're not making a game - you're making a spreadsheet. Games create tension through immediate, understandable consequences that affect player decisions going forward.

## GAME INITIALIZATION
- Ensure game initialization leverages JSON files, parsers, GameWorldInitialization, Repositories, and GameWorld classes
- Create comprehensive tests to validate that all content from JSON files is correctly saved into GameWorld

## LETTER QUEUE TRANSFORMATION STATUS

**Current State**: Existing contract-based trading RPG with economic mechanics
**Target State**: Letter queue management game with 8-slot priority system and connection token economy
**Transformation Plan**: See `LETTER-QUEUE-TRANSFORMATION-ANALYSIS.md` for comprehensive 12-week plan

**Key Transformation Principles**:
1. **Maintain Playability**: Never break the game during transformation phases
2. **Parallel Systems**: Run letter queue alongside existing systems during migration
3. **Content Evolution**: Transform NPCs from vendors to relationship partners with letter generation
4. **UI Revolution**: Replace location-based screens with queue-centric interface
5. **Save Migration**: Implement versioned saves with safe rollback options

**🎯 IMMEDIATE NEXT STEPS**: Implement **Connection Gravity System** (see details in `IMPLEMENTATION-PLAN.md`)
**Current Status**: Minimal POC achieved, relationship transparency complete, gravity design complete
**Critical Path**: Connection Gravity → Network Referrals → Patron Resources → Full Vision

### Important Documentation

**CRITICAL TRANSFORMATION DOCUMENTS** (Read these for complete understanding):
- 🎯 **`LETTER-QUEUE-TRANSFORMATION-ANALYSIS.md`** - Comprehensive analysis of the letter queue transformation with step-by-step implementation plan
- 📬 **`LETTER-QUEUE-UI-SPECIFICATION.md`** - Complete UI architecture for the 3-screen letter queue system
- 🔄 **`LETTER-QUEUE-INTEGRATION-PLAN.md`** - How existing systems transform to serve the queue
- 📋 **`POC-IMPLEMENTATION-ROADMAP.md`** - Phase-by-phase development plan for letter queue POC

**CORE DESIGN DOCUMENTS**:
- `IMPLEMENTATION-PLAN.md` - Complete system architecture, roadmap, and core design philosophy
- `INTENDED-GAMEPLAY.md` - The letter queue player experience and Kvothe moments
- `LOGICAL-SYSTEM-INTERACTIONS.md` - Queue mechanics and token economy rules
- `POC-TARGET-DESIGN.md` - Minimal POC with 8-slot queue and connection tokens
- `SESSION-HANDOFF.md` - Current implementation status and next steps

**TECHNICAL DOCUMENTS**:
- `GAME-ARCHITECTURE.md` - Repository patterns, per-NPC token tracking, and testing requirements
- `UI-DESIGN-IMPLEMENTATION-PRINCIPLES.md` - Queue-centric UI principles and anti-patterns
- `SESSION-HANDOFF.md` - Current transformation status and next steps

**SUPPORTING MATERIALS**:
- `/docs/` - Original game design documentation (40+ documents)
- Existing source code in `/src/` - To be transformed per the implementation plan