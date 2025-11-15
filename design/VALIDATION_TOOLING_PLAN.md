# Wayfarer Validation Tooling Plan

## Purpose

This document outlines the validation tooling strategy for ensuring balanced situation design. It covers manual validation procedures, automated tooling opportunities, and integration into the development workflow.

**Status:** Planning document for future implementation
**Priority:** High (prevents balance issues before they reach players)

---

## Validation Hierarchy

### Layer 1: Manual Pre-Commit Checklist (IMPLEMENTED)

**Location:** design/DESIGN_GUIDE.md "Pre-Commit Validation Checklist"

**Purpose:** Human designer validates each situation before committing

**Coverage:**
- Structural validation (choice count, orthogonal costs, progression guarantee)
- Rule compliance (8 balance rules)
- Scaling validation (stat/coin/session resource costs match tier)
- Edge case testing (6 player archetypes)
- Verisimilitude validation (fiction supports mechanics)
- Reward proportionality (comparison matrix)

**Strengths:**
- Human judgment for narrative quality
- Catches subtle design flaws
- Validates verisimilitude effectively

**Weaknesses:**
- Manual, error-prone
- Time-consuming for bulk content
- Inconsistent application across designers
- Cannot validate aggregate patterns (stat distribution across 100 situations)

**Status:** Fully documented in DESIGN_GUIDE.md, ready for use

### Layer 2: Automated Pre-Commit Validation (PLANNED)

**Purpose:** Scripts validate JSON situation files before git commit

**Coverage:**
- JSON schema validation
- Progression guarantee enforcement
- Cost validation against baseline values
- Orthogonal cost detection
- Reward proportionality checks

**Strengths:**
- Instant feedback
- Consistent enforcement
- Catches structural errors before review
- Scales to infinite content

**Weaknesses:**
- Cannot validate narrative quality
- Limited verisimilitude checking
- May produce false positives

**Status:** Not yet implemented (see implementation plan below)

### Layer 3: Aggregate Content Analysis (PLANNED)

**Purpose:** Analyze patterns across large content sets (100+ situations)

**Coverage:**
- Stat distribution balance (all stats used proportionally)
- Economic viability analysis (B-to-A ratio validation)
- Progression curve smoothness
- Crisis situation frequency
- Multi-stat pattern usage

**Strengths:**
- Catches systemic imbalances
- Validates long-term player experience
- Identifies content gaps

**Weaknesses:**
- Requires large content corpus
- Cannot run on single situation
- May not catch individual situation flaws

**Status:** Not yet implemented (see implementation plan below)

### Layer 4: Playtesting Validation (FUTURE)

**Purpose:** Simulate player playthroughs across archetypes

**Coverage:**
- Worst-case player simulation
- Specialist build simulations (Insight, Authority, Rapport, etc.)
- Generalist build simulation
- Scattered progression simulation
- Economic viability through actual play

**Strengths:**
- Real player experience validation
- Catches emergent issues
- Validates full progression curve

**Weaknesses:**
- Slow (requires simulation)
- Complex to implement
- May miss edge cases without exhaustive testing

**Status:** Future enhancement (requires game engine integration)

---

## Automated Validation Tools: Implementation Plan

### Tool 1: JSON Schema Validator

**Purpose:** Enforce structural correctness of situation JSON files

**Input:** Situation JSON file (e.g., `data/situations/A07_innkeeper_conversation.json`)

**Validation Rules:**

```json
{
  "required_fields": [
    "situationId",
    "tier",
    "sceneNumber",
    "choices"
  ],
  "choices_minimum": 2,
  "choices_a_story_recommended": 4,
  "fallback_required_for_a_story": true,
  "choice_schema": {
    "required": ["choiceId", "pathType", "outcome"],
    "pathType_values": ["InstantSuccess", "Challenge", "Fallback"],
    "outcome_required": ["spawnsScene", "tags", "rewards"]
  }
}
```

**Validation Checks:**

1. **Required Fields Present:**
   ```
   FAIL if missing: situationId, tier, sceneNumber, choices
   PASS if all present
   ```

2. **Minimum Choice Count:**
   ```
   FAIL if choices.length < 2
   WARN if A-story && choices.length < 4
   PASS if choices.length >= 2 (B-story) or >= 4 (A-story)
   ```

3. **Fallback Path Exists (A-story only):**
   ```
   FAIL if A-story && no choice with:
     - pathType = "Fallback"
     - requirements = [] (empty)
     - outcome.spawnsScene exists
   PASS if B-story OR fallback exists
   ```

4. **PathType Values Valid:**
   ```
   FAIL if pathType not in ["InstantSuccess", "Challenge", "Fallback"]
   PASS if all pathTypes valid
   ```

**Implementation:**
- Language: Python or TypeScript
- Library: JSON Schema validator (ajv for JS, jsonschema for Python)
- Integration: Git pre-commit hook
- Output: Error messages with line numbers, specific violations

**Priority:** High (prevents broken JSON from entering codebase)

**Effort:** Low (2-4 hours, standard JSON schema validation)

### Tool 2: Orthogonal Cost Validator

**Purpose:** Detect false choices (two choices costing same resource)

**Input:** Situation JSON file

**Validation Logic:**

```python
def validate_orthogonal_costs(choices):
    resource_usage = {}

    for choice in choices:
        resource_type = identify_resource_type(choice)

        # Check if this resource type already used
        if resource_type in resource_usage:
            # Same resource used by multiple choices
            existing_choice = resource_usage[resource_type]
            existing_cost = get_cost_value(existing_choice)
            current_cost = get_cost_value(choice)

            # FAIL if costs similar (within 20%)
            if abs(existing_cost - current_cost) / max(existing_cost, current_cost) < 0.2:
                return FAIL(f"Choices {existing_choice.id} and {choice.id} both cost similar {resource_type}")

            # WARN if same resource but different costs
            return WARN(f"Choices {existing_choice.id} and {choice.id} both cost {resource_type} (verify rewards proportional)")

        resource_usage[resource_type] = choice

    return PASS("All costs orthogonal")

def identify_resource_type(choice):
    if choice.statRequirement:
        return "stat_" + choice.statRequirement.statType
    elif choice.coinCost:
        return "coins"
    elif choice.sessionResourceCost:
        return "session_" + choice.sessionResourceCost.resourceType
    elif choice.timeBlocksCost:
        return "time"
    else:
        return "free"

def get_cost_value(choice):
    if choice.statRequirement:
        return choice.statRequirement.threshold
    elif choice.coinCost:
        return choice.coinCost
    elif choice.sessionResourceCost:
        return choice.sessionResourceCost.amount
    elif choice.timeBlocksCost:
        return choice.timeBlocksCost
    return 0
```

**Example Output:**

```
FAIL: data/situations/A09_negotiation.json
  Line 34: Choice "pay_small_bribe" costs 10 coins
  Line 48: Choice "pay_large_bribe" costs 12 coins
  Violation: Both choices cost coins with similar values (10 vs 12, within 20%)
  Fix: Make costs more differentiated OR provide proportionally better rewards
```

**Priority:** High (catches common balance violation)

**Effort:** Medium (4-8 hours, requires parsing choice cost structures)

### Tool 3: Cost Scaling Validator

**Purpose:** Verify costs match progression tier baseline values

**Input:** Situation JSON file + tier number

**Validation Logic:**

```python
def validate_cost_scaling(situation):
    tier = situation.tier
    expected_ranges = get_baseline_ranges(tier)

    for choice in situation.choices:
        if choice.statRequirement:
            threshold = choice.statRequirement.threshold
            min_expected = expected_ranges.stat_min
            max_expected = expected_ranges.stat_max

            if threshold < min_expected or threshold > max_expected:
                return WARN(f"Stat requirement {threshold} outside expected range [{min_expected}-{max_expected}] for Tier {tier}")

        if choice.coinCost:
            cost = choice.coinCost
            min_expected = expected_ranges.coin_min
            max_expected = expected_ranges.coin_max

            if cost < min_expected * 0.8 or cost > max_expected * 1.2:
                return FAIL(f"Coin cost {cost} outside acceptable range [{min_expected * 0.8}-{max_expected * 1.2}] for Tier {tier}")

        if choice.sessionResourceCost:
            cost = choice.sessionResourceCost.amount
            min_expected = expected_ranges.session_min
            max_expected = expected_ranges.session_max

            if cost < min_expected or cost > max_expected:
                return WARN(f"Session resource cost {cost} outside expected range [{min_expected}-{max_expected}] for Tier {tier}")

    return PASS("All costs within expected ranges")

def get_baseline_ranges(tier):
    # From BASELINE_ECONOMY.md
    baselines = {
        1: {"stat_min": 0, "stat_max": 3, "coin_min": 10, "coin_max": 15, "session_min": 2, "session_max": 3},
        2: {"stat_min": 4, "stat_max": 5, "coin_min": 20, "coin_max": 30, "session_min": 3, "session_max": 4},
        3: {"stat_min": 6, "stat_max": 7, "coin_min": 35, "coin_max": 50, "session_min": 4, "session_max": 5},
        4: {"stat_min": 7, "stat_max": 8, "coin_min": 60, "coin_max": 100, "session_min": 5, "session_max": 6}
    }
    return baselines[tier]
```

**Example Output:**

```
WARN: data/situations/A15_scholar_conversation.json (Tier 3)
  Line 28: Stat requirement Insight 9
  Expected range for Tier 3: [6-7]
  Actual: 9 (exceeds maximum by 2 points)
  Impact: Only extreme specialists (Insight 9+) can access this path
  Recommendation: Reduce to Insight 7 OR add secondary path at Insight 5-6
```

**Priority:** High (prevents inappropriate difficulty)

**Effort:** Medium (6-10 hours, requires baseline value lookup and range checking)

### Tool 4: Reward Proportionality Validator

**Purpose:** Verify higher costs yield better rewards

**Input:** Situation JSON file

**Validation Logic:**

```python
def validate_reward_proportionality(choices):
    # Calculate reward value for each choice
    choice_values = []
    for choice in choices:
        cost_tier = calculate_cost_tier(choice)
        reward_value = calculate_reward_value(choice.outcome)
        choice_values.append({
            "choice_id": choice.id,
            "cost_tier": cost_tier,
            "reward_value": reward_value
        })

    # Sort by cost tier (highest to lowest)
    choice_values.sort(key=lambda x: x.cost_tier, reverse=True)

    # Verify rewards decrease as costs decrease
    for i in range(len(choice_values) - 1):
        current = choice_values[i]
        next_choice = choice_values[i + 1]

        if current.cost_tier > next_choice.cost_tier:
            # Higher cost, should have higher reward
            if current.reward_value < next_choice.reward_value:
                return FAIL(f"{current.choice_id} costs more than {next_choice.choice_id} but gives LESS reward ({current.reward_value} vs {next_choice.reward_value})")

    return PASS("Rewards proportional to costs")

def calculate_cost_tier(choice):
    # Stat-gated = Tier 4 (highest, permanent investment)
    # Money-gated = Tier 3 (consumable but expensive)
    # Challenge = Tier 2 (risky, variable)
    # Fallback/time = Tier 1 (guaranteed but slow)

    if choice.statRequirement:
        return 4
    elif choice.coinCost and choice.coinCost > 0:
        return 3
    elif choice.sessionResourceCost:
        return 2
    elif choice.timeBlocksCost or choice.pathType == "Fallback":
        return 1
    return 0

def calculate_reward_value(outcome):
    value = 0

    # Relationship changes
    if outcome.relationshipChanges:
        for change in outcome.relationshipChanges:
            value += change.amount * 10  # +2 relationship = 20 value points

    # Stat gains
    if outcome.statGains:
        for gain in outcome.statGains:
            value += gain.amount * 15  # +1 stat = 15 value points

    # Coin rewards
    if outcome.coinReward:
        value += outcome.coinReward * 0.5  # 10 coins = 5 value points

    # Bonus items
    if outcome.bonusItems:
        value += len(outcome.bonusItems) * 10  # Each item = 10 value points

    return value
```

**Example Output:**

```
FAIL: data/situations/A12_merchant_negotiation.json
  Stat-gated choice "charm_merchant" (Rapport 5):
    Cost tier: 4 (highest - permanent stat investment)
    Reward value: 25 (relationship +1, coins +10)
  Money-gated choice "pay_premium" (30 coins):
    Cost tier: 3 (lower than stat-gated)
    Reward value: 35 (relationship +2, coins +15)
  Violation: Lower cost yields HIGHER reward
  Fix: Increase stat-gated rewards OR reduce money-gated rewards
```

**Priority:** Medium (important but not critical)

**Effort:** High (10-15 hours, requires reward value calculation heuristics)

### Tool 5: Progression Guarantee Validator

**Purpose:** Ensure A-story situations have zero-requirement fallback

**Input:** Situation JSON file

**Validation Logic:**

```python
def validate_progression_guarantee(situation):
    if situation.storyType != "A-story":
        return PASS("B-story, no progression guarantee required")

    # Find fallback path
    fallback = None
    for choice in situation.choices:
        if choice.pathType == "Fallback":
            fallback = choice
            break

    if not fallback:
        return FAIL("A-story situation MUST have Fallback path")

    # Verify fallback has zero requirements
    if fallback.statRequirement:
        return FAIL(f"Fallback path has stat requirement ({fallback.statRequirement}), must be zero requirements")

    if fallback.coinCost and fallback.coinCost > 0:
        return FAIL(f"Fallback path costs {fallback.coinCost} coins, must be free (zero coin cost)")

    if fallback.sessionResourceCost:
        return FAIL(f"Fallback path requires session resources, must have zero requirements")

    # Verify fallback spawns next scene
    if not fallback.outcome.spawnsScene:
        return FAIL("Fallback path must spawn next scene (progression guarantee)")

    # Verify fallback cannot fail
    if fallback.pathType == "Challenge":
        return FAIL("Fallback path cannot be Challenge (must be Instant action, cannot fail)")

    return PASS("Progression guarantee satisfied")
```

**Example Output:**

```
FAIL: data/situations/A08_gate_guard.json
  Situation type: A-story
  Fallback path: choice "wait_patiently"
  Violation: Fallback path costs 5 coins (must be free)
  Fix: Remove coin cost from fallback OR change to time cost (acceptable)
```

**Priority:** Critical (prevents soft-locks)

**Effort:** Low (3-5 hours, straightforward requirement checking)

---

## Aggregate Content Analysis Tools

### Tool 6: Stat Distribution Auditor

**Purpose:** Verify all five stats used proportionally across content set

**Input:** Directory of situation JSON files (e.g., `data/situations/A10-A20/`)

**Validation Logic:**

```python
def audit_stat_distribution(situation_files):
    stat_usage = {
        "Insight": 0,
        "Rapport": 0,
        "Authority": 0,
        "Diplomacy": 0,
        "Cunning": 0
    }

    total_stat_gates = 0

    for file in situation_files:
        situation = parse_json(file)
        for choice in situation.choices:
            if choice.statRequirement:
                stat_type = choice.statRequirement.statType
                stat_usage[stat_type] += 1
                total_stat_gates += 1

    # Calculate percentages
    for stat, count in stat_usage.items():
        percentage = (count / total_stat_gates) * 100 if total_stat_gates > 0 else 0
        stat_usage[stat] = {
            "count": count,
            "percentage": percentage
        }

    # Validate balance
    target_percentage = 20.0  # 20% each for five stats
    tolerance = 10.0  # ±10% acceptable

    imbalanced_stats = []
    for stat, data in stat_usage.items():
        if data["percentage"] < target_percentage - tolerance:
            imbalanced_stats.append(f"{stat}: {data['percentage']:.1f}% (UNDERUSED, target 20%)")
        elif data["percentage"] > target_percentage + tolerance:
            imbalanced_stats.append(f"{stat}: {data['percentage']:.1f}% (OVERUSED, target 20%)")

    if imbalanced_stats:
        return WARN(f"Stat distribution imbalanced across {len(situation_files)} situations:\n" + "\n".join(imbalanced_stats))

    return PASS("Stat distribution balanced (all stats within ±10% of 20% target)")
```

**Example Output:**

```
WARN: Stat distribution across A10-A20 (11 situations, 44 total stat gates):
  Insight: 13 gates (29.5%) - OVERUSED (target 20%)
  Rapport: 6 gates (13.6%) - UNDERUSED (target 20%)
  Authority: 12 gates (27.3%) - OVERUSED (target 20%)
  Diplomacy: 7 gates (15.9%) - Acceptable (within ±10%)
  Cunning: 6 gates (13.6%) - UNDERUSED (target 20%)

Recommendation: Add more Rapport/Cunning-gated situations, reduce Insight/Authority situations
```

**Priority:** Medium (important for long-term balance)

**Effort:** Medium (8-12 hours, requires directory traversal and aggregation)

### Tool 7: Economic Viability Calculator

**Purpose:** Verify scattered players can afford money-gated paths via B-stories

**Input:** Tier configuration + situation cost data

**Validation Logic:**

```python
def calculate_economic_viability(tier, situation_costs):
    baseline = get_baseline_economy(tier)

    avg_money_gate_cost = calculate_average(situation_costs)
    b_story_reward = baseline.b_story_reward_typical
    situations_per_tier = 10  # Example: 10 A-situations per tier

    # Worst case: Scattered player uses money-gated paths 80% of time
    money_path_usage_rate = 0.8
    total_coin_cost = avg_money_gate_cost * situations_per_tier * money_path_usage_rate

    # How many B-stories needed?
    b_stories_needed = total_coin_cost / b_story_reward

    # What's the B-to-A ratio?
    ratio = b_stories_needed / situations_per_tier

    # Validation thresholds
    if ratio > 2.0:
        return FAIL(f"B-to-A ratio {ratio:.2f}:1 too high (scattered players must grind excessively)")
    elif ratio > 1.5:
        return WARN(f"B-to-A ratio {ratio:.2f}:1 approaching upper limit (acceptable but tight)")
    else:
        return PASS(f"B-to-A ratio {ratio:.2f}:1 acceptable (scattered players can afford money paths)")

def get_baseline_economy(tier):
    economies = {
        1: {"b_story_reward_typical": 7},
        2: {"b_story_reward_typical": 20},
        3: {"b_story_reward_typical": 38},
        4: {"b_story_reward_typical": 65}
    }
    return economies[tier]
```

**Example Output:**

```
WARN: Economic viability for Tier 2 (A7-A12):
  Average money-gated path cost: 32 coins
  B-story typical reward: 20 coins
  Situations per tier: 10
  Money path usage (80% scattered player): 8 situations
  Total cost: 8 × 32 = 256 coins
  B-stories needed: 256 / 20 = 12.8 B-stories
  B-to-A ratio: 12.8 / 10 = 1.28:1

Status: ACCEPTABLE (within 1.5:1 limit)
Recommendation: Monitor - approaching upper comfort limit
```

**Priority:** Medium (validates scattered player experience)

**Effort:** Medium (6-10 hours, requires economic modeling)

---

## Integration Strategy

### Development Workflow Integration

**Step 1: Pre-Commit Validation (Automated)**

```bash
# Git pre-commit hook (.git/hooks/pre-commit)
#!/bin/bash

echo "Running Wayfarer situation validation..."

# Find all modified .json files in data/situations/
CHANGED_FILES=$(git diff --cached --name-only --diff-filter=ACM | grep "data/situations/.*\.json")

if [ -z "$CHANGED_FILES" ]; then
    echo "No situation files changed, skipping validation"
    exit 0
fi

# Run validation tools
python tools/validate_situations.py $CHANGED_FILES

# Capture exit code
VALIDATION_RESULT=$?

if [ $VALIDATION_RESULT -ne 0 ]; then
    echo "❌ Validation failed. Fix errors before committing."
    exit 1
fi

echo "✓ All validations passed"
exit 0
```

**Step 2: Manual Designer Review (Human)**

Designer completes checklist from DESIGN_GUIDE.md before requesting code review:
- [ ] All automated validations passed
- [ ] Verisimilitude validated (fiction supports mechanics)
- [ ] Narrative quality reviewed
- [ ] Edge cases tested mentally
- [ ] Comparison matrix created

**Step 3: Code Review (Peer)**

Reviewer verifies:
- [ ] Automated validation output reviewed
- [ ] Designer checklist completed
- [ ] Narrative quality acceptable
- [ ] No obvious balance issues

**Step 4: Aggregate Analysis (Weekly/Monthly)**

Run aggregate tools on full content corpus:
- Stat distribution audit (weekly)
- Economic viability analysis (monthly)
- Progression curve validation (monthly)

**Step 5: Playtesting (Milestone)**

Simulate full playthroughs before major releases:
- Worst-case player simulation
- All specialist simulations
- Generalist simulation
- Scattered progression simulation

### Tool Access Patterns

**Content Authors:**
- Use: Manual checklist (Layer 1)
- See: Pre-commit validation output (Layer 2)
- Review: Aggregate analysis reports (Layer 3)

**Lead Designer:**
- Use: All manual checklists
- Run: Aggregate analysis tools manually
- Review: All validation outputs

**Developers:**
- Implement: Automated validation tools
- Maintain: Pre-commit hooks
- Debug: Validation failures

---

## Implementation Priorities

### Phase 1: Critical Foundation (Week 1-2)

**Priority:** CRITICAL
**Effort:** 15-25 hours

1. Tool 5: Progression Guarantee Validator (prevents soft-locks)
2. Tool 1: JSON Schema Validator (prevents broken JSON)
3. Git pre-commit hook integration

**Deliverables:**
- Python scripts for Tools 1 & 5
- Git hook configured
- Documentation for running validators

### Phase 2: Core Balance (Week 3-4)

**Priority:** HIGH
**Effort:** 20-30 hours

1. Tool 2: Orthogonal Cost Validator
2. Tool 3: Cost Scaling Validator
3. BASELINE_ECONOMY.md integration (load baseline values)

**Deliverables:**
- Python scripts for Tools 2 & 3
- Baseline value lookup module
- Enhanced pre-commit validation

### Phase 3: Advanced Validation (Week 5-6)

**Priority:** MEDIUM
**Effort:** 25-35 hours

1. Tool 4: Reward Proportionality Validator
2. Tool 6: Stat Distribution Auditor
3. Tool 7: Economic Viability Calculator

**Deliverables:**
- Python scripts for Tools 4, 6, & 7
- Weekly aggregate analysis reports
- Dashboard for viewing validation results

### Phase 4: Playtesting Simulation (Future)

**Priority:** LOW (nice-to-have)
**Effort:** 50-100+ hours

1. Build playthrough simulator
2. Implement player archetypes (worst-case, specialists, generalist)
3. Run full progression validation

**Deliverables:**
- Simulation engine
- Automated playthrough reports
- Regression testing suite

---

## Technical Implementation Notes

### Language and Framework

**Recommendation:** Python 3.8+

**Rationale:**
- JSON parsing built-in
- Excellent for scripting and validation
- Easy integration with git hooks
- Rich ecosystem for data analysis (pandas, matplotlib for reports)

**Alternative:** TypeScript/Node.js if tight integration with game codebase needed

### File Structure

```
tools/
├── validate_situations.py          # Main entry point
├── validators/
│   ├── __init__.py
│   ├── schema_validator.py         # Tool 1
│   ├── orthogonal_cost_validator.py  # Tool 2
│   ├── cost_scaling_validator.py   # Tool 3
│   ├── reward_proportionality_validator.py  # Tool 4
│   ├── progression_guarantee_validator.py  # Tool 5
│   ├── stat_distribution_auditor.py  # Tool 6
│   └── economic_viability_calculator.py  # Tool 7
├── config/
│   └── baseline_economy.json        # BASELINE_ECONOMY.md as JSON
├── schemas/
│   └── situation_schema.json        # JSON schema definition
└── tests/
    └── test_validators.py           # Unit tests for validators
```

### Running Validators

**Single File Validation:**
```bash
python tools/validate_situations.py data/situations/A09_innkeeper.json
```

**Batch Validation:**
```bash
python tools/validate_situations.py data/situations/A10-A20/*.json
```

**Aggregate Analysis:**
```bash
python tools/aggregate_analysis.py --tier 2 --path data/situations/A07-A12/
```

### Output Format

**Console Output (human-readable):**
```
Validating: data/situations/A09_innkeeper.json
  ✓ Schema validation passed
  ✓ Progression guarantee satisfied
  ✓ Orthogonal costs validated
  ✗ Cost scaling warning: Coin cost 35 exceeds Tier 2 maximum (30)
  ✓ Reward proportionality validated

Summary: 4 passed, 0 failed, 1 warning
```

**JSON Output (for CI/CD integration):**
```json
{
  "file": "data/situations/A09_innkeeper.json",
  "validations": [
    {"tool": "schema", "status": "pass"},
    {"tool": "progression_guarantee", "status": "pass"},
    {"tool": "orthogonal_costs", "status": "pass"},
    {"tool": "cost_scaling", "status": "warn", "message": "Coin cost 35 exceeds Tier 2 maximum (30)"},
    {"tool": "reward_proportionality", "status": "pass"}
  ],
  "summary": {"passed": 4, "failed": 0, "warnings": 1}
}
```

---

## Success Metrics

### Validation Tool Effectiveness

**Metric 1: False Positive Rate**
- Target: < 10% (fewer than 1 in 10 warnings are incorrect)
- Measure: Designer feedback on validation warnings
- Action: If false positive rate > 10%, refine validation heuristics

**Metric 2: Coverage**
- Target: 100% of committed situations validated automatically
- Measure: Git hook execution rate
- Action: If < 100%, enforce pre-commit hooks strictly

**Metric 3: Balance Issue Detection**
- Target: > 80% of balance issues caught before code review
- Measure: Balance issues found in review vs pre-commit
- Action: If < 80%, add new validators for common issues

**Metric 4: Time to Validate**
- Target: < 5 seconds per situation file
- Measure: Average validation execution time
- Action: If > 5 seconds, optimize validators

### Designer Experience

**Metric 5: Designer Confidence**
- Target: Designers feel confident situations are balanced before submitting
- Measure: Survey after 2 months of usage
- Action: If low confidence, improve validation feedback clarity

**Metric 6: Iteration Speed**
- Target: Designers can iterate on situations quickly (< 1 minute feedback loop)
- Measure: Time from situation edit to validation results
- Action: If > 1 minute, optimize git hook or run validators in IDE

---

## Future Enhancements

### Enhancement 1: IDE Integration

Integrate validators into game editor (if visual editor exists):
- Real-time validation as designer edits
- Inline error highlighting
- Autocomplete for baseline values

### Enhancement 2: Visual Dashboards

Create web dashboard showing:
- Stat distribution graphs (pie chart)
- Economic viability trends (line graph)
- Validation pass/fail rates over time
- Content gaps (which situation types underrepresented)

### Enhancement 3: AI-Assisted Validation

Use LLM to validate verisimilitude:
- "Does this cost make narrative sense?"
- "Are these rewards appropriately valued for this NPC personality?"
- Complement automated validation with semantic analysis

### Enhancement 4: Regression Testing

Build test suite that:
- Locks in "known good" situations
- Detects when changes break existing balance
- Prevents accidental balance regressions

---

## Related Documentation

**Manual Validation:**
- [DESIGN_GUIDE.md](DESIGN_GUIDE.md) - Pre-commit validation checklist, edge case procedures, design process

**Baseline Values:**
- [BASELINE_ECONOMY.md](BASELINE_ECONOMY.md) - Authoritative numeric values for cost scaling validation

**Balance Philosophy:**
- [08_balance_philosophy.md](08_balance_philosophy.md) - Rationale for validation rules, examples, crisis situations

**Technical Implementation:**
- [arc42/08_crosscutting_concepts.md](../08_crosscutting_concepts.md) - Catalogue pattern, entity initialization, JSON parsing

---

**Document Status:** Planning document for future implementation
**Last Updated:** 2025-11
**Maintained By:** Design team, development team
**Next Steps:** Implement Phase 1 (Critical Foundation) tools
