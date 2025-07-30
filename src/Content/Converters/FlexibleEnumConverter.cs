using System;
using Newtonsoft.Json;

/// <summary>
/// Flexible enum converter that maps JSON values to enum values with fallback support
/// </summary>
public class FlexibleEnumConverter<T> : JsonConverter<T> where T : struct, Enum
{
    public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Null)
            return default(T);
            
        var value = reader.Value?.ToString();
        if (string.IsNullOrEmpty(value))
            return default(T);
            
        // Try exact match first
        if (Enum.TryParse<T>(value, true, out var result))
            return result;
            
        // Handle special mappings for EffectType
        if (typeof(T) == typeof(EffectType))
        {
            var mappedValue = value switch
            {
                "RemoveFlag" => "ClearFlag",
                "GiveCoins" => "ModifyCoins",
                "AddToQueue" => "AddLetterToQueue",
                _ => value
            };
            
            if (Enum.TryParse<T>(mappedValue, true, out var mapped))
                return mapped;
        }
        
        // Handle special mappings for ConditionType
        if (typeof(T) == typeof(ConditionType))
        {
            var mappedValue = value switch
            {
                "LetterInQueue" => "HasLetter",
                _ => value
            };
            
            if (Enum.TryParse<T>(mappedValue, true, out var mapped))
                return mapped;
        }
        
        // Log warning and return default
        Console.WriteLine($"[FlexibleEnumConverter] Warning: Unknown {typeof(T).Name} value: {value}");
        return default(T);
    }
    
    public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
    {
        writer.WriteValue(value.ToString());
    }
}