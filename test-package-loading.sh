#!/bin/bash

# Test script to verify package loading is working correctly

echo "Testing Package Loading System"
echo "=============================="

# Build the project
echo "Building project..."
cd /mnt/c/git/wayfarer/src
dotnet build --no-incremental > /dev/null 2>&1
if [ $? -ne 0 ]; then
    echo "❌ Build failed"
    exit 1
fi
echo "✅ Build succeeded"

# Run tests
echo ""
echo "Running skeleton system tests..."
cd /mnt/c/git/wayfarer
dotnet test --filter "FullyQualifiedName~SkeletonSystemTests" --no-build > test-output.txt 2>&1
if grep -q "Passed!" test-output.txt; then
    echo "✅ All skeleton tests passed"
else
    echo "❌ Some tests failed"
    cat test-output.txt
    exit 1
fi

# Check that legacy JSON files are removed
echo ""
echo "Verifying legacy JSON files are removed..."
LEGACY_FILES=("cards.json" "npcs.json" "locations.json" "routes.json" "letterTemplates.json")
FOUND_LEGACY=false
for file in "${LEGACY_FILES[@]}"; do
    if [ -f "/mnt/c/git/wayfarer/src/Content/$file" ]; then
        echo "❌ Found legacy file: $file"
        FOUND_LEGACY=true
    fi
done
if [ "$FOUND_LEGACY" = false ]; then
    echo "✅ No legacy JSON files found"
fi

# Check that new package files exist
echo ""
echo "Verifying new package files exist..."
PACKAGE_FILES=("01_base_mechanics.json" "02_locations.json" "03_npcs.json" "04_observations.json")
ALL_FOUND=true
for file in "${PACKAGE_FILES[@]}"; do
    if [ ! -f "/mnt/c/git/wayfarer/src/Content/$file" ]; then
        echo "❌ Missing package file: $file"
        ALL_FOUND=false
    fi
done
if [ "$ALL_FOUND" = true ]; then
    echo "✅ All package files found"
fi

# Check for test packages
echo ""
echo "Verifying test packages exist..."
TEST_PACKAGES=("test_01_npcs_with_missing_refs.json" "test_02_locations_resolving_skeletons.json" "test_03_letters_with_missing_npcs.json")
ALL_TEST_FOUND=true
for file in "${TEST_PACKAGES[@]}"; do
    if [ ! -f "/mnt/c/git/wayfarer/src/Content/$file" ]; then
        echo "❌ Missing test package: $file"
        ALL_TEST_FOUND=false
    fi
done
if [ "$ALL_TEST_FOUND" = true ]; then
    echo "✅ All test packages found"
fi

echo ""
echo "=============================="
echo "Package Loading System Status:"
if [ "$FOUND_LEGACY" = false ] && [ "$ALL_FOUND" = true ] && [ "$ALL_TEST_FOUND" = true ]; then
    echo "✅ READY - All legacy code removed, new system in place"
else
    echo "⚠️  INCOMPLETE - Some issues found"
fi

# Clean up
rm -f test-output.txt