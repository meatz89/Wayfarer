#!/bin/bash

# Comprehensive type fixing script for Letter -> DeliveryObligation refactoring
# This handles the bulk of remaining type conversion issues

echo "Starting comprehensive type conversion fixes..."

# Find all C# files to process
find /mnt/c/git/wayfarer/src -name "*.cs" -type f | while read file; do
    echo "Processing: $file"
    
    # Fix method return types
    sed -i 's/public Letter\[\]/public DeliveryObligation[]/g' "$file"
    sed -i 's/private Letter\[\]/private DeliveryObligation[]/g' "$file"
    sed -i 's/List<Letter>/List<DeliveryObligation>/g' "$file"
    sed -i 's/IEnumerable<Letter>/IEnumerable<DeliveryObligation>/g' "$file"
    
    # Fix variable declarations
    sed -i 's/Letter\[\] /DeliveryObligation[] /g' "$file"
    sed -i 's/Letter? /DeliveryObligation? /g' "$file"
    sed -i 's/Letter /DeliveryObligation /g' "$file"
    
    # Fix specific patterns that should be DeliveryObligation
    sed -i 's/Letter letter =/DeliveryObligation letter =/g' "$file"
    sed -i 's/Letter obligation =/DeliveryObligation obligation =/g' "$file"
    sed -i 's/new List<Letter>/new List<DeliveryObligation>/g' "$file"
    
    # Fix LINQ operations on queue
    sed -i 's/\.Where(l => l != null && l\./\.Where(l => l != null \&\& l\./g' "$file"
    sed -i 's/\.Select(l => l\./\.Select(l => l\./g' "$file"
    
    # Fix property access that doesn't exist on DeliveryObligation
    sed -i 's/\.Size/\/\* Size removed \*\//g' "$file"
    sed -i 's/\.PhysicalProperties/\/\* PhysicalProperties removed \*\//g' "$file"
    sed -i 's/\.HasPhysicalProperty/\/\* HasPhysicalProperty removed \*\//g' "$file"
    sed -i 's/\.RequiredEquipment/\/\* RequiredEquipment removed \*\//g' "$file"
    sed -i 's/\.Weight/\/\* Weight removed from obligations \*\//g' "$file"
    
    # Fix method calls that don't exist
    sed -i 's/\.CreateInventoryItem()/\/\* CreateInventoryItem removed \*\//g' "$file"
    sed -i 's/\.GetRequiredSlots()/\/\* GetRequiredSlots removed \*\//g' "$file"
    sed -i 's/\.GetItemSizeCategory()/\/\* GetItemSizeCategory removed \*\//g' "$file"
    sed -i 's/\.GetPhysicalConstraintsDescription()/\/\* GetPhysicalConstraintsDescription removed \*\//g' "$file"
    
    # Fix current view references
    sed -i 's/CurrentViews\.ObligationQueueScreen/CurrentViews.LetterQueueScreen/g' "$file"
    
    # Fix GameConfiguration references  
    sed -i 's/\.ObligationQueue\./.LetterQueue\./g' "$file"
    
    # Fix cases where Letter should remain Letter (special letters)
    sed -i 's/DeliveryObligation introductionDeliveryObligation/Letter introductionLetter/g' "$file"
    sed -i 's/DeliveryObligation accessPermitDeliveryObligation/Letter accessPermitLetter/g' "$file"
    sed -i 's/DeliveryObligation specialDeliveryObligation/Letter specialLetter/g' "$file"
    
done

echo "Type conversion fixes complete!"
echo "Note: Files may have /* removed */ comments that need manual cleanup"