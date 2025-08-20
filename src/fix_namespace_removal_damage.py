#!/usr/bin/env python3

import os
import glob
import re

def fix_orphaned_closing_brace(filepath):
    """Fix files that have orphaned closing braces from namespace removal"""
    with open(filepath, 'r') as f:
        lines = f.readlines()
    
    fixed = False
    
    # Check for orphaned closing brace (usually around line 10)
    for i in range(min(20, len(lines))):
        if lines[i].strip() == '}' and i > 0:
            # Check if previous lines are comments or blank
            has_code_before = False
            for j in range(i):
                line = lines[j].strip()
                if line and not line.startswith('//') and not line.startswith('using') and not line.startswith('///'):
                    has_code_before = True
                    break
            
            if not has_code_before:
                print(f"Removing orphaned closing brace at line {i+1} in {filepath}")
                lines.pop(i)
                fixed = True
                break
    
    if fixed:
        with open(filepath, 'w') as f:
            f.writelines(lines)
    
    return fixed

def add_missing_closing_braces(filepath):
    """Add missing closing braces to files"""
    with open(filepath, 'r') as f:
        content = f.read()
    
    open_count = content.count('{')
    close_count = content.count('}')
    
    if open_count > close_count:
        missing = open_count - close_count
        print(f"Adding {missing} closing brace(s) to {filepath}")
        
        # Add missing closing braces at the end
        for _ in range(missing):
            content += '\n}'
        
        with open(filepath, 'w') as f:
            f.write(content)
        return True
    
    return False

# Find all C# files
cs_files = glob.glob('**/*.cs', recursive=True)

print("Phase 1: Removing orphaned closing braces...")
fixed_count = 0
for cs_file in cs_files:
    if fix_orphaned_closing_brace(cs_file):
        fixed_count += 1
print(f"Fixed {fixed_count} files with orphaned closing braces\n")

print("Phase 2: Adding missing closing braces...")
fixed_count = 0
for cs_file in cs_files:
    if add_missing_closing_braces(cs_file):
        fixed_count += 1
print(f"Added missing closing braces to {fixed_count} files\n")

# Final check
print("Final brace count check...")
issues = []
for cs_file in cs_files:
    with open(cs_file, 'r') as f:
        content = f.read()
        open_count = content.count('{')
        close_count = content.count('}')
        if open_count != close_count:
            issues.append(f"{cs_file}: {open_count} open, {close_count} close")

if issues:
    print("Remaining brace mismatches:")
    for issue in issues[:10]:  # Show first 10
        print(f"  {issue}")
    if len(issues) > 10:
        print(f"  ... and {len(issues) - 10} more")
else:
    print("All files have matching braces!")