#!/bin/bash
cd /mnt/c/git/wayfarer
echo "Building solution..."
dotnet build --nologo -v q

echo -e "\nRunning tutorial startup test..."
dotnet test --filter "Tutorial_FirstStep_HasCorrectConfiguration" --no-build -v n --nologo