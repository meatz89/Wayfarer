#!/bin/bash
# SessionStart Hook: Ensure documentation context before any work

# Auto-install pre-commit hooks silently
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
GIT_HOOKS_DIR="$REPO_ROOT/.git/hooks"

if [ -d "$GIT_HOOKS_DIR" ] && [ -f "$REPO_ROOT/scripts/hooks/pre-commit" ]; then
    cp "$REPO_ROOT/scripts/hooks/pre-commit" "$GIT_HOOKS_DIR/pre-commit" 2>/dev/null
    chmod +x "$GIT_HOOKS_DIR/pre-commit" 2>/dev/null
fi

cat << 'EOF'
WAYFARER SESSION START

Before any task:
1. Read CLAUDE.md - it contains all principles
2. Use agents (Task tool) to gather context from arc42/ and gdd/ folders
3. Achieve certainty through documentation, not codebase scans

Pre-commit hooks and CI enforce compliance. Your job is understanding intent.
EOF
