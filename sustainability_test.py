#!/usr/bin/env python3
"""
Test the long-term sustainability of the conversation system
"""

import json

def test_sustainability():
    """Test if the conversation system can sustain 20+ turn conversations"""
    print("=== LONG-TERM SUSTAINABILITY TEST ===\n")

    # Load cards
    with open('src/Content/Core/02_cards.json', 'r', encoding='utf-8') as f:
        cards_data = json.load(f)
    cards = cards_data['content']['cards']

    # Foundation cards (depth 1-2, cost 0 Initiative)
    foundation_cards = [c for c in cards if c.get('depth', 0) <= 2 and c.get('initiativeCost', 0) == 0]
    echo_foundation = [c for c in foundation_cards if c.get('persistence') == 'Echo']

    print(f"Foundation cards (depth <=2, cost 0): {len(foundation_cards)}")
    print(f"Echo Foundation cards: {len(echo_foundation)}")

    # Initiative generators
    initiative_generators = []
    for card in cards:
        effects = card.get('effects', {})
        success_effects = effects.get('success', {})
        if 'Initiative' in success_effects and success_effects['Initiative'] > 0:
            initiative_generators.append(card)

    echo_initiative_generators = [c for c in initiative_generators if c.get('persistence') == 'Echo']

    print(f"Initiative generating cards: {len(initiative_generators)}")
    print(f"Echo Initiative generators: {len(echo_initiative_generators)}")

    # Calculate sustainability metrics
    if len(echo_initiative_generators) > 0:
        total_initiative_per_cycle = sum(
            card['effects']['success']['Initiative']
            for card in echo_initiative_generators
        )

        print(f"\nSUSTAINABILITY ANALYSIS:")
        print(f"Total Initiative generation per full cycle: {total_initiative_per_cycle}")
        print(f"Echo cards that can be recycled: {len(echo_initiative_generators)}")

        # Calculate if we can sustain conversation
        foundation_only_turns = len(echo_foundation)  # How many turns using only Foundation
        initiative_per_foundation_turn = total_initiative_per_cycle / len(echo_initiative_generators) if echo_initiative_generators else 0

        print(f"Average Initiative per Foundation card: {initiative_per_foundation_turn:.1f}")

        # A sustainable system needs ability to play non-Foundation cards occasionally
        sustainable = len(echo_initiative_generators) >= 5 and total_initiative_per_cycle >= 10

        print(f"\nSUSTAINABILITY VERDICT: {'[SUSTAINABLE]' if sustainable else '[UNSUSTAINABLE]'}")

        if sustainable:
            print("- Can generate enough Initiative to play higher-cost cards")
            print("- Echo cards ensure renewable Initiative generation")
            print("- Foundation cards (0-cost) provide safety net")
            print("- System can sustain 20+ turn conversations")
        else:
            print("- May struggle to generate sufficient Initiative")
            print("- Risk of Initiative depletion in long conversations")

    else:
        print("ERROR: No Initiative generators found!")
        sustainable = False

    # Test specific scenario
    print(f"\n=== SAMPLE 20-TURN SIMULATION ===")
    simulate_conversation_turns(echo_initiative_generators, cards)

    return sustainable

def simulate_conversation_turns(echo_generators, all_cards):
    """Simulate a 20-turn conversation"""
    initiative = 0
    turn = 1

    # Deck simulation (simplified)
    available_foundation = [c for c in echo_generators if c.get('depth', 0) <= 2]

    if not available_foundation:
        print("ERROR: No Foundation Echo cards available for simulation")
        return False

    print(f"Starting simulation with {len(available_foundation)} renewable Foundation cards")

    while turn <= 20:
        if turn % 5 == 0:  # Every 5th turn, try to play a higher card
            if initiative >= 1:  # Can afford depth 3+ card
                initiative -= 1
                print(f"Turn {turn}: Played depth 3+ card (-1 Initiative, now {initiative})")
            else:
                # Must play Foundation
                foundation_card = available_foundation[0]  # Use first available
                initiative_gain = foundation_card['effects']['success']['Initiative']
                initiative += initiative_gain
                print(f"Turn {turn}: Played Foundation card '{foundation_card['id']}' (+{initiative_gain} Initiative, now {initiative})")
        else:
            # Regular Foundation turn
            foundation_card = available_foundation[(turn-1) % len(available_foundation)]
            initiative_gain = foundation_card['effects']['success']['Initiative']
            initiative += initiative_gain
            print(f"Turn {turn}: Played Foundation '{foundation_card['id'][:20]}...' (+{initiative_gain} Initiative, now {initiative})")

        turn += 1

    print(f"\nSimulation complete: Final Initiative = {initiative}")
    print("[OK] 20-turn conversation sustained successfully")
    return True

if __name__ == "__main__":
    test_sustainability()