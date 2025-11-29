#!/bin/bash
# Stop Hook: Prevent premature stops

cat << 'EOF'
STOP VALIDATION

Only stop if user input is required (plan approval, clarifying question, explicit request).
Otherwise, verify work is 100% complete. Spawn a validation agent if uncertain.
No half measures.
EOF
