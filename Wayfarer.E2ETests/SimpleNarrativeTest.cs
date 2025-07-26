using System;

public class SimpleNarrativeTest
{
    public static void Main()
    {
        Console.WriteLine("=== SIMPLE NARRATIVE TEST ===\n");
        
        try
        {
            // First test if narrative definitions can be built
            Console.WriteLine("Building narrative definitions...");
            NarrativeContentBuilder.BuildAllNarratives();
            
            Console.WriteLine($"Narratives built. Count: {NarrativeDefinitions.All.Count}");
            
            foreach (var narrative in NarrativeDefinitions.All)
            {
                Console.WriteLine($"\nNarrative: {narrative.Id}");
                Console.WriteLine($"  Title: {narrative.Title}");
                Console.WriteLine($"  Steps: {narrative.Steps.Count}");
                if (narrative.Steps.Count > 0)
                {
                    Console.WriteLine($"  First step: {narrative.Steps[0].Name} ({narrative.Steps[0].Id})");
                }
            }
            
            Console.WriteLine("\n✓ Narrative definitions built successfully!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n✗ ERROR: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
    }
}