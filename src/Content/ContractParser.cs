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
            
            // Completion action pattern properties
            RequiredDestinations = GetStringArray(root, "requiredDestinations"),
            RequiredTransactions = GetTransactionArray(root, "requiredTransactions"),
            RequiredNPCConversations = GetStringArray(root, "requiredNPCConversations"),
            RequiredLocationActions = GetStringArray(root, "requiredLocationActions"),
            
            // Initialize completed action tracking
            CompletedDestinations = new HashSet<string>(),
            CompletedTransactions = new List<ContractTransaction>(),
            CompletedNPCConversations = new HashSet<string>(),
            CompletedLocationActions = new HashSet<string>()
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
}