* CLAUDE.md

**⚠️ MANDATORY: READ THE ENTIRE CLAUDE.MD FILE FULLY ⚠️**

**📋 IMPLEMENTATION LOCKED: Follow IMPLEMENTATION-PLAN.md EXACTLY**
- The game design is FINAL - see IMPLEMENTATION-PLAN.md
- No feature additions or scope changes allowed
- Build exactly what's specified, nothing more
- All debates must reference the implementation plan

**CRITICAL DIRECTIVE: Before implementing ANY change to Wayfarer, you MUST debate all agents with the proposed change.**
This includes:
- New mechanics (ONLY if fixing bugs in IMPLEMENTATION-PLAN.md)
- Modified systems (ONLY if specified in IMPLEMENTATION-PLAN.md)
- UI changes (MUST match IMPLEMENTATION-PLAN.md specifications)
- Content structures (MUST follow IMPLEMENTATION-PLAN.md)
- Rule adjustments (NOT ALLOWED - plan is final)
- Feature additions (NOT ALLOWED - scope is locked)
- Feature removals (NOT ALLOWED - build complete plan)
No exceptions. Even "small" changes must be reviewed by all specialized personas.

**🚨 MANDATORY: ANALYZE BEFORE ANY CHANGE 🚨**
- You MUST ALWAYS proactively DEBATE WITH EVERY ONE of my specialized agents for EVERY CHANGE
- You MUST ALWAYS proactively think ahead and plan your steps BEFORE making ANY change
- You MUST analyze ALL related files and understand the complete system BEFORE modifying anything
- You MUST understand how components interact and depend on each other
- You MUST check for compilation errors BEFORE assuming a change will work
- NEVER make changes based on assumptions - ALWAYS verify first
- NEVER make partial changes without understanding the full impact
- If you haven't analyzed the codebase thoroughly, DO NOT MAKE THE CHANGE
- ALWAYS check if a file exist before creating it
- **ALWAYS understand the full context before writing ANY code**

**🚨 CRITICAL RULE: NEVER MARK ANYTHING AS COMPLETE WITHOUT TESTING 🚨**
- You MUST build and run tests before claiming completion
- You MUST verify the code works before saying "done" or "complete"
- NEVER assume code works - ALWAYS TEST
- If you haven't run `dotnet build` and the E2E test, IT'S NOT COMPLETE
- Saying something is "complete" without testing is UNACCEPTABLE

**⚠️ CRITICAL: ALWAYS READ ALL FILES FULLY BEFORE MODIFYING IT ⚠️**
**NEVER make changes to a file without reading it completely first. This is non-negotiable.**
**DOUBLE-CHECK core architectural components (navigation, routing, service registration) - analyze ALL related files and dependencies before making ANY changes to avoid breaking the application architecture.**

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

*** PRIME PRINCIPLES ***
- You are too agreeable by default. I want you objective. I want a partner. Not a sycophant.
- NEVER ASSUME. Check the documentation and codebase and ask the user for clarification
- **FOLLOW IMPLEMENTATION-PLAN.md** - This is the ONLY source of truth for what to build. No deviations.
- You have Gemini as a registered MCP tool. Everytime before you implement a solution, you must first fight Gemini and me about it until everyone is in agreement about the correct way to implement it.
- **NO SILENT BACKEND ACTIONS** - Nothing should happen silently in the backend. If automatic, the player MUST be notified via MessageSystem. If manual, the player MUST click a button to initiate. All game state changes must be visible and intentional.
- **NEVER CREATE DUPLICATE MARKDOWN FILES** - ALWAYS check for existing .md files in root directory first. Update existing documentation files instead of creating new ones. If IMPLEMENTATION-PLAN.md exists, UPDATE IT. If SESSION-HANDOFF.md exists, UPDATE IT. Creating duplicate files is unacceptable.
- **ALWAYS UPDATE GITHUB AFTER CHANGES** - After making significant changes or completing tasks, ALWAYS update the GitHub issues and kanban board to reflect current progress. Use `gh issue comment` to add progress updates and `gh project` commands to update the kanban board status.

** CODE WRITING PRINCIPLES **

*** Error Handling Philosophy ***
- **Let exceptions bubble up** naturally for better error visibility
- NEVER THROW EXCEPTIONS
- NEVER USE TRY CATCH
- Prefer clear failures over hidden bugs

** Code Style Guidelines **

*** Code Style ***
- Self-descriptive code over excessive comments
- Comments for intent, not implementation

*** Anti-Defensive Programming Philosophy ***
- **Fail Fast**: Let exceptions bubble up naturally for clear debugging information
- **Minimal Try-Catch**: Only use try-catch when absolutely necessary for error recovery
- **No Excessive Null Checks**: Avoid defensive programming for things that should never be null
- **Assumption Validation**: It's fine to assume correctness for things that would fail during initialization and be caught in basic smoke testing

**Why**: Defensive programming hides bugs instead of revealing them. Clear exceptions with full stack traces are more valuable than swallowed errors.

**TRANSFORMATION APPROACH**:
- **RENAME AND RECONTEXTUALIZE** - Don't wrap new functionality in old classes, rename them to reflect new purpose
- **DELETE LEGACY CODE ENTIRELY** - Remove old contract system, reputation system, favor system completely
- **NO COMPATIBILITY LAYERS** - Clean break from old mechanics to new queue/token system
- **NO FEATURE CREEP** - If it's not in IMPLEMENTATION-PLAN.md, don't build it
- **DELETE LEGACY CODE ENTIRELY** - Remove anything not in the implementation plan
- **FRESH TEST SUITE** - Test only what's in IMPLEMENTATION-PLAN.md

**GENERAL PRINCIPLES**:
- **GAMEWORLD HAS NO DEPENDENCIES (CRITICAL)** - GameWorld is the single source of truth and must have NO dependencies on any services, managers, or external components. All dependencies flow INWARD towards GameWorld, never outward from it. GameWorld does NOT create any managers or services.
- **GAMEWORLD INITIALIZATION (CRITICAL)** - GameWorld MUST be created through a static GameWorldInitializer during startup. ServiceConfiguration MUST NOT use GetRequiredService or any DI service locator pattern to create GameWorld - this violates clean architecture and causes circular dependencies during prerendering. GameWorldInitializer must be a static class that can create GameWorld without needing dependency injection. This ensures clean initialization during startup without breaking ServerPrerendered mode or causing request hangs.
- **NAVIGATION HANDLER ARCHITECTURE (CRITICAL)** - GameUIBase (the root component at @page "/") is the ONLY navigation handler in the application. MainGameplayView is a regular component rendered by GameUIBase, NOT a navigation handler. NavigationService accepts registration via RegisterNavigationHandler() method. This pattern avoids circular dependencies while maintaining clean architecture. NO other components should implement INavigationHandler. This architectural decision prevents navigation conflicts and maintains a single point of control for all navigation operations.
- **SINGLE SOURCE OF TRUTH FOR STATE** - Never duplicate state tracking across multiple objects. When you find duplicate state (e.g., location tracked in both Player and WorldState), identify which is used more frequently and make that the single source of truth. Other objects should delegate to it, not maintain their own copies.
- **never rename classes that already exist unless specifically ordered to (i.e. ConversationManager -> DeterministicConversationManager). Before creating classes, always check if classes with similar or overlapping functionality already exist**
- **NEVER use class inheritance/extensions** - Add helper methods to existing classes instead of creating subclasses
- **UNDERSTAND BEFORE REMOVING** - Always understand the purpose of code before removing it. Determine if it's safe to remove or needs refactoring. Never assume code is redundant without understanding its context and dependencies.
- **READ ALL RELEVANT FILES BEFORE MODIFYING** - NEVER modify code without first reading ALL related files (models, repositories, managers, UI components). You MUST understand the complete data flow, types, and dependencies before making any changes. This prevents type mismatches and broken implementations.
- Do not leave comments in code that are not TODOs or SERIOUSLY IMPORTANT
- After each change, run the tests to check for broken functionality. Never commit while tests are failing
- **ALWAYS write unit tests confirming errors before fixing them** - This ensures the bug is properly understood and the fix is validated
- You must run all tests and execute the game and do quick smoke tests before every commit
- **E2E TEST** - Run E2E test before any changes to catch ALL startup and runtime issues. The test is in `/mnt/c/git/Wayfarer.E2ETests/`. Run with: `cd /mnt/c/git/Wayfarer.E2ETests && dotnet run`. This single test validates GameWorld creation, web server startup, and all critical services. If this test passes, the game will start without errors.
- **Never keep legacy code for compatibility**
- **NEVER use suffixes like "New", "Revised", "V2", etc.** - Replace old implementations completely and use the correct final name immediately. Delete old code, don't leave it behind.
- **NEVER use Compile Remove in .csproj files** - This hides compilation errors and mistakes. Fix the code, rename files, or delete them entirely. Using Remove patterns in project files masks problems instead of solving them.
- **ALWAYS read files FULLY before making changes** - Never make assumptions about file contents. Read the entire file to understand context and avoid mistakes.
- **RENAME instead of DELETE/RECREATE** - When refactoring systems (e.g., Encounter → Conversation), rename files and classes to preserve git history and ensure complete transformation.
- **COMPLETE refactorings IMMEDIATELY** - Never leave systems half-renamed. If you start renaming Encounter to Conversation, finish ALL references before moving to other tasks.

*** GAME DESIGN PRINCIPLE: NO SPECIAL RULES (CRITICAL)

**"Special rules" are a design smell. When tempted to add special behavior, create new categorical mechanics instead.**

**Key Principle**: The game should have very few special rules. When you find yourself wanting to add special cases or unique behaviors, this indicates a need to enrich the existing systems rather than adding exceptions.

**Examples of the Wrong Approach:**
- ❌ "Patron letters always go to position 1" - Special rule
- ❌ "Noble letters can't be refused" - Special exception
- ❌ "Shadow NPCs offer illegal work when player is desperate" - Conditional special case

**The Right Approach - Categorical Mechanics:**
- ✅ Create a "Leverage" system where debt affects ALL letter positions
- ✅ Add "Obligation" mechanics that modify ALL queue behaviors
- ✅ Use "Desperation" as a player state that affects ALL NPC interactions

**Design Process:**
1. **Identify the special case** - "I want patron letters to be high priority"
2. **Ask why** - "Because the patron has power over the player"
3. **Generalize the concept** - "Power dynamics affect letter priority"
4. **Create categorical system** - "Leverage system: debt creates priority for ALL NPCs"
5. **Special case becomes regular case** - "Patron starts with high leverage due to employment debt"

**Benefits:**
- Emergent gameplay from system interactions
- Players discover strategies rather than memorizing exceptions
- Code remains clean and general
- New content automatically inherits system behaviors

**📝 JSON VALIDATION BEST PRACTICES**
- **Always use case-insensitive property matching** - JSON files use camelCase while C# DTOs use PascalCase
- **Inherit from BaseValidator** - Provides TryGetPropertyCaseInsensitive helper method for robust validation
- **Test validators with actual JSON** - Don't assume field names match between JSON and DTOs
- 
**📝 CONTENT REQUIREMENTS (from IMPLEMENTATION-PLAN.md)**
- **5 NPCs** with schedules, tokens, conversation templates
- **5 Locations** with travel times and available actions
- **9 Letter Templates** (3 types × 3 stakes)
- **45 Conversation Combinations** (5 NPCs × 3 verbs × 3 states)
- **No procedural generation** - All content is deterministic
- See IMPLEMENTATION-PLAN.md Section 'Content Requirements' for exact specifications

**📘 FINAL GAME DESIGN: See IMPLEMENTATION-PLAN.md**


## Key Design Insights from Initial Game Design Conversation

**Core Game Concept:**
- Medieval letter carrier simulation exploring social obligations
- Focuses on managing impossible delivery deadlines
- Emphasizes relationships over traditional RPG progression
- No magic, no world-saving narrative
- Survival through navigating social networks

**Design Philosophy Highlights:**
- Elegance over complexity
- Strong verisimilitude throughout
- Systems that reinforce social obligation fantasy
- No arbitrary mechanics
- Information and relationships as primary gameplay mechanics

**Core Mechanical Innovations:**
- Multi-context token system (Trust, Commerce, Status, Shadow)
- Isolated relationship tracking per context
- Time as universal pressure
- Tiered access system (routes, actions, information)
- Information discovery as core progression mechanic

**Design Principles:**
- No special rules or exceptions
- Emergent gameplay through system interactions
- Every system touches every other system
- Player choices create narrative through systemic pressures

**Tone and Scope:**
- Ordinary medieval life simulation
- Focus on daily survival and social navigation
- Finding human connection amid complex obligations
- Inspired by slice-of-life narratives in challenging environments
- 
## What We're Building (Locked Scope)

**Core Game Loop:**
- Wake up with more letters than you can deliver
- Only deliver from queue position 1
- Travel costs time, NPCs have schedules
- Missing deadlines has permanent consequences
- Manage queue through conversations
- Face mounting pressure each day

**Exact Systems (No Additions):**
- **3 Verbs**: HELP, NEGOTIATE, INVESTIGATE
- **3 Tokens**: Trust, Commerce, Status (no Shadow)
- **8-Slot Queue**: Weight system, position-based delivery
- **16 Active Hours**: 6 AM to 10 PM gameplay
- **5 NPCs**: Each with schedules and emotional states
- **5 Locations**: With specific travel times

**What Creates Tension:**
- More letters than time (core pressure)
- Queue position restrictions
- NPC availability windows
- Cascading consequences for failures
- Token debt creating obligations

**UI Requirements (Exact):**
- Queue always visible with weight/deadline indicators
- Max 5 conversation choices
- Attention points (3 per conversation)
- Clear consequence previews
- Time and deadline displays

**We Are NOT Building:**
- AI narrative generation
- Procedural content
- Complex branching stories
- Magic or fantasy elements
- RPG progression systems
- Anything not in IMPLEMENTATION-PLAN.md

**Principles and Memories:**
- ALWAYS read the full file before editing
- ALWAYS check IMPLEMENTATION-PLAN.md before making any decision
- NEVER add features not in the implementation plan
- NEVER change core mechanics from the plan
- Build EXACTLY what's specified, test it, ship it