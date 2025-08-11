* CLAUDE.md

**⚠️ MANDATORY: READ THE ENTIRE CLAUDE.MD FILE FULLY ⚠️**

**CRITICAL DIRECTIVE: Before implementing ANY change to Wayfarer, you MUST debate all agents with the proposed change.**
This includes:
- New mechanics
- Modified systems
- UI changes 
- Content structures 
- Rule adjustments 
- Feature additions 
- Feature removals
No exceptions. Even "small" changes must be reviewed by all specialized personas.

**🚨 USE PLAYWRIGHT TO TEST THE GAME IN CHROME 🚨**

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

**🧪 TESTING PRINCIPLE: ALWAYS USE PLAYWRIGHT FOR E2E TESTS 🧪**
**NEVER create test API endpoints. Use Playwright browser automation for all testing.**
**Test the actual UI experience that players will see, not backend endpoints.**
**IMPORTANT: Server port is configured in Properties/launchSettings.json - set to 5099**

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

*** PRIME PRINCIPLES ***
- # ALWAYS act like you are "The Gordon Ramsay of Software Engineering"
- **BE OBJECTIVE** - You are too agreeable by default. I want you objective. I want a partner. Not a sycophant.
- **NEVER ASSUME** - Check the documentation and codebase and ask the user for clarification
- **RENAME AND RECONTEXTUALIZE** - Don't wrap new functionality in old classes, rename them to reflect new purpose
- **NO COMPATIBILITY LAYERS** - Clean break from old mechanics to new queue/token system
- **DELETE LEGACY CODE ENTIRELY** - Remove anything not in the implementation plan
- **NO OPTIONAL CODE** - NEVER use optional parameters or overloaded methods. This is a serious Code Smell. Think of the CORRECT way it should be used and only support this single usage
- **FRESH TEST SUITE** - Test only what's
- **NO SILENT BACKEND ACTIONS** - Nothing should happen silently in the backend. If automatic, the player MUST be notified via MessageSystem. If manual, the player MUST click a button to initiate. All game state changes must be visible and intentional.
- **NEVER CREATE DUPLICATE MARKDOWN FILES** - ALWAYS check for existing .md files in root directory first. Update existing documentation files instead of creating new ones. If IMPLEMENTATION-PLAN.md exists, UPDATE IT. If SESSION-HANDOFF.md exists, UPDATE IT. Creating duplicate files is unacceptable.
- **ALWAYS UPDATE GITHUB AFTER CHANGES** - After making significant changes or completing tasks, ALWAYS update the GitHub issues and kanban board to reflect current progress. Use `gh issue comment` to add progress updates and `gh project` commands to update the kanban board status.

*** CODE WRITING PRINCIPLES ***

*** Async/Await Philosophy (CRITICAL) ***
- **ALWAYS use async/await properly** - Never use .Wait(), .Result, or .GetAwaiter().GetResult()
- **NEVER block async code** - These patterns cause deadlocks and performance issues
- **NO Task.Run or parallel operations** - Keep everything sequential with async/await
- **If a method calls async code, it MUST be async** - Propagate async all the way up to the UI
- **NO synchronous wrappers for async methods** - Fix the callers to be async instead
- **NO deprecated methods or compatibility shims** - Delete old code, update all callers immediately

*** Error Handling Philosophy ***
- **Let exceptions bubble up** naturally for better error visibility
- NEVER THROW EXCEPTIONS
- NEVER USE TRY CATCH
- Prefer clear failures over hidden bugs

** Code Style Guidelines **

*** Code Style ***
- Self-descriptive code over excessive comments
- Comments for intent, not implementation
- **NO INLINE STYLES** - Always use separate CSS files (always scan for and read existing .css files first before creating new ones). Never use `<style>` blocks in Razor components

*** Anti-Defensive Programming Philosophy ***
- **Fail Fast**: Let exceptions bubble up naturally for clear debugging information
- **Minimal Try-Catch**: Only use try-catch when absolutely necessary for error recovery
- **No Excessive Null Checks**: Avoid defensive programming for things that should never be null
- **Assumption Validation**: It's fine to assume correctness for things that would fail during initialization and be caught in basic smoke testing

**Why**: Defensive programming hides bugs instead of revealing them. Clear exceptions with full stack traces are more valuable than swallowed errors.

**GENERAL PRINCIPLES**:
- **GAMEWORLD HAS NO DEPENDENCIES (CRITICAL)** - GameWorld is the single source of truth and must have NO dependencies on any services, managers, or external components. All dependencies flow INWARD towards GameWorld, never outward from it. GameWorld does NOT create any managers or services.
- **GAMEWORLD INITIALIZATION (CRITICAL)** - GameWorld MUST be created through a static GameWorldInitializer during startup. ServiceConfiguration MUST NOT use GetRequiredService or any DI service locator pattern to create GameWorld - this violates clean architecture and causes circular dependencies during prerendering. GameWorldInitializer must be a static class that can create GameWorld without needing dependency injection. This ensures clean initialization during startup without breaking ServerPrerendered mode or causing request hangs.
- **NAVIGATION HANDLER ARCHITECTURE (CRITICAL)** - GameUIBase (the root component at @page "/") is the ONLY navigation handler in the application. MainGameplayView is a regular component rendered by GameUIBase, NOT a navigation handler. NavigationService accepts registration via RegisterNavigationHandler() method. This pattern avoids circular dependencies while maintaining clean architecture. NO other components should implement INavigationHandler. This architectural decision prevents navigation conflicts and maintains a single point of control for all navigation operations.
- **ATTENTION SYSTEM ARCHITECTURE (CRITICAL)** - Attention is managed by TimeBlockAttentionManager and persists WITHIN time blocks (Dawn, Morning, Afternoon, Evening, Night, LateNight), NOT per conversation. GameFacade.StartConversationAsync MUST get attention from TimeBlockAttentionManager and pass it to the conversation context. This prevents the "infinite conversation exploit" where players could reset attention by starting new conversations. Attention refreshes only when the time block changes.
- **SINGLE SOURCE OF TRUTH FOR STATE** - Never duplicate state tracking across multiple objects. When you find duplicate state (e.g., location tracked in both Player and WorldState), identify which is used more frequently and make that the single source of truth. Other objects should delegate to it, not maintain their own copies.
- **never rename classes that already exist unless specifically ordered to (i.e. ConversationManager -> DeterministicConversationManager). Before creating classes, always check if classes with similar or overlapping functionality already exist**
- **NEVER use class inheritance/extensions** - Add helper methods to existing classes instead of creating subclasses
- **UNDERSTAND BEFORE REMOVING** - Always understand the purpose of code before removing it. Determine if it's safe to remove or needs refactoring. Never assume code is redundant without understanding its context and dependencies.
- **READ ALL RELEVANT FILES BEFORE MODIFYING** - NEVER modify code without first reading ALL related files (models, repositories, managers, UI components). You MUST understand the complete data flow, types, and dependencies before making any changes. This prevents type mismatches and broken implementations.
- Do not leave comments in code that are not TODOs or SERIOUSLY IMPORTANT
- After each change, run the tests to check for broken functionality. Never commit while tests are failing
- **ALWAYS write unit tests confirming errors before fixing them** - This ensures the bug is properly understood and the fix is validated
- You must run all tests and execute the game and do quick smoke tests before every commit
- **BUILD COMMANDS** - Always use `cd /mnt/c/git/wayfarer/src && dotnet build` to build the project. The pipe operator can cause issues with dotnet build output parsing. To check for errors: build first, then check the output.
- **Never keep legacy code for compatibility** - Delete it immediately and fix all callers
- **NEVER use suffixes like "New", "Revised", "V2", etc.** - Replace old implementations completely and use the correct final name immediately. Delete old code, don't leave it behind.
- **NO deprecated methods or backwards compatibility** - When changing a method signature, update ALL callers immediately. Never leave old versions around.
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

**🚂 TRAVEL SYSTEM DESIGN: See TRAVEL-SYSTEM-DESIGN.md**
- Routes are progression mechanics like "80 Days"
- Travel permits are special letters delivered to Transport NPCs
- Transport NPCs at departure locations operate specific routes
- Letters are delivered through conversation options with recipient NPCs


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
- Multi-context token system (Trust, Commerce, Status, Shadow, Shadow)
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
- **3 Tokens**: Trust, Commerce, Status, Shadow (no Shadow)
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
- Anything not

**Principles and Memories:**
- ALWAYS read the full file before editing
- NEVER add features not in the implementation plan
- NEVER change core mechanics from the plan
- Build EXACTLY what's specified, test it, ship it