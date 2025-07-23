using System;
using System.Collections.Generic;
using System.Text.Json;
using Wayfarer.Content.Utilities;

namespace Wayfarer.Content.Validation.Validators
{
    /// <summary>
    /// Validates route JSON content files.
    /// </summary>
    public class RouteValidator : IContentValidator
    {
        private readonly HashSet<string> _requiredFields = new HashSet<string>
        {
            "id", "fromLocationId", "toLocationId", "method", "baseTravelTime", "baseCost"
        };
        
        public bool CanValidate(string fileName)
        {
            return fileName.Equals("routes.json", StringComparison.OrdinalIgnoreCase) ||
                   fileName.EndsWith("_routes.json", StringComparison.OrdinalIgnoreCase);
        }
        
        public IEnumerable<ValidationError> Validate(string content, string fileName)
        {
            var errors = new List<ValidationError>();
            
            try
            {
                using var doc = JsonDocument.Parse(content);
                var root = doc.RootElement;
                
                if (root.ValueKind != JsonValueKind.Array)
                {
                    errors.Add(new ValidationError(
                        fileName,
                        "Routes file must contain a JSON array",
                        ValidationSeverity.Critical));
                    return errors;
                }
                
                int index = 0;
                var routeIds = new HashSet<string>();
                
                foreach (var routeElement in root.EnumerateArray())
                {
                    ValidateRoute(routeElement, index, fileName, errors, routeIds);
                    index++;
                }
            }
            catch (Exception ex)
            {
                errors.Add(new ValidationError(
                    fileName,
                    $"Failed to validate routes: {ex.Message}",
                    ValidationSeverity.Critical));
            }
            
            return errors;
        }
        
        private void ValidateRoute(JsonElement route, int index, string fileName, 
            List<ValidationError> errors, HashSet<string> routeIds)
        {
            string routeId = GetStringProperty(route, "id") ?? $"Route[{index}]";
            
            // Check for duplicate IDs
            if (!string.IsNullOrEmpty(routeId) && !routeIds.Add(routeId))
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{routeId}",
                    $"Duplicate route ID: {routeId}",
                    ValidationSeverity.Critical));
            }
            
            // Check required fields
            foreach (var field in _requiredFields)
            {
                if (!route.TryGetProperty(field, out _))
                {
                    errors.Add(new ValidationError(
                        $"{fileName}:{routeId}",
                        $"Missing required field: {field}",
                        ValidationSeverity.Critical));
                }
            }
            
            // Validate travel method
            if (route.TryGetProperty("method", out var method) && 
                method.ValueKind == JsonValueKind.String)
            {
                var methodStr = method.GetString();
                if (!string.IsNullOrEmpty(methodStr) && 
                    !EnumParser.TryParse<TravelMethods>(methodStr, out _))
                {
                    errors.Add(new ValidationError(
                        $"{fileName}:{routeId}",
                        $"Invalid travel method: '{methodStr}'",
                        ValidationSeverity.Critical));
                }
            }
            
            // Validate numeric fields
            ValidateNumericField(route, "baseTravelTime", routeId, fileName, errors, min: 1);
            ValidateNumericField(route, "baseCost", routeId, fileName, errors, min: 0);
            
            // Validate departure times
            if (route.TryGetProperty("departureTimes", out var departureTimes) && 
                departureTimes.ValueKind == JsonValueKind.Array)
            {
                foreach (var time in departureTimes.EnumerateArray())
                {
                    if (time.ValueKind == JsonValueKind.String)
                    {
                        var timeStr = time.GetString();
                        if (!string.IsNullOrEmpty(timeStr) && 
                            !EnumParser.TryParse<TimeBlocks>(timeStr, out _))
                        {
                            errors.Add(new ValidationError(
                                $"{fileName}:{routeId}",
                                $"Invalid departure time: '{timeStr}'",
                                ValidationSeverity.Warning));
                        }
                    }
                }
            }
            
            // Validate terrain types
            if (route.TryGetProperty("terrainTypes", out var terrainTypes) && 
                terrainTypes.ValueKind == JsonValueKind.Array)
            {
                foreach (var terrain in terrainTypes.EnumerateArray())
                {
                    if (terrain.ValueKind == JsonValueKind.String)
                    {
                        var terrainStr = terrain.GetString();
                        if (!string.IsNullOrEmpty(terrainStr) && 
                            !EnumParser.TryParse<TerrainCategory>(terrainStr, out _))
                        {
                            errors.Add(new ValidationError(
                                $"{fileName}:{routeId}",
                                $"Invalid terrain type: '{terrainStr}'",
                                ValidationSeverity.Warning));
                        }
                    }
                }
            }
            
            // Validate route consistency
            var fromId = GetStringProperty(route, "fromLocationId");
            var toId = GetStringProperty(route, "toLocationId");
            
            if (!string.IsNullOrEmpty(fromId) && !string.IsNullOrEmpty(toId) && fromId == toId)
            {
                errors.Add(new ValidationError(
                    $"{fileName}:{routeId}",
                    "Route cannot have the same from and to location",
                    ValidationSeverity.Critical));
            }
        }
        
        private void ValidateNumericField(JsonElement item, string fieldName, string itemId, 
            string fileName, List<ValidationError> errors, int? min = null, int? max = null)
        {
            if (item.TryGetProperty(fieldName, out var field))
            {
                if (field.ValueKind != JsonValueKind.Number || !field.TryGetInt32(out int value))
                {
                    errors.Add(new ValidationError(
                        $"{fileName}:{itemId}",
                        $"Field '{fieldName}' must be a valid integer",
                        ValidationSeverity.Critical));
                }
                else
                {
                    if (min.HasValue && value < min.Value)
                    {
                        errors.Add(new ValidationError(
                            $"{fileName}:{itemId}",
                            $"Field '{fieldName}' value {value} is below minimum {min.Value}",
                            ValidationSeverity.Critical));
                    }
                    if (max.HasValue && value > max.Value)
                    {
                        errors.Add(new ValidationError(
                            $"{fileName}:{itemId}",
                            $"Field '{fieldName}' value {value} is above maximum {max.Value}",
                            ValidationSeverity.Critical));
                    }
                }
            }
        }
        
        private string GetStringProperty(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var property) && 
                property.ValueKind == JsonValueKind.String)
            {
                return property.GetString();
            }
            return null;
        }
    }
}