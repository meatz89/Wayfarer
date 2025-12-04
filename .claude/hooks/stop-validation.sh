#!/bin/bash
# Stop Hook: Prevent premature stops and ensure architectural compliance

cat << 'EOF'
STOP VALIDATION - MANDATORY ARCHITECTURE COMPLIANCE CHECK

Before stopping, you MUST spawn an architecture validation agent.

SPAWN AGENT with Task tool (subagent_type: lead-architect) with this prompt:

"MANDATORY COMPLIANCE CHECK - Read ALL documents before responding.

1. READ relevant files in arc42/ folder
2. READ relevant files in gdd/ folder

After reading ALL documents, analyze the code changes made in this session:
- Compare implementation against EVERY principle you discovered
- Compare implementation against EVERY game design concept you discovered
- Check for violations of ANY documented rule, pattern, or constraint

Report back:
- Violations found (with specific file:line references)
- Which document section defines the violated principle
- Holistic remediation plan if violations exist
- Confirmation of compliance if no violations

Do not assume you know the principles - READ THEM."

AFTER AGENT RETURNS:
- If violations found: Present report to user, create fix plan, DO NOT STOP
- If no violations: Proceed with task completion check

Only stop if agent confirms compliance OR user input required.
EOF
