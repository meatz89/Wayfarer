#!/usr/bin/env python3
"""
Validation script for conversation system requirements
"""

import json
import re

def validate_conversation_system():
    """Main validation function"""
    print("=== CONVERSATION SYSTEM ACCEPTANCE VALIDATION ===\n")

    # Load cards
    try:
        with open('src/Content/Core/02_cards.json', 'r', encoding='utf-8') as f:
            cards_data = json.load(f)
        cards = cards_data['content']['cards']
    except FileNotFoundError:
        print("ERROR: Cards file not found")
        return False
    except json.JSONDecodeError as e:
        print(f"ERROR: Invalid JSON in cards file: {e}")
        return False

    print(f"Total cards loaded: {len(cards)}")

    # 1. Validate Foundation sustainability (depth 1-2)
    foundation_cards = [c for c in cards if c.get('depth', 0) <= 2]
    echo_foundation = [c for c in foundation_cards if c.get('persistence') == 'Echo']
    statement_foundation = [c for c in foundation_cards if c.get('persistence') == 'Statement']

    print(f"\n1. FOUNDATION SUSTAINABILITY VALIDATION")
    print(f"   Foundation cards (depth <=2): {len(foundation_cards)}")
    print(f"   Echo Foundation cards: {len(echo_foundation)}")
    print(f"   Statement Foundation cards: {len(statement_foundation)}")

    if len(foundation_cards) > 0:
        echo_percentage = (len(echo_foundation) / len(foundation_cards)) * 100
        print(f"   Echo percentage: {echo_percentage:.1f}%")
        requirement_1_pass = echo_percentage >= 70
        print(f"   [OK] 70% Echo requirement: {'PASS' if requirement_1_pass else 'FAIL'}")
    else:
        requirement_1_pass = False
        print("   [ERROR] No Foundation cards found")

    # 2. Validate Initiative generators are Echo
    initiative_generators = []
    for card in cards:
        effects = card.get('effects', {})
        success_effects = effects.get('success', {})
        if 'Initiative' in success_effects and success_effects['Initiative'] > 0:
            initiative_generators.append(card)

    print(f"\n2. INITIATIVE GENERATOR VALIDATION")
    print(f"   Cards that generate Initiative: {len(initiative_generators)}")

    non_echo_initiative_generators = [c for c in initiative_generators if c.get('persistence') != 'Echo']
    requirement_2_pass = len(non_echo_initiative_generators) == 0

    for card in initiative_generators[:5]:  # Show first 5
        print(f"   - {card['id']}: Initiative +{card['effects']['success']['Initiative']}, Persistence: {card.get('persistence', 'Unknown')}")

    if len(non_echo_initiative_generators) > 0:
        print(f"   [ERROR] Non-Echo Initiative generators found: {len(non_echo_initiative_generators)}")
        for card in non_echo_initiative_generators:
            print(f"      - {card['id']}: {card.get('persistence')}")
        requirement_2_pass = False
    else:
        print(f"   [OK] All Initiative generators are Echo: PASS")

    # 3. Validate depth distributions exist
    print(f"\n3. CONVERSATION TYPE VALIDATION")
    conv_types_exist = False
    try:
        with open('src/Content/Core/01_foundation.json', 'r', encoding='utf-8') as f:
            foundation_data = json.load(f)
        conv_types = foundation_data.get('content', {}).get('conversationTypes', [])
        conv_types_exist = len(conv_types) > 0
        print(f"   Conversation types defined: {len(conv_types)}")

        # Check if they have depth distributions
        has_distributions = any('distribution' in ct for ct in conv_types)
        print(f"   [OK] Depth distributions: {'DEFINED' if has_distributions else 'MISSING'}")
        requirement_3_pass = has_distributions
    except:
        requirement_3_pass = False
        print("   [ERROR] Could not validate conversation types")

    # 4. Check for hand limit implementation
    print(f"\n4. HAND LIMIT VALIDATION")
    try:
        with open('src/GameState/ConversationSession.cs', 'r', encoding='utf-8') as f:
            session_content = f.read()

        hand_limit_7 = 'HandSize > 7' in session_content
        discard_down = 'DiscardDown' in session_content
        requirement_4_pass = hand_limit_7 and discard_down

        print(f"   [OK] 7-card hand limit: {'IMPLEMENTED' if hand_limit_7 else 'MISSING'}")
        print(f"   [OK] Discard mechanism: {'IMPLEMENTED' if discard_down else 'MISSING'}")
    except:
        requirement_4_pass = False
        print("   [ERROR] Could not validate hand limit")

    # 5. Check LISTEN mechanics
    print(f"\n5. LISTEN MECHANICS VALIDATION")
    try:
        with open('src/Subsystems/Conversation/ConversationFacade.cs', 'r', encoding='utf-8') as f:
            facade_content = f.read()

        doubt_reset = 'CurrentDoubt = 0' in facade_content
        momentum_reduction = 'CurrentMomentum - doubtCleared' in facade_content
        cadence_change = 'Cadence - 3' in facade_content

        print(f"   [OK] Doubt reset to 0: {'IMPLEMENTED' if doubt_reset else 'MISSING'}")
        print(f"   [OK] Momentum reduction: {'IMPLEMENTED' if momentum_reduction else 'MISSING'}")
        print(f"   [OK] Cadence change: {'IMPLEMENTED' if cadence_change else 'MISSING'}")

        requirement_5_pass = doubt_reset and momentum_reduction and cadence_change
    except:
        requirement_5_pass = False
        print("   [ERROR] Could not validate LISTEN mechanics")

    # Summary
    all_requirements = [requirement_1_pass, requirement_2_pass, requirement_3_pass, requirement_4_pass, requirement_5_pass]
    passed_count = sum(all_requirements)

    print(f"\n=== FINAL VALIDATION SUMMARY ===")
    print(f"Requirements passed: {passed_count}/5")
    print(f"Overall status: {'[ACCEPT]' if passed_count == 5 else '[REJECT]'}")

    if passed_count < 5:
        print(f"\nFAILED REQUIREMENTS:")
        req_names = [
            "Foundation sustainability (70% Echo)",
            "Initiative generators are Echo",
            "Conversation type distributions",
            "7-card hand limit",
            "LISTEN mechanics"
        ]
        for i, passed in enumerate(all_requirements):
            if not passed:
                print(f"- {req_names[i]}")

    return passed_count == 5

if __name__ == "__main__":
    validate_conversation_system()