#!/bin/bash
# SessionStart Hook: Ensure documentation context before any work

cat << 'EOF'
WAYFARER SESSION START

Before any task:
1. Read CLAUDE.md - it contains all principles
2. Use agents (Task tool) to gather context from arc42/ and gdd/ folders
3. Achieve certainty through documentation, not codebase scans

Pre-commit hooks and CI enforce compliance. Your job is understanding intent.
EOF
