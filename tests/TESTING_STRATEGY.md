# Procedural Tracing HIGHLANDER Refactoring - Testing Strategy

## Overview
This testing strategy verifies the complete HIGHLANDER refactoring where ALL NodeId strings were eliminated and replaced with direct object references throughout the procedural tracing system.

## Test Layers

### Layer 1: Unit Tests (Core Logic)
**File:** `ProceduralTracingTests.cs`
**Focus:** ProceduralContentTracer core methods, object reference integrity
**Duration:** ~10 seconds

### Layer 2: Integration Tests (Backend Hooks)
**File:** `ProceduralTracingTests.cs`
**Focus:** Backend integration points, context storage, reward application
**Duration:** ~30 seconds

### Layer 3: E2E Tests (UI + Full Flow)
**File:** `ProceduralTracingE2ETests.cs`
**Focus:** UI rendering, expand/collapse with HashSet<object>, full gameplay flow
**Duration:** ~2-5 minutes

---

## Execution Instructions

### Prerequisites
```bash
# Ensure dotnet SDK installed
dotnet --version  # Should be 6.0 or higher

# Install Playwright
dotnet tool install --global Microsoft.Playwright.CLI
playwright install chromium

# Navigate to project directory
cd /home/user/Wayfarer
```

### Step 1: Build Project
**CRITICAL:** Build MUST succeed before running tests.

```bash
cd src
dotnet build

# Expected output: Build succeeded. 0 Warning(s). 0 Error(s).
```

**If build fails:**
- Check for compilation errors related to NodeId references (should be eliminated)
- Check for type mismatches in object parameters
- Verify all using statements are correct

### Step 2: Run Unit + Integration Tests
```bash
cd tests
dotnet test --filter "FullyQualifiedName~ProceduralTracingTests"

# Expected output: All tests passed
```

**Key Tests to Verify:**
- ✅ `RecordSceneSpawn_CreatesNodeWithObjectReferences` - No NodeId property
- ✅ `RecordSituationSpawn_CreatesNodeWithParentSceneReference` - Object identity verified
- ✅ `RecordChoiceExecution_CreatesNodeWithParentSituationReference` - Bidirectional links work
- ✅ `ContextStack_PushPop_WorksWithObjects` - Stack<TNode> works correctly
- ✅ `SceneSpawn_WithChoiceContext_LinksToParentChoice` - Parent-child linking works
- ✅ `GetNodeForScene_ReturnsObjectReference` - ConditionalWeakTable lookup works
- ✅ `InstantChoice_RecordsChoiceAndSpawnsScene` - Full instant choice flow
- ✅ `ChallengeChoice_StoresContextThenAppliesReward` - Challenge context stores objects

**If tests fail:**
- Check error messages for null reference exceptions
- Verify ProceduralContentTracer API matches test expectations
- Check that challenge contexts store ChoiceExecution object (not NodeId string)

### Step 3: Run Application for E2E Tests
**Terminal 1:** Start application
```bash
cd src
ASPNETCORE_URLS="http://localhost:6000" dotnet run --no-build
```

**Terminal 2:** Run E2E tests
```bash
cd tests
dotnet test --filter "FullyQualifiedName~ProceduralTracingE2ETests"
```

**Key E2E Tests to Verify:**
- ✅ `TraceViewer_OpensWithoutErrors` - UI renders without console errors
- ✅ `TraceViewer_DisplaysRootScenes` - Scenes display with metadata
- ✅ `ExpandCollapse_WorksWithObjectReferences` - HashSet<object> tracking works
- ✅ `ExpandAll_ExpandsAllNodes` - Bulk expand works
- ✅ `CollapseAll_CollapsesAllNodes` - Bulk collapse works
- ✅ `FilterByCategory_FiltersScenes` - Filtering works
- ✅ `SearchBox_FiltersScenesByName` - Search works
- ✅ `CompleteFlow_InstantChoice_RecordsAndDisplays` - End-to-end instant choice flow

**If E2E tests fail:**
- Check Playwright screenshots in `tests/screenshots/` (if configured)
- Run with `Headless = false` to see browser visually
- Check browser console for JavaScript errors
- Verify UI selectors match actual HTML structure

### Step 4: Manual Verification (Critical!)
Even if automated tests pass, **manually verify** the following:

#### 4.1: Start New Game
```bash
# Start app in terminal
cd src
ASPNETCORE_URLS="http://localhost:6000" dotnet run
```

Open browser: `http://localhost:6000`

#### 4.2: Execute Instant Choice
1. Click "New Game"
2. Find and execute an instant choice (button/card)
3. Verify no errors in browser console (F12 → Console)

#### 4.3: Open Procedural Trace Viewer
1. Click "Procedural Trace" button (or navigate to trace viewer)
2. Verify viewer displays without errors
3. Verify at least one root scene displays

#### 4.4: Test Expand/Collapse
1. Click on a scene node
2. Verify situations expand
3. Click scene again
4. Verify situations collapse
5. Click "Expand All"
6. Verify all nodes expand
7. Click "Collapse All"
8. Verify all nodes collapse

**CRITICAL CHECK:** No console errors during any of these operations!

#### 4.5: Execute Challenge Choice (If Available)
1. Navigate to scene with challenge choice
2. Execute challenge choice
3. Complete challenge (success or failure)
4. Open trace viewer
5. Expand all
6. Verify choice→challenge→scene chain displays
7. Verify spawned scenes link to parent choice

#### 4.6: Verify Parent Links
1. In trace viewer, expand a scene
2. Look for "Spawned by choice: [View Parent]" link
3. Click the link
4. Verify parent is highlighted/selected (depending on implementation)

---

## Expected Results

### ✅ All Tests Pass
- **Unit Tests:** 7/7 passing
- **Integration Tests:** 3/3 passing
- **E2E Tests:** 7/7 passing (minimum)

### ✅ No Compilation Errors
- Build succeeds with 0 errors, 0 warnings
- No references to NodeId properties anywhere
- All object references resolve correctly

### ✅ No Runtime Errors
- No null reference exceptions
- No type cast exceptions
- No browser console errors
- Expand/collapse works smoothly

### ✅ Data Integrity
- Parent-child links maintained via object references
- ConditionalWeakTable lookups return correct nodes
- Context stacks push/pop correctly
- Spawned scenes link to parent choices

---

## Debugging Guide

### Issue: Build Fails with "NodeId does not exist"
**Cause:** Incomplete refactoring - some code still references old NodeId properties
**Fix:** Search codebase for `NodeId` and replace with object references:
```bash
grep -r "NodeId" src/ --include="*.cs" --exclude-dir=obj
```

### Issue: NullReferenceException in Tests
**Cause:** Object reference not properly set or ConditionalWeakTable lookup failed
**Fix:**
1. Check that RecordSceneSpawn/RecordSituationSpawn actually create nodes
2. Verify ConditionalWeakTable.Add is called
3. Ensure entity object is same instance used for lookup

### Issue: Expand/Collapse Doesn't Work
**Cause:** HashSet<object> equality not working correctly
**Fix:**
1. Verify node objects are same instance (not cloned)
2. Check that Contains() uses reference equality
3. Ensure ExpandedNodes collection is passed down to child components

### Issue: Challenge Context Stores null
**Cause:** ChoiceExecution property not set when creating context
**Fix:**
1. Check SceneContent.razor.cs lines ~535, ~545, ~555
2. Verify `ChoiceExecution = choiceNode` (not `ChoiceExecutionNodeId`)
3. Ensure choiceNode is not null before storing

### Issue: Parent Links Don't Display
**Cause:** ParentChoice/ParentSituation/ParentScene are null
**Fix:**
1. Verify context stack is pushed before spawning
2. Check RecordSceneSpawn uses GetCurrentChoiceContext()
3. Ensure context is popped AFTER spawning (in finally block)

---

## Success Criteria

### Minimum Requirements (Must Pass)
- ✅ All unit tests pass
- ✅ All integration tests pass
- ✅ Build succeeds with no errors
- ✅ Trace viewer opens without errors
- ✅ Expand/collapse works without errors

### Comprehensive Verification (Ideal)
- ✅ All E2E tests pass
- ✅ Manual instant choice flow works end-to-end
- ✅ Manual challenge choice flow works end-to-end
- ✅ Parent links display and navigation works
- ✅ No console errors during entire gameplay session
- ✅ Performance acceptable (no lag when expanding large trees)

---

## Test Coverage Matrix

| Component | Unit | Integration | E2E | Manual |
|-----------|------|-------------|-----|--------|
| ProceduralContentTracer core | ✅ | ✅ | ❌ | ❌ |
| Context stacks (Push/Pop) | ✅ | ✅ | ❌ | ❌ |
| ConditionalWeakTable lookup | ✅ | ✅ | ❌ | ❌ |
| Scene spawn recording | ✅ | ✅ | ✅ | ✅ |
| Situation spawn recording | ✅ | ✅ | ✅ | ✅ |
| Choice execution recording | ✅ | ✅ | ✅ | ✅ |
| Instant choice flow | ❌ | ✅ | ✅ | ✅ |
| Challenge choice flow | ❌ | ✅ | ✅ | ✅ |
| Challenge context storage | ❌ | ✅ | ❌ | ✅ |
| Reward application tracing | ❌ | ✅ | ❌ | ✅ |
| UI expand/collapse | ❌ | ❌ | ✅ | ✅ |
| UI filtering | ❌ | ❌ | ✅ | ✅ |
| UI search | ❌ | ❌ | ✅ | ✅ |
| Parent link navigation | ❌ | ❌ | ❌ | ✅ |

---

## Next Steps After Testing

### If All Tests Pass ✅
1. Create PR with test results in description
2. Document HIGHLANDER alignment in PR
3. Merge to main branch

### If Tests Fail ❌
1. Document failure details (which tests, error messages)
2. Create issue with reproduction steps
3. Fix issues in order:
   - Unit test failures first (core logic broken)
   - Integration test failures second (backend hooks broken)
   - E2E test failures third (UI broken)
4. Re-run full test suite after each fix

### Performance Monitoring
After deployment, monitor:
- Trace viewer load time with large graphs (100+ nodes)
- Expand All performance with deep trees
- Memory usage (ConditionalWeakTable should allow GC)
- Browser rendering performance

---

## Questions for Review

1. **Do all unit tests pass?** (Expected: Yes)
2. **Do all integration tests pass?** (Expected: Yes)
3. **Does build succeed without errors?** (Expected: Yes)
4. **Does trace viewer open without console errors?** (Expected: Yes)
5. **Does expand/collapse work correctly?** (Expected: Yes)
6. **Do parent links display correctly?** (Expected: Yes)
7. **Can you complete full instant choice flow without errors?** (Expected: Yes)
8. **Can you complete full challenge flow without errors?** (Expected: Yes)
9. **Are there ANY references to NodeId in the codebase?** (Expected: No, except in old comments)
10. **Is performance acceptable?** (Expected: Yes)

If you answer "No" to ANY of these questions, STOP and debug before proceeding.
