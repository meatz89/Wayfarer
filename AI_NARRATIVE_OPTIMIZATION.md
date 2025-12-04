# AI Narrative Optimization Pipeline

## Quick Start for Next Session

```bash
# 1. Ollama runs as a background service - no need to start it

# 2. Build project
cd C:/Git/Wayfarer && dotnet build

# 3. Run AI narrative evaluation tests
dotnet test --filter "Evaluate_AllSituationFixtures_WithExport" --no-build

# 4. Check results in generated JSON
ls Wayfarer.Tests.Project/AI/narrative_eval_*.json
```

---

## How This Process Works

### Architecture Overview

```
┌─────────────────────┐     ┌──────────────────────┐     ┌─────────────────────┐
│  ScenePromptBuilder │ --> │   Ollama (Local AI)  │ --> │  Quality Reports    │
│  (Prompt Template)  │     │   gemma3:12b-it-qat  │     │  (JSON Export)      │
└─────────────────────┘     └──────────────────────┘     └─────────────────────┘
         │                                                        │
         │                                                        v
         │                                              ┌─────────────────────┐
         │<─────────────────────────────────────────────│  Human/AI Analysis  │
         │              Prompt Improvements             │  (This Session)     │
         └──────────────────────────────────────────────└─────────────────────┘
```

### Key Files

| File | Purpose |
|------|---------|
| `src/Subsystems/Scene/ScenePromptBuilder.cs` | **PRODUCTION PROMPT** - Edit this to change AI behavior |
| `Wayfarer.Tests.Project/AI/NarrativeTestFixtures.cs` | Test fixtures with validation criteria |
| `Wayfarer.Tests.Project/AI/NarrativeEvaluationTests.cs` | Test runner and validation logic |
| `Wayfarer.Tests.Project/AI/NarrativeQualityReport.cs` | JSON export structure for analysis |
| `src/Infrastructure/AI/OllamaConfiguration.cs` | Model config (gemma3:12b-it-qat default) |

---

## Validation Philosophy (CRITICAL)

### Additive, Not Conflicting

AI narrative is **ADDITIVE** to mechanical context, not **CONFLICTING**:

| Scenario | Result |
|----------|--------|
| AI writes pure atmosphere without names | **PASS** - Valid choice |
| AI uses correct entity names from context | **PASS** - Correct reference |
| AI invents wrong/different names | **FAIL** - Breaks mechanical consistency |

### Why This Matters

Both AI narrative AND mechanical entities are displayed to the player in UI. They MUST be consistent:
- Player sees NPC name "Martha Holloway" in character panel
- AI narrative must NOT call her "Sarah" or invent a different name
- AI CAN say "the innkeeper" (generic) or "Martha Holloway" (exact) - both valid

### Two-Tier Validation

1. **Automated (in tests)**:
   - `RequiredContextMarkers` - Atmospheric words that prove context awareness (threshold-based)
   - `ExpectedLengthRange` - Character limits for UI fit
   - `ValidEntityNames` - Tracked for reporting (not strict validation)

2. **Human/AI Review (via JSON export)**:
   - Detect fabricated names (hard to automate)
   - Verify narrative tone matches context
   - Assess overall quality

---

## Test Fixtures Explained

Each fixture in `NarrativeTestFixtures.cs` represents a game scenario with:

| Property | Purpose |
|----------|---------|
| `Context` | NPC, Location, Time, Weather data |
| `Hints` | Tone, Theme, Style guidance |
| `Situation` | Mechanical situation type |
| `ValidEntityNames` | Names ALLOWED if AI references them (not required) |
| `RequiredContextMarkers` | Atmospheric words proving context awareness |
| `RequiredMarkerCount` | Minimum markers that must appear (threshold) |
| `ExpectedLengthRange` | Character count limits for UI fit |

**Marker Design**: Include synonyms since AI may rephrase. "chill" matches "fog", "sun" matches "midday".

---

## Optimization Workflow

### CRITICAL PRINCIPLE: Passing Tests ≠ Perfect Output

**Tests passing does NOT mean there's nothing to improve.**

Tests validate minimum quality thresholds (length, context markers). They do NOT validate:
- Grammatical correctness
- Formatting cleanliness (no markdown in output)
- Narrative quality and flow
- Appropriate word choices
- Natural phrasing

**ALWAYS manually review the JSON output** even when all tests pass. Look for:

| Issue Type | Example | Fix Location |
|------------|---------|--------------|
| **Markdown in output** | `**Thornfield Manor**` | Add rule to prompt: "No markdown formatting" |
| **Grammar issues** | "ahead of Manor" vs "to Manor" | May require prompt guidance or model change |
| **Weak context** | Guard scene doesn't mention "gate" | Strengthen context emphasis in prompt |
| **Unusual phrasing** | "quiet sentinel" for scholar | Review if personality guidance is clear |
| **Repetitive patterns** | Same sentence structure across all | Add variety instruction to prompt |

### Step 1: Run Tests, Get Baseline

```bash
dotnet test --filter "Evaluate_AllSituationFixtures_WithExport"
```

### Step 2: Analyze JSON Results (Even If Passing!)

Look at the exported JSON (`narrative_eval_YYYYMMDD_HHMMSS.json`):

**For failing tests**, check the `allFailureReasons` array which lists specific issues like length violations or missing context markers.

**For passing tests**, still manually review each `response` field. Tests only validate thresholds - they don't catch:
- Markdown formatting that slipped through
- Grammar issues
- Repetitive phrasing across responses
- Model artifacts (end-of-turn tokens)

### Step 3: Identify Root Cause

| Failure Pattern | Root Cause | Fix Location |
|-----------------|------------|--------------|
| Too long/short | Prompt length guidance weak | `ScenePromptBuilder.cs` output rules |
| Missing context markers | AI not using provided context | `ScenePromptBuilder.cs` context emphasis |
| Invented names | AI hallucinating entity names | `ScenePromptBuilder.cs` name rules |
| Wrong tone | Hints not clear enough | `ScenePromptBuilder.cs` narrative direction |

### Step 4: Edit Prompt Template

Edit `ScenePromptBuilder.BuildSituationPrompt()` to address the root cause:
- For length issues: Strengthen character count guidance
- For clichés: Add anti-cliché instructions
- For formatting: Add explicit "no markdown" rule
- For entity names: Add examples of correct vs incorrect usage

### Step 5: Re-run Tests, Verify Improvement

```bash
dotnet test --filter "Evaluate_AllSituationFixtures_WithExport"
```

Compare new JSON to previous. Iterate until pass rate improves.

---

## Common Issues & Solutions

### Issue: AI responses too long

**Symptom**: `Too long: 450 chars (max: 200)`

**Fix in ScenePromptBuilder.cs**: Strengthen length guidance with explicit character counts and "count carefully" instruction.

### Issue: AI invents names

**Symptom**: Response contains names not in context (detected via human review)

**Fix in ScenePromptBuilder.cs**: Add entity name rules with explicit examples of correct (using given names or generic terms) vs incorrect (inventing names) patterns.

### Issue: AI ignores context

**Symptom**: `Context insufficiently referenced: 0/1 markers found`

**Root cause usually in NarrativeTestFixtures.cs** (not prompt):
- Context markers may be too narrow/literal
- AI uses synonyms: "chill" instead of "fog", "sun" instead of "midday"
- **Best practice**: Include multiple synonyms and related words per concept
- Set `RequiredMarkerCount = 1` so only one of many synonyms needs to appear

### Issue: Tests always pass in 2ms (not actually running)

**Symptom**: Test "passes" instantly but no JSON file created

**Causes**:
1. **Ollama health check failed** - Test returns early with skip logic
2. **Old DLL used** - Test run used `--no-build` after code changes

**Fix**:
- **ALWAYS rebuild after code changes**: `dotnet test` (without `--no-build`)
- Verify Ollama accessible: `curl http://localhost:11434/api/tags`
- Check for JSON output file after test run

### Issue: Tests skip (Ollama not available)

**Symptom**: `SKIPPED: Ollama not available` or test passes in 2ms (suspiciously fast)

**Understanding Ollama on Windows**:
- Ollama runs as a **Windows startup app** (system tray icon)
- Listens on port 11434 by default
- **NEVER call `ollama serve`** - this creates a conflicting instance
- **IPv6 vs IPv4**: On Windows, Ollama binds to IPv6 (`::1`). Use `localhost` not `127.0.0.1`

**Fix**:
1. Check system tray for Ollama icon
2. If missing: Start Ollama from Start Menu ("Ollama")
3. Wait 5-10 seconds for it to initialize
4. Verify: `curl http://localhost:11434/api/tags` should return model list
   - **IMPORTANT**: Use `localhost` not `127.0.0.1` (IPv6 binding issue)

**If Ollama is unresponsive** (connects but returns empty):
1. Kill all Ollama processes: `taskkill //F //IM ollama.exe`
2. Also kill: `taskkill //F //IM "ollama app.exe"`
3. Restart Ollama from Start Menu
4. Wait 10 seconds before running tests

---

## Model Configuration

Default model: `gemma3:12b-it-qat` (defined in `OllamaConfiguration.cs`)

To test different models:
1. Edit `OllamaConfiguration.DefaultModel`
2. Re-run tests
3. Compare JSON results

---

## Prompt Engineering History

### v2 (2024-12-04): Stricter Length + Anti-Cliché

**Problem Identified**: v1 prompts produced responses 100-170 chars with repetitive patterns ("dust motes dance" appeared in multiple responses) and occasional markdown artifacts.

**Changes Made** (see `ScenePromptBuilder.BuildSituationPrompt()`):
- Changed character limit from "200 max" to "50-120 strict"
- Added explicit "ONE sentence only" instruction
- Added anti-cliché rule to avoid repetitive openings
- Clarified "pure sensory atmosphere, no character actions"
- Added "plain text only, no markdown" rule

**Results**:
| Fixture | v1 Length | v2 Length | Improvement |
|---------|-----------|-----------|-------------|
| InnkeeperLodgingNegotiation | 103-133 | 83 | ✅ More concise |
| GuardCheckpointConfrontation | 133-176 | 78 | ✅ More concise |
| MerchantInformationExchange | 131-145 | 79 | ✅ More concise |
| ScholarResearchAssistance | 143-166 | 93* | ✅ No "dust motes" |
| ForestPathEncounter | 125-136 | 94 | ✅ More concise |

*\*After stripping model artifacts*

**Key Wins**:
- "Dust motes" cliché eliminated
- Responses now 78-94 chars (vs 100-176 before)
- No markdown formatting in outputs
- Pure atmospheric sentences without character actions

### Model Artifacts Discovered

Gemma3 sometimes appends end-of-turn tokens to responses. Added post-processing in `SceneNarrativeService.CleanAIResponse()` to strip these model-specific artifacts.

---

## Files Modified in Optimization Sessions

### Session 2024-12-04 (v2)
- `src/Subsystems/Scene/ScenePromptBuilder.cs` - Stricter length limits, anti-cliché rules
- `src/Subsystems/Scene/SceneNarrativeService.cs` - Added model artifact stripping
- `Wayfarer.Tests.Project/AI/NarrativeTestFixtures.cs` - Updated ExpectedLengthRange, expanded context markers

### Session Previous (v1)
- `src/Subsystems/Scene/ScenePromptBuilder.cs` - Initial prompt template
- `Wayfarer.Tests.Project/AI/NarrativeTestFixtures.cs` - Test fixtures and validation model
- `Wayfarer.Tests.Project/AI/NarrativeEvaluationTests.cs` - Validation logic
