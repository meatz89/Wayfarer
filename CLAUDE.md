* CLAUDE.md

**⚠️ MANDATORY: READ THE ENTIRE CLAUDE.MD FILE FULLY ⚠️**
**⚠️ MANDATORY: READ ALL MD FILES IN DOCS FOLDER FULLY ⚠️**
**⚠️ MANDATORY: READ ALL HTML FILES IN UI-MOCKUPS FOLDER FULLY ⚠️**
**⚠️ MANDATORY: READ OUR EXiSTING JSON CONTENT FILES AND CSS FILES ⚠️**

**🚨 CRITICAL RULE: NEVER MARK ANYTHING AS COMPLETE WITHOUT TESTING 🚨**
**ALWAYS CHECK BEFORE CLAIMING SUCCESS. TAKE SCREENSHOTS. VERIFY THE ACTUAL RESULT.**
**DO NOT LIE OR MAKE FALSE CLAIMS. IF YOU HAVEN'T VERIFIED, SAY "LET ME CHECK" NOT "IT'S FIXED"**
- You MUST clean build and run manual playwright tests before claiming completion
- You MUST verify the code works before saying "done" or "complete"
- NEVER assume code works - ALWAYS TEST
- If you haven't run `dotnet build`, IT'S NOT COMPLETE
- Saying something is "complete" without testing is UNACCEPTABLE

**⚠️ MANDATORY: READ ALL MARKDOWN FILES IN /DOCS FOLDER FULLY ⚠️**

**🚨 USE PLAYWRIGHT TO TEST THE GAME IN CHROME 🚨**
**🧪 TESTING PRINCIPLE: ALWAYS USE PLAYWRIGHT FOR E2E TESTS 🧪**
**NEVER create test API endpoints. Use Playwright browser automation for all testing.**
**Test the actual UI experience that players will see, not backend endpoints.**
**IMPORTANT: Server port is configured in Properties/launchSettings.json**

**🚨🚨🚨 CRITICAL: NEVER ASSUME - ASK QUESTIONS FIRST 🚨🚨🚨**
- BEFORE implementing any feature, ASK: "What are the ACTUAL values of the data I'm working with?"
- BEFORE assuming properties are set correctly, ASK: "Where are these values actually assigned?"
- BEFORE implementing UI changes, ASK: "What is the ACTUAL data flow from backend to frontend?"
- BEFORE claiming something works, ASK: "Have I actually VERIFIED this with real data?"
- STOP GOING IN CIRCLES: If something doesn't work as expected, INVESTIGATE THE ACTUAL DATA
- LOOK AT THE FULL PICTURE: Examine the complete system, not just the piece you're working on
- THINK FIRST: Before writing code, understand WHY the current approach isn't working

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

**🚨 HIGHLANDER PRINCIPLE: THERE CAN BE ONLY ONE 🚨**
- NEVER have duplicate enums, classes, or concepts for the same thing. If you find EmotionalState and NPCEmotionalState, DELETE ONE. If you find two ways to track the same state, DELETE ONE. No mapping, no conversion, no compatibility layers. ONE source of truth, ONE enum, ONE class per concept.

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

**📝 JSON VALIDATION BEST PRACTICES**
- **Always use case-insensitive property matching** - JSON files use camelCase while C# DTOs use PascalCase
- **Inherit from BaseValidator** - Provides TryGetPropertyCaseInsensitive helper method for robust validation
- **Test validators with actual JSON** - Don't assume field names match between JSON and DTOs

**Principles and Memories:**
- ALWAYS read the full file before editing
- game mechanical values, that could be changed during balancing, should be read from GameRules configuration file.
- avoid defensive programming like checking for null values, try catch blocks, throwing exceptions, using defaults or fallback values and so on. this increases complexity of the code and hides errors. just let it fail and let the program crash fast