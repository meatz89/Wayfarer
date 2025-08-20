#!/bin/bash

echo "Fixing missing closing braces in specific files..."

# List of files that need closing braces based on error messages
files=(
    "/mnt/c/git/wayfarer/src/Content/Validation/Validators/LetterTemplateValidator.cs"
    "/mnt/c/git/wayfarer/src/Game/MainSystem/LetterPositioningMessage.cs"
    "/mnt/c/git/wayfarer/src/GameMechanics/SpecialLetterEventType.cs"
    "/mnt/c/git/wayfarer/src/GameState/DeliveryObligation.cs"
    "/mnt/c/git/wayfarer/src/GameState/Letter.cs"
    "/mnt/c/git/wayfarer/src/GameState/LetterHistory.cs"
    "/mnt/c/git/wayfarer/src/GameState/LetterPositioningReason.cs"
    "/mnt/c/git/wayfarer/src/GameState/LetterTemplate.cs"
    "/mnt/c/git/wayfarer/src/ViewModels/LetterQueueViewModel.cs"
)

for file in "${files[@]}"; do
    if [ -f "$file" ]; then
        echo "Adding closing brace to: $file"
        echo "}" >> "$file"
    fi
done

echo "Done!"