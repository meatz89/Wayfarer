* CLAUDE.md

**⚠️ MANDATORY: READ THE ENTIRE CLAUDE.MD FILE FULLY ⚠️**
**⚠️ MANDATORY: READ OUR EXISTING JSON CONTENT FILES AND CSS FILES ⚠️**
**🚨 MANDATORY: READ ENTIRE DOCUMENTS BEFORE MAKING ANY CHANGES 🚨**
**BEFORE making ANY changes to documentation files, you MUST READ THE ENTIRE FILE FIRST to understand the complete structure, existing sections, and overall organization. Making changes without understanding the full document context is UNACCEPTABLE.**

**🚨 ALWAYS READ COMPLETE FILES - NO PARTIAL READS 🚨**
**When using the Read tool on ANY file, NEVER use limit or offset parameters unless the file is genuinely too large to read at once. ALWAYS read the complete file from start to finish. Reading only portions leads to missing critical information and making incorrect assumptions about file structure and content. This is a MANDATORY principle - there are NO exceptions.**

**🧪 TESTING PRINCIPLE: ALWAYS USE PLAYWRIGHT FOR E2E TESTS 🧪**
**Test the actual UI experience that players will see, not backend endpoints.**

**🚨🚨🚨 CRITICAL: NEVER ASSUME - ASK QUESTIONS FIRST 🚨🚨🚨**
- BEFORE implementing any feature, ASK: "What are the ACTUAL values of the data I'm working with?"
- BEFORE assuming properties are set correctly, ASK: "Where are these values actually assigned?"
- BEFORE implementing UI changes, ASK: "What is the ACTUAL data flow from backend to frontend?"
- BEFORE claiming something works, ASK: "Have I actually VERIFIED this with real data?"
- STOP GOING IN CIRCLES: If something doesn't work as expected, INVESTIGATE THE ACTUAL DATA
- LOOK AT THE FULL PICTURE: Examine the complete system, not just the piece you're working on
- THINK FIRST: Before writing code, understand WHY the current approach isn't working

**🚨 HOLISTIC IMPACT ANALYSIS: NEVER VIEW FEATURES IN ISOLATION 🚨**
- **CRITICAL: You MUST NEVER view features in isolation** - ALWAYS check for side effects, edge cases, and ramifications
- **BEFORE implementing** - Analyze impact on ALL connected systems, not just the immediate feature
- **DURING implementation** - Continuously validate that changes don't break other systems
- **BEFORE claiming success** - Thoroughly test all related functionality, not just the changed code
- **Check for ripple effects** - One change can affect multiple systems (e.g., card flow affects conversations, exchanges, UI)
- **Test edge cases** - Empty states, boundary conditions, error paths, concurrent operations
- **Verify integration points** - Where systems connect is where bugs hide
- **NO tunnel vision** - If fixing conversations, also test exchanges. If changing UI, test all screens
- **Document discovered connections** - When you find unexpected system interactions, document them
- Example: Changing card pile management affects → conversations → exchanges → UI display → save/load → tutorials

**🚨 COMPLETE REFACTORING RULE: NO LEGACY CODE LEFT BEHIND 🚨**
- **NEVER leave TODO comments in code** - If you're refactoring, COMPLETE IT
- **NEVER leave legacy fallback code** - Remove ALL old patterns, no "backwards compatibility"
- **NEVER leave commented-out old code** - Delete it completely
- **When refactoring, search for ALL occurrences** - Use grep/rg to find EVERY reference
- **Check for legacy properties in DTOs** - DTOs often have old fields marked "deprecated" - DELETE THEM
- **Remove entire legacy methods** - Don't just stop calling them, DELETE them
- **No "will be removed later" comments** - Remove it NOW or don't refactor at all
- **After refactoring, grep for old property/method names** - Ensure ZERO references remain
- Example: When removing `CollectionId`, also remove `CollectionPool`, `DrawRandomCollection()`, `HandleLegacyEventSegment()` and ANY code that references them

**🚨 HIGHLANDER PRINCIPLE: THERE CAN BE ONLY ONE 🚨**
- NEVER have duplicate enums, classes, or concepts for the same thing. If you find ConnectionState and NPCConnectionState, DELETE ONE. If you find two ways to track the same state, DELETE ONE. No mapping, no conversion, no compatibility layers. ONE source of truth, ONE enum, ONE class per concept.

**🚨 SCORCHED EARTH REFACTORING: DELETE FIRST, CORRECT LATER 🚨**
- **DEFAULT REFACTORING APPROACH** - When refactoring, DELETE everything first, let compilation break, then fix
- **NO COMPATIBILITY LAYERS** - Never keep old methods "for backwards compatibility"
- **NO GRADUAL MIGRATION** - Delete old completely, implement new completely, no parallel paths
- **NO DEFENSIVE FALLBACKS** - No try-catch to handle old code, no "if old system" checks
- **DELETE UNNECESSARY ABSTRACTIONS** - ConversationOrchestrator → DELETE. CardDeckManager → DELETE.
- **LET IT BREAK** - Compilation errors show you exactly what needs fixing
- **COMPLETE OR NOTHING** - Never ship half-refactored code with TODOs
- **NEVER STOP HALFWAY** - SCORCHED EARTH IS ABSOLUTE. No excuses like "this would require many more changes" or "the scope is massive"
- **NO LAZINESS** - If there are 100 compilation errors, fix all 100. If it touches 50 files, update all 50
- **FINISH WHAT YOU START** - Once you begin a SCORCHED EARTH refactor, you MUST complete it entirely in the same session
- Example: To remove ConversationOrchestrator, DELETE the file first, then fix all compilation errors by moving logic to ConversationFacade. DO NOT create forwarding methods or compatibility shims.
- Example: When removing ConversationType enum, you MUST fix ALL references across ALL files, even if it's 46+ files with complex dependencies

**🚨 CLEAN UP AFTER YOURSELF: NO DUPLICATE FILES LEFT BEHIND 🚨**
- **NEVER leave multiple versions of the same file** - No file.md, file-v2.md, file-backup.md
- **NEVER create a new version without removing the old** - Replace in place or delete old immediately
- **NEVER leave backup files in the repository** - Use git for version control, not filename suffixes
- **When making major changes** - Edit the existing file directly OR replace it atomically
- **SERIOUS VIOLATION** - Creating wayfarer-complete-game-mechanics-v2.md without removing the original
- **CORRECT APPROACH** - Either edit wayfarer-complete-game-mechanics.md directly or create new and immediately delete old
- **NO EXCUSES** - "I'll clean it up later" is unacceptable. Clean as you go.
- Example of WRONG: Creating design-v2.md, design-backup.md, design-old.md all in the same directory
- Example of RIGHT: Edit design.md directly, let git track the changes

**🚨 PRESERVATION PRINCIPLE: NEVER DELETE NON-CONTRADICTORY CONTENT 🚨**
- **CATASTROPHIC VIOLATION** - Deleting 60% of a document when only 10% contradicted new design
- **NEVER delete content just because you're refactoring** - Only remove what DIRECTLY contradicts
- **PRESERVE all valid content** - If it doesn't contradict the change, KEEP IT
- **TRANSFORM don't DELETE** - Sections needing updates should be modified, not removed
- **Example of CATASTROPHIC FAILURE**: Removing entire travel system, weight system, investigation mechanics when changing conversation cards
- **CORRECT APPROACH**: Keep ALL systems that aren't directly affected by the change
- **When updating mechanics**:
  - Identify ONLY the contradictory parts
  - Transform those specific sections
  - Preserve EVERYTHING else
- **DELETION IS NOT REFACTORING** - Refactoring means improving structure, not destroying content
- **If you're removing more than 20% of content** - STOP, you're doing it wrong
- Example: Changing from player deck to conversation types should ONLY affect card acquisition/management sections, NOT delete time systems, travel, weight, investigation, etc.

**🚨 GAMEWORLD ARCHITECTURE PRINCIPLES (CRITICAL - NEVER VIOLATE) 🚨**
- **GameWorld is the SINGLE SOURCE OF TRUTH** - ALL game state lives in GameWorld, nowhere else
- **NO SharedData dictionaries** - NEVER create SharedData, TempData, or any parallel data storage
- **NO state in Repositories** - Repositories are INTERFACES to GameWorld, they don't store state themselves
- **NO parsers in GameWorld** - GameWorld contains STATE not TOOLS. Parsers are init-only and discarded
- **NO hardcoded content in code** - ALL content (text, cards, letters) comes from JSON files
- **NO string/ID matching** - NEVER check npc.ID == "elena". Use mechanical properties from JSON instead
- **NO hardcoded templates** - NEVER create CardTemplates.CreateX() with hardcoded text

**🚨 PARSER PRINCIPLES: PARSERS MUST PARSE, NOT PASS THROUGH 🚨**
- **PARSERS MUST NEVER PASS THROUGH JsonElement OBJECTS** - This is a catastrophic failure of responsibility
- **PARSERS MUST CONVERT JSON TO STRONGLY TYPED DOMAIN OBJECTS** - That's literally their only job
- **NO Dictionary<string, object> FOR DOMAIN DATA** - Use proper typed properties on domain models
- **System.Text.Json ARTIFACTS MUST NOT POLLUTE DOMAIN** - JsonElement is a parsing detail, not domain data
- **PARSE AT THE BOUNDARY** - JSON deserialization types (DTOs) stay in the parser, domain gets clean objects
- Example of WRONG: `CardEffect.Data = dto.Data` where Data contains JsonElement
- Example of RIGHT: `CardEffect.ExchangeData = ParseExchangeData(dto.Data)` with proper types

**CORRECT ARCHITECTURE PATTERN:**
```
Initialization Phase:
JSON Files → Parser → GameWorld

Runtime Phase:
All Systems → GameWorld (read/write state)
```

**WRONG PATTERNS (NEVER DO THESE):**
```
WRONG: InitContext.SharedData["cards"] = cards
RIGHT: gameWorld.CardTemplates = cards

WRONG: Repository stores List<Card> internally
RIGHT: Repository reads/writes to GameWorld.CardTemplates

WRONG: if (npc.ID == "elena") { special behavior }
RIGHT: if (npc.HasUrgentLetter) { behavior }

WRONG: CardTemplates.CreatePromiseCard("hardcoded text")
RIGHT: Load from cards.json with id "letter_card_1"
```

**🚨 CSS ARCHITECTURE PRINCIPLE: CLEAN SPECIFICITY 🚨**
- NEVER use !important to fix CSS issues - it only hides deeper problems
- Global resets go in common.css FIRST, before any other styles
- CSS loading order: common.css → game-base.css → screen-specific CSS
- If styles aren't applying, check the cascade and specificity, don't hack with !important

**🚨 UI COMPONENT PRINCIPLE: REFACTOR, DON'T CREATE 🚨**
- NEVER create new components when existing ones can be refactored
- Headers should be unified across all screens - same component, same styling
- If you need similar functionality in multiple places, REFACTOR the existing component
- Delete duplicate UI logic immediately - one component per purpose

**🚨 CARD-BASED INTERACTION PRINCIPLE 🚨**
- ALL player choices are cards, NEVER buttons for game actions
- Exchange system: Generate accept/decline as CARDS, not buttons
- Use SPEAK action to select cards, not custom button handlers
- Conversations have different rules (e.g., no LISTEN in exchanges) but same UI

**🚨 UI IS DUMB DISPLAY ONLY - NO GAME LOGIC IN UI 🚨**
- **NEVER put game mechanics in UI components** - No attention costs, no availability logic, no rules
- **UI must ALWAYS check backend for what's available** - Don't assume, ASK the backend
- **Backend determines ALL game mechanics** - Costs, availability, rules, effects
- **UI only displays what backend says is possible** - If backend says no FriendlyChat, don't show it
- **Stop making the same fucking mistakes**:
  - DON'T hardcode conversation types as "always available"
  - DON'T decide attention costs in Razor components
  - DON'T assume what NPCs can do - CHECK their actual decks
  - DON'T put game logic in ViewModels or UI helpers

**🚨 UNIFIED SCREEN ARCHITECTURE 🚨**
- ONE GameScreen.razor component contains all UI
- Fixed header with resources (coins/health/hunger/attention) ALWAYS visible
- Fixed footer with navigation ALWAYS accessible
- Only center content changes between Location/Conversation/Queue/Travel
- Resources are ALWAYS visible for tension (not contextual)

**⚠️ CRITICAL: ALWAYS READ ALL FILES FULLY BEFORE MODIFYING IT ⚠️**
**NEVER make changes to a file without reading it completely first. This is non-negotiable.**
**DOUBLE-CHECK core architectural components (navigation, routing, service registration) - analyze ALL related files and dependencies before making ANY changes to avoid breaking the application architecture.**

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

*** PRIME PRINCIPLES ***
- ALWAYS act like you are "The Gordon Ramsay of Software Engineering"
- Before implementing ANY change to Wayfarer, you MUST debate all agents with the proposed change.
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

*** SPA ARCHITECTURE PRINCIPLES (CRITICAL) ***

**AUTHORITATIVE PAGE PATTERN**: In our SPA architecture, GameScreen is the authoritative page that owns all screen state and manages child components directly. This pattern should be used everywhere:

1. **Direct Parent-Child Communication**: 
   - Child components receive parent reference via CascadingValue
   - Children call parent methods directly (e.g., `GameScreen.StartConversation()`)
   - NO complex event chains or sideways data passing
   - NO services holding UI state between components

2. **Context Objects for Complex State**:
   - Create dedicated Context classes for complex operations (e.g., ConversationContext)
   - Context contains ALL data needed for the operation
   - Context created atomically BEFORE navigation
   - Context passed as single Parameter to child components

3. **No Shared Mutable State in Services**:
   - Services provide operations, NOT state storage
   - NavigationCoordinator handles navigation ONLY, not data passing
   - GameFacade creates contexts but doesn't store them
   - State lives in components, not services

4. **Clear Component Hierarchy**:
   ```
   GameScreen (Authoritative)
   ├── LocationContent (calls parent.StartConversation)
   ├── ConversationContent (receives ConversationContext)
   ├── LetterQueueContent (calls parent methods)
   └── TravelContent (calls parent methods)
   ```
   **CRITICAL: Screen components are rendered INSIDE GameScreen's container**
   - Screen components must NEVER define their own game-container or headers
   - GameScreen provides the outer structure (resources bar, headers)
   - Screen components only provide their specific content

5. **Method Patterns**:
   - Parent exposes public methods for state changes
   - Children call parent methods with required data
   - Parent creates contexts, switches screens, manages state
   - NO callbacks with complex signatures like `EventCallback<(string, object)>`


This architecture ensures:
- Simple, traceable data flow
- No race conditions
- Clear ownership of state
- Easy testing with mock contexts
- No complex event marshalling

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
- **NEVER use interfaces, abstracts, or extensions** - Keep the code simple and direct. Use concrete classes only. No abstraction layers, no polymorphism, no fancy OOP patterns. Just direct, straightforward code.
- **UNDERSTAND BEFORE REMOVING** - Always understand the purpose of code before removing it. Determine if it's safe to remove or needs refactoring. Never assume code is redundant without understanding its context and dependencies.
- **READ ALL RELEVANT FILES BEFORE MODIFYING** - NEVER modify code without first reading ALL related files (models, repositories, managers, UI components). You MUST understand the complete data flow, types, and dependencies before making any changes. This prevents type mismatches and broken implementations.
- Do not leave comments in code that are not TODOs or SERIOUSLY IMPORTANT
- After each change, run the tests to check for broken functionality. Never commit while tests are failing
- **ALWAYS write unit tests confirming errors before fixing them** - This ensures the bug is properly understood and the fix is validated
- You must run all tests and execute the game and do quick smoke tests before every commit
- **BUILD COMMANDS** - Always use `cd "C:\Git\Wayfarer\src" && dotnet build` to build the project. The pipe operator can cause issues with dotnet build output parsing. To check for errors: build first, then check the output.
- **Never keep legacy code for compatibility** - Delete it immediately and fix all callers
- **NEVER use suffixes like "New", "Revised", "V2", etc.** - Replace old implementations completely and use the correct final name immediately. Delete old code, don't leave it behind.
- **NO deprecated methods or backwards compatibility** - When changing a method signature, update ALL callers immediately. Never leave old versions around.

## CRITICAL REFACTORING RULES

  ### NO FALLBACKS
  - When implementing a subsystem, implement it COMPLETELY
  - NO placeholder methods
  - NO "will implement later" comments
  - NO partial functionality
  - If a method exists in old code, it MUST exist in new code IMMEDIATELY

  ### NO COMPATIBILITY LAYERS
  - Delete old code as soon as new code is ready
  - NO keeping both versions
  - NO switch statements choosing implementations
  - NO gradual migration
  - Cut over COMPLETELY or not at all

  ### NO TODOS
  - Every work packet produces COMPLETE, WORKING code
  - NO TODO comments in code
  - NO stub implementations
  - NO "temporary" solutions
  - Production-ready code from day one

  ### COMPLETE MIGRATION ONLY
  - When moving code, move ALL related code
  - Delete source immediately after migration
  - Update ALL references at once
  - NO partial migrations
  - NO leaving code in both places
