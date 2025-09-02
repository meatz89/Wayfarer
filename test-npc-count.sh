#!/bin/bash

echo "Testing NPC count issue..."

# Create a simple test program
cat > /tmp/test-npc-count.cs << 'EOF'
using System;
using System.Linq;

public class TestProgram
{
    public static void Main()
    {
        var gameWorld = GameWorldInitializer.CreateGameWorld();
        Console.WriteLine($"Total NPCs: {gameWorld.NPCs.Count}");
        foreach (var npc in gameWorld.NPCs)
        {
            Console.WriteLine($"  - {npc.ID}: {npc.Name} (IsSkeleton: {npc.IsSkeleton})");
        }
        
        Console.WriteLine($"\nSkeleton Registry: {gameWorld.SkeletonRegistry.Count} entries");
        foreach (var kvp in gameWorld.SkeletonRegistry)
        {
            Console.WriteLine($"  - {kvp.Key}: {kvp.Value}");
        }
    }
}
EOF

# Compile and run
cd /mnt/c/git/wayfarer/src
dotnet run --project . -- /tmp/test-npc-count.cs 2>/dev/null || echo "Direct test failed, trying another approach..."

# Alternative: Create a test that shows the NPCs
cd /mnt/c/git/wayfarer
dotnet test --filter "GameWorldInitializer_CreatesValidGameWorld" --no-build -v detailed 2>&1 | grep -E "NPC:|Skeleton" | head -20