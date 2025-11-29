#!/bin/bash
# PostToolUse Hook: Remind about document structure after arc42/gdd edits
# Receives: CLAUDE_TOOL_INPUT containing file path

# Extract file path from tool input (JSON format)
FILE_PATH=$(echo "$CLAUDE_TOOL_INPUT" | grep -oP '"file_path"\s*:\s*"\K[^"]+' 2>/dev/null || echo "")

# Check if arc42 document
if echo "$FILE_PATH" | grep -q 'arc42/.*\.md'; then
    cat << 'EOF'
ARC42 EDIT: Review the content you just wrote.

Arc42 describes WHAT and WHY, never HOW. "Dare to leave gaps."
Pre-commit hook will catch violations - no need to scan.
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
GDD EDIT: Review the content you just wrote.

GDD describes GAME EXPERIENCE, not implementation.
Pre-commit hook will catch violations - no need to scan.
EOF
    exit 0
fi

# Not a documentation file - no output needed
exit 0
