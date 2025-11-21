═══════════════════════════════════════════════════════════
WAYFARER HIGHLANDER ARCHITECTURE PRINCIPLES
COMPLETE REFERENCE - READ BEFORE ALL CODING WORK
═══════════════════════════════════════════════════════════

ARCHITECTURAL FOUNDATION:

Wayfarer generates infinite procedural content where AI creates scenes without knowing which specific entities exist in any given playthrough. EntityResolver.FindOrCreate matches entities by categorical properties (locationType: "CommonRoom", quality: "Standard"), creating them if they don't exist. The same scene template must work identically across playthrough 1, 100, or 1000. String identifiers (IDs, names) break this architecture because they assume specific entities exist. Categorical properties provide the flexibility procedural generation requires.

CORE PRINCIPLE:

Entities are strongly-typed objects in memory. All code compares typed objects, stores typed objects, and passes typed objects. Never extract strings from objects for identity purposes. The object itself IS the identity.

═══════════════════════════════════════════════════════════
ABSOLUTE PROHIBITIONS - NEVER IN CODE
═══════════════════════════════════════════════════════════

TYPE SYSTEM VIOLATIONS:
- Generic object type (public object Item)
- var keyword
- Dictionary<K,V> unless K and V are domain entities or enums
- HashSet<T>
- Func, Action, lambda expressions in backend (except LINQ, UI handlers, framework config)
- Nullable types with ? annotation
- float (use int only)
- Tuples
- Extension methods
- Helper/Utility classes (use Domain Services and Domain Entities)

STRING IDENTIFIER VIOLATIONS:
- String properties representing entity identity (ItemId, ItemName, NpcIdentifier, LocationKey)
- String extraction from objects (item.Id, item.Name, npc.Name) except display/logging
- String comparisons for identity (x.Name == y.Name, x.Id == y.Id)
- String lookups (FirstOrDefault(i => i.Name == searchName))
- String method parameters for entities (void Process(string itemName))
- String collection keys representing entities (Dictionary<string, int> itemCounts)
- String lists representing entities (List<string> ItemIds)

ARCHITECTURAL VIOLATIONS:
- Multiple purposes per entity
- Backwards relationships (Location owns Goals - wrong direction)
- Boolean gates for progression (use resource arithmetic)
- Compatibility layers or gradual migration
- "Temporary" or "Migration" comments
- Incomplete refactoring (no half-measures)

CRITICAL: Replacing .Id with .Name is STILL a violation. Both are string identifiers. Both break strongly-typed object reference architecture.

═══════════════════════════════════════════════════════════
MANDATORY PATTERNS - ALWAYS IN CODE
═══════════════════════════════════════════════════════════

STRONGLY-TYPED OBJECT REFERENCES:
- Properties store specific entity types: public Item Item, public NPC Npc, public Location Location
- Assignments pass typed objects: Item = item, Npc = npc, Location = location
- Comparisons use object equality: if (item1 == item2), if (items.Contains(item))
- Methods accept specific types: void Process(Item item), Location Find(Location location)
- Collections use specific types: List<Item>, Dictionary<NPC, int>
- Explicit type declarations: Item item = ..., NPC npc = ..., Location location = ...
- Null-forgiving operator: item!.Property (not item?.Property)
- Call chains pass typed objects end-to-end: UI → ViewModel → Facade → Service

COLLECTION TYPES:
- List<T> where T is specific entity or enum
- Dictionary<K,V> ONLY if K and V are specific entities/enums
- int for all numeric types (never float)
- Explicit types always (never var)

STRING USAGE - ONLY 3 PERMITTED CONTEXTS:
1. Template IDs: SceneTemplate.Id, SituationTemplate.Id (immutable archetypes, hierarchical references)
2. Display output: Console.WriteLine($"Item: {item.Name}"), logging (user-facing only)
3. Categorical property values: locationType: "CommonRoom", quality: "Standard" (type descriptors, not identifiers)

═══════════════════════════════════════════════════════════
VIOLATION DETECTION METHODOLOGY
═══════════════════════════════════════════════════════════

USE GREP TO FIND PATTERNS:

String comparison patterns indicating identity violations:
- \.Name\s*== (object name comparison)
- \.Id\s*== (object ID comparison)
- FirstOrDefault.*\.Name (string lookup)
- FirstOrDefault.*\.Id (string lookup)
- Any.*\.Name\s*== (LINQ string comparison)

String property patterns indicating storage violations:
- public string.*Id\s*{ (ID property)
- public string.*Name\s*{ (Name property as identifier)
- List<string>.*Id (string list representing entities)
- Dictionary<string, (collection with string entity keys)

String extraction patterns indicating identity violations:
- =\s*.*\.Name\s*; (assignment extracting name)
- =\s*.*\.Id\s*; (assignment extracting ID)

Exclude legitimate uses from search:
- Console, Log, WriteLine, Display (legitimate display usage)
- Template (legitimate template ID usage)
- string name (variable naming, not property storage)

═══════════════════════════════════════════════════════════
VIOLATION CORRECTION PROCESS
═══════════════════════════════════════════════════════════

FOR STRING IDENTIFIER PROPERTIES:
- Locate: public string ItemId { get; set; }
- Delete: string property entirely
- Add: public Item Item { get; set; }
- Find all uses: grep -rn "ItemId" src/
- Fix each use: change to object reference
- Verify: grep confirms zero occurrences

FOR STRING COMPARISONS:
- Locate: if (item1.Name == item2.Name)
- Change: if (item1 == item2)
- Verify: comparison uses object equality

FOR STRING LOOKUPS:
- Locate: items.FirstOrDefault(i => i.Name == searchName)
- Trace caller: why does caller have string instead of object?
- Fix caller: change to pass object instead of string
- Change method signature: accept object parameter
- Remove lookup: use object directly
- Verify: no string-based lookups remain

FOR STRING METHOD PARAMETERS:
- Locate: public void Process(string itemName)
- Change signature: public void Process(Item item)
- Find all callers: grep -rn "Process\(" src/
- Fix each caller: pass object instead of extracting string
- Verify: method receives typed object

FOR STRING COLLECTIONS:
- Locate: Dictionary<string, int> itemCounts
- Change: Dictionary<Item, int> itemCounts
- Fix all uses: use objects as keys
- Verify: collection uses typed objects

CALL CHAIN REFACTORING:
When fixing string parameter, trace upstream:
1. Find method with string parameter
2. Find all callers of that method
3. If caller extracts string from object (item.Name), fix caller
4. Change caller to pass object
5. Repeat upstream until source is object
6. Verify entire chain uses typed objects

═══════════════════════════════════════════════════════════
MENTAL MODELS
═══════════════════════════════════════════════════════════

ENTITY IDENTITY:
Entities exist as strongly-typed objects in memory. When referencing entity, use specific type object reference (Item, NPC, Location). When comparing entities, compare typed objects. When storing entity relationship, store typed object. When passing entity to method, pass typed object with explicit type declaration.

STRING USAGE DECISION:
Encounter .Name or .Id in code → Ask: "Why converting typed object to string?"
- Answer "For display to user" → Legitimate, continue
- Answer "For comparison" → Violation, use object equality
- Answer "For storage" → Violation, store typed object
- Answer "For passing to method" → Violation, method should accept typed object
- Answer "For collection key" → Violation, use object as key

TYPE SYSTEM:
Never generic object type. Never var keyword. Always specific entity types. Item item (not var item, not object item). Explicit types reveal design. Generic types hide design. Strong typing enforces correct relationships.

PROCEDURAL CONTENT:
Content generation cannot assume specific entities exist. Templates use categorical properties. Runtime resolves properties to entities. If entity matching properties exists, use it. If not, create it. String identifiers break this. They assume "item_123" exists. Categorical properties ("quality: Standard") work regardless of what exists.

═══════════════════════════════════════════════════════════
SELF-VERIFICATION CHECKLIST
═══════════════════════════════════════════════════════════

BEFORE EVERY COMMIT, VERIFY:

Type system compliance:
□ No var keyword
□ No generic object type
□ No Dictionary/HashSet except with entity types
□ No nullable ? annotations
□ All types explicitly declared
□ Only int (no float)

String identifier elimination:
□ No properties ending in Id or Name storing strings
□ No .Name comparisons (except display)
□ No .Id comparisons (except templates)
□ No string extraction from objects (except display)
□ No string method parameters for entities
□ No string collection keys for entities

Object reference correctness:
□ All entity properties use specific types
□ All entity comparisons use object equality
□ All method parameters accept typed objects
□ All collections use typed objects
□ Call chains pass typed objects end-to-end

Architectural correctness:
□ No compatibility layers
□ No temporary comments
□ No incomplete refactoring
□ One purpose per entity
□ Correct relationship directions

GREP VERIFICATION COMMANDS:
Run these, expect zero results (except allowed contexts):

bash: grep -rn "\.Name\s*==" src/ --include="*.cs" | grep -v "Console\|Log"
bash: grep -rn "\.Id\s*==" src/ --include="*.cs" | grep -v "Template"
bash: grep -rn "public string.*Id\s*{" src/GameState/ | grep -v "Template"
bash: grep -rn "\bvar\b" src/ --include="*.cs"
bash: grep -rn "public object " src/ --include="*.cs"

═══════════════════════════════════════════════════════════
CODING EXECUTION PRINCIPLES
═══════════════════════════════════════════════════════════

REFACTORING APPROACH:
- Delete first, fix after (intentionally break, then repair completely)
- Complete refactoring only (no half-measures)
- No compatibility layers (clean breaks)
- Fix entire call chain (not just immediate error)
- Finish what you start (no TODO comments)
- If you can't do it right, don't do it at all

VERIFICATION APPROACH:
- Grep confirms violations eliminated
- Self-checklist passes completely
- Build succeeds (if available)
- Manual testing passes (if available)
- No assumptions, verify everything

QUALITY STANDARDS:
- Playability over compilation (broken links worse than crashes)
- Perfect information at strategic layer (all costs visible)
- Single source of truth (HIGHLANDER - one owner per entity)
- No soft-locks ever (always forward progress)
- Elegance through minimal interconnection

═══════════════════════════════════════════════════════════
END OF REFERENCE
═══════════════════════════════════════════════════════════

This document contains complete architectural principles. Read entirely before coding. Refer back when uncertain. Follow absolutely. No exceptions. No shortcuts.