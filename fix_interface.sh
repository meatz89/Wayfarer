#!/bin/bash

# Fix ConversationEffects.cs
sed -i 's/public string GetDescriptionForPlayer()/public List<MechanicalEffectDescription> GetDescriptionsForPlayer()/g' /mnt/c/git/wayfarer/src/Game/ConversationSystem/ConversationEffects.cs

# Fix the return statements to return a list
perl -i -pe 's/return "(.*?)";/return new List<MechanicalEffectDescription> { new MechanicalEffectDescription { Text = "$1", Category = EffectCategory.StateChange } };/g' /mnt/c/git/wayfarer/src/Game/ConversationSystem/ConversationEffects.cs

# Fix InvestigateEffects.cs
sed -i 's/public string GetDescriptionForPlayer()/public List<MechanicalEffectDescription> GetDescriptionsForPlayer()/g' /mnt/c/git/wayfarer/src/Game/ConversationSystem/InvestigateEffects.cs

# Fix the return statements to return a list
perl -i -pe 's/return \$"(.*?)";/return new List<MechanicalEffectDescription> { new MechanicalEffectDescription { Text = "$1", Category = EffectCategory.InformationGain } };/g' /mnt/c/git/wayfarer/src/Game/ConversationSystem/InvestigateEffects.cs

# Fix Effects directory files
sed -i 's/public string GetDescriptionForPlayer()/public List<MechanicalEffectDescription> GetDescriptionsForPlayer()/g' /mnt/c/git/wayfarer/src/Game/ConversationSystem/Effects/*.cs

echo "Interface fixes applied. Manual review needed for complex return statements."