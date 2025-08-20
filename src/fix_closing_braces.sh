#!/bin/bash

# Find all C# files with misplaced closing braces
echo "Fixing misplaced closing braces in C# files..."

# Process each file
find . -type f -name "*.cs" | while read -r file; do
    # Check if file has a closing brace on line 3
    if sed -n '3p' "$file" | grep -q "^}$"; then
        echo "Fixing: $file"
        
        # Create temp file with correct structure
        temp_file="${file}.tmp"
        
        # Get everything except line 3 (the misplaced closing brace)
        sed '3d' "$file" > "$temp_file"
        
        # Add closing brace at the end
        echo "}" >> "$temp_file"
        
        # Replace original file
        mv "$temp_file" "$file"
    fi
done

echo "Done fixing closing braces"