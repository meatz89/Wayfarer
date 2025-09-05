using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class TestJsonDeserialization
{
    public static void Run()
    {
        Console.WriteLine("\n=== Testing JSON Deserialization Issue ===\n");

        // Load the JSON file
        string jsonPath = "/mnt/c/git/wayfarer/src/Content/Core/core_game_package.json";
        string json = File.ReadAllText(jsonPath);

        // Deserialize to Package
        JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        Package package = JsonSerializer.Deserialize<Package>(json, options);

        // Find an exchange card (marcus_bulk_purchase)
        ConversationCardDTO? exchangeCard = package.Content.Cards.Find(c => c.Id == "marcus_bulk_purchase");
        if (exchangeCard == null)
        {
            Console.WriteLine("ERROR: Could not find marcus_bulk_purchase card");
            return;
        }

        Console.WriteLine($"Found card: {exchangeCard.Id}");
        Console.WriteLine($"SuccessEffect Type: {exchangeCard.SuccessEffect?.Type}");
        Console.WriteLine($"SuccessEffect.Data is null? {exchangeCard.SuccessEffect?.Data == null}");

        if (exchangeCard.SuccessEffect?.Data != null)
        {
            Console.WriteLine($"SuccessEffect.Data type: {exchangeCard.SuccessEffect.Data.GetType().FullName}");
            Console.WriteLine($"SuccessEffect.Data count: {exchangeCard.SuccessEffect.Data.Count}");

            foreach (KeyValuePair<string, object> kvp in exchangeCard.SuccessEffect.Data)
            {
                Console.WriteLine($"  Key: {kvp.Key}, Value type: {kvp.Value?.GetType().FullName ?? "null"}");

                // This is the problem - the nested dictionaries are JsonElement, not Dictionary
                if (kvp.Value is JsonElement jsonElement)
                {
                    Console.WriteLine($"    JsonElement ValueKind: {jsonElement.ValueKind}");

                    if (jsonElement.ValueKind == JsonValueKind.Object)
                    {
                        // We need to manually convert JsonElement to Dictionary
                        Dictionary<string, object> dict = new Dictionary<string, object>();
                        foreach (JsonProperty prop in jsonElement.EnumerateObject())
                        {
                            Console.WriteLine($"      Property: {prop.Name} = {prop.Value} (Kind: {prop.Value.ValueKind})");

                            // Convert JsonElement values to appropriate types
                            if (prop.Value.ValueKind == JsonValueKind.Number)
                            {
                                dict[prop.Name] = prop.Value.GetInt32();
                            }
                            else if (prop.Value.ValueKind == JsonValueKind.String)
                            {
                                dict[prop.Name] = prop.Value.GetString();
                            }
                            else
                            {
                                dict[prop.Name] = prop.Value.ToString();
                            }
                        }
                        Console.WriteLine($"      Converted to dictionary with {dict.Count} items");
                    }
                }
            }
        }

        Console.WriteLine("\n=== This is the core issue - nested objects become JsonElement, not Dictionary ===");
        Console.WriteLine("The fix is to handle JsonElement in SessionCardDeck.CreateFromTemplates");
    }

    public static void Main(string[] args)
    {
        Run();
    }
}