#!/bin/bash
# Stop Hook: Validates work is 100% complete before stopping
# This output is injected into Claude's context when conversation would stop

# Get the stop reason from environment or stdin
STOP_REASON="${CLAUDE_STOP_REASON:-unknown}"

# Output validation reminder
cat << 'EOF'
STOP VALIDATION CHECKPOINT:

Before stopping, verify this is NOT a premature stop. Ask yourself:

1. Is this ONLY a plan approval request? (If yes, proceed to stop - user must approve plans)
2. Is this ONLY a clarifying question that requires user input? (If yes, proceed to stop)
3. Did the user explicitly ask to stop or wait? (If yes, proceed to stop)

If NONE of the above, you MUST NOT STOP. Instead:

A. CHECK YOUR TODO LIST:
   - Are ALL todos marked completed?
   - If not, continue working on pending items

B. SPAWN VALIDATION AGENT:
   - Use Task tool to spawn an agent asking:
   - "Review changes holistically. What's missing? Is this 100% complete?"
   - "Did we wire up ALL layers: JSON → DTO → Parser → Entity → Service → UI?"
   - "Are there any TODO comments, partial implementations, or unfinished work?"

C. RULE #0 CHECK:
   - NO HALF MEASURES - is the task FULLY complete?
   - No "quick wins" or partial implementations
   - No deferring work ("let's come back to this later")

If validation reveals incomplete work → CONTINUE IMPLEMENTATION
Only stop when work is genuinely 100% complete or user input is required.
EOF
