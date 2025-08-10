#!/usr/bin/env python3
import re
import sys

# Mapping of effect classes to their categories and properties
effect_mappings = {
    'LetterReorderEffect': ('LetterReorder', ['_letterId', '_targetPosition', '_tokenType', '_tokenCost']),
    'GainTokensEffect': ('TokenGain', ['_tokenType', '_amount']),
    'BurnTokensEffect': ('TokenSpend', ['_tokenType', '_amount']),
    'ConversationTimeEffect': ('TimePassage', ['_minutes']),
    'RemoveLetterTemporarilyEffect': ('LetterRemove', ['_letterId']),
    'AcceptLetterEffect': ('LetterAdd', ['_letter']),
    'ExtendDeadlineEffect': ('DeadlineExtend', ['_letterId', '_daysToAdd']),
    'ShareInformationEffect': ('InformationGain', ['_route']),
    'CreateObligationEffect': ('ObligationCreate', ['_obligationId', '_npcId']),
    'UnlockRoutesEffect': ('RouteUnlock', ['_routes']),
    'UnlockNPCEffect': ('NpcUnlock', ['_npcToUnlock']),
    'UnlockLocationEffect': ('LocationUnlock', ['_locationId']),
    'DiscoverRouteEffect': ('RouteUnlock', ['_route']),
    'MaintainStateEffect': ('StateChange', []),
    'OpenNegotiationEffect': ('NegotiationOpen', []),
    'GainInformationEffect': ('InformationGain', ['_information', '_infoType']),
    'TimePassageEffect': ('TimePassage', ['_minutes']),
    'CreateBindingObligationEffect': ('ObligationCreate', ['_npcId', '_obligationText']),
    'DeepInvestigationEffect': ('InformationReveal', ['_topic']),
    'UnlockRouteEffect': ('RouteUnlock', ['_routeName']),
    'LockedEffect': ('StateChange', ['_reason']),
    # InvestigateEffects
    'RevealLetterPropertyEffect': ('InformationReveal', ['_letterId', '_propertyToReveal']),
    'PredictConsequenceEffect': ('InformationReveal', ['_letterId']),
    'LearnNPCScheduleEffect': ('InformationGain', ['_npcId']),
    'DiscoverLetterNetworkEffect': ('InformationReveal', ['_letterId']),
    'SwapLetterPositionsEffect': ('LetterSwap', ['_letterId1', '_letterId2', '_tokenType', '_tokenCost'])
}

def convert_method(class_name, method_body):
    """Convert a GetDescriptionForPlayer method to return List<MechanicalEffectDescription>"""
    
    # Extract the text from the method
    text_match = re.search(r'return\s+["\$]([^"]+)"', method_body, re.DOTALL)
    if not text_match:
        # Handle complex returns
        if 'if (' in method_body:
            # Extract conditional logic
            lines = method_body.split('\n')
            text_parts = []
            for line in lines:
                if 'return' in line:
                    match = re.search(r'return\s+["\$]([^"]+)"', line)
                    if match:
                        text_parts.append(match.group(1))
            if text_parts:
                text = ' | '.join(text_parts)
            else:
                text = "Effect applied"
        else:
            text = "Effect applied"
    else:
        text = text_match.group(1)
    
    category, props = effect_mappings.get(class_name, ('StateChange', []))
    
    # Build the properties section
    properties = []
    properties.append(f'                Text = $"{text}"')
    properties.append(f'                Category = EffectCategory.{category}')
    
    # Add specific properties based on class
    if class_name == 'LetterReorderEffect' and '_targetPosition' in props:
        properties.append('                LetterPosition = _targetPosition')
    if class_name in ['GainTokensEffect', 'BurnTokensEffect'] and '_tokenType' in props:
        properties.append('                TokenType = _tokenType')
        properties.append('                TokenAmount = _amount')
    if class_name == 'ConversationTimeEffect' and '_minutes' in props:
        properties.append('                TimeMinutes = _minutes')
    if '_letterId' in props:
        properties.append('                LetterId = _letterId')
    if '_npcId' in props:
        properties.append('                NpcId = _npcId')
    if '_locationId' in props:
        properties.append('                LocationId = _locationId')
    if '_routeName' in props:
        properties.append('                RouteName = _routeName')
    if class_name in ['CreateObligationEffect', 'CreateBindingObligationEffect']:
        properties.append('                IsObligationBinding = true')
    if class_name in ['RevealLetterPropertyEffect', 'DeepInvestigationEffect', 'DiscoverLetterNetworkEffect']:
        properties.append('                IsInformationRevealed = true')
    
    props_str = ',\n'.join(properties)
    
    return f'''    public List<MechanicalEffectDescription> GetDescriptionsForPlayer()
    {{
        return new List<MechanicalEffectDescription>
        {{
            new MechanicalEffectDescription
            {{
{props_str}
            }}
        }};
    }}'''

def process_file(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()
    
    # Find all classes that implement IMechanicalEffect
    class_pattern = r'public class (\w+)\s*:\s*IMechanicalEffect'
    classes = re.findall(class_pattern, content)
    
    for class_name in classes:
        # Find the GetDescriptionForPlayer method for this class
        method_pattern = rf'(class {class_name}.*?)public string GetDescriptionForPlayer\(\)(.*?)(?=public |private |protected |}})'
        match = re.search(method_pattern, content, re.DOTALL)
        
        if match:
            full_match = match.group(0)
            method_body = match.group(2)
            
            # Convert the method
            new_method = convert_method(class_name, method_body)
            
            # Replace in content
            old_method_pattern = r'public string GetDescriptionForPlayer\(\).*?(?=\n    [}])'
            new_content = re.sub(old_method_pattern, new_method[:-1], full_match, 1, re.DOTALL)
            content = content.replace(full_match, new_content)
    
    # Write back
    with open(filepath, 'w', encoding='utf-8') as f:
        f.write(content)
    
    print(f"Updated {filepath}")

if __name__ == "__main__":
    process_file("/mnt/c/git/wayfarer/src/Game/ConversationSystem/ConversationEffects.cs")
    process_file("/mnt/c/git/wayfarer/src/Game/ConversationSystem/InvestigateEffects.cs")