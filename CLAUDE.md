# CLAUDE.md

**⚠️ MANDATORY: READ THE ENTIRE CLAUDE.MD FILE BEFORE WRITING TO IT ⚠️**

**⚠️ CRITICAL: ALWAYS READ THE FULL FILE BEFORE MODIFYING IT ⚠️**
**NEVER make changes to a file without reading it completely first. This is non-negotiable.**

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
The game's vision is best captured in INTENDED-GAMEPLAY.md ("Dude, Let Me Tell You About Wayfarer"). To achieve this experience through RESOURCE COMPETITION:

**Core Design Principle**: Independent systems compete for shared resources (hours, stamina, inventory, queue slots, tokens). No cross-system rules - just resource scarcity creating strategic tension.

**The Five Shared Resources**:
1. **HOURS** (12-16 per day) - Every action costs 1 hour
2. **STAMINA** (0-10 scale) - Physical activities drain energy  
3. **INVENTORY SLOTS** (8 total) - Letters and equipment compete
4. **QUEUE POSITIONS** (8 slots) - Delivery order constraints
5. **CONNECTION TOKENS** (Per-NPC) - Social capital currency

**Critical Features to Implement**:
1. **Three-State Letter System** - Offered → Accepted → Collected states
2. **Hour-Based Actions** - Every meaningful action costs 1 hour
3. **Fixed Stamina Costs** - Travel (2), Work (2), Deliver (1), Rest (+3)
4. **Physical Collection Requirement** - Position 1 must be collected before delivery
5. **Token Thresholds** - 0=Stranger, 3+=Letters offered, 5+=Better categories
6. **Route Knowledge Gates** - Routes require token thresholds with specific NPCs
7. **NPC Availability Windows** - Time periods when NPCs can be found

**CRITICAL: Compound Actions Through Natural Emergence**
- **NO SPECIAL COMPOUND RULES** - Actions remain atomic and independent
- **Natural Overlaps** - Same location/time/NPC enables multiple actions
- **Discovery Through Play** - Players find efficiencies through exploration and experimentation
- **Context Creates Opportunity** - Being at merchant when delivering allows trading
- **Never Code Combinations** - Just let independent systems share resources

**CRITICAL: More Options, Not Better Options**
- **NEVER make actions "more efficient" in special contexts** - This violates player agency
- **ADD additional action choices** - Tavern doesn't make rest better, it offers "buy drinks while resting"
- **Each action keeps its base efficiency** - Rest is always +3 stamina, work is always 2 stamina→4 coins
- **Context adds NEW options** - Dawn baker offers different work (1 stamina→2 coins+bread), not better work
- **Player discovers combinations** - Buying drinks gets rest AND tokens, but costs more
- **ALL EFFECTS VISIBLE IN UI** - Every contextual option must clearly show ALL effects before choosing

**The Patron Dynamic**: You never meet your patron, just receive gold-sealed letters that jump to slots 1-3. They provide resources but their motives are unclear. Are you an agent with purpose or just a pawn being used?

**The Carrier Network**: Other carriers serve different patrons. You share routes and coordinate deliveries, but when patrons conflict, friendships shatter. (This is only a facade for the player, no simulation)

See IMPLEMENTATION-PLAN.md for complete resource competition implementation details.

## PROJECT: WAYFARER - Letters and Ledgers

**WAYFARER - Letters and Ledgers** is a medieval letter-carrier RPG built as a Blazor Server application (.NET 8.0). Inspired by Kingkiller Chronicles' impossible social obligations and 80 Days' journey mechanics, players manage a priority queue of letters while navigating deadline pressure, relationship management, and mysterious patron demands. The game features a deterministic queue system with connection token economy creating constant priority dilemmas.

## CORE GAME DESIGN PRINCIPLES

### The Triumvirate Core: Letter Queue, Connection Tokens, and Standing Obligations

The entire game emerges from three interconnected systems that form the core loop:

1. **Letter Queue** - Your 8-slot priority queue of visible obligations
2. **Connection Tokens** - Relationships as spendable currency for favors and unlocks
3. **Standing Obligations** - Permanent character modifications from your choices

**See Core Design Philosophy section in `IMPLEMENTATION-PLAN.md`** for how these three systems interconnect.

### The Letter Queue: Your Life in 8 Slots

The game centers around a **priority queue of 8 letters** that represents your social obligations and promises. This queue creates the entire strategic framework:

1. **Queue Order is Sacred** - You must deliver letters in order (1→2→3...) or burn relationships
2. **Deadlines Create Pressure** - Each letter has a deadline creating impossible optimization puzzles
3. **Token Thresholds** - More tokens with NPCs unlock better letter categories
4. **Every Acceptance Matters** - New letters enter based on sender leverage
5. **Standing Obligations Reshape Play** - Permanent modifiers that alter queue behavior forever

**CRITICAL: The Queue IS the Story** - The queue isn't just a task list, it's a visual representation of all your promises and commitments. Every position matters, every deadline conflicts, every choice cascades into new problems.

**CRITICAL: Leverage-Based Queue Entry** - Letters enter based on sender's social leverage:
- **Base Positions**: Patron (1), Noble (3), Trade/Shadow (5), Common/Trust (7)
- **Token Debt Modifies Position**: Negative tokens = higher leverage = earlier position
- **Queue Displacement**: High-leverage letters force entry, pushing others down
- **Overflow Discard**: Letters pushed past position 8 are discarded automatically
- **Leverage Inversion**: Debt can make commoners have noble-level priority

**See `LEVERAGE-SYSTEM-IMPLEMENTATION.md` for complete technical details and `USER-STORIES.md` for acceptance criteria.**

This creates a visual metaphor: the queue shows who has power over your time through the physics of social obligation.

### Connection Token Economy

The game uses **5 types of connection tokens** as universal currency and reputation:

1. **Trust (Green)** - Personal bonds, friendships, romance
2. **Trade (Blue)** - Merchant relationships, commercial reputation  
3. **Noble (Purple)** - Aristocratic connections, court standing
4. **Common (Brown)** - Everyday folk, local knowledge
5. **Shadow (Black)** - Underworld contacts, forbidden knowledge

Tokens serve multiple purposes:
- **Currency**: Spend on queue manipulation, route unlocking, special actions
- **Letter Categories**: Token thresholds unlock better paying letter types
- **Relationships**: Per-NPC tracking shows individual bonds
- **Leverage Through Debt**: Negative tokens create power dynamics in the queue

**CRITICAL: Token Debt Creates Leverage** - Going into token debt fundamentally changes power dynamics:
- **Positive Tokens (1-3)**: Normal relationship, standard queue positions
- **High Tokens (4+)**: Mutual respect, letters enter 1 position later
- **Zero Tokens**: Neutral relationship, no modifications
- **Negative Tokens**: Each -1 token moves letters 1 position earlier (more leverage)
- **Deep Debt (-3+)**: Extreme leverage, can push letters to top positions

Example: A merchant you owe -2 Trade tokens has their letters enter at position 3 instead of 5, reflecting how debt gives them power over your priorities.

**CRITICAL: Contextual Token Spending Principle** - ALL token spending must be contextually tied to specific NPCs:
- **Letter Queue Manipulation**: Spend specific token types from the letter SENDER's relationship
- **Route Unlocking**: Tokens spent from the NPC who controls/knows that route
- **Special Actions**: Tokens spent from the NPC providing the service or access
- **Standing Obligations**: Tokens lost from the specific NPC relationships violated

This creates narrative coherence: you're never abstractly spending "Trade tokens" - you're specifically burning your relationship with Marcus the Merchant or Elena the Scribe. Every token transaction damages or leverages a specific relationship.

**CRITICAL: No Abstract Token Generation** - NEVER generate tokens with abstract entities:
- ❌ **FORBIDDEN**: "+1 token with tavern patrons" - "tavern patrons" is not an NPC
- ❌ **FORBIDDEN**: "+1 token with local merchants" - must be specific merchant
- ❌ **FORBIDDEN**: "+1 token with the community" - no abstract collectives
- ✅ **REQUIRED**: "+1 token with Marcus" - specific NPC relationship
- ✅ **REQUIRED**: "Choose NPC present to share drinks with" - player selects real NPC
- ✅ **REQUIRED**: All token changes MUST reference actual NPC game objects

**CRITICAL: NPC Token Type Limitations** - NPCs only offer letters in their character-appropriate categories:
- **Most NPCs have 1-2 token types** that fit their profession and personality
- **Elena (Scribe)**: Only offers Trust letters (personal correspondence)
- **Marcus (Merchant)**: Only offers Trade letters (commercial deliveries)
- **River Worker**: Offers both Trade (legitimate shipping) AND Shadow (smuggling)
- **NO NPC offers all 5 types** - this forces players to build diverse relationships

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
- Token thresholds with NPCs unlock better letter categories
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
5. **Token thresholds reward specialization** - More tokens unlock better letter categories

### Core Design Philosophy: Token-Based Crisis Management

The game follows a **connection token economy** principle where relationships become spendable resources:

1. **Five token types map to social spheres** - Trust/Trade/Noble/Common/Shadow
2. **Tokens enable queue manipulation** - Spend social capital to solve priority crises  
3. **Token costs create meaningful trade-offs** - Burning relationships for immediate needs
4. **Better letters emerge from relationships** - Token thresholds unlock higher paying categories
5. **Every token spent weakens relationships** - Crisis management has permanent cost

### Architectural Patterns

The game follows strict architectural patterns for maintainability and testability. 

**See GAME-ARCHITECTURE.md for:**
- Repository-Mediated Access patterns
- UI Access patterns (queries vs actions)
- Stateless Repository requirements
- Dependency injection patterns
- Testing architecture
- Complete technical implementation details

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

The game follows a clean architecture with clear separation of concerns:
- **GameState/** - Core game logic and managers
- **Content/** - JSON loading and repositories
- **Pages/** - Blazor UI components
- **wwwroot/** - Static assets and CSS

For detailed project structure, see the solution explorer or project documentation.


### DESIGN DECISION PRINCIPLE (Critical)
**Every ambiguous design choice introduces technical debt. Make clear decisions immediately.**

When implementing features, NEVER leave design decisions unmade:
- ❌ **FORBIDDEN**: "Use profession OR token type" - pick ONE
- ❌ **FORBIDDEN**: "Could be X or Y" - decide which
- ❌ **FORBIDDEN**: Adding fields "just in case" - YAGNI
- ✅ **REQUIRED**: Make a clear choice and document why
- ✅ **REQUIRED**: If genuinely unsure, ask user for design decision
- ✅ **REQUIRED**: Document decisions in code and architecture docs

**Example**: Letter generation should use token type matching, NOT profession matching, because NPCs already define their letterTokenTypes and this creates a single source of truth.

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
4. **Thresholds Reward Specialization**: Token accumulation unlocks better letters, encouraging relationship focus.
5. **Standing Obligations Reshape Play**: Permanent modifiers create unique playthroughs based on choices.
6. **Debt Creates Leverage**: Token debt inverts power dynamics - those you owe control your priorities.
7. **All Mechanics Visible**: Queue position, deadlines, token costs, and leverage effects must be clear in UI.

### Categorical Interconnection Requirements

**All entities must have unique types/categories** that interact with the queue and token systems:
- NPCs: `TokenType` (Trust/Trade/Noble/Common/Shadow), `LetterSender` (true/false), `DeliveryRecipient` (true/false)
- Letters: `Size` (Small/Medium/Large), `TokenType` (matches sender), `Deadline` (days), `QueuePosition` (1-8)
- Locations: `AccessLevel` (Public/Private/Restricted), `TokenRequirement` (which tokens needed for access)
- Equipment: `EnablesAccess` (Noble/Guild/Shadow areas), `EnablesRoutes` (mountain/forest paths)

**Game rules emerge from queue position and token interactions**:
- Queue Position + Deadline → Delivery pressure and crisis decisions
- Token Accumulation + Letter Type → Better letter category unlocks
- Token Spending + Queue Position → Crisis management options
- Standing Obligations + Letter Type → Permanent queue behavior changes

**Core Design Rules:**
- **NEVER** allow free queue reordering - always require token cost or relationship burn
- **ALWAYS** make deadlines create real pressure through queue position conflicts  
- **ENSURE** token costs are meaningful - spending tokens burns actual relationships
- **VISIBLE** queue state - players must see position, deadlines, and token costs clearly

*See GAME-ARCHITECTURE.md for detailed categorical system implementations.*

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
- **NEVER use Compile Remove in .csproj files** - This hides compilation errors and mistakes. Fix the code, rename files, or delete them entirely. Using Remove patterns in project files masks problems instead of solving them.
- **ALWAYS read files FULLY before making changes** - Never make assumptions about file contents. Read the entire file to understand context and avoid mistakes.
- **RENAME instead of DELETE/RECREATE** - When refactoring systems (e.g., Encounter → Conversation), rename files and classes to preserve git history and ensure complete transformation.
- **COMPLETE refactorings IMMEDIATELY** - Never leave systems half-renamed. If you start renaming Encounter to Conversation, finish ALL references before moving to other tasks.

### UI STATE MANAGEMENT PRINCIPLE (CRITICAL)

**ALL UI components must follow the architecture's state management pattern:**

1. **GameWorld is the ONLY source of truth** - All game state lives in GameWorld
2. **MainGameplayView polls GameWorld** - The PollGameState() method is the ONLY allowed polling mechanism
3. **NO separate state queries** - UI components CANNOT query repositories or managers directly
4. **NO separate timers** - Components CANNOT have their own Timer or polling loops
5. **State passed as Parameters** - Parent components pass state down via Parameters

**Example:**
```csharp
// ❌ WRONG: Component polls for its own state
@inject MessageSystem MessageSystem
@code {
    Timer _timer = new Timer(_ => CheckForMessages(), null, 0, 500);
}

// ✅ CORRECT: Component receives state as parameter
@code {
    [Parameter] public List<SystemMessage> Messages { get; set; }
}
```

**The MainGameplayView polling loop:**
```csharp
protected override async Task OnInitializedAsync()
{
    _ = Task.Run(async () =>
    {
        while (true)
        {
            await InvokeAsync(() =>
            {
                PollGameState();  // This pulls ALL state from GameWorld
                StateHasChanged();
            });
            await Task.Delay(50);
        }
    });
}
```

This ensures:
- Single source of truth
- Predictable state updates
- No race conditions
- Testable components
- Performance optimization

**See GAME-ARCHITECTURE.md for critical technical principles:**
- NO CIRCULAR DEPENDENCIES
- NEVER USE REFLECTION  
- LEGACY CODE ELIMINATION
- CONTENT/LOGIC SEPARATION
- PROPER DEPENDENCY INJECTION
- And all other architectural patterns and code quality requirements

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

**See UI-DESIGN-IMPLEMENTATION-PRINCIPLES.md for:**
- UI Design Principles (Game vs App UX)
- CSS Architecture Principles
- Frontend Performance Principles
- Complete UI implementation patterns


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

**Example**: When a letter expires, the player loses 2 tokens with that NPC. That's it. No violation counter, no reputation hit, no permanent record. The lost tokens ARE the consequence, and they naturally affect which letters that NPC will offer.

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

**🎯 IMMEDIATE NEXT STEPS**: Implement **Leverage System** (see `LEVERAGE-SYSTEM-IMPLEMENTATION.md`)
**Current Status**: Token debt supported, need leverage-based queue positioning
**Critical Path**: Leverage System → Patron Requests → Emergency Actions → Full Vision

### Important Documentation

**CRITICAL TRANSFORMATION DOCUMENTS** (Read these for complete understanding):
- 🎯 **`LETTER-QUEUE-TRANSFORMATION-ANALYSIS.md`** - Comprehensive analysis of the letter queue transformation
- 📬 **`LETTER-QUEUE-UI-SPECIFICATION.md`** - Complete UI architecture for the 3-screen letter queue system
- 🔄 **`LETTER-QUEUE-INTEGRATION-PLAN.md`** - How existing systems transform to serve the queue (includes roadmap and todo list)

**CORE DESIGN DOCUMENTS**:
- ⚡ **`LEVERAGE-SYSTEM-IMPLEMENTATION.md`** - Technical guide for leverage through token debt
- 📋 **`USER-STORIES.md`** - Complete acceptance criteria for all game features
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