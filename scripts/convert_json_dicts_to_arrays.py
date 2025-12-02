#!/usr/bin/env python3
"""
Convert Dictionary patterns in JSON to Array-of-Objects patterns.
DOMAIN COLLECTION PRINCIPLE: { "key": value } → [{ "key": "...", "value": ... }]
"""

import json
import os
import sys
from pathlib import Path

# Define conversions: property_name -> (key_field_name, value_field_name)
CONVERSIONS = {
    # Token patterns
    "initialTokens": ("tokenType", "amount"),
    "tokenGains": ("tokenType", "amount"),
    "tokenGenerationBonuses": ("tokenType", "amount"),
    "tokenGate": ("tokenType", "amount"),
    "tokens": ("tokenType", "amount"),

    # Stat patterns
    "statRequirements": ("stat", "value"),
    "stats": ("stat", "threshold"),
    "minStats": ("stat", "threshold"),

    # XP/Reward patterns
    "completionRewardXP": ("stat", "xpAmount"),
    "discoveryQuantities": ("discoveryType", "quantity"),

    # Other patterns
    "grantConditions": ("conditionType", "value"),
    "cardCounts": ("cardId", "count"),
    "npcRelationshipDeltas": ("npcId", "delta"),
    "locationVisits": ("locationId", "visitCount"),
    "npcBond": ("npcId", "bondStrength"),
    "locationReputation": ("locationId", "reputationScore"),
    "routeTravelCount": ("routeId", "travelCount"),
    "tokenRequirement": ("tokenType", "amount"),

    # Professions by time (complex - list of strings as value)
    "availableProfessionsByTime": ("timeBlock", "professions"),
}

def convert_dict_to_array(data, key_name, value_name):
    """Convert { "k1": v1, "k2": v2 } to [{ key_name: "k1", value_name: v1 }, ...]"""
    if not isinstance(data, dict):
        return data
    return [{key_name: k, value_name: v} for k, v in data.items()]

def process_value(value, parent_key=None):
    """Recursively process a JSON value, converting dictionaries where needed."""
    if isinstance(value, dict):
        # Check if this dict should be converted based on parent key
        new_dict = {}
        for k, v in value.items():
            if k in CONVERSIONS and isinstance(v, dict):
                key_name, value_name = CONVERSIONS[k]
                new_dict[k] = convert_dict_to_array(v, key_name, value_name)
            else:
                new_dict[k] = process_value(v, k)
        return new_dict
    elif isinstance(value, list):
        return [process_value(item, parent_key) for item in value]
    else:
        return value

def process_file(filepath):
    """Process a single JSON file."""
    print(f"Processing: {filepath}")

    with open(filepath, 'r', encoding='utf-8') as f:
        data = json.load(f)

    converted = process_value(data)

    with open(filepath, 'w', encoding='utf-8') as f:
        json.dump(converted, f, indent=2, ensure_ascii=False)

    print(f"  Done: {filepath}")

def main():
    content_dir = Path("/home/user/Wayfarer/src/Content")

    # Find all JSON files
    json_files = list(content_dir.glob("**/*.json"))

    print(f"Found {len(json_files)} JSON files")

    for filepath in json_files:
        try:
            process_file(filepath)
        except Exception as e:
            print(f"  ERROR processing {filepath}: {e}")

if __name__ == "__main__":
    main()
