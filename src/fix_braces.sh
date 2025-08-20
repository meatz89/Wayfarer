#!/bin/bash

echo "Adding missing closing braces to all .cs files..."

# For each .cs file that's not in Pages folder, add a closing brace at the end
find /mnt/c/git/wayfarer/src -name "*.cs" -type f | while read -r file; do
    # Skip Pages folder (they should have proper namespace)
    if [[ "$file" == *"/Pages/"* ]]; then
        continue
    fi
    
    # Check if file ends with a closing brace
    last_char=$(tail -c 2 "$file" | head -c 1)
    
    if [ "$last_char" != "}" ]; then
        echo "Adding closing brace to: $file"
        echo "}" >> "$file"
    fi
done

echo "Done!"