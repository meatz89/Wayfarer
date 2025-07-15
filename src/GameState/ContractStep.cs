using Wayfarer.Game.ActionSystem;

/// <summary>
/// Represents a single step that must be completed to fulfill a contract.
/// This unified approach replaces the separate requirement arrays in Contract class.
/// </summary>
public abstract class ContractStep
{
    /// <summary>
    /// Unique identifier for this step within the contract
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>
    /// Human-readable description of what this step requires
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Whether this step has been completed
    /// </summary>
    public bool IsCompleted { get; set; } = false;

    /// <summary>
    /// Whether this step is required for contract completion (vs optional bonus steps)
    /// </summary>
    public bool IsRequired { get; set; } = true;

    /// <summary>
    /// Order hint for steps that should be completed in sequence (0 = no order requirement)
    /// </summary>
    public int OrderHint { get; set; } = 0;

    /// <summary>
    /// Check if the player's current action completes this step
    /// </summary>
    public abstract bool CheckCompletion(Player player, string currentLocationId, object actionContext = null, ItemRepository itemRepository = null);

    /// <summary>
    /// Get detailed information about what the player needs to do to complete this step
    /// </summary>
    public abstract ContractStepRequirement GetRequirement();

    /// <summary>
    /// Reset this step to incomplete state (for contract resets)
    /// </summary>
    public virtual void Reset()
    {
        IsCompleted = false;
    }
}

/// <summary>
/// Contract step that requires traveling to a specific location
/// </summary>
public class TravelStep : ContractStep
{
    public string RequiredLocationId { get; set; } = "";

    public override bool CheckCompletion(Player player, string currentLocationId, object actionContext = null, ItemRepository itemRepository = null)
    {
        if (IsCompleted) return true;

        bool completed = currentLocationId == RequiredLocationId;
        if (completed)
        {
            IsCompleted = true;
        }

        return completed;
    }

    public override ContractStepRequirement GetRequirement()
    {
        return new ContractStepRequirement
        {
            Type = ContractStepType.Travel,
            Description = Description,
            TargetLocationId = RequiredLocationId,
            IsCompleted = IsCompleted
        };
    }
}

/// <summary>
/// Contract step that requires buying or selling an item at a specific location
/// </summary>
public class TransactionStep : ContractStep
{
    public string ItemId { get; set; } = "";
    public string LocationId { get; set; } = "";
    public TransactionType TransactionType { get; set; }
    public int Quantity { get; set; } = 1;
    public int? MinPrice { get; set; }
    public int? MaxPrice { get; set; }

    public override bool CheckCompletion(Player player, string currentLocationId, object actionContext = null, ItemRepository itemRepository = null)
    {
        Console.WriteLine($"[DEBUG] TransactionStep {Id}: CheckCompletion called");
        if (IsCompleted) 
        {
            Console.WriteLine($"[DEBUG] TransactionStep {Id}: Already completed");
            return true;
        }

        // ActionContext should be a transaction details object
        if (actionContext is not TransactionContext transaction) 
        {
            Console.WriteLine($"[DEBUG] TransactionStep {Id}: ActionContext is not TransactionContext (got {actionContext?.GetType().Name})");
            return false;
        }

        Console.WriteLine($"[DEBUG] TransactionStep {Id}: Comparing transaction");
        Console.WriteLine($"[DEBUG] TransactionStep {Id}: Expected - ItemId={ItemId}, LocationId={LocationId}, Type={TransactionType}, Qty={Quantity}");
        Console.WriteLine($"[DEBUG] TransactionStep {Id}: Actual - ItemId={transaction.ItemId}, LocationId={transaction.LocationId}, Type={transaction.TransactionType}, Qty={transaction.Quantity}");

        bool itemMatch = transaction.ItemId == ItemId;
        bool locationMatch = transaction.LocationId == LocationId;
        bool typeMatch = transaction.TransactionType == TransactionType;
        bool quantityMatch = transaction.Quantity >= Quantity;
        bool priceMatch = (MinPrice == null || transaction.Price >= MinPrice) && (MaxPrice == null || transaction.Price <= MaxPrice);

        Console.WriteLine($"[DEBUG] TransactionStep {Id}: Matches - Item={itemMatch}, Location={locationMatch}, Type={typeMatch}, Quantity={quantityMatch}, Price={priceMatch}");

        bool completed = itemMatch && locationMatch && typeMatch && quantityMatch && priceMatch;

        if (completed)
        {
            Console.WriteLine($"[DEBUG] TransactionStep {Id}: COMPLETED! Setting IsCompleted=true");
            IsCompleted = true;
        }
        else
        {
            Console.WriteLine($"[DEBUG] TransactionStep {Id}: NOT completed");
        }

        return completed;
    }

    public override ContractStepRequirement GetRequirement()
    {
        return new ContractStepRequirement
        {
            Type = ContractStepType.Transaction,
            Description = Description,
            ItemId = ItemId,
            TargetLocationId = LocationId,
            TransactionType = TransactionType,
            Quantity = Quantity,
            MinPrice = MinPrice,
            MaxPrice = MaxPrice,
            IsCompleted = IsCompleted
        };
    }
}

/// <summary>
/// Contract step that requires talking to a specific NPC
/// </summary>
public class ConversationStep : ContractStep
{
    public string RequiredNPCId { get; set; } = "";
    public string RequiredLocationId { get; set; } = ""; // Optional: NPC must be at specific location

    public override bool CheckCompletion(Player player, string currentLocationId, object actionContext = null, ItemRepository itemRepository = null)
    {
        if (IsCompleted) return true;

        // ActionContext should be conversation details
        if (actionContext is not ConversationContext conversation) return false;

        bool completed = conversation.NPCId == RequiredNPCId &&
                        (string.IsNullOrEmpty(RequiredLocationId) || currentLocationId == RequiredLocationId);

        if (completed)
        {
            IsCompleted = true;
        }

        return completed;
    }

    public override ContractStepRequirement GetRequirement()
    {
        return new ContractStepRequirement
        {
            Type = ContractStepType.Conversation,
            Description = Description,
            RequiredNPCId = RequiredNPCId,
            TargetLocationId = RequiredLocationId,
            IsCompleted = IsCompleted
        };
    }
}

/// <summary>
/// Contract step that requires performing a specific location action
/// </summary>
public class LocationActionStep : ContractStep
{
    public string RequiredActionId { get; set; } = "";
    public string RequiredLocationId { get; set; } = "";

    public override bool CheckCompletion(Player player, string currentLocationId, object actionContext = null, ItemRepository itemRepository = null)
    {
        if (IsCompleted) return true;

        // ActionContext should be location action details
        if (actionContext is not LocationActionContext locationAction) return false;

        bool completed = locationAction.ActionId == RequiredActionId &&
                        currentLocationId == RequiredLocationId;

        if (completed)
        {
            IsCompleted = true;
        }

        return completed;
    }

    public override ContractStepRequirement GetRequirement()
    {
        return new ContractStepRequirement
        {
            Type = ContractStepType.LocationAction,
            Description = Description,
            RequiredActionId = RequiredActionId,
            TargetLocationId = RequiredLocationId,
            IsCompleted = IsCompleted
        };
    }
}

/// <summary>
/// Contract step that requires having specific equipment categories
/// </summary>
public class EquipmentStep : ContractStep
{
    public List<EquipmentCategory> RequiredEquipmentCategories { get; set; } = new();

    public override bool CheckCompletion(Player player, string currentLocationId, object actionContext = null, ItemRepository itemRepository = null)
    {
        if (IsCompleted) return true;

        // Check if player has all required equipment categories
        bool completed = RequiredEquipmentCategories.All(category =>
            PlayerHasEquipmentCategory(player, category, itemRepository));

        if (completed)
        {
            IsCompleted = true;
        }

        return completed;
    }

    public override ContractStepRequirement GetRequirement()
    {
        return new ContractStepRequirement
        {
            Type = ContractStepType.Equipment,
            Description = Description,
            RequiredEquipmentCategories = RequiredEquipmentCategories.ToList(),
            IsCompleted = IsCompleted
        };
    }

    private bool PlayerHasEquipmentCategory(Player player, EquipmentCategory category, ItemRepository itemRepository)
    {
        // Check if player has any items with the required equipment category
        // Uses proper repository access to get actual Item objects and check their categories
        
        if (itemRepository == null)
        {
            // If no ItemRepository is provided, cannot check equipment categories
            return false;
        }
        
        foreach (string itemId in player.Inventory.ItemSlots)
        {
            if (string.IsNullOrEmpty(itemId)) continue;
            
            Item item = itemRepository.GetItemById(itemId);
            if (item?.HasEquipmentCategory(category) == true)
            {
                return true;
            }
        }
        
        return false;
    }
}

// === ACTION CONTEXT CLASSES ===

/// <summary>
/// Context information for transaction actions
/// </summary>
public class TransactionContext
{
    public string ItemId { get; set; } = "";
    public string LocationId { get; set; } = "";
    public TransactionType TransactionType { get; set; }
    public int Quantity { get; set; }
    public int Price { get; set; }
}

/// <summary>
/// Context information for NPC conversations
/// </summary>
public class ConversationContext
{
    public string NPCId { get; set; } = "";
    public string LocationId { get; set; } = "";
}

/// <summary>
/// Context information for location actions
/// </summary>
public class LocationActionContext
{
    public string ActionId { get; set; } = "";
    public string LocationId { get; set; } = "";
    public string LocationSpotId { get; set; } = "";
}

// === REQUIREMENT INFORMATION CLASSES ===

/// <summary>
/// Information about what a contract step requires for completion
/// </summary>
public class ContractStepRequirement
{
    public ContractStepType Type { get; set; }
    public string Description { get; set; } = "";
    public bool IsCompleted { get; set; }

    // Travel requirements
    public string TargetLocationId { get; set; } = "";

    // Transaction requirements
    public string ItemId { get; set; } = "";
    public TransactionType TransactionType { get; set; }
    public int Quantity { get; set; }
    public int? MinPrice { get; set; }
    public int? MaxPrice { get; set; }

    // Conversation requirements
    public string RequiredNPCId { get; set; } = "";

    // Location action requirements
    public string RequiredActionId { get; set; } = "";

    // Equipment requirements
    public List<EquipmentCategory> RequiredEquipmentCategories { get; set; } = new();
}

/// <summary>
/// Types of contract steps
/// </summary>
public enum ContractStepType
{
    Travel,
    Transaction,
    Conversation,
    LocationAction,
    Equipment
}