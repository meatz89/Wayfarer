#!/usr/bin/env python3

import os
import glob

def fix_file(filepath):
    """Fix a file with misplaced closing brace on line 3"""
    with open(filepath, 'r') as f:
        lines = f.readlines()
    
    # Check if line 3 is just a closing brace
    if len(lines) > 2 and lines[2].strip() == '}':
        print(f"Fixing: {filepath}")
        # Remove the misplaced closing brace
        lines.pop(2)
        # Add closing brace at the end
        lines.append('}\n')
        
        with open(filepath, 'w') as f:
            f.writelines(lines)
        return True
    return False

# Find all C# files
cs_files = glob.glob('**/*.cs', recursive=True)

fixed_count = 0
for cs_file in cs_files:
    if fix_file(cs_file):
        fixed_count += 1

print(f"Fixed {fixed_count} files")

# Now check for files missing closing braces
print("\nChecking for files missing closing braces...")
for cs_file in cs_files:
    with open(cs_file, 'r') as f:
        content = f.read()
        open_count = content.count('{')
        close_count = content.count('}')
        if open_count != close_count:
            print(f"Brace mismatch in {cs_file}: {open_count} open, {close_count} close")