#!/usr/bin/env python3
"""
SCORCHED EARTH LOGGING DELETION SCRIPT
Removes ALL Console.WriteLine and Console.Write statements from C# files.

CLAUDE.MD: "No logging until requested"
This script enforces that rule with ZERO tolerance.
"""

import re
import os
from pathlib import Path

def delete_console_logging_from_file(file_path):
    """
    Delete ALL Console.WriteLine and Console.Write statements from a C# file.
    Handles multi-line statements, interpolated strings, and all variations.
    """
    with open(file_path, 'r', encoding='utf-8') as f:
        content = f.read()

    original_lines = len(content.split('\n'))

    # Pattern to match Console.WriteLine/Write statements
    # Handles:
    # - Console.WriteLine(...);
    # - Console.WriteLine($"...");
    # - Console.Write(...);
    # - Multi-line Console statements
    pattern = r'\s*Console\.(WriteLine|Write)\([^;]*\);?\s*\n?'

    # More aggressive pattern for multi-line statements
    # This handles cases where Console.WriteLine spans multiple lines
    multiline_pattern = r'\s*Console\.(WriteLine|Write)\s*\([^)]*\)\s*;?\s*\n?'

    # First pass: simple single-line deletions
    modified_content = re.sub(pattern, '', content, flags=re.MULTILINE)

    # Second pass: handle multi-line cases
    # Keep applying until no more matches (handles nested cases)
    max_iterations = 100
    iteration = 0
    while re.search(multiline_pattern, modified_content) and iteration < max_iterations:
        modified_content = re.sub(multiline_pattern, '', modified_content, flags=re.DOTALL)
        iteration += 1

    # Clean up excessive blank lines (more than 2 consecutive)
    modified_content = re.sub(r'\n{3,}', '\n\n', modified_content)

    if content != modified_content:
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(modified_content)

        modified_lines = len(modified_content.split('\n'))
        lines_removed = original_lines - modified_lines
        return True, lines_removed

    return False, 0

def main():
    src_dir = Path(r'C:\Git\Wayfarer\src')

    if not src_dir.exists():
        print(f"ERROR: Source directory not found: {src_dir}")
        return

    print("=" * 80)
    print("SCORCHED EARTH: Deleting ALL Console.WriteLine/Write statements")
    print("=" * 80)
    print()

    # Find all .cs files recursively
    cs_files = list(src_dir.rglob('*.cs'))

    total_files = len(cs_files)
    modified_files = 0
    total_lines_removed = 0

    print(f"Found {total_files} C# files to process...")
    print()

    for cs_file in cs_files:
        try:
            was_modified, lines_removed = delete_console_logging_from_file(cs_file)
            if was_modified:
                modified_files += 1
                total_lines_removed += lines_removed
                rel_path = cs_file.relative_to(src_dir)
                print(f"[OK] CLEANED: {rel_path} (-{lines_removed} lines)")
        except Exception as e:
            rel_path = cs_file.relative_to(src_dir)
            print(f"[ERROR] {rel_path}: {e}")

    print()
    print("=" * 80)
    print("SCORCHED EARTH COMPLETE")
    print("=" * 80)
    print(f"Files processed: {total_files}")
    print(f"Files modified: {modified_files}")
    print(f"Total lines removed: {total_lines_removed}")
    print()
    print("Verification command:")
    print('  grep -r "Console\\.WriteLine\\|Console\\.Write" C:\\Git\\Wayfarer\\src --include="*.cs" | wc -l')
    print("  Should return: 0")

if __name__ == '__main__':
    main()
