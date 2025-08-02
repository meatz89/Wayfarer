import os
import re

# Dictionary of files to update and their specific patterns
updates = {
    # Update MinDeadline and MaxDeadline in all CS files
    "MinDeadline": "MinDeadlineInDays",
    "MaxDeadline": "MaxDeadlineInDays",
    # Update .Deadline property access
    r"\.Deadline\b": ".DeadlineInDays",
    # Update Deadline = assignments
    r"\bDeadline\s*=": "DeadlineInDays =",
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
                for pattern, replacement in updates.items():
                    content = re.sub(pattern, replacement, content)
                
                # Write back if changed
                if content != original_content:
                    with open(filepath, 'w', encoding='utf-8') as f:
                        f.write(content)
                    print(f"Updated: {filepath}")
                    
            except Exception as e:
                print(f"Error processing {filepath}: {e}")

print("Done!")