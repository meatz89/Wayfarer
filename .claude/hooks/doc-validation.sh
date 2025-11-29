#!/bin/bash
# PostToolUse Hook: Validate arc42 and gdd document structure after edits
# Receives: CLAUDE_TOOL_INPUT containing file path

# Extract file path from tool input (JSON format)
FILE_PATH=$(echo "$CLAUDE_TOOL_INPUT" | grep -oP '"file_path"\s*:\s*"\K[^"]+' 2>/dev/null || echo "")

# Check if arc42 document
if echo "$FILE_PATH" | grep -q 'arc42/.*\.md'; then
    cat << 'EOF'
ARC42 DOCUMENT VALIDATION

Verify this arc42 document follows the template standard:

1. STRUCTURE: Each section uses pattern/principle tables, not prose paragraphs
2. CONTENT: Describes WHAT and WHY, never HOW (no implementation details)
3. FORMAT: Uses "**Consequences:**" and "**Forbidden:**" sections
4. BREVITY: "Dare to leave gaps" - only document what matters
5. ANTI-PATTERNS:
   - Code blocks (C#, JSON) - forbidden
   - Concrete numbers (50 coins, 1.1x) - forbidden
   - File paths (src/*.cs) - forbidden
   - Enum value lists - forbidden
   - Redundant sections duplicating other documents

Arc42 is a structured cabinet, not a form to fill. Remove irrelevant sections.
EOF
    exit 0
fi

# Check if gdd document
if echo "$FILE_PATH" | grep -q 'gdd/.*\.md'; then
    # Skip BASELINE_ECONOMY which is allowed to have concrete values
    if echo "$FILE_PATH" | grep -q 'BASELINE_ECONOMY'; then
        exit 0
    fi

    cat << 'EOF'
GDD DOCUMENT VALIDATION

Verify this game design document follows the project standard:

1. STRUCTURE: Uses "Why" and "How it manifests" pattern for pillars
2. CONTENT: Design intent and player experience, not implementation
3. FORMAT: Conceptual tables, not code examples
4. PILLARS: Every feature traces back to a design pillar
5. ANTI-PATTERNS:
   - Code blocks - forbidden (except BASELINE_ECONOMY)
   - Implementation details - belong in arc42 or code
   - Specific file references - forbidden
   - JSON structures - forbidden

GDD describes the GAME EXPERIENCE. Technical details belong elsewhere.
EOF
    exit 0
fi

# Not a documentation file - no validation needed
exit 0
