#!/bin/bash

# Find all files with CurrentLocation references
echo "Files with CurrentLocation references:"
grep -r "\.CurrentLocation" src/ --include="*.cs" --include="*.razor" | grep -v "CurrentLocationSpot" | cut -d: -f1 | sort | uniq

echo -e "\n\nDetailed references:"
grep -r "\.CurrentLocation" src/ --include="*.cs" --include="*.razor" | grep -v "CurrentLocationSpot" | grep -n "CurrentLocation"