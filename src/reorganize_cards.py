#!/usr/bin/env python3
import json

# Read the original file
with open('Content/Core/02_cards.json', 'r') as f:
    data = json.load(f)

# Initialize the new organized structures
npc_request_cards = {}
npc_progression_cards = {}
npc_promise_cards = {}
npc_exchange_cards = {}

# Cards to keep in the main cards array
general_cards = []

# Process each card and organize by type and NPC
for card in data['content']['cards']:
    card_id = card['id']

    # Check if it's an NPC-specific card (contains NPC name prefix)
    if card_id.startswith('elena_'):
        npc_name = 'elena'

        if card['type'] == 'Request':
            if npc_name not in npc_request_cards:
                npc_request_cards[npc_name] = []
            npc_request_cards[npc_name].append(card)
        elif card['type'] == 'Promise':
            if npc_name not in npc_promise_cards:
                npc_promise_cards[npc_name] = []
            npc_promise_cards[npc_name].append(card)
        elif card['type'] == 'Exchange':
            if npc_name not in npc_exchange_cards:
                npc_exchange_cards[npc_name] = []
            npc_exchange_cards[npc_name].append(card)
        elif card['type'] == 'Normal' and any(marker in card_id for marker in ['understanding', 'confidant', 'bond', 'devotion', 'loyalty']):
            # These are progression cards
            if npc_name not in npc_progression_cards:
                npc_progression_cards[npc_name] = []
            npc_progression_cards[npc_name].append(card)
        else:
            # Other elena cards stay in general for now
            general_cards.append(card)

    elif card_id.startswith('marcus_'):
        npc_name = 'marcus'

        if card['type'] == 'Request':
            if npc_name not in npc_request_cards:
                npc_request_cards[npc_name] = []
            npc_request_cards[npc_name].append(card)
        elif card['type'] == 'Promise':
            if npc_name not in npc_promise_cards:
                npc_promise_cards[npc_name] = []
            npc_promise_cards[npc_name].append(card)
        elif card['type'] == 'Exchange':
            if npc_name not in npc_exchange_cards:
                npc_exchange_cards[npc_name] = []
            npc_exchange_cards[npc_name].append(card)
        elif card['type'] == 'Normal' and any(marker in card_id for marker in ['bargain', 'favor', 'trust', 'knowledge', 'secret']):
            # These are progression cards
            if npc_name not in npc_progression_cards:
                npc_progression_cards[npc_name] = []
            npc_progression_cards[npc_name].append(card)
        else:
            # Other marcus cards (like marcus_trade_request, marcus_business_proposal)
            # Actually, let's check these more carefully
            if 'request' in card_id.lower() or 'proposal' in card_id.lower():
                # These are actually request cards even if marked as "Normal"
                if npc_name not in npc_request_cards:
                    npc_request_cards[npc_name] = []
                npc_request_cards[npc_name].append(card)
            else:
                general_cards.append(card)
    else:
        # Not an NPC-specific card - keep in general
        general_cards.append(card)

# Update the data structure
data['content']['cards'] = general_cards

# Add the new NPC-specific structures
data['content']['npcRequestCards'] = npc_request_cards
data['content']['npcProgressionCards'] = npc_progression_cards
data['content']['npcPromiseCards'] = npc_promise_cards
data['content']['npcExchangeCards'] = npc_exchange_cards

# Write the reorganized file
with open('Content/Core/02_cards.json', 'w') as f:
    json.dump(data, f, indent=2)

print("Reorganization complete!")
print(f"General cards: {len(general_cards)}")
print(f"NPC Request cards: {sum(len(cards) for cards in npc_request_cards.values())}")
print(f"NPC Progression cards: {sum(len(cards) for cards in npc_progression_cards.values())}")
print(f"NPC Promise cards: {sum(len(cards) for cards in npc_promise_cards.values())}")
print(f"NPC Exchange cards: {sum(len(cards) for cards in npc_exchange_cards.values())}")

# Print details for each NPC
for npc_name in sorted(set(list(npc_request_cards.keys()) + list(npc_progression_cards.keys()) +
                             list(npc_promise_cards.keys()) + list(npc_exchange_cards.keys()))):
    print(f"\n{npc_name}:")
    if npc_name in npc_request_cards:
        print(f"  Request cards: {[c['id'] for c in npc_request_cards[npc_name]]}")
    if npc_name in npc_progression_cards:
        print(f"  Progression cards: {[c['id'] for c in npc_progression_cards[npc_name]]}")
    if npc_name in npc_promise_cards:
        print(f"  Promise cards: {[c['id'] for c in npc_promise_cards[npc_name]]}")
    if npc_name in npc_exchange_cards:
        print(f"  Exchange cards: {[c['id'] for c in npc_exchange_cards[npc_name]]}")