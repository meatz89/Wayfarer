#!/bin/bash

# Update all C# files to use DeadlineInDays instead of Deadline
find /mnt/c/git/wayfarer -name "*.cs" -type f | while read file; do
    # Skip files that define the property itself
    if grep -q "public int Deadline { get; set; }" "$file"; then
        continue
    fi
    
    # Update references to .Deadline
    if grep -q "\.Deadline" "$file"; then
        echo "Updating $file"
        sed -i 's/\.Deadline/\.DeadlineInDays/g' "$file"
    fi
    
    # Update references to just Deadline in assignments
    if grep -q " Deadline =" "$file"; then
        sed -i 's/ Deadline =/ DeadlineInDays =/g' "$file"
    fi
done

# Update view models
find /mnt/c/git/wayfarer -name "*.cs" -type f | while read file; do
    # Update specific view model properties that might use Deadline
    if grep -q "public int Deadline {" "$file"; then
        if ! grep -q "public int DeadlineInDays" "$file"; then
            echo "Updating view model in $file"
            sed -i 's/public int Deadline {/public int DeadlineInDays {/g' "$file"
        fi
    fi
done

echo "Update complete!"