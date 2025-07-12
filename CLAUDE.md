# CLAUDE.md

**⚠️ MANDATORY: READ THE ENTIRE CLAUDE.MD FILE BEFORE WRITING TO IT ⚠️**

[... existing content remains unchanged ...]

### CODE WRITING PRINCIPLES
- Do not leave comments in code that are not TODOs or SERIOUSLY IMPORTANT
- After each change, run the tests to check for broken functionality. Never commit while tests are failing
- **ALWAYS write unit tests confirming errors before fixing them** - This ensures the bug is properly understood and the fix is validated
- You must run all tests and execute the game and do quick smoke tests before every commit
- **Never keep legacy code for compatibility**
- **NEVER use suffixes like "Func", "New", "Revised", "V2", etc.** - Replace old implementations completely and use the correct final name immediately. Delete old code, don't leave it behind.
- **CRITICAL: NO FUNC/ACTION/PREDICATE IN MAIN CODE** - Never use `Func<>`, `Action<>`, `Predicate<>` or similar delegate types in main application code. Use concrete interfaces and classes instead for maintainability, testability, and clarity. 
  - ✅ **EXCEPTIONS**: Test files and builder patterns are allowed to use these for convenience
  - ❌ **FORBIDDEN**: Main application code (src/ directory) must use concrete types
  - **REPLACEMENT PATTERN**: Create specific interfaces like `IRouteValidator` instead of `Func<RouteOption, bool>`
- **CRITICAL: IMMEDIATE LEGACY CODE ELIMINATION** - If you discover ANY legacy code, compilation errors, or deprecated patterns during development, you MUST immediately:
  1. **CREATE HIGH-PRIORITY TODO ITEM** to fix the legacy code
  2. **STOP current work** and fix the legacy code immediately
  3. **NEVER ignore or postpone** legacy code fixes
  4. **NEVER say "these are just dependency fixes"** - fix them now or create immediate todo items

[... rest of the existing content remains unchanged ...]