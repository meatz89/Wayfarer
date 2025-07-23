using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Wayfarer.Content.Utilities;

/// <summary>
/// Loads game configuration from JSON files.
/// Supports hot-reload for development and balance testing.
/// </summary>
public class GameConfigurationLoader
{
    private readonly IContentDirectory _contentDirectory;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public GameConfigurationLoader(IContentDirectory contentDirectory)
    {
        _contentDirectory = contentDirectory;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
            Converters = 
            { 
                new JsonStringEnumConverter(),
                new ConnectionTypeDictionaryConverter(),
                new ProfessionsDictionaryConverter()
            }
        };
    }
    
    /// <summary>
    /// Load configuration from game-config.json
    /// </summary>
    public GameConfiguration LoadConfiguration()
    {
        var configPath = Path.Combine(_contentDirectory.Path, "game-config.json");
        
        if (!File.Exists(configPath))
        {
            // Return default configuration if file doesn't exist
            Console.WriteLine($"Warning: game-config.json not found at {configPath}. Using default configuration.");
            return GameConfiguration.CreateDefault();
        }
        
        try
        {
            var json = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<GameConfiguration>(json, _jsonOptions);
            
            if (config == null)
            {
                throw new InvalidOperationException("Failed to deserialize game configuration");
            }
            
            // Validate configuration
            ValidateConfiguration(config);
            
            return config;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading game configuration: {ex.Message}");
            Console.WriteLine("Using default configuration.");
            return GameConfiguration.CreateDefault();
        }
    }
    
    /// <summary>
    /// Reload configuration from disk (for hot-reload during development)
    /// </summary>
    public GameConfiguration ReloadConfiguration()
    {
        return LoadConfiguration();
    }
    
    /// <summary>
    /// Save current configuration to disk (for development/testing)
    /// </summary>
    public void SaveConfiguration(GameConfiguration config)
    {
        var configPath = Path.Combine(_contentDirectory.Path, "game-config.json");
        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true,
            Converters = 
            { 
                new JsonStringEnumConverter(),
                new ConnectionTypeDictionaryConverter(),
                new ProfessionsDictionaryConverter()
            }
        });
        
        File.WriteAllText(configPath, json);
    }
    
    private void ValidateConfiguration(GameConfiguration config)
    {
        // Validate queue sizes
        if (config.LetterQueue.MaxQueueSize < 1 || config.LetterQueue.MaxQueueSize > 20)
        {
            throw new InvalidOperationException("Invalid queue size. Must be between 1 and 20.");
        }
        
        // Validate base positions
        foreach (var (tokenType, position) in config.LetterQueue.BasePositions)
        {
            if (position < 1 || position > config.LetterQueue.MaxQueueSize)
            {
                throw new InvalidOperationException($"Invalid base position for {tokenType}. Must be between 1 and {config.LetterQueue.MaxQueueSize}.");
            }
        }
        
        // Validate time configuration
        if (config.Time.HoursPerDay != 24)
        {
            throw new InvalidOperationException("Hours per day must be 24.");
        }
        
        if (config.Time.ActiveDayStartHour >= config.Time.ActiveDayEndHour)
        {
            throw new InvalidOperationException("Active day start must be before active day end.");
        }
        
        // Validate stamina
        if (config.Stamina.MaxStamina < 1 || config.Stamina.MaxStamina > 100)
        {
            throw new InvalidOperationException("Invalid max stamina. Must be between 1 and 100.");
        }
        
        // Validate token thresholds
        if (config.TokenEconomy.BasicLetterThreshold > config.TokenEconomy.QualityLetterThreshold ||
            config.TokenEconomy.QualityLetterThreshold > config.TokenEconomy.PremiumLetterThreshold)
        {
            throw new InvalidOperationException("Token thresholds must be in ascending order: Basic < Quality < Premium.");
        }
    }
}

/// <summary>
/// Custom converter for Dictionary<ConnectionType, T>
/// </summary>
public class ConnectionTypeDictionaryConverter : JsonConverter<Dictionary<ConnectionType, int>>
{
    public override Dictionary<ConnectionType, int> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dictionary = new Dictionary<ConnectionType, int>();
        
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }
        
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return dictionary;
            }
            
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }
            
            var propertyName = reader.GetString();
            reader.Read();
            
            if (EnumParser.TryParse<ConnectionType>(propertyName, out var connectionType))
            {
                dictionary[connectionType] = reader.GetInt32();
            }
        }
        
        throw new JsonException();
    }
    
    public override void Write(Utf8JsonWriter writer, Dictionary<ConnectionType, int> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        foreach (var kvp in value)
        {
            writer.WriteNumber(kvp.Key.ToString(), kvp.Value);
        }
        
        writer.WriteEndObject();
    }
}

/// <summary>
/// Custom converter for Dictionary<Professions, T>
/// </summary>
public class ProfessionsDictionaryConverter : JsonConverter<Dictionary<Professions, WorkReward>>
{
    public override Dictionary<Professions, WorkReward> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dictionary = new Dictionary<Professions, WorkReward>();
        
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }
        
        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return dictionary;
            }
            
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }
            
            var propertyName = reader.GetString();
            reader.Read();
            
            if (EnumParser.TryParse<Professions>(propertyName, out var profession))
            {
                var workReward = JsonSerializer.Deserialize<WorkReward>(ref reader, options);
                if (workReward != null)
                {
                    dictionary[profession] = workReward;
                }
            }
        }
        
        throw new JsonException();
    }
    
    public override void Write(Utf8JsonWriter writer, Dictionary<Professions, WorkReward> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        foreach (var kvp in value)
        {
            writer.WritePropertyName(kvp.Key.ToString());
            JsonSerializer.Serialize(writer, kvp.Value, options);
        }
        
        writer.WriteEndObject();
    }
}