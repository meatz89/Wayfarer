#!/usr/bin/env python3

import os
import re
import glob

def should_keep_namespace(filepath):
    """Check if this file should keep its namespace (Blazor components)"""
    # Keep namespaces in Pages folder and Components
    if '/Pages/' in filepath or '\\Pages\\' in filepath:
        return True
    if '/Components/' in filepath or '\\Components\\' in filepath:
        return True
    # Keep namespaces in .razor files
    if filepath.endswith('.razor') or filepath.endswith('.razor.cs'):
        return True
    return False

def remove_namespace_from_file(filepath):
    """Remove namespace from a C# file while preserving structure"""
    
    if should_keep_namespace(filepath):
        print(f"Skipping (Blazor component): {filepath}")
        return False
    
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Check if file has namespace
    if 'namespace ' not in content:
        return False
    
    lines = content.split('\n')
    new_lines = []
    in_namespace = False
    namespace_brace_count = 0
    namespace_started = False
    
    for i, line in enumerate(lines):
        # Check if this line starts a namespace
        if not in_namespace and re.match(r'^\s*namespace\s+[\w\.]+\s*$', line):
            print(f"Removing namespace from: {filepath}")
            in_namespace = True
            namespace_started = True
            # Skip the namespace line
            continue
        
        # If we just started namespace, skip the opening brace
        if namespace_started and line.strip() == '{':
            namespace_started = False
            namespace_brace_count = 1
            continue
        
        # Track braces within namespace
        if in_namespace and namespace_brace_count > 0:
            # Count braces
            open_braces = line.count('{')
            close_braces = line.count('}')
            namespace_brace_count += open_braces - close_braces
            
            # If we're at the namespace's closing brace
            if namespace_brace_count == 0:
                # Skip the namespace's closing brace
                in_namespace = False
                continue
        
        # Add all other lines (dedent if they were in namespace)
        if in_namespace and line.startswith('    '):
            # Remove one level of indentation (4 spaces)
            new_lines.append(line[4:])
        elif in_namespace and line.startswith('\t'):
            # Remove one tab
            new_lines.append(line[1:])
        else:
            new_lines.append(line)
    
    # Write the modified content back
    new_content = '\n'.join(new_lines)
    
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(new_content)
    
    return True

def remove_using_namespace_statements(filepath):
    """Remove 'using Wayfarer.*' statements except for Pages"""
    
    with open(filepath, 'r', encoding='utf-8') as f:
        lines = f.readlines()
    
    new_lines = []
    removed_any = False
    
    for line in lines:
        # Keep using statements for Wayfarer.Pages (needed for Blazor)
        if re.match(r'^\s*using\s+Wayfarer\.Pages', line):
            new_lines.append(line)
        # Remove other Wayfarer using statements
        elif re.match(r'^\s*using\s+Wayfarer\.\w+', line):
            removed_any = True
            # Skip this line
            continue
        else:
            new_lines.append(line)
    
    if removed_any:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.writelines(new_lines)
        return True
    
    return False

# Main execution
print("Removing namespaces from C# files (preserving Blazor components)...")
print("=" * 60)

# Find all C# files
cs_files = glob.glob('**/*.cs', recursive=True)
razor_cs_files = glob.glob('**/*.razor.cs', recursive=True)
all_files = list(set(cs_files + razor_cs_files))

# First pass: Remove namespaces
removed_count = 0
for cs_file in all_files:
    if remove_namespace_from_file(cs_file):
        removed_count += 1

print(f"\nRemoved namespaces from {removed_count} files")

# Second pass: Remove using statements
print("\nRemoving unnecessary using statements...")
using_count = 0
for cs_file in all_files:
    if remove_using_namespace_statements(cs_file):
        using_count += 1

print(f"Cleaned using statements in {using_count} files")

print("\nDone! Blazor components in Pages/ and Components/ were preserved.")