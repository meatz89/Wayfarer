#!/usr/bin/env python3
import re
import os

def fix_effect_files():
    """Update all effect files to use the new interface"""
    
    # Files to update
    files = [
        "/mnt/c/git/wayfarer/src/Game/ConversationSystem/ConversationEffects.cs",
        "/mnt/c/git/wayfarer/src/Game/ConversationSystem/InvestigateEffects.cs",
        "/mnt/c/git/wayfarer/src/Game/ConversationSystem/Effects/DeliverLetterEffect.cs",
        "/mnt/c/git/wayfarer/src/Game/ConversationSystem/Effects/EndConversationEffect.cs"
    ]
    
    for filepath in files:
        if not os.path.exists(filepath):
            print(f"File not found: {filepath}")
            continue
            
        with open(filepath, 'r') as f:
            content = f.read()
        
        # Replace method signature
        content = re.sub(
            r'public string GetDescriptionForPlayer\(\)',
            'public List<MechanicalEffectDescription> GetDescriptionsForPlayer()',
            content
        )
        
        # Fix simple return statements - need to be more careful here
        # We'll need to handle each file's specific return patterns
        
        with open(filepath, 'w') as f:
            f.write(content)
        
        print(f"Updated: {filepath}")

if __name__ == "__main__":
    fix_effect_files()