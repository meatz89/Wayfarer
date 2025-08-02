import os
import re

# Fix the double InDaysInDays issue
replacements = {
    "MinDeadlineInDaysInDays": "MinDeadlineInDays",
    "MaxDeadlineInDaysInDays": "MaxDeadlineInDays",
}

# Find all CS files
for root, dirs, files in os.walk("/mnt/c/git/wayfarer/src"):
    for file in files:
        if file.endswith(".cs") or file.endswith(".razor"):
            filepath = os.path.join(root, file)
            
            # Read file
            try:
                with open(filepath, 'r', encoding='utf-8') as f:
                    content = f.read()
                    
                original_content = content
                
                # Apply all replacements
                for pattern, replacement in replacements.items():
                    content = content.replace(pattern, replacement)
                
                # Write back if changed
                if content != original_content:
                    with open(filepath, 'w', encoding='utf-8') as f:
                        f.write(content)
                    print(f"Fixed: {filepath}")
                    
            except Exception as e:
                print(f"Error processing {filepath}: {e}")

print("Done!")