# DOCUMENTATION MAINTENANCE CHECKLIST

This document provides procedures for keeping Wayfarer documentation current and accurate across development sessions.

## REGULAR MAINTENANCE PROCEDURES

### **Session Startup Documentation Review**

Before beginning any coding work, verify:

1. ✅ **CLAUDE.md References**: All file references point to existing files
2. ✅ **Session Handoff Currency**: session-handoff.md reflects current system state
3. ✅ **Test Status Accuracy**: Documentation shows correct test failure counts
4. ✅ **Architectural Status**: Current violations/compliance status is accurate

### **Session Completion Documentation Update**

After completing any work, update:

1. ✅ **session-handoff.md**: New discoveries, completed features, next priorities
2. ✅ **GAME-ARCHITECTURE.md**: New architectural patterns or failure modes discovered
3. ✅ **CLAUDE.md**: Only if core architectural principles change (rare)

## AUTOMATED DETECTION COMMANDS

### **Check for Broken File References**
```bash
# Find all .md file references in documentation
grep -r "\.md" /mnt/c/git/wayfarer/*.md

# Verify each referenced file exists
for file in $(grep -o "[A-Z-]*\.md" /mnt/c/git/wayfarer/CLAUDE.md); do
    if [ ! -f "/mnt/c/git/wayfarer/$file" ]; then
        echo "MISSING FILE: $file"
    fi
done
```

### **Check for Outdated Violation References**
```bash
# Find references to specific files that have been fixed
grep -r "MarketManager.*violation\|TravelManager.*violation" /mnt/c/git/wayfarer/*.md

# Find outdated test status references
grep -r "failing.*test\|test.*fail" /mnt/c/git/wayfarer/*.md
```

### **Check for Outdated Architectural Patterns in Tests**
```bash
# Check for old static property usage
grep -r "GameWorld\.AllContracts\|gameWorld\.AllContracts" /mnt/c/git/wayfarer/Wayfarer.Tests/ --include="*.cs"

# Check for old efficiency multiplier references
grep -r "efficiency.*0\.[0-9]\|efficiency.*1\.[0-9]" /mnt/c/git/wayfarer/Wayfarer.Tests/ --include="*.cs"

# Check for hardcoded bonus/penalty references
grep -r "bonus.*[0-9]\|penalty.*[0-9]" /mnt/c/git/wayfarer/Wayfarer.Tests/ --include="*.cs"
```

## DOCUMENTATION ARCHITECTURE VALIDATION

### **File Purpose Compliance Check**

Verify each documentation file stays within its intended scope:

**CLAUDE.md**:
- ✅ Should contain: Architectural patterns, design principles, file locations
- ❌ Should NOT contain: Session progress, temporary status, specific bug details

**session-handoff.md**:
- ✅ Should contain: Current progress, immediate next steps, session discoveries
- ❌ Should NOT contain: Permanent architectural principles, historical information

**GAME-ARCHITECTURE.md**:
- ✅ Should contain: Critical architectural discoveries, failure patterns, validation checklists
- ❌ Should NOT contain: Session progress, temporary fixes

**LOGICAL-SYSTEM-INTERACTIONS.md**:
- ✅ Should contain: Design principles for system interactions, validation rules
- ❌ Should NOT contain: Implementation details, session-specific information

### **Reference Consistency Check**

Ensure all documentation files reference each other correctly:

1. **CLAUDE.md startup checklist** should reference all critical documents
2. **session-handoff.md** should reference relevant architectural documents
3. **GAME-ARCHITECTURE.md** should reference validation checklists
4. **All file references** should use exact case-sensitive filenames

## COMMON DOCUMENTATION DEBT PATTERNS

### **Pattern 1: Outdated Status Information**

**Problem**: Documentation claims violations exist that have been fixed
**Detection**: Search for "violation", "failing", "broken" in architectural docs
**Solution**: Update status to reflect current system state

### **Pattern 2: Broken File References**

**Problem**: Documentation references files that don't exist or have wrong names
**Detection**: Manually verify each .md file reference
**Solution**: Update references to correct existing files

### **Pattern 3: Misplaced Information**

**Problem**: Session-specific information appears in architectural documents
**Detection**: Look for detailed implementation notes in CLAUDE.md
**Solution**: Move session progress to session-handoff.md

### **Pattern 4: Duplicate Information**

**Problem**: Same information appears in multiple documents
**Detection**: Search for similar content across documentation files
**Solution**: Establish single source of truth for each type of information

## DOCUMENTATION UPDATE TRIGGERS

Update documentation when:

### **Architectural Changes**
- New patterns discovered or enforced
- Major violations found and fixed
- Service configuration changes
- Repository pattern modifications

### **System State Changes**
- Test status changes (failures fixed/introduced)
- Compliance status changes
- Major feature completion
- Critical bug fixes

### **File Structure Changes**
- Documentation files added/removed/renamed
- New critical documents created
- Reference structure modifications

## VALIDATION CHECKLIST

Before ending any session, verify:

1. ✅ **All file references work**: No broken .md links in documentation
2. ✅ **Status accuracy**: Test counts, violation lists, compliance status current
3. ✅ **Session handoff updated**: New discoveries and next priorities documented
4. ✅ **Architecture documentation**: New patterns/failures added if discovered
5. ✅ **Reference consistency**: All documents point to correct files

## EMERGENCY DOCUMENTATION RECOVERY

If documentation becomes inconsistent:

### **Quick Reset Procedure**
1. **Identify authoritative source**: Check git history for last known good state
2. **Verify current system state**: Run all tests, check architectural compliance
3. **Update session-handoff.md**: Document current accurate status
4. **Fix broken references**: Update CLAUDE.md with correct file names
5. **Validate consistency**: Ensure all references work and status is accurate

### **Prevention Measures**
- Never update architectural documents without verifying current system state
- Always test referenced files exist before committing documentation changes
- Keep session progress separate from permanent architectural principles
- Regular maintenance prevents accumulated documentation debt

This checklist ensures documentation remains accurate and useful across all development sessions.