#!/bin/bash
# SessionStart Hook: Reminds Claude to read CLAUDE.md and gather documentation context
# This output is injected into Claude's context at session start

cat << 'EOF'
WAYFARER PROJECT SESSION START CHECKLIST:

1. READ CLAUDE.md FIRST - Contains all coding principles and architectural rules
2. Use agents to gather context from documentation:
   - arc42/ folder: Technical architecture documentation
   - gdd/ folder: Game design documentation
   - Both glossaries (arc42/12_glossary.md and gdd/08_glossary.md)
3. Key principles to remember:
   - NO HALF MEASURES (Rule #0) - Complete tasks fully
   - DDR-007: Integer math only, no float/double/decimal multipliers
   - HIGHLANDER: No entity instance IDs, use object references
   - CATALOGUE PATTERN: Parse-time only, no runtime catalogue calls
   - Documentation and Boundaries First: Document → Enforce → Implement
4. Before any implementation:
   - Understand the complete system holistically
   - Use Task tool with Explore agent to search codebase
   - Read ALL relevant files completely
5. After each change:
   - Spawn review agent asking "What's missing? Is this 100% complete?"
   - Verify all layers updated: JSON → DTO → Parser → Entity → Service → UI
EOF
