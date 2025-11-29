#!/bin/bash
# Stop Hook: Prevent premature stops

cat << 'EOF'
STOP VALIDATION

Before stopping, reflect on YOUR TASK:
- Wiring: Did you connect all layers (JSON → DTO → Parser → Entity → Service → UI)?
- Data flow: Does data flow correctly through the changes?
- Player experience: How do these changes affect gameplay?
- Repercussions: What are the consequences of your changes?

Ask: "What's missing from THIS task?" - not generic codebase issues.
If uncertain, spawn an agent to validate YOUR SPECIFIC CHANGES.

Only stop if user input is required (plan approval, clarifying question).
EOF
