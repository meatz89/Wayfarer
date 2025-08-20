#!/bin/bash

echo "Removing all unnecessary namespaces from C# files..."

# Process all .cs files (excluding Blazor components in Pages folder)
find /mnt/c/git/wayfarer/src -name "*.cs" -type f | while read -r file; do
    # Skip Pages folder files (Blazor components)
    if [[ "$file" == *"/Pages/"* ]]; then
        echo "Skipping Blazor component: $file"
        continue
    fi
    
    echo "Processing: $file"
    
    # Remove namespace declarations and their closing braces
    # This handles both namespace Foo.Bar { ... } and namespace Foo.Bar; styles
    sed -i '/^namespace\s/,/^{$/d' "$file"
    sed -i '/^namespace\s.*;$/d' "$file"
    
    # Remove the closing brace at the end if it's the only thing on the line
    # Count opening and closing braces to determine if we need to remove the last one
    open_braces=$(grep -c '^{$' "$file")
    close_braces=$(grep -c '^}$' "$file")
    
    if [ "$close_braces" -gt "$open_braces" ]; then
        # Remove the last closing brace (namespace closing brace)
        sed -i '$ { /^}$/d; }' "$file"
    fi
    
    # Remove using statements that reference internal namespaces (Wayfarer.*)
    # Keep System.* and Microsoft.* and other external libraries
    sed -i '/^using Wayfarer\./d' "$file"
done

# Now process Pages folder separately - keep Wayfarer.Pages namespace
find /mnt/c/git/wayfarer/src/Pages -name "*.cs" -type f | while read -r file; do
    echo "Processing Blazor component: $file"
    
    # Check if file already has namespace Wayfarer.Pages
    if ! grep -q "namespace Wayfarer.Pages" "$file"; then
        # Check if file has any namespace
        if grep -q "^namespace\s" "$file"; then
            # Replace existing namespace with Wayfarer.Pages
            sed -i 's/^namespace\s.*/namespace Wayfarer.Pages/' "$file"
        else
            # Add namespace Wayfarer.Pages if no namespace exists
            # Find the first using statement or class declaration
            first_class_line=$(grep -n "^\(public\|internal\|private\|protected\)\s*\(partial\s*\)\?\(class\|interface\|enum\|struct\)" "$file" | head -1 | cut -d: -f1)
            if [ ! -z "$first_class_line" ]; then
                # Insert namespace before the class
                sed -i "${first_class_line}i\\namespace Wayfarer.Pages\n{" "$file"
                echo "}" >> "$file"
            fi
        fi
    fi
    
    # Remove internal namespace usings except Wayfarer.Pages
    sed -i '/^using Wayfarer\./{ /^using Wayfarer.Pages/!d; }' "$file"
done

echo "Namespace removal complete!"
echo "Now fixing compilation by removing type prefixes..."

# Remove namespace prefixes from types in all files
find /mnt/c/git/wayfarer/src -name "*.cs" -type f | while read -r file; do
    # Remove Wayfarer.GameState. prefix
    sed -i 's/Wayfarer\.GameState\.//g' "$file"
    # Remove Wayfarer.Game.MainSystem. prefix  
    sed -i 's/Wayfarer\.Game\.MainSystem\.//g' "$file"
    # Remove Wayfarer.Content. prefix
    sed -i 's/Wayfarer\.Content\.//g' "$file"
    # Remove Wayfarer.Core.Repositories. prefix
    sed -i 's/Wayfarer\.Core\.Repositories\.//g' "$file"
    # Remove Wayfarer.GameMechanics. prefix
    sed -i 's/Wayfarer\.GameMechanics\.//g' "$file"
    # Remove Wayfarer.Services. prefix
    sed -i 's/Wayfarer\.Services\.//g' "$file"
    # Remove Wayfarer.ViewModels. prefix
    sed -i 's/Wayfarer\.ViewModels\.//g' "$file"
    # Remove Wayfarer.UIHelpers. prefix
    sed -i 's/Wayfarer\.UIHelpers\.//g' "$file"
    # Remove any other Wayfarer. prefixes
    sed -i 's/Wayfarer\.\([A-Z][a-zA-Z]*\)\.\([A-Z][a-zA-Z]*\)\.//g' "$file"
done

echo "Script complete! All namespaces removed except Wayfarer.Pages for Blazor components."