using System.Text.Json;

public static class ContractParser
{
    public static Contract ParseContract(string json)
    {
        JsonDocumentOptions options = new JsonDocumentOptions
        {
            AllowTrailingCommas = true
        };

        using JsonDocument doc = JsonDocument.Parse(json, options);
        JsonElement root = doc.RootElement;

        Contract contract = new Contract
        {
            Id = GetStringProperty(root, "id", ""),
            Description = GetStringProperty(root, "description", ""),
            StartDay = GetIntProperty(root, "startDay", 1),
            DueDay = GetIntProperty(root, "dueDay", 5),
            Payment = GetIntProperty(root, "payment", 0),
            FailurePenalty = GetStringProperty(root, "failurePenalty", ""),
            IsCompleted = GetBoolProperty(root, "isCompleted", false),
            IsFailed = GetBoolProperty(root, "isFailed", false),
            UnlocksContractIds = GetStringArray(root, "unlocksContractIds"),
            LocksContractIds = GetStringArray(root, "locksContractIds"),

            CompletionSteps = GetCompletionStepsArray(root, "completionSteps")
        };

        return contract;
    }


    private static string GetStringProperty(JsonElement element, string propertyName, string defaultValue)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property) &&
            property.ValueKind == JsonValueKind.String)
        {
            string value = property.GetString() ?? defaultValue;
            return !string.IsNullOrWhiteSpace(value) ? value : defaultValue;
        }
        return defaultValue;
    }

    private static int GetIntProperty(JsonElement element, string propertyName, int defaultValue)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property) &&
            property.ValueKind == JsonValueKind.Number)
        {
            return property.GetInt32();
        }
        return defaultValue;
    }

    private static bool GetBoolProperty(JsonElement element, string propertyName, bool defaultValue)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property) &&
            property.ValueKind == JsonValueKind.True || property.ValueKind == JsonValueKind.False)
        {
            return property.GetBoolean();
        }
        return defaultValue;
    }

    private static List<string> GetStringArray(JsonElement element, string propertyName)
    {
        List<string> results = new List<string>();

        if (element.TryGetProperty(propertyName, out JsonElement arrayElement) &&
            arrayElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in arrayElement.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    string value = item.GetString() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        results.Add(value);
                    }
                }
            }
        }

        return results;
    }

    private static List<ContractTransaction> GetTransactionArray(JsonElement element, string propertyName)
    {
        List<ContractTransaction> results = new List<ContractTransaction>();

        if (element.TryGetProperty(propertyName, out JsonElement arrayElement) &&
            arrayElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in arrayElement.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Object)
                {
                    string itemId = GetStringProperty(item, "itemId", "");
                    string locationId = GetStringProperty(item, "locationId", "");
                    string transactionTypeStr = GetStringProperty(item, "transactionType", "Sell");
                    int quantity = GetIntProperty(item, "quantity", 1);

                    if (Enum.TryParse<TransactionType>(transactionTypeStr, out TransactionType transactionType))
                    {
                        results.Add(new ContractTransaction(itemId, locationId, transactionType, quantity));
                    }
                }
            }
        }

        return results;
    }

    /// <summary>
    /// Parse completion steps array from JSON with polymorphic step type support
    /// </summary>
    private static List<ContractStep> GetCompletionStepsArray(JsonElement element, string propertyName)
    {
        List<ContractStep> results = new List<ContractStep>();

        if (element.TryGetProperty(propertyName, out JsonElement arrayElement) &&
            arrayElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement stepElement in arrayElement.EnumerateArray())
            {
                if (stepElement.ValueKind == JsonValueKind.Object)
                {
                    ContractStep step = ParseContractStep(stepElement);
                    if (step != null)
                    {
                        results.Add(step);
                    }
                }
            }
        }

        return results;
    }

    /// <summary>
    /// Parse a single contract step based on its type discriminator
    /// </summary>
    private static ContractStep ParseContractStep(JsonElement stepElement)
    {
        string stepType = GetStringProperty(stepElement, "type", "");

        // Common properties for all step types
        string id = GetStringProperty(stepElement, "id", "");
        string description = GetStringProperty(stepElement, "description", "");
        bool isRequired = GetBoolProperty(stepElement, "isRequired", true);
        int orderHint = GetIntProperty(stepElement, "orderHint", 0);
        bool isCompleted = GetBoolProperty(stepElement, "isCompleted", false);

        ContractStep step = stepType switch
        {
            "TravelStep" => new TravelStep
            {
                RequiredLocationId = GetStringProperty(stepElement, "requiredLocationId", "")
            },
            "TransactionStep" => new TransactionStep
            {
                ItemId = GetStringProperty(stepElement, "itemId", ""),
                LocationId = GetStringProperty(stepElement, "locationId", ""),
                TransactionType = Enum.TryParse<TransactionType>(
                    GetStringProperty(stepElement, "transactionType", "Sell"),
                    out TransactionType transType) ? transType : TransactionType.Sell,
                Quantity = GetIntProperty(stepElement, "quantity", 1),
                MinPrice = GetNullableIntProperty(stepElement, "minPrice"),
                MaxPrice = GetNullableIntProperty(stepElement, "maxPrice")
            },
            "ConversationStep" => new ConversationStep
            {
                RequiredNPCId = GetStringProperty(stepElement, "requiredNPCId", ""),
                RequiredLocationId = GetStringProperty(stepElement, "requiredLocationId", "")
            },
            "LocationActionStep" => new LocationActionStep
            {
                RequiredActionId = GetStringProperty(stepElement, "requiredActionId", ""),
                RequiredLocationId = GetStringProperty(stepElement, "requiredLocationId", "")
            },
            "EquipmentStep" => new EquipmentStep
            {
                RequiredEquipmentCategories = GetEquipmentCategoryArray(stepElement, "requiredEquipmentCategories")
            },
            _ => null // Unknown step type, skip
        };

        // Apply common properties if step was created successfully
        if (step != null)
        {
            step.Id = id;
            step.Description = description;
            step.IsRequired = isRequired;
            step.OrderHint = orderHint;
            step.IsCompleted = isCompleted;
        }

        return step;
    }

    /// <summary>
    /// Get nullable integer property for optional price constraints
    /// </summary>
    private static int? GetNullableIntProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out JsonElement property) &&
            property.ValueKind == JsonValueKind.Number)
        {
            return property.GetInt32();
        }
        return null;
    }

    /// <summary>
    /// Parse equipment category array for EquipmentStep
    /// </summary>
    private static List<EquipmentCategory> GetEquipmentCategoryArray(JsonElement element, string propertyName)
    {
        List<EquipmentCategory> results = new List<EquipmentCategory>();

        if (element.TryGetProperty(propertyName, out JsonElement arrayElement) &&
            arrayElement.ValueKind == JsonValueKind.Array)
        {
            foreach (JsonElement item in arrayElement.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    string categoryStr = item.GetString() ?? "";
                    if (Enum.TryParse<EquipmentCategory>(categoryStr, out EquipmentCategory category))
                    {
                        results.Add(category);
                    }
                }
            }
        }

        return results;
    }
}