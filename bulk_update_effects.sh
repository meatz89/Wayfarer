#!/bin/bash

# This script will update all remaining GetDescriptionForPlayer methods
# Since the pattern is consistent, we can use sed more effectively

# First, let's handle simple one-line returns in ConversationEffects.cs
sed -i 's/public string GetDescriptionForPlayer()/public List<MechanicalEffectDescription> GetDescriptionsForPlayer()/g' Game/ConversationSystem/ConversationEffects.cs
sed -i 's/public string GetDescriptionForPlayer()/public List<MechanicalEffectDescription> GetDescriptionsForPlayer()/g' Game/ConversationSystem/InvestigateEffects.cs

echo "Signatures updated. Now you need to manually update the return statements to return List<MechanicalEffectDescription>"